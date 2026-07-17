using CashFlow.Domain.Enums;

namespace CashFlow.Application.Entries.CreateEntry.Commands;

public sealed record CreateEntryCommandResult(
    Guid Id,
    decimal Amount,
    EEntryType Type,
    string? Description,
    DateTimeOffset CreatedAtUtc);