using CashFlow.Shared.Application.Abstractions;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Application.Abstractions;

public interface IOutboxMessageRepository : IRepository<OutboxMessage> { }

