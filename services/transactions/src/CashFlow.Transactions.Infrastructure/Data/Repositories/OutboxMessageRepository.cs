using CashFlow.Infrastructure.Data;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Infrastructure.Data.Repositories;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}
