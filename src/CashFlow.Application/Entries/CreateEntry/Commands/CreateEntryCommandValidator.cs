namespace CashFlow.Application.Entries.CreateEntry.Commands;

public class CreateEntryCommandValidator : AbstractValidator<CreateEntryCommand>
{
    public CreateEntryCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid entry type.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.");
    }
}