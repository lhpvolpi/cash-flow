using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Domain.Tests;

public class OutboxMessageTests
{
    [Fact]
    public void Constructor_SetsQueueNameAndPayload_AndNoErrorByDefault()
    {
        // Arrange
        var payload = JsonDocument.Parse("""{"transactionId":"11111111-1111-1111-1111-111111111111"}""");

        // Act
        var message = new OutboxMessage("daily-balance-updates", payload);

        // Assert
        Assert.Equal("daily-balance-updates", message.QueueName);
        Assert.Same(payload, message.Payload);
        Assert.Null(message.ErrorMessage);
    }

    [Fact]
    public void SetError_StoresTheErrorMessage()
    {
        // Arrange
        var message = new OutboxMessage("daily-balance-updates", JsonDocument.Parse("{}"));

        // Act
        message.SetError("Connection refused");

        // Assert
        Assert.Equal("Connection refused", message.ErrorMessage);
    }
}
