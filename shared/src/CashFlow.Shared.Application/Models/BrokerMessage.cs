namespace CashFlow.Shared.Application.Models;

public record BrokerMessage(Guid Id, JsonDocument Payload, DateTimeOffset CreatedAtUtc);