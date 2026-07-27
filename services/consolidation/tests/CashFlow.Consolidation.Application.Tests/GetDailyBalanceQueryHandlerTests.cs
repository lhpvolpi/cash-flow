using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;
using CashFlow.Consolidation.Application.DailyBalances.Queries.GetDailyBalance;
using CashFlow.Consolidation.Application.DailyBalances.Shared.Specifications;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Shared.Domain.Enums;

namespace CashFlow.Consolidation.Application.Tests;

public class GetDailyBalanceQueryHandlerTests
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository = Substitute.For<IDailyBalanceRepository>();

    [Fact]
    public async Task Handle_WhenNoBalanceExistsForTheDate_ReturnsNull()
    {
        // Arrange
        _dailyBalanceRepository
            .FirstOrDefaultAsync(Arg.Any<GetDailyBalanceSpecification>(), Arg.Any<CancellationToken>())
            .Returns((DailyBalance?)null);

        var handler = new GetDailyBalanceQueryHandler(_dailyBalanceRepository);

        // Act
        var result = await handler.Handle(new GetDailyBalanceQuery(new DateOnly(2026, 7, 27)), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenBalanceExists_ReturnsTheMappedDto()
    {
        // Arrange
        var dailyBalance = new DailyBalance(new DateOnly(2026, 7, 27));
        dailyBalance.Apply(ETransactionType.Credit, 200.00m);
        dailyBalance.Apply(ETransactionType.Debit, 50.00m);

        _dailyBalanceRepository
            .FirstOrDefaultAsync(Arg.Any<GetDailyBalanceSpecification>(), Arg.Any<CancellationToken>())
            .Returns(dailyBalance);

        var handler = new GetDailyBalanceQueryHandler(_dailyBalanceRepository);

        // Act
        var result = await handler.Handle(new GetDailyBalanceQuery(new DateOnly(2026, 7, 27)), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dailyBalance.Id, result!.Id);
        Assert.Equal(200.00m, result.TotalCredits);
        Assert.Equal(50.00m, result.TotalDebits);
        Assert.Equal(150.00m, result.Balance);
    }
}
