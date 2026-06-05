using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.Rivo;

/// <summary>
/// Per-row (tenant, currency, MethodType) configuration for the Rivo payment platform.
/// Mirrors <c>ExLinkCashierOptions</c>: <see cref="Currency"/> and <see cref="PaymentMethod"/>
/// are per-currency and required; adding THB / IDR / etc. later is a config-only change.
///
/// Bank-code ownership (see also <see cref="RivoBankCodes"/>):
///   • Runtime authority lives in <see cref="Banks"/> below — per merchant, per tenant, mutable
///     without a deploy. Empty / missing = "fall back to vendor defaults".
///   • Code-side <see cref="RivoBankCodes"/> is the **seed snapshot** of Appendix C v1.1
///     (63 VN entries). Used to bootstrap new rows and as a fallback when <see cref="Banks"/>
///     for the currency hasn't been populated yet. Never the runtime source of truth.
///
/// Doc: https://merchant.rivoglobal.io/rivo-api-docs.html
/// </summary>
public class RivoOptions
{
    [JsonProperty("mchId")]
    public string MchId { get; set; } = string.Empty;

    [JsonProperty("secretKey")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Prod: <c>https://api.rivoglobal.io</c>. Sandbox: <c>https://api.toprivo.com</c>.</summary>
    [JsonProperty("apiBaseUrl")]
    public string ApiBaseUrl { get; set; } = "https://api.rivoglobal.io";

    /// <summary><c>1.0</c> or <c>1.1</c>. <c>1.1</c> requires <c>timestamp</c> + <c>nonce</c> in every signed payload.</summary>
    [JsonProperty("version")]
    public string Version { get; set; } = "1.1";

    /// <summary>Currency code for this row (e.g. <c>VND</c>; later <c>THB</c>, <c>IDR</c>, ...). REQUIRED.</summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>Pay-in method code, per-currency (e.g. <c>01</c> = VietQR for VND). REQUIRED.</summary>
    [JsonProperty("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Pay-out method (e.g. <c>BankTransfer</c>). Same value across currencies per the doc.</summary>
    [JsonProperty("payoutMethod")]
    public string PayoutMethod { get; set; } = "BankTransfer";

    /// <summary>Pay-out account type (e.g. <c>01</c> = Bank Account).</summary>
    [JsonProperty("accType")]
    public string AccType { get; set; } = "01";

    /// <summary>
    /// Optional default subject sent on pay-in <c>order/create</c>.
    /// Some merchant accounts prefer a stable order title for reconciliation.
    /// </summary>
    [JsonProperty("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Per-currency bank whitelist + display-name table for **payout** flows. Authoritative at
    /// runtime; the back-office bank dropdown is populated from this. Key = ISO currency code
    /// (`VND`, `THB`, `IDR`, ...).
    ///
    /// Two on-disk shapes are accepted (see <see cref="RivoBanksConverter"/>):
    /// 1. Canonical: <c>"VND": [{ "Code": "VCB", "Name": "Vietcombank ..." }, ...]</c>
    /// 2. Legacy code-only: <c>"VND": ["VCB", "BIDV", "MB"]</c> — converter upgrades each code to
    ///    a <see cref="RivoBankEntry"/> with <c>Name = Code</c>.
    ///
    /// Empty / missing currency entry means "no per-row override" — <see cref="IsBankSupported"/>
    /// falls back to <see cref="RivoBankCodes"/> (the Appendix C v1.1 seed snapshot).
    /// </summary>
    [JsonProperty("banks")]
    [JsonConverter(typeof(RivoBanksConverter))]
    public Dictionary<string, RivoBankEntry[]> Banks { get; set; } = new();

    // ---- Callback wiring (mirrors ChinaPayOptions.CallbackUri) -------------------------------

    /// <summary>Public base URL of this gateway (e.g. <c>https://gateway.example.com</c>).</summary>
    [JsonProperty("callbackDomain")]
    public string CallbackDomain { get; set; } = string.Empty;

    [JsonProperty("payCallbackPathTemplate")]
    public string PayCallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/rivo/pay";

    [JsonProperty("payoutCallbackPathTemplate")]
    public string PayoutCallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/rivo/payout";

    /// <summary>Populated by the caller (Deposit/Payout service) before signing — not stored in JSON.</summary>
    [JsonIgnore]
    public long TenantId { get; set; }

    public string PayCallbackUri => Build(PayCallbackPathTemplate);
    public string PayoutCallbackUri => Build(PayoutCallbackPathTemplate);

    private string Build(string template) =>
        $"{CallbackDomain.TrimEnd('/')}{template.Replace("{tenantId}", TenantId.ToString())}";

    public static RivoOptions FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<RivoOptions>(json) ?? new RivoOptions();
        }
        catch
        {
            return new RivoOptions();
        }
    }

    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(MchId))
            return (false, "MchId is required.");
        if (string.IsNullOrWhiteSpace(SecretKey))
            return (false, "SecretKey is required.");
        if (string.IsNullOrWhiteSpace(ApiBaseUrl))
            return (false, "ApiBaseUrl is required.");
        if (!Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out _))
            return (false, "ApiBaseUrl must be a valid absolute URL.");
        if (string.IsNullOrWhiteSpace(Currency))
            return (false, "Currency is required (per-row; e.g. \"VND\").");
        if (string.IsNullOrWhiteSpace(PaymentMethod))
            return (false, "PaymentMethod is required (per-currency code; e.g. \"01\" for VND VietQR).");
        if (string.IsNullOrWhiteSpace(CallbackDomain))
            return (false, "CallbackDomain is required.");
        if (!Uri.TryCreate(CallbackDomain, UriKind.Absolute, out _))
            return (false, "CallbackDomain must be a valid absolute URL.");
        if (TenantId <= 0)
            return (false, "TenantId must be set before signing.");
        if (Version is not ("1.0" or "1.1"))
            return (false, $"Version must be \"1.0\" or \"1.1\" (was: \"{Version}\").");

        return (true, string.Empty);
    }

    // ---- Bank helpers (DB-first, code-seed fallback) -----------------------------------------

    /// <summary>
    /// Returns this row's configured banks for <paramref name="currency"/>, or empty if the
    /// row doesn't override that currency. **Does not** fall back to the seed table — use
    /// <see cref="GetBanksForCurrencyOrSeed"/> when you want fallback behaviour.
    /// </summary>
    public RivoBankEntry[] GetBanksForCurrency(string currency)
        => Banks.TryGetValue(currency ?? string.Empty, out var banks)
            ? banks
            : Array.Empty<RivoBankEntry>();

    /// <summary>
    /// Returns this row's configured banks for <paramref name="currency"/>; if the row
    /// hasn't overridden that currency, falls back to <see cref="RivoBankCodes.GetSeedBanks"/>
    /// (the Appendix C v1.1 vendor snapshot). Used by tests and back-office UI when the
    /// row's <see cref="Banks"/> hasn't been populated yet.
    /// </summary>
    public IReadOnlyList<RivoBankEntry> GetBanksForCurrencyOrSeed(string currency)
    {
        var local = GetBanksForCurrency(currency);
        return local.Length > 0 ? local : RivoBankCodes.GetSeedBanks(currency);
    }

    /// <summary>
    /// Soft membership check. Looks at <see cref="Banks"/> first (per-row override); if that
    /// currency has no entries on this row, falls back to <see cref="RivoBankCodes.IsKnown"/>.
    /// Returns <c>false</c> when neither layer recognises the code — caller decides whether
    /// to block (we currently warn-log and continue, since Rivo's master list is non-authoritative).
    /// </summary>
    public bool IsBankSupported(string currency, string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode)) return false;

        var local = GetBanksForCurrency(currency);
        if (local.Length > 0)
            return local.Any(b => string.Equals(b.Code, bankCode, StringComparison.OrdinalIgnoreCase));

        return RivoBankCodes.IsKnown(currency, bankCode);
    }
}

/// <summary>
/// One entry in the per-currency Rivo bank whitelist. Display "Code - Name" on the back-office
/// payout dropdown; the submitted value on the wire is always <see cref="Code"/>.
/// </summary>
public sealed class RivoBankEntry
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Accepts both the canonical object-array shape
/// <c>"VND": [{ "Code": "VCB", "Name": "Vietcombank ..." }, ...]</c>
/// and a legacy code-only shape <c>"VND": ["VCB", "BIDV"]</c> (kept as a forward-compat hook
/// in case a future seed script writes just codes). Legacy entries become
/// <see cref="RivoBankEntry"/> with <c>Name = Code</c>.
/// </summary>
internal sealed class RivoBanksConverter : JsonConverter<Dictionary<string, RivoBankEntry[]>>
{
    public override Dictionary<string, RivoBankEntry[]> ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<string, RivoBankEntry[]>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var result = new Dictionary<string, RivoBankEntry[]>();
        if (reader.TokenType == JsonToken.Null) return result;
        if (reader.TokenType != JsonToken.StartObject) return result;

        var root = JObject.Load(reader);
        foreach (var (currency, value) in root)
        {
            if (value is not JArray arr) continue;

            var entries = new List<RivoBankEntry>(arr.Count);
            foreach (var token in arr)
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                    {
                        var code = token.Value<string>() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(code))
                            entries.Add(new RivoBankEntry { Code = code, Name = code });
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
                            entries.Add(new RivoBankEntry { Code = code, Name = name });
                        break;
                    }
                }
            }

            result[currency] = entries.ToArray();
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, RivoBankEntry[]>? value, JsonSerializer serializer)
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
