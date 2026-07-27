using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Common.Dtos;

namespace CashFlow.Transactions.Application.Transactions.ListTransactions.Queries;

public sealed record ListTransactionsQuery(
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<TransactionDto>>;

