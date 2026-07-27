using CashFlow.Shared.Application.Models;

namespace CashFlow.Transactions.Application.Abstractions;

public interface IMessageBrokerPublisher<in TMessage> where TMessage : BrokerMessage
{
    Task SendMessageAsync(string queueName, TMessage message, CancellationToken cancellationToken = default);
}
