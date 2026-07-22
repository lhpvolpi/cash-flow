using CashFlow.Infrastructure.Data;
using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Infrastructure.Data.Repositories;

public class TransactionsRepository : Repository<Transaction>, ITransactionRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Transaction> _dbset;

    public TransactionsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
        _dbset = _dbContext.Set<Transaction>();
    }

    public async Task<PaginatedList<Transaction>> ToPaginatedListAsync(
        ISpecification<Transaction> specification,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await SpecificationEvaluator.GetQuery(
            _dbset.AsQueryable(),
            specification,
            evaluateCriteriaOnly: true
        ).CountAsync(cancellationToken);

        var items = await _dbset.WithSpecification(specification).ToListAsync(cancellationToken);

        var skip = specification.Skip;
        var take = specification.Take;
        var pageNumber = (skip / take) + 1;
        var pageSize = take;

        return new PaginatedList<Transaction>(items!, totalCount, pageNumber, pageSize);
    }
}
