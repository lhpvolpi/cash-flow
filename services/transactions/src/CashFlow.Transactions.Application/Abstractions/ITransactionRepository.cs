using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Application.Abstractions;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<PaginatedList<Transaction>> ToPaginatedListAsync(
         ISpecification<Transaction> specification,
         CancellationToken cancellationToken = default
     );
}

