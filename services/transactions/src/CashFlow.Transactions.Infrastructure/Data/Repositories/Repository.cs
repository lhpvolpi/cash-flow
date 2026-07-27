using CashFlow.Infrastructure.Data;
using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Domain.Entities;

namespace CashFlow.Transactions.Infrastructure.Data.Repositories;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : BaseEntity
{
    public Repository(ApplicationDbContext dbContext) : base(dbContext) { }
}

