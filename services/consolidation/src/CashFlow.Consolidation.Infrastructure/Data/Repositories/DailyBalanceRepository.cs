namespace CashFlow.Consolidation.Infrastructure.Data.Repositories;

public class DailyBalanceRepository : Repository<DailyBalance>, IDailyBalanceRepository
{
    public DailyBalanceRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}
