using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Application.Common.Dtos;
using CashFlow.Transactions.Application.Transactions.ListTransactions.Specifications;

namespace CashFlow.Transactions.Application.Transactions.ListTransactions.Queries;

public class ListTransactionsQueryHandler : IRequestHandler<ListTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public ListTransactionsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<PaginatedList<TransactionDto>> Handle(ListTransactionsQuery request, CancellationToken cancellationToken)
    {
        var specification = new ListTransactionsSpecification(
            request.StartDate,
            request.EndDate,
            request.PageNumber,
            request.PageSize);

        var transactions = await _transactionRepository.ToPaginatedListAsync(specification, cancellationToken);

        var result = transactions.Items.Select(i => new TransactionDto(
            i.Id,
            i.Amount,
            i.Type,
            i.Description,
            i.CreatedAtUtc)).ToList();

        return new PaginatedList<TransactionDto>(
            result,
            transactions.TotalItems,
            transactions.PageNumber,
            transactions.PageSize);
    }
}

