using CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;

namespace CashFlow.Consolidation.Web.Endpoints;

public static class DailyBalanceEndpoints
{
    public static void MapDailyBalanceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/daily-balances")
            .WithName("Daily Balances")
            .RequireAuthorization();

        group.MapGet("{date:datetime}", GetDailyBalance)
            .WithName("Get Daily Balance")
            .WithDescription("Get daily balance for a specific date");
    }

    private static async Task<IResult> GetDailyBalance(
        DateTime date,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetDailyBalanceQuery(DateOnly.FromDateTime(date));
        var result = await sender.Send(query, cancellationToken);

        if (result is null)
        {
            return Results.NotFound(new { message = $"No daily balance found for date {date:yyyy-MM-dd}" });
        }

        return Results.Ok(result);
    }
}
