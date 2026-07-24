using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Shared.Application.Models;

namespace CashFlow.Consolidation.Consumer;

public class ProcessBrokerMessagesWorker : IHostedService, IDisposable
{
    private const string LogPrefix = "BrokerMessageConsumer";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessBrokerMessagesWorker> _logger;
    private readonly string _queueName;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _consumerTask;
    private bool _disposed;

    public ProcessBrokerMessagesWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ProcessBrokerMessagesWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = configuration.GetValue("MessageBroker:QueueName", "daily-balance-updates")
            ?? throw new InvalidOperationException("MessageBroker:QueueName not found in appsettings.json");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} starting at: {Time}", LogPrefix, DateTimeOffset.UtcNow);

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _consumerTask = Task.Run(
            async () => await ConsumeMessages(_cancellationTokenSource.Token),
            _cancellationTokenSource.Token);

        _logger.LogInformation("{LogPrefix} started, listening to queue: {QueueName}", LogPrefix, _queueName);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} stopping at: {Time}", LogPrefix, DateTimeOffset.UtcNow);

        _cancellationTokenSource?.Cancel();

        if (_consumerTask != null)
        {
            try
            {
                await _consumerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
        }

        _logger.LogInformation("{LogPrefix} stopped", LogPrefix);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
            }

            _disposed = true;
        }
    }

    private async Task ConsumeMessages(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IConsolidationMessageBrokerConsumer>();

            // Define message handler
            async Task HandleMessage(BrokerMessage message, CancellationToken ct)
            {
                _logger.LogInformation(
                    "{LogPrefix} processing message {MessageId} from queue {QueueName}",
                    LogPrefix,
                    message.Id,
                    _queueName);

                using var handlerScope = _serviceProvider.CreateScope();

                // Get the handler from DI container
                // This should be the application use case handler
                // For now, we'll just log success - the actual handler would process the message
                await Task.CompletedTask;

                _logger.LogInformation(
                    "{LogPrefix} message {MessageId} processed successfully",
                    LogPrefix,
                    message.Id);
            }

            // Start consuming
            await consumer.StartConsumingAsync(_queueName, HandleMessage, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{LogPrefix} consumer cancelled", LogPrefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogPrefix} consumer error", LogPrefix);
            throw;
        }
    }
}
