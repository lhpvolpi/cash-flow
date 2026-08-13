using FluentValidation;

namespace CashFlow.Auth.Application.AuthTokens.Login.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(i => i.Username)
            .NotEmpty()
                .WithMessage("Username is required");

        RuleFor(i => i.Password)
            .NotEmpty()
                .WithMessage("Password is required");
    }
}

