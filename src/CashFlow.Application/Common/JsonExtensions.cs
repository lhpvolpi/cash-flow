namespace CashFlow.Application.Common;

public static class JsonExtensions
{
    public static JsonDocument ToJsonDocument<T>(this T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonDocument.Parse(json);
    }
}

