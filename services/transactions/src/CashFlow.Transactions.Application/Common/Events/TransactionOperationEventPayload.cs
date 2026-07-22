using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Transactions.Application.Common.Events;

public sealed record TransactionOperationEventPayload(
    Guid TransactionId,
    decimal Amount,
    ETransactionType TransactionType,
    ETransactionOperationEventType OperationType,
    DateTimeOffset OccurredAtUtc
);

