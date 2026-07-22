namespace CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;

public sealed class GetDailyBalanceQueryValidator : AbstractValidator<GetDailyBalanceQuery>
{
    public GetDailyBalanceQueryValidator()
    {
        RuleFor(q => q.Date)
            .NotEmpty()
            .WithMessage("Date is required");
    }
}
