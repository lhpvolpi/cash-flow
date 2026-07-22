namespace CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
                .WithMessage("amount must be greater than zero.");

        RuleFor(x => x.Type)
            .IsInEnum()
                .WithMessage("invalid entry type.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
                .WithMessage("description cannot exceed 200 characters.");
    }
}