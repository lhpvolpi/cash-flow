using CashFlow.Shared.Domain.Entities;

namespace CashFlow.Consolidation.Domain.Entities;

public sealed class ProcessedTransaction : BaseEntity
{
    public Guid TransactionId { get; private set; }

    public Guid DailyBalanceId { get; private set; }

    private ProcessedTransaction() { }

    public ProcessedTransaction(Guid transactionId, Guid dailyBalanceId)
    {
        TransactionId = transactionId;
        DailyBalanceId = dailyBalanceId;
    }
}
