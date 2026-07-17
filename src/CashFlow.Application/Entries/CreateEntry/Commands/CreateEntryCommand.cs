using CashFlow.Domain.Enums;

namespace CashFlow.Application.Entries.CreateEntry.Commands;

public record CreateEntryCommand(
    decimal Amount,
    EEntryType Type,
    string? Description) : IRequest<CreateEntryCommandResult>;