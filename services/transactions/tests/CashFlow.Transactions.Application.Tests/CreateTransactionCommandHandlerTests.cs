using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Domain.Enums;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Application.Tests;

public class CreateTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IOutboxMessageRepository _outboxMessageRepository = Substitute.For<IOutboxMessageRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateTransactionCommandHandler CreateHandler() =>
        new(_transactionRepository, _outboxMessageRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsTransactionAndOutboxMessage_WithinATransaction()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateTransactionCommand(150.00m, ETransactionType.Credit, "Venda de produto");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(150.00m, result.Amount);
        Assert.Equal(ETransactionType.Credit, result.Type);
        Assert.Equal("Venda de produto", result.Description);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _transactionRepository.Received(1).AddAsync(
            Arg.Is<Transaction>(t => t.Amount == 150.00m && t.Type == ETransactionType.Credit),
            Arg.Any<CancellationToken>());
        await _outboxMessageRepository.Received(1).AddAsync(
            Arg.Is<OutboxMessage>(m => m.QueueName == "daily-balance-updates"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPersistenceFails_RollsBackAndRethrows()
    {
        // Arrange
        _transactionRepository
            .AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("database unavailable"));

        var handler = CreateHandler();
        var command = new CreateTransactionCommand(100.00m, ETransactionType.Debit, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        await _unitOfWork.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
