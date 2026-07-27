using CashFlow.Shared.Domain.Entities;
using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Consolidation.Domain.Entities;

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

    public void Apply(ETransactionType type, decimal amount)
    {
        switch (type)
        {
            case ETransactionType.Credit:
                TotalCredits += amount;
                break;

            case ETransactionType.Debit:
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

