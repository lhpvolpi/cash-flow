using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Shared.Application.Common;
using CashFlow.Shared.Application.Models;

namespace CashFlow.Consolidation.Infrastructure.MessageBroker;

public sealed class RabbitMqMessageBrokerConsumer
    : IConsolidationMessageBrokerConsumer
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonEnumConverterFactory()
            }
        };

    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqMessageBrokerConsumer> _logger;
    private readonly int _maxRetries;
    private readonly int _retryDelayMilliseconds;

    public RabbitMqMessageBrokerConsumer(
        IConnection connection,
        IConfiguration configuration,
        ILogger<RabbitMqMessageBrokerConsumer> logger)
    {
        _connection = connection;
        _logger = logger;

        _maxRetries = configuration.GetValue<int?>("MessageBroker:MaxRetries") ?? 5;
        _retryDelayMilliseconds = configuration.GetValue<int?>("MessageBroker:RetryDelayMilliseconds") ?? 5000;
    }

    public async Task StartConsumingAsync(
        string queueName,
        Func<BrokerMessage, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(messageHandler);

        var retryQueueName = $"{queueName}.retry";
        var failedQueueName = $"{queueName}.failed";

        using var channel = _connection.CreateModel();

        channel.QueueDeclare(
            queue: failedQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueDeclare(
            queue: retryQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-message-ttl"] = _retryDelayMilliseconds,
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = queueName
            });

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = retryQueueName
            });

        channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (_, args) =>
        {
            var attempt = GetDeathCount(args.BasicProperties, retryQueueName) + 1;

            try
            {
                var message = Deserialize(args.Body);

                if (message is null)
                {
                    _logger.LogWarning(
                        "Could not deserialize message {DeliveryTag} from queue {QueueName} (attempt {Attempt}/{MaxRetries})",
                        args.DeliveryTag,
                        queueName,
                        attempt,
                        _maxRetries);

                    HandleFailure(channel, args, attempt, failedQueueName);

                    return;
                }

                await messageHandler(message, cancellationToken);

                channel.BasicAck(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                channel.BasicNack(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing message {DeliveryTag} from queue {QueueName} (attempt {Attempt}/{MaxRetries})",
                    args.DeliveryTag,
                    queueName,
                    attempt,
                    _maxRetries);

                HandleFailure(channel, args, attempt, failedQueueName);
            }
        };

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Started consuming queue {QueueName}",
            queueName);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Stopped consuming queue {QueueName}",
                queueName);
        }
    }

    private void HandleFailure(
        IModel channel,
        BasicDeliverEventArgs args,
        int attempt,
        string failedQueueName)
    {
        if (attempt >= _maxRetries)
        {
            _logger.LogError(
                "Message {DeliveryTag} exceeded {MaxRetries} attempts, moving to {FailedQueue}",
                args.DeliveryTag,
                _maxRetries,
                failedQueueName);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: failedQueueName,
                basicProperties: args.BasicProperties,
                body: args.Body);

            channel.BasicAck(
                deliveryTag: args.DeliveryTag,
                multiple: false);

            return;
        }

        channel.BasicNack(
            deliveryTag: args.DeliveryTag,
            multiple: false,
            requeue: false);
    }

    private static int GetDeathCount(IBasicProperties properties, string retryQueueName)
    {
        if (properties?.Headers is null
            || !properties.Headers.TryGetValue("x-death", out var raw)
            || raw is not List<object> deaths)
        {
            return 0;
        }

        foreach (var entry in deaths)
        {
            if (entry is not Dictionary<string, object> death
                || !death.TryGetValue("queue", out var queueValue))
            {
                continue;
            }

            var queue = queueValue switch
            {
                byte[] bytes => Encoding.UTF8.GetString(bytes),
                string text => text,
                _ => null
            };

            if (queue == retryQueueName
                && death.TryGetValue("count", out var countValue)
                && countValue is long count)
            {
                return (int)count;
            }
        }

        return 0;
    }

    private static BrokerMessage? Deserialize(ReadOnlyMemory<byte> body)
    {
        return JsonSerializer.Deserialize<BrokerMessage>(
            body.Span,
            JsonOptions);
    }
}
