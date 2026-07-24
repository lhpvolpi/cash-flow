namespace CashFlow.Consolidation.Application.Abstractions;

public interface IMessageBrokerConsumer<out TMessage>
{
    Task StartConsumingAsync(
        string queueName,
        Func<TMessage, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default);
}