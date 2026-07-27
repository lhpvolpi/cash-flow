using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Shared.Application.Common.Events;


public sealed record OperationEventPayload(
    Guid TransactionId,
    decimal Amount,
    ETransactionType TransactionType,
    EOperationEventType OperationType,
    DateTimeOffset OccurredAtUtc
);

