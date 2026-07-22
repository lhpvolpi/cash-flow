namespace CashFlow.Consolidation.Infrastructure.Data.Repositories;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : BaseEntity
{
    public Repository(ApplicationDbContext dbContext) : base(dbContext) { }
}
