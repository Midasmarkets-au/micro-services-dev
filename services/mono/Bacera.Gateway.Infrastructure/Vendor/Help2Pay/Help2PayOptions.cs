using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.Help2Pay.Models;

public class Help2PayOptions
{
    public string MerchantCode { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string EndPoint { get; set; } = null!;
    public string CallbackDomain { get; set; } = null!;
    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{10000}/help2pay";

    /// <summary>
    /// Help2Pay channel this PaymentMethod row represents. One of:
    /// "OB" | "QR" | "EWalletQR" | "EWalletNative" | "LBT" | "VA" | "Crypto".
    /// Defaults to "OB" for backward compatibility with rows that predate the multi-row split.
    /// </summary>
    // public string MethodType { get; set; } = "OB";

    /// <summary>
    /// Per-currency whitelist of Help2Pay banks exposed by this row, each entry carrying its
    /// own display name (so we don't keep a hardcoded code→name table in C#). Key = ISO currency
    /// code (MYR / THB / IDR / PHP / ...). Empty / missing currency => no banks supported on
    /// this row for that currency.
    ///
    /// Backward compatibility: the JSON value may also be a plain <c>string[]</c> of bank codes
    /// (the legacy shape). <see cref="Help2PayBanksConverter"/> upgrades each code to a
    /// <see cref="BankEntry"/> with <c>Name = Code</c> on read; the FE then renders code-only
    /// for those rows until the DB is migrated.
    /// </summary>
    [JsonConverter(typeof(Help2PayBanksConverter))]
    public Dictionary<string, BankEntry[]> Banks { get; set; } = new();

    public BankEntry[] GetBanksForCurrency(string currencyCode)
        => Banks.TryGetValue(currencyCode, out var banks) ? banks : Array.Empty<BankEntry>();

    public bool IsBankSupported(string currencyCode, string bankCode)
        => GetBanksForCurrency(currencyCode)
            .Any(b => string.Equals(b.Code, bankCode, StringComparison.OrdinalIgnoreCase));

    public static Help2PayOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<Help2PayOptions>(json);

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantCode)
        && !string.IsNullOrEmpty(SecurityCode)
        && !string.IsNullOrEmpty(EndPoint)
        && !string.IsNullOrEmpty(CallbackDomain);
}

/// <summary>
/// One entry in the per-currency Help2Pay bank whitelist. Display "Code - Name" on the
/// deposit modal; submitted value back to Help2Pay is always <see cref="Code"/>.
/// </summary>
public sealed class BankEntry
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Accepts both the canonical object-array shape
/// <c>"MYR": [{ "Code": "MBB", "Name": "Maybank Berhad" }, ...]</c>
/// and the legacy code-only shape <c>"MYR": ["MBB", "PBB"]</c>. Legacy entries become
/// <see cref="BankEntry"/> with <c>Name = Code</c>; on the wire the FE then renders just the
/// code (no "MBB - MBB" duplication) until the DB row is migrated to include real names.
/// </summary>
internal sealed class Help2PayBanksConverter : JsonConverter<Dictionary<string, BankEntry[]>>
{
    public override Dictionary<string, BankEntry[]> ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<string, BankEntry[]>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var result = new Dictionary<string, BankEntry[]>();
        if (reader.TokenType == JsonToken.Null) return result;
        if (reader.TokenType != JsonToken.StartObject) return result;

        var root = JObject.Load(reader);
        foreach (var (currency, value) in root)
        {
            if (value is not JArray arr) continue;

            var entries = new List<BankEntry>(arr.Count);
            foreach (var token in arr)
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                    {
                        var code = token.Value<string>() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(code))
                            entries.Add(new BankEntry { Code = code, Name = code });
                        break;
                    }
                    case JTokenType.Object:
                    {
                        var obj = (JObject)token;
                        var code = obj.GetValue("Code", StringComparison.OrdinalIgnoreCase)?.Value<string>()
                                   ?? string.Empty;
                        var name = obj.GetValue("Name", StringComparison.OrdinalIgnoreCase)?.Value<string>()
                                   ?? code;
                        if (!string.IsNullOrWhiteSpace(code))
                            entries.Add(new BankEntry { Code = code, Name = name });
                        break;
                    }
                }
            }

            result[currency] = entries.ToArray();
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, BankEntry[]>? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        if (value != null)
        {
            foreach (var (currency, entries) in value)
            {
                writer.WritePropertyName(currency);
                serializer.Serialize(writer, entries);
            }
        }
        writer.WriteEndObject();
    }
}
