namespace CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;

public sealed record ProcessTransactionEventCommand(
    Guid TransactionId,
    decimal Amount,
    ETransactionType TransactionType,
    DateTimeOffset OccurredAtUtc
) : IRequest;
