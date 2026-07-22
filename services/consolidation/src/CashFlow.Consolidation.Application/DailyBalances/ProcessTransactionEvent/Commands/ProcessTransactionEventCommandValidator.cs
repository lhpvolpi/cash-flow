namespace CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;

public sealed class ProcessTransactionEventCommandValidator : AbstractValidator<ProcessTransactionEventCommand>
{
    public ProcessTransactionEventCommandValidator()
    {
        RuleFor(c => c.TransactionId)
            .NotEmpty()
            .WithMessage("TransactionId is required");

        RuleFor(c => c.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(c => c.TransactionType)
            .IsInEnum()
            .WithMessage("TransactionType is invalid");

        RuleFor(c => c.OccurredAtUtc)
            .NotEmpty()
            .WithMessage("OccurredAtUtc is required");
    }
}
