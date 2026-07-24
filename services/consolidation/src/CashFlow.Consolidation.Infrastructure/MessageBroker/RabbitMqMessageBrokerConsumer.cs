using CashFlow.Shared.Application.Models;


namespace CashFlow.Consolidation.Infrastructure.MessageBroker;

public sealed class RabbitMqMessageBrokerConsumer : IConsolidationMessageBrokerConsumer, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqMessageBrokerConsumer> _logger;
    private IModel? _channel;

    public RabbitMqMessageBrokerConsumer(
        IConnection connection,
        ILogger<RabbitMqMessageBrokerConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(logger);

        _connection = connection;
        _logger = logger;
    }

    public async Task StartConsumingAsync(
        string queueName,
        Func<BrokerMessage, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(messageHandler);

        _channel = _connection.CreateModel();

        // Declare queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<BrokerMessage>(json, JsonOptions);

                if (message != null)
                {
                    await messageHandler(message, cancellationToken);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Started consuming from queue {QueueName}", queueName);

        // Keep consuming until cancellation
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}