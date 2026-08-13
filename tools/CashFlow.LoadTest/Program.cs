using CashFlow.Shared.Application.Common;
using CashFlow.Shared.Application.Common.Events;
using CashFlow.Shared.Application.Models;
using CashFlow.Shared.Domain.Enums;

const string QueueName = "daily-balance-updates";
const double MaxAcceptableLossPercentage = 5.0;

var ratePerSecond = args.Length > 0 ? int.Parse(args[0]) : 50;
var durationSeconds = args.Length > 1 ? int.Parse(args[1]) : 60;
var gracePeriod = TimeSpan.FromSeconds(35);

var rabbitMqConnectionString = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING")
    ?? "amqp://guest:guest@localhost:5672/";
var consolidationConnectionString = Environment.GetEnvironmentVariable("CONSOLIDATION_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=consolidation;Username=postgres;Password=postgres";

var totalMessages = ratePerSecond * durationSeconds;
var interval = TimeSpan.FromSeconds(1.0 / ratePerSecond);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    Converters = { new JsonEnumConverterFactory() }
};

Console.WriteLine(
    $"Publicando {totalMessages} mensagens em ~{durationSeconds}s (~{ratePerSecond}/s) na fila '{QueueName}'...");

var factory = new ConnectionFactory { Uri = new Uri(rabbitMqConnectionString) };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.ConfirmSelect();

channel.QueueDeclare(
    queue: QueueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: new Dictionary<string, object>
    {
        ["x-dead-letter-exchange"] = string.Empty,
        ["x-dead-letter-routing-key"] = $"{QueueName}.retry"
    });

var sentTransactionIds = new List<Guid>(totalMessages);
var random = new Random();

using (var timer = new PeriodicTimer(interval))
{
    for (var i = 0; i < totalMessages && await timer.WaitForNextTickAsync(); i++)
    {
        var transactionId = Guid.NewGuid();
        var payload = new OperationEventPayload(
            transactionId,
            Math.Round((decimal)(random.NextDouble() * 1000) + 1, 2),
            i % 2 == 0 ? ETransactionType.Credit : ETransactionType.Debit,
            EOperationEventType.TransactionCreated,
            DateTimeOffset.UtcNow);

        var brokerMessage = new BrokerMessage(Guid.NewGuid(), payload.ToJsonDocument(), DateTimeOffset.UtcNow);
        var body = JsonSerializer.SerializeToUtf8Bytes(brokerMessage, jsonOptions);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.ContentEncoding = "utf-8";
        properties.MessageId = brokerMessage.Id.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: true,
            basicProperties: properties,
            body: body);

        sentTransactionIds.Add(transactionId);

        if ((i + 1) % ratePerSecond == 0)
        {
            Console.WriteLine($"{i + 1}/{totalMessages} publicadas...");
        }
    }
}

channel.WaitForConfirmsOrDie();
Console.WriteLine($"Publicação concluída: {sentTransactionIds.Count} mensagens enviadas.");
Console.WriteLine($"Aguardando {gracePeriod.TotalSeconds}s para o consumer processar (incluindo retries/DLQ)...");
await Task.Delay(gracePeriod);

await using var dbConnection = new NpgsqlConnection(consolidationConnectionString);
await dbConnection.OpenAsync();

await using var command = new NpgsqlCommand(
    "SELECT COUNT(*) FROM processed_transactions WHERE transaction_id = ANY(@ids)",
    dbConnection);
command.Parameters.AddWithValue("ids", sentTransactionIds.ToArray());

var processedCount = (long)(await command.ExecuteScalarAsync())!;
var lost = sentTransactionIds.Count - processedCount;
var lossPercentage = (double)lost / sentTransactionIds.Count * 100;

Console.WriteLine();
Console.WriteLine("===== Resultado do teste de carga =====");
Console.WriteLine($"Mensagens enviadas:      {sentTransactionIds.Count}");
Console.WriteLine($"Mensagens consolidadas:  {processedCount}");
Console.WriteLine($"Mensagens perdidas:      {lost}");
Console.WriteLine($"Taxa de perda:           {lossPercentage:F2}%");
Console.WriteLine(lossPercentage <= MaxAcceptableLossPercentage
    ? $"RESULTADO: dentro do limite de {MaxAcceptableLossPercentage}% de perda exigido pelo NFR."
    : $"RESULTADO: FORA do limite de {MaxAcceptableLossPercentage}% de perda exigido pelo NFR.");
