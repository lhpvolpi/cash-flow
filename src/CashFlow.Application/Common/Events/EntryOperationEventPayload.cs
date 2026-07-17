
namespace CashFlow.Application.Common.Events;

public sealed record EntryOperationEventPayload(
    Guid EntryId,
    string OperationType,
    DateTimeOffset OccurredAtUtc
);

