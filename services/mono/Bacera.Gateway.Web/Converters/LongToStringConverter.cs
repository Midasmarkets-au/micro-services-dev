using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Converters;

/// <summary>
/// Serializes long values exceeding JS Number.MAX_SAFE_INTEGER (2^53-1) as JSON strings
/// to prevent precision loss in JavaScript clients when handling Snowflake IDs.
/// </summary>
public class LongToStringConverter : JsonConverter<long>
{
    private const long MaxSafeInteger = 9007199254740991L; // 2^53 - 1

    public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
            return long.Parse((string)reader.Value!);
        return Convert.ToInt64(reader.Value);
    }

    public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
    {
        if (value > MaxSafeInteger || value < -MaxSafeInteger)
            writer.WriteValue(value.ToString());
        else
            writer.WriteValue(value);
    }
}
