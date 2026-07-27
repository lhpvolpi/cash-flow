using CashFlow.Shared.Domain.Enums;
using CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;

namespace CashFlow.Transactions.Application.Tests;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new CreateTransactionCommand(100.00m, ETransactionType.Credit, "Venda");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_HasError(decimal amount)
    {
        // Arrange
        var command = new CreateTransactionCommand(amount, ETransactionType.Credit, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithInvalidType_HasError()
    {
        // Arrange
        var command = new CreateTransactionCommand(100.00m, (ETransactionType)99, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Validate_WithDescriptionLongerThan200Characters_HasError()
    {
        // Arrange
        var command = new CreateTransactionCommand(100.00m, ETransactionType.Debit, new string('a', 201));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
