using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Transactions.Application.Common.Dtos;

public sealed record TransactionDto(Guid Id,
    decimal Amount,
    ETransactionType Type,
    string? Description,
    DateTimeOffset CreatedAtUtc);
