namespace CashFlow.Transactions.Application.Abstractions;

public interface IRawSqlSpecification
{
    string Sql { get; }
    object[] Parameters { get; }
}

