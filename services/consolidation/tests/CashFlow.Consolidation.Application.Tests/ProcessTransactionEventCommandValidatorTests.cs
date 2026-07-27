using CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;
using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Consolidation.Application.Tests;

public class ProcessTransactionEventCommandValidatorTests
{
    private readonly ProcessTransactionEventCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new ProcessTransactionEventCommand(
            Guid.NewGuid(), 100.00m, ETransactionType.Credit, DateTimeOffset.UtcNow);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTransactionId_HasError()
    {
        // Arrange
        var command = new ProcessTransactionEventCommand(
            Guid.Empty, 100.00m, ETransactionType.Credit, DateTimeOffset.UtcNow);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TransactionId);
    }

    [Fact]
    public void Validate_WithNonPositiveAmount_HasError()
    {
        // Arrange
        var command = new ProcessTransactionEventCommand(
            Guid.NewGuid(), 0m, ETransactionType.Credit, DateTimeOffset.UtcNow);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithInvalidTransactionType_HasError()
    {
        // Arrange
        var command = new ProcessTransactionEventCommand(
            Guid.NewGuid(), 100.00m, (ETransactionType)99, DateTimeOffset.UtcNow);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TransactionType);
    }
}
