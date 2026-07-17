using CashFlow.Domain.Enums;

namespace CashFlow.Domain.Entities;

public sealed class DailyBalance : BaseEntity
{
    public DateOnly Date { get; private set; }

    public decimal TotalCredits { get; private set; }

    public decimal TotalDebits { get; private set; }

    public decimal Balance => TotalCredits - TotalDebits;

    private DailyBalance() { }

    public DailyBalance(DateOnly date)
    {
        Date = date;
    }

    public void Apply(EEntryType type, decimal amount)
    {
        switch (type)
        {
            case EEntryType.Credit:
                TotalCredits += amount;
                break;

            case EEntryType.Debit:
                TotalDebits += amount;
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Invalid entry type.");

        }
    }
}

