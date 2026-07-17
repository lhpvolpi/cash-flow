using CashFlow.Domain.Enums;

namespace CashFlow.Domain.Entities;

public sealed class Entry : BaseEntity
{
    public decimal Amount { get; private set; }

    public EEntryType Type { get; private set; }

    public string? Description { get; private set; }

    public Entry() { }

    public Entry(decimal amount, EEntryType type, string? description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentException(
                "Amount cannot have more than 2 decimal places.",
                nameof(amount));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                "Invalid entry type.");
        }

        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (description?.Length > 200)
        {
            throw new ArgumentException(
               "Description cannot exceed 200 characters.",
               nameof(description));
        }

        Amount = amount;
        Type = type;
        Description = description;
    }
}

