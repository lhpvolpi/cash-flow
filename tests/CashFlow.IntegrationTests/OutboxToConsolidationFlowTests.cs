using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Consolidation.Application.Common.Dtos;
using CashFlow.Consolidation.Application.DailyBalances.GetDailyBalance.Queries;
using CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;
using CashFlow.Shared.Application.Common;
using CashFlow.Shared.Application.Common.Events;
using CashFlow.Shared.Application.Models;
using CashFlow.Shared.Domain.Enums;
using CashFlow.Transactions.Application.OutboxMessages.PublishMessage.Commands;
using CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;
using TransactionsDbContext = CashFlow.Infrastructure.Data.ApplicationDbContext;

namespace CashFlow.IntegrationTests;

public sealed class OutboxToConsolidationFlowTests : IClassFixture<IntegrationTestFixture>
{
    private const string QueueName = "daily-balance-updates";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonEnumConverterFactory() }
    };

    private readonly IntegrationTestFixture _fixture;

    public OutboxToConsolidationFlowTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FullFlow_CreateTransaction_PublishesOutboxAndConsolidatesBalance()
    {
        // Arrange
        var occurredDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);

        // Act
        await using (var scope = _fixture.TransactionsServices.CreateAsyncScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new CreateTransactionCommand(150.00m, ETransactionType.Credit, "Venda de produto"));
        }

        await using (var scope = _fixture.TransactionsServices.CreateAsyncScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new PublishOutboxMessagesCommand(100));
        }

        await AssertOutboxIsEmptyAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await using var consumerScope = _fixture.ConsolidationServices.CreateAsyncScope();
        var consumer = consumerScope.ServiceProvider.GetRequiredService<IConsolidationMessageBrokerConsumer>();

        var consumeTask = consumer.StartConsumingAsync(QueueName, DispatchToConsolidationAsync, cts.Token);

        var balance = await WaitForDailyBalanceAsync(occurredDate, TimeSpan.FromSeconds(15));

        cts.Cancel();
        await AwaitCancellationAsync(consumeTask);

        // Assert
        Assert.NotNull(balance);
        Assert.Equal(150.00m, balance!.TotalCredits);
        Assert.Equal(0m, balance.TotalDebits);
        Assert.Equal(150.00m, balance.Balance);
    }

    [Fact]
    public async Task Consumer_WhenMessageRepeatedlyFailsToProcess_MovesItToTheDeadLetterQueue()
    {
        // Arrange
        var queueName = $"dlq-test-{Guid.NewGuid():N}";
        var failedQueueName = $"{queueName}.failed";

        await using var consolidationServices = _fixture.BuildFastRetryConsolidationServices(maxRetries: 2, retryDelayMilliseconds: 250);
        await using var scope = consolidationServices.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsolidationMessageBrokerConsumer>();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        var consumeTask = consumer.StartConsumingAsync(
            queueName,
            (_, _) => throw new InvalidOperationException("Simulated permanent processing failure."),
            cts.Token);

        await Task.Delay(500);

        // Act
        PublishRawMessage(queueName, """{"id":"11111111-1111-1111-1111-111111111111","payload":{},"createdAtUtc":"2026-07-27T00:00:00+00:00"}""");

        var failedMessage = await WaitForMessageAsync(failedQueueName, TimeSpan.FromSeconds(15));

        cts.Cancel();
        await AwaitCancellationAsync(consumeTask);

        // Assert
        Assert.NotNull(failedMessage);
    }

    private async Task DispatchToConsolidationAsync(BrokerMessage message, CancellationToken cancellationToken)
    {
        var operationEvent = message.Payload.RootElement.Deserialize<OperationEventPayload>(JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize operation event.");

        await using var scope = _fixture.ConsolidationServices.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(
            new ProcessTransactionEventCommand(
                operationEvent.TransactionId,
                operationEvent.Amount,
                operationEvent.TransactionType,
                operationEvent.OccurredAtUtc),
            cancellationToken);
    }

    private static async Task AwaitCancellationAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task AssertOutboxIsEmptyAsync()
    {
        await using var scope = _fixture.TransactionsServices.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        var pendingCount = await dbContext.OutboxMessages.CountAsync();
        Assert.Equal(0, pendingCount);
    }

    private async Task<DailyBalanceDto?> WaitForDailyBalanceAsync(DateOnly date, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            await using (var scope = _fixture.ConsolidationServices.CreateAsyncScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(new GetDailyBalanceQuery(date));

                if (result is not null)
                {
                    return result;
                }
            }

            await Task.Delay(200);
        }

        return null;
    }

    private void PublishRawMessage(string queueName, string jsonBody)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_fixture.RabbitMqConnectionString) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: Encoding.UTF8.GetBytes(jsonBody));
    }

    private async Task<string?> WaitForMessageAsync(string queueName, TimeSpan timeout)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_fixture.RabbitMqConnectionString) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            var result = channel.BasicGet(queueName, autoAck: true);

            if (result is not null)
            {
                return Encoding.UTF8.GetString(result.Body.Span);
            }

            await Task.Delay(200);
        }

        return null;
    }
}
