using CashFlow.Shared.Domain.Enums;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Domain.Tests;

public class TransactionTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        // Arrange & Act
        var transaction = new Transaction(150.00m, ETransactionType.Credit, "Venda de produto");

        // Assert
        Assert.Equal(150.00m, transaction.Amount);
        Assert.Equal(ETransactionType.Credit, transaction.Type);
        Assert.Equal("Venda de produto", transaction.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-150.50)]
    public void Constructor_WithNonPositiveAmount_Throws(decimal amount)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Transaction(amount, ETransactionType.Credit, null));
    }

    [Fact]
    public void Constructor_WithMoreThanTwoDecimalPlaces_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new Transaction(10.123m, ETransactionType.Credit, null));
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Transaction(10m, (ETransactionType)99, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithBlankDescription_StoresNull(string? description)
    {
        // Arrange & Act
        var transaction = new Transaction(10m, ETransactionType.Debit, description);

        // Assert
        Assert.Null(transaction.Description);
    }

    [Fact]
    public void Constructor_TrimsDescription()
    {
        // Arrange & Act
        var transaction = new Transaction(10m, ETransactionType.Debit, "  Pagamento de fornecedor  ");

        // Assert
        Assert.Equal("Pagamento de fornecedor", transaction.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionLongerThan200Characters_Throws()
    {
        // Arrange
        var description = new string('a', 201);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new Transaction(10m, ETransactionType.Debit, description));
    }

    [Fact]
    public void Constructor_WithDescriptionOfExactly200Characters_DoesNotThrow()
    {
        // Arrange
        var description = new string('a', 200);

        // Act
        var transaction = new Transaction(10m, ETransactionType.Debit, description);

        // Assert
        Assert.Equal(description, transaction.Description);
    }
}
