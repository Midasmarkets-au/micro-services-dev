using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Converters;

/// <summary>
/// Serializes decimal values as JSON strings to prevent IEEE 754 float precision loss in JavaScript.
/// </summary>
public class DecimalToStringConverter : JsonConverter<decimal>
{
    public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
            return decimal.Parse((string)reader.Value!);
        return Convert.ToDecimal(reader.Value);
    }

    public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("0.####"));
    }
}
