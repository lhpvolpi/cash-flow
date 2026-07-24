namespace CashFlow.Transactions.Application.OutboxMessages.PublishMessage.Commands;

public sealed record PublishOutboxMessagesCommand(int BatchSize) : IRequest;

