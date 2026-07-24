using CashFlow.Transactions.Application.Abstractions;

namespace CashFlow.Transactions.Infrastructure.Data.Repositories.Specifications;

public class ListOutboxMessagesSpecification : IRawSqlSpecification
{
    public int BatchSize { get; private set; }

    public ListOutboxMessagesSpecification(int batchSize)
    {
        BatchSize = batchSize;
    }

    public string Sql => """
        SELECT * FROM outbox_messages
        WHERE error_message IS NULL
        ORDER BY created_at_utc ASC
        FOR UPDATE SKIP LOCKED
        LIMIT {0}
    """;

    public object[] Parameters => new object[] { BatchSize };
}
