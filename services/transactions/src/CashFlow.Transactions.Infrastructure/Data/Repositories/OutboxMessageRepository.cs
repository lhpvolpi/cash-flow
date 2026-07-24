using CashFlow.Infrastructure.Data;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Infrastructure.Data.Repositories.Specifications;

namespace CashFlow.Transactions.Infrastructure.Data.Repositories;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    private readonly DbSet<OutboxMessage> _dbset;

    public OutboxMessageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbset = dbContext.Set<OutboxMessage>();
    }

    public async Task<List<OutboxMessage>> GetNextBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var specification = new ListOutboxMessagesSpecification(batchSize);
        return await _dbset.FromSqlRaw(sql: specification.Sql, parameters: specification.Parameters).ToListAsync(cancellationToken);
    }
}
