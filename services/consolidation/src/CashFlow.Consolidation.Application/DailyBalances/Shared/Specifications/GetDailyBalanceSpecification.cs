namespace CashFlow.Consolidation.Application.DailyBalances.Shared.Specifications;

public sealed class GetDailyBalanceSpecification : Specification<DailyBalance>
{
    public GetDailyBalanceSpecification(DateOnly date)
    {
        Query
            .Where(x => x.Date == date)
            .AsNoTracking();
    }
}
