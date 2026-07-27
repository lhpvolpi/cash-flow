using CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;
using CashFlow.Transactions.Application.Transactions.ListTransactions.Queries;

namespace CashFlow.Transactions.Web.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/transactions")
            .WithName("Transactions");

        group.MapPost("", CreateTransaction)
            .WithName("Create Transaction")
            .WithDescription("Create a new transaction");

        group.MapGet("", ListTransactions)
            .WithName("List Transactions")
            .WithDescription("List transactions with pagination and optional filtering by date");
    }

    private static async Task<IResult> CreateTransaction(
        CreateTransactionCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ListTransactions(
        [AsParameters] ListTransactionsQuery query,
    ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}

