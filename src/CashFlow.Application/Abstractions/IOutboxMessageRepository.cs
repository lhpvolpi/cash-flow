using CashFlow.Domain.Entities;

namespace CashFlow.Application.Abstractions;

public interface IOutboxMessageRepository : IRepository<OutboxMessage> { }

