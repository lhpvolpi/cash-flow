using CashFlow.Shared.Application.Models;

namespace CashFlow.Consolidation.Application.Abstractions;

public interface IConsolidationMessageBrokerConsumer : IMessageBrokerConsumer<BrokerMessage> { }

