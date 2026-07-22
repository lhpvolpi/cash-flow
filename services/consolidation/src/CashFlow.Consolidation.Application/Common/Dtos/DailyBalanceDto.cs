namespace CashFlow.Consolidation.Application.Common.Dtos;

public sealed record DailyBalanceDto(
    Guid Id,
    DateOnly Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance
);
