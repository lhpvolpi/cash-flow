namespace CashFlow.Shared.Application.Common;

/// <summary>
/// Custom JSON converter for enums that handles both camelCase and PascalCase
/// </summary>
public sealed class JsonEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(JsonEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>
/// Generic enum converter that handles camelCase and PascalCase
/// </summary>
public sealed class JsonEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
            {
                throw new JsonException($"Empty string is not a valid value for enum {typeof(T).Name}");
            }

            // Try exact match first
            if (Enum.TryParse<T>(stringValue, ignoreCase: true, out var result))
            {
                return result;
            }

            // Try converting camelCase to PascalCase
            if (char.IsLower(stringValue[0]))
            {
                var pascalCase = char.ToUpperInvariant(stringValue[0]) + stringValue.Substring(1);
                if (Enum.TryParse<T>(pascalCase, ignoreCase: true, out result))
                {
                    return result;
                }
            }

            throw new JsonException($"Unable to convert value '{stringValue}' to enum {typeof(T).Name}");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var intValue))
            {
                if (Enum.IsDefined(typeof(T), intValue))
                {
                    return (T)Enum.ToObject(typeof(T), intValue);
                }
            }

            throw new JsonException($"Unable to convert numeric value to enum {typeof(T).Name}");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum {typeof(T).Name}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
