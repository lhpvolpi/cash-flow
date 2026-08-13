using CashFlow.Auth.Application.AuthTokens.Login.Commands;

namespace CashFlow.Auth.Application.Tests;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new LoginCommand("admin", "admin123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUsername_HasError()
    {
        // Arrange
        var command = new LoginCommand("", "admin123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required");
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasError()
    {
        // Arrange
        var command = new LoginCommand("admin", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }
}
