using CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;

namespace CashFlow.Consolidation.Application.Tests;

public class GetDailyBalanceQueryValidatorTests
{
    private readonly GetDailyBalanceQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidDate_HasNoErrors()
    {
        // Arrange
        var query = new GetDailyBalanceQuery(new DateOnly(2026, 7, 27));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDefaultDate_HasError()
    {
        // Arrange
        var query = new GetDailyBalanceQuery(default);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }
}
