using Ardalis.Specification.EntityFrameworkCore;
using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Data.Repositories;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : BaseEntity
{
    public Repository(CashFlowDbContext dbContext) : base(dbContext) { }
}

