namespace CashFlow.Transactions.Application.Transactions.ListTransactions.Queries;

public class ListTransactionsQueryValidator : AbstractValidator<ListTransactionsQuery>
{
    public ListTransactionsQueryValidator()
    {
        RuleFor(i => i.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(i => i.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must not exceed 100.");

        When(i => i.StartDate.HasValue, () =>
        {
            RuleFor(i => i.StartDate)
                .NotNull()
                .WithMessage("Start date is required.");
        });

        When(i => i.EndDate.HasValue, () =>
        {
            RuleFor(i => i.EndDate)
                .NotNull()
                .WithMessage("End date is required.");
        });
    }
}

