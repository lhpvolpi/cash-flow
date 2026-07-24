using CashFlow.Transactions.Application.OutboxMessages.PublishMessage.Commands;

namespace CashFlow.Transactions.Outbox;

public class PublishOutboxMessagesWorker : IHostedService, IDisposable
{
    private const string LogPrefix = "PublishOutboxMessages";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PublishOutboxMessagesWorker> _logger;

    private readonly int _intervalInMilliseconds;
    private readonly int _batchSize;

    private Timer? _timer;
    private bool _disposed;

    public PublishOutboxMessagesWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<PublishOutboxMessagesWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _intervalInMilliseconds = configuration.GetValue("Outbox:IntervalInMilliseconds", 500);
        _batchSize = configuration.GetValue("Outbox:BatchSize", 100);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} starting at: {Time}", LogPrefix, DateTimeOffset.UtcNow);

        var intervalTimeSpan = TimeSpan.FromMilliseconds(_intervalInMilliseconds);
        _timer = new Timer(async _ => await ExecuteTask(), null, intervalTimeSpan, intervalTimeSpan);

        _logger.LogInformation("{LogPrefix} started", LogPrefix);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} stopping at: {Time}", LogPrefix, DateTimeOffset.UtcNow);

        _timer?.Change(Timeout.Infinite, 0);

        _logger.LogInformation("{LogPrefix} stopped", LogPrefix);

        return Task.CompletedTask;
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
                _timer?.Dispose();
            }

            _disposed = true;
        }
    }

    private async Task ExecuteTask()
    {
        try
        {
            _logger.LogInformation("{LogPrefix} executing at: {Time}", LogPrefix, DateTimeOffset.UtcNow);

            await using var scope = _serviceProvider.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var command = new PublishOutboxMessagesCommand(_batchSize);
            await mediator.Send(command);

            _logger.LogInformation("{LogPrefix} executed successfully at: {Time}", LogPrefix, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing {LogPrefix} at: {Time}", LogPrefix, DateTimeOffset.UtcNow);
        }
    }
}

