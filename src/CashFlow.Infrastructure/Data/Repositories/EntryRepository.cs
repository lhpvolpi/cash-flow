using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Data.Repositories;

public class EntryRepository : Repository<Entry>, IEntryRepository
{
    public EntryRepository(CashFlowDbContext dbContext) : base(dbContext) { }
}
