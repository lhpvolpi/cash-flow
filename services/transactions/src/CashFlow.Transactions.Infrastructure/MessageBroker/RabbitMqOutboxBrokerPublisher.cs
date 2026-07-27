using CashFlow.Shared.Application.Common;
using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Abstractions;

namespace CashFlow.Transactions.Infrastructure.MessageBroker;

public sealed class RabbitMqOutboxBrokerPublisher : IOutboxBrokerPublisher, IDisposable
{
    private const int MaxAttempts = 3;
    private const int BaseRetryDelayMilliseconds = 100;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonEnumConverterFactory()
            }
        };

    private readonly IModel _channel;
    private readonly ILogger<RabbitMqOutboxBrokerPublisher> _logger;
    private readonly object _channelLock = new();

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

        var body = JsonSerializer.SerializeToUtf8Bytes(
            message,
            JsonOptions);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                Publish(queueName, message, body);

                _logger.LogInformation(
                    "Message {MessageId} published to queue {QueueName}",
                    message.Id,
                    queueName);

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                var retryDelay = TimeSpan.FromMilliseconds(
                    BaseRetryDelayMilliseconds * Math.Pow(2, attempt - 1));

                _logger.LogWarning(
                    ex,
                    "Failed to publish message {MessageId} to queue {QueueName}. " +
                    "Attempt {Attempt}/{MaxAttempts}",
                    message.Id,
                    queueName,
                    attempt,
                    MaxAttempts);

                await Task.Delay(retryDelay, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish message {MessageId} to queue {QueueName} " +
                    "after {MaxAttempts} attempts",
                    message.Id,
                    queueName,
                    MaxAttempts);

                throw;
            }
        }
    }

    private void Publish(
        string queueName,
        BrokerMessage message,
        ReadOnlyMemory<byte> body)
    {
        lock (_channelLock)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = string.Empty,
                    ["x-dead-letter-routing-key"] = $"{queueName}.retry"
                });

            var properties = _channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.ContentEncoding = "utf-8";
            properties.MessageId = message.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body);

            _channel.WaitForConfirmsOrDie();
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
