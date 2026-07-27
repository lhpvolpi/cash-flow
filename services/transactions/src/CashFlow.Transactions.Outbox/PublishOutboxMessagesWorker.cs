using CashFlow.Transactions.Application.OutboxMessages.PublishMessage.Commands;

namespace CashFlow.Transactions.Outbox;

public sealed class PublishOutboxMessagesWorker : BackgroundService
{
    private const string LogPrefix = "PublishOutboxMessages";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PublishOutboxMessagesWorker> _logger;
    private readonly TimeSpan _interval;
    private readonly int _batchSize;

    public PublishOutboxMessagesWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PublishOutboxMessagesWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var intervalInMilliseconds =
            configuration.GetValue("Outbox:IntervalInMilliseconds", 500);

        _interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
        _batchSize = configuration.GetValue("Outbox:BatchSize", 100);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "{LogPrefix} started. Interval: {Interval}ms, BatchSize: {BatchSize}",
            LogPrefix,
            _interval.TotalMilliseconds,
            _batchSize);

        using var timer = new PeriodicTimer(_interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PublishMessagesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "{LogPrefix} cancellation requested",
                LogPrefix);
        }
        finally
        {
            _logger.LogInformation(
                "{LogPrefix} stopped",
                LogPrefix);
        }
    }

    private async Task PublishMessagesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "{LogPrefix} execution started",
                LogPrefix);

            await using var scope = _scopeFactory.CreateAsyncScope();

            var mediator =
                scope.ServiceProvider.GetRequiredService<IMediator>();

            var command = new PublishOutboxMessagesCommand(_batchSize);

            await mediator.Send(command, cancellationToken);

            _logger.LogDebug(
                "{LogPrefix} execution completed successfully",
                LogPrefix);
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
                "{LogPrefix} execution failed",
                LogPrefix);
        }
    }
}
