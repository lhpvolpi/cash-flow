using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Application.OutboxMessages.PublishMessage.Commands;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Application.Tests;

public class PublishOutboxMessagesCommandHandlerTests
{
    private readonly IOutboxMessageRepository _outboxMessageRepository = Substitute.For<IOutboxMessageRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOutboxBrokerPublisher _publisher = Substitute.For<IOutboxBrokerPublisher>();

    private PublishOutboxMessagesCommandHandler CreateHandler() =>
        new(_outboxMessageRepository, _unitOfWork, _publisher);

    private static OutboxMessage CreateOutboxMessage() =>
        new("daily-balance-updates", JsonDocument.Parse("""{"transactionId":"11111111-1111-1111-1111-111111111111"}"""));

    [Fact]
    public async Task Handle_WithNoPendingMessages_CommitsWithoutPublishing()
    {
        // Arrange
        _outboxMessageRepository
            .GetNextBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var handler = CreateHandler();

        // Act
        await handler.Handle(new PublishOutboxMessagesCommand(100), CancellationToken.None);

        // Assert
        await _publisher.DidNotReceiveWithAnyArgs().SendMessageAsync(default!, default!, default);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPublishSucceeds_DeletesTheOutboxMessage()
    {
        // Arrange
        var message = CreateOutboxMessage();

        _outboxMessageRepository
            .GetNextBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([message]);

        var handler = CreateHandler();

        // Act
        await handler.Handle(new PublishOutboxMessagesCommand(100), CancellationToken.None);

        // Assert
        await _publisher.Received(1).SendMessageAsync(
            "daily-balance-updates",
            Arg.Is<BrokerMessage>(m => m.Id == message.Id),
            Arg.Any<CancellationToken>());
        await _outboxMessageRepository.Received(1).DeleteAsync(message, Arg.Any<CancellationToken>());
        await _outboxMessageRepository.DidNotReceive().UpdateAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
        Assert.Null(message.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WhenPublishFails_MarksTheMessageWithTheErrorInsteadOfThrowing()
    {
        // Arrange
        var message = CreateOutboxMessage();

        _outboxMessageRepository
            .GetNextBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([message]);

        _publisher
            .SendMessageAsync(Arg.Any<string>(), Arg.Any<BrokerMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("broker unreachable"));

        var handler = CreateHandler();

        // Act
        await handler.Handle(new PublishOutboxMessagesCommand(100), CancellationToken.None);

        // Assert
        Assert.Equal("broker unreachable", message.ErrorMessage);
        await _outboxMessageRepository.Received(1).UpdateAsync(message, Arg.Any<CancellationToken>());
        await _outboxMessageRepository.DidNotReceive().DeleteAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
