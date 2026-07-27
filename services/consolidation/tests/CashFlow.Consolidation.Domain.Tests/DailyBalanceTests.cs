using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Consolidation.Domain.Tests;

public class DailyBalanceTests
{
    [Fact]
    public void Constructor_SetsDate_AndStartsAtZero()
    {
        // Arrange
        var date = new DateOnly(2026, 7, 27);

        // Act
        var dailyBalance = new DailyBalance(date);

        // Assert
        Assert.Equal(date, dailyBalance.Date);
        Assert.Equal(0m, dailyBalance.TotalCredits);
        Assert.Equal(0m, dailyBalance.TotalDebits);
        Assert.Equal(0m, dailyBalance.Balance);
    }

    [Fact]
    public void Apply_Credit_IncreasesTotalCreditsAndBalance()
    {
        // Arrange
        var dailyBalance = new DailyBalance(new DateOnly(2026, 7, 27));

        // Act
        dailyBalance.Apply(ETransactionType.Credit, 150.00m);

        // Assert
        Assert.Equal(150.00m, dailyBalance.TotalCredits);
        Assert.Equal(0m, dailyBalance.TotalDebits);
        Assert.Equal(150.00m, dailyBalance.Balance);
    }

    [Fact]
    public void Apply_Debit_IncreasesTotalDebits_AndDecreasesBalance()
    {
        // Arrange
        var dailyBalance = new DailyBalance(new DateOnly(2026, 7, 27));

        // Act
        dailyBalance.Apply(ETransactionType.Debit, 40.00m);

        // Assert
        Assert.Equal(0m, dailyBalance.TotalCredits);
        Assert.Equal(40.00m, dailyBalance.TotalDebits);
        Assert.Equal(-40.00m, dailyBalance.Balance);
    }

    [Fact]
    public void Apply_MultipleTimes_Accumulates()
    {
        // Arrange
        var dailyBalance = new DailyBalance(new DateOnly(2026, 7, 27));

        // Act
        dailyBalance.Apply(ETransactionType.Credit, 200.00m);
        dailyBalance.Apply(ETransactionType.Credit, 50.00m);
        dailyBalance.Apply(ETransactionType.Debit, 30.00m);

        // Assert
        Assert.Equal(250.00m, dailyBalance.TotalCredits);
        Assert.Equal(30.00m, dailyBalance.TotalDebits);
        Assert.Equal(220.00m, dailyBalance.Balance);
    }

    [Fact]
    public void Apply_WithInvalidEnumValue_Throws()
    {
        // Arrange
        var dailyBalance = new DailyBalance(new DateOnly(2026, 7, 27));

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => dailyBalance.Apply((ETransactionType)99, 10m));
    }
}
