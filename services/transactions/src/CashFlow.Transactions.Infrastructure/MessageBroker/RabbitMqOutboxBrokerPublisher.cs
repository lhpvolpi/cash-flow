using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Abstractions;

namespace CashFlow.Transactions.Infrastructure.MessageBroker;

public sealed class RabbitMqOutboxBrokerPublisher : IOutboxBrokerPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxRetries = 3;
    private const int BaseDelayMs = 100;

    private readonly IModel _channel;
    private readonly ILogger<RabbitMqOutboxBrokerPublisher> _logger;
    private readonly object _publishLock = new();

    public RabbitMqOutboxBrokerPublisher(
        IConnection connection,
        ILogger<RabbitMqOutboxBrokerPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _channel = connection.CreateModel();

        _channel.ConfirmSelect();
    }

    public async Task SendMessageAsync(
        string queueName,
        BrokerMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(message);

        cancellationToken.ThrowIfCancellationRequested();

        await Task.Run(() => PublishWithRetryAsync(queueName, message, cancellationToken), cancellationToken);
    }

    private async Task PublishWithRetryAsync(
        string queueName,
        BrokerMessage message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message, JsonOptions);
        var body = Encoding.UTF8.GetBytes(payload);
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            try
            {
                attempt++;

                lock (_publishLock)
                {
                    // Declare queue if it doesn't exist
                    _channel.QueueDeclare(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var properties = _channel.CreateBasicProperties();

                    properties.Persistent = true;
                    properties.ContentType = "application/json";
                    properties.ContentEncoding = "utf-8";
                    properties.MessageId = message.Id.ToString();
                    properties.Timestamp =
                        new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    _channel.BasicPublish(
                        exchange: string.Empty,
                        routingKey: queueName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    _channel.WaitForConfirmsOrDie();
                }

                _logger.LogInformation(
                    "Message {MessageId} published to queue {QueueName} (attempt {Attempt}/{MaxRetries})",
                    message.Id,
                    queueName,
                    attempt,
                    MaxRetries);

                return; // Success
            }
            catch (Exception exception) when (attempt < MaxRetries)
            {
                var delayMs = BaseDelayMs * (int)Math.Pow(2, attempt - 1);

                _logger.LogWarning(
                    exception,
                    "Failed to publish message {MessageId} to queue {QueueName}. Attempt {Attempt}/{MaxRetries}. Retrying in {DelayMs}ms",
                    message.Id,
                    queueName,
                    attempt,
                    MaxRetries,
                    delayMs);

                await Task.Delay(delayMs, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to publish message {MessageId} to queue {QueueName} after {MaxRetries} attempts",
                    message.Id,
                    queueName,
                    MaxRetries);

                throw;
            }
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
