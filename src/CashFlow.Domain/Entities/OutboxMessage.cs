namespace CashFlow.Domain.Entities;

public sealed class OutboxMessage : BaseEntity
{
    public string QueueName { get; private set; }

    public JsonDocument Payload { get; private set; }

    public string? ErrorMessage { get; private set; } = null;

    public OutboxMessage(string queueName, JsonDocument payload)
    {
        QueueName = queueName;
        Payload = payload;
    }

    public void SetError(string error) => ErrorMessage = error;
}

