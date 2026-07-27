namespace CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Specifications;

public sealed class ProcessedTransactionByTransactionIdSpecification : Specification<ProcessedTransaction>
{
    public ProcessedTransactionByTransactionIdSpecification(Guid transactionId)
    {
        Query
            .Where(x => x.TransactionId == transactionId)
            .AsNoTracking();
    }
}
