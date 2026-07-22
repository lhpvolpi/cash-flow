namespace CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;

public sealed record GetDailyBalanceQuery(DateOnly Date) : IRequest<DailyBalanceDto?>;
