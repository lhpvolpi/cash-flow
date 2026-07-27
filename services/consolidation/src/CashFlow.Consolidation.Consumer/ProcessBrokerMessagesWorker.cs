using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;
using CashFlow.Shared.Application.Common;
using CashFlow.Shared.Application.Common.Events;
using CashFlow.Shared.Application.Models;

namespace CashFlow.Consolidation.Consumer;

public sealed class ProcessBrokerMessagesWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonEnumConverterFactory()
            }
        };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProcessBrokerMessagesWorker> _logger;
    private readonly string _queueName;

    public ProcessBrokerMessagesWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ProcessBrokerMessagesWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _queueName =
            configuration.GetValue<string>("MessageBroker:QueueName")
            ?? "daily-balance-updates";
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consumer starting. Queue: {QueueName}",
            _queueName);

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var consumer =
                scope.ServiceProvider
                    .GetRequiredService<IConsolidationMessageBrokerConsumer>();

            await consumer.StartConsumingAsync(
                _queueName,
                ProcessMessageAsync,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Consumer stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Consumer terminated unexpectedly");

            throw;
        }
    }

    private async Task ProcessMessageAsync(
        BrokerMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var operationEvent =
                message.Payload.RootElement
                    .Deserialize<OperationEventPayload>(JsonOptions);

            if (operationEvent is null)
            {
                _logger.LogWarning(
                    "Could not deserialize message {MessageId}",
                    message.Id);

                return;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();

            var mediator =
                scope.ServiceProvider.GetRequiredService<IMediator>();

            var command = new ProcessTransactionEventCommand(
                operationEvent.TransactionId,
                operationEvent.Amount,
                operationEvent.TransactionType,
                operationEvent.OccurredAtUtc);

            await mediator.Send(command, cancellationToken);

            _logger.LogInformation(
                "Message {MessageId} processed",
                message.Id);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing message {MessageId}",
                message.Id);

            throw;
        }
    }
}
