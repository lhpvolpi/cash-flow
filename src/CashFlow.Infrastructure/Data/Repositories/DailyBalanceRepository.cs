using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Data.Repositories;

public sealed class DailyBalanceRepository : Repository<DailyBalance>, IDailyBalanceRepository
{
    public DailyBalanceRepository(CashFlowDbContext dbContext) : base(dbContext) { }
}
