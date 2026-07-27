using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;
using CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Specifications;
using CashFlow.Consolidation.Application.DailyBalances.Shared.Specifications;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Consolidation.Application.Tests;

public class ProcessTransactionEventCommandHandlerTests
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository = Substitute.For<IDailyBalanceRepository>();
    private readonly IProcessedTransactionRepository _processedTransactionRepository = Substitute.For<IProcessedTransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ProcessTransactionEventCommandHandler CreateHandler() =>
        new(_dailyBalanceRepository, _processedTransactionRepository, _unitOfWork);

    private static ProcessTransactionEventCommand CreateCommand(
        Guid? transactionId = null,
        decimal amount = 150.00m,
        ETransactionType type = ETransactionType.Credit) =>
        new(
            transactionId ?? Guid.NewGuid(),
            amount,
            type,
            new DateTimeOffset(2026, 7, 27, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_WhenTransactionAlreadyProcessed_IsANoOp()
    {
        // Arrange
        _processedTransactionRepository
            .AnyAsync(Arg.Any<ProcessedTransactionByTransactionIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = CreateHandler();

        // Act
        await handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _dailyBalanceRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _dailyBalanceRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenNoDailyBalanceExistsForTheDate_CreatesOneAndAppliesTheAmount()
    {
        // Arrange
        _processedTransactionRepository
            .AnyAsync(Arg.Any<ProcessedTransactionByTransactionIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _dailyBalanceRepository
            .FirstOrDefaultAsync(Arg.Any<GetDailyBalanceSpecification>(), Arg.Any<CancellationToken>())
            .Returns((DailyBalance?)null);

        var handler = CreateHandler();
        var command = CreateCommand(amount: 200.00m, type: ETransactionType.Credit);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());

        await _dailyBalanceRepository.Received(1).AddAsync(
            Arg.Is<DailyBalance>(d => d.Date == DateOnly.FromDateTime(command.OccurredAtUtc.DateTime)
                && d.TotalCredits == 200.00m),
            Arg.Any<CancellationToken>());

        await _processedTransactionRepository.Received(1).AddAsync(
            Arg.Is<ProcessedTransaction>(p => p.TransactionId == command.TransactionId),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDailyBalanceAlreadyExistsForTheDate_UpdatesItInsteadOfCreatingANewOne()
    {
        // Arrange
        var existingBalance = new DailyBalance(new DateOnly(2026, 7, 27));
        existingBalance.Apply(ETransactionType.Credit, 100.00m);

        _processedTransactionRepository
            .AnyAsync(Arg.Any<ProcessedTransactionByTransactionIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _dailyBalanceRepository
            .FirstOrDefaultAsync(Arg.Any<GetDailyBalanceSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existingBalance);

        var handler = CreateHandler();
        var command = CreateCommand(amount: 30.00m, type: ETransactionType.Debit);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _dailyBalanceRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _dailyBalanceRepository.Received(1).UpdateAsync(existingBalance, Arg.Any<CancellationToken>());

        Assert.Equal(100.00m, existingBalance.TotalCredits);
        Assert.Equal(30.00m, existingBalance.TotalDebits);
        Assert.Equal(70.00m, existingBalance.Balance);
    }

    [Fact]
    public async Task Handle_WhenCommitFails_RollsBackAndRethrows()
    {
        // Arrange
        _processedTransactionRepository
            .AnyAsync(Arg.Any<ProcessedTransactionByTransactionIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _dailyBalanceRepository
            .FirstOrDefaultAsync(Arg.Any<GetDailyBalanceSpecification>(), Arg.Any<CancellationToken>())
            .Returns((DailyBalance?)null);

        _unitOfWork
            .CommitAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("database unavailable"));

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(CreateCommand(), CancellationToken.None));

        await _unitOfWork.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}
