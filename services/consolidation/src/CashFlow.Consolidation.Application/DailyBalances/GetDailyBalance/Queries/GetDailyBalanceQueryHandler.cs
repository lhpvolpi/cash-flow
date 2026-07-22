using CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;

namespace CashFlow.Consolidation.Application.DailyBalances.Queries.GetDailyBalance;

public class GetDailyBalanceQueryHandler : IRequestHandler<GetDailyBalanceQuery, DailyBalanceDto?>
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;

    public GetDailyBalanceQueryHandler(IDailyBalanceRepository dailyBalanceRepository)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
    }

    public async Task<DailyBalanceDto?> Handle(GetDailyBalanceQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetDailyBalanceSpecification(request.Date);
        var dailyBalance = await _dailyBalanceRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (dailyBalance is null)
            return null;

        return new DailyBalanceDto(
            dailyBalance.Id,
            dailyBalance.Date,
            dailyBalance.TotalCredits,
            dailyBalance.TotalDebits,
            dailyBalance.Balance);
    }
}
