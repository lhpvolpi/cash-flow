namespace CashFlow.Shared.Application.Common;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static JsonDocument ToJsonDocument<T>(this T obj)
    {
        var json = JsonSerializer.Serialize(obj, DefaultOptions);
        return JsonDocument.Parse(json);
    }

    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }
}

