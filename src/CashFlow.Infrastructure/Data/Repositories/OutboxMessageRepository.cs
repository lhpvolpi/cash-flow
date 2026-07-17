using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Data.Repositories;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(CashFlowDbContext dbContext) : base(dbContext) { }
}
