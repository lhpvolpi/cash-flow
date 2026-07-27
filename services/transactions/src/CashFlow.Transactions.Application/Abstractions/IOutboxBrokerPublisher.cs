using CashFlow.Shared.Application.Models;

namespace CashFlow.Transactions.Application.Abstractions;

public interface IOutboxBrokerPublisher : IMessageBrokerPublisher<BrokerMessage> { }

