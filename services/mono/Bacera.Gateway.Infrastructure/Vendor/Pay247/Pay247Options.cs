using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.Pay247;

/// <summary>
/// Per-row (tenant, currency, MethodType, pay_method) configuration for the Pay247 payment platform.
/// Mirrors <c>RivoOptions</c>: <see cref="Currency"/> + <see cref="PayMethod"/> are per-row and
/// required; one <c>_PaymentMethod</c> row per (currency, pay_method) so ops can enable/disable
/// individual channels (e.g. turn off DANA without touching BANK/QRIS for IDR).
///
/// Bank-code ownership (see also <see cref="Pay247BankCodes"/>):
///   • Runtime authority lives in <see cref="Banks"/> below — per merchant, per tenant, mutable
///     without a deploy. Empty / missing = "fall back to vendor defaults".
///   • Code-side <see cref="Pay247BankCodes"/> is the <b>seed snapshot</b> of the vendor's
///     country bank tables (ID / MY / VN / PH). Used to bootstrap new rows and as a fallback
///     when <see cref="Banks"/> for the currency hasn't been populated yet. Never the runtime
///     source of truth.
///
/// Doc: https://docs.pay247.io/api-reference/en/common.md
/// </summary>
public class Pay247Options
{
    [JsonProperty("mchId")]
    public string MchId { get; set; } = string.Empty;

    [JsonProperty("secretKey")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Prod-only today: <c>https://gateway.pay247.io</c>. No published sandbox.</summary>
    [JsonProperty("apiBaseUrl")]
    public string ApiBaseUrl { get; set; } = "https://gateway.pay247.io";

    /// <summary>Pay247 docs hard-code <c>v3.0</c> as the only version. We keep it configurable in case they bump.</summary>
    [JsonProperty("version")]
    public string Version { get; set; } = "v3.0";

    /// <summary>Currency code for this row (e.g. <c>VND</c>, <c>IDR</c>, <c>MYR</c>, <c>PHP</c>). REQUIRED.</summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Pay247 <c>pay_method</c> for this row:
    /// IDR-deposit: <c>BANK</c> / <c>QRIS</c> / <c>DANA</c>; IDR-withdrawal: <c>BANK</c>.
    /// MYR-deposit: <c>BANK</c> / <c>EWALLET</c>; MYR-withdrawal: <c>BANK</c>.
    /// VND-deposit: <c>BANK</c> / <c>VIETQR</c>; VND-withdrawal: <c>BANK</c>.
    /// PHP-deposit: <c>GCASH</c> / <c>MAYA</c>; PHP-withdrawal: <c>BANK</c> / <c>MAYA</c>.
    /// REQUIRED.
    /// </summary>
    [JsonProperty("payMethod")]
    public string PayMethod { get; set; } = string.Empty;

    /// <summary>
    /// Pay-in cashier mode: <c>link</c> (hosted, returns <c>pay_url</c> to redirect) or
    /// <c>custom</c> (returns <c>pay_params</c> with bank/QR details for us to render).
    /// Phase-1 default is <c>link</c> for all currencies — we just redirect.
    /// </summary>
    [JsonProperty("payTheme")]
    public string PayTheme { get; set; } = "link";

    /// <summary>Optional default order subject; some merchant accounts prefer a stable title for reconciliation.</summary>
    [JsonProperty("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Per-currency bank whitelist + display-name table for <c>pay_method=BANK</c> flows.
    /// Authoritative at runtime; the back-office bank dropdown is populated from this.
    /// Key = ISO currency code (<c>IDR</c>, <c>MYR</c>, <c>VND</c>, <c>PHP</c>).
    ///
    /// Two on-disk shapes are accepted (see <see cref="Pay247BanksConverter"/>):
    /// 1. Canonical: <c>"VND": [{ "Code": "201", "Name": "VietcomBank" }, ...]</c>
    /// 2. Legacy code-only: <c>"VND": ["201", "203", "205"]</c> — converter upgrades each code to
    ///    a <see cref="Pay247BankEntry"/> with <c>Name = Code</c>.
    ///
    /// Empty / missing currency entry means "no per-row override" — <see cref="IsBankSupported"/>
    /// falls back to <see cref="Pay247BankCodes"/> (the vendor seed snapshot).
    /// Non-BANK rows (QRIS, DANA, GCASH, MAYA, EWALLET, VIETQR) can leave this empty —
    /// the <c>bank_code</c> on the wire equals the <see cref="PayMethod"/> itself for those.
    /// </summary>
    [JsonProperty("banks")]
    [JsonConverter(typeof(Pay247BanksConverter))]
    public Dictionary<string, Pay247BankEntry[]> Banks { get; set; } = new();

    // ---- Callback wiring (mirrors RivoOptions) -----------------------------------------------

    /// <summary>Public base URL of this gateway (e.g. <c>https://gateway.example.com</c>).</summary>
    [JsonProperty("callbackDomain")]
    public string CallbackDomain { get; set; } = string.Empty;

    [JsonProperty("payCallbackPathTemplate")]
    public string PayCallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/pay247/pay";

    [JsonProperty("payoutCallbackPathTemplate")]
    public string PayoutCallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/pay247/payout";

    /// <summary>Populated by the caller (Deposit/Payout service) before signing — not stored in JSON.</summary>
    [JsonIgnore]
    public long TenantId { get; set; }

    public string PayCallbackUri => Build(PayCallbackPathTemplate);
    public string PayoutCallbackUri => Build(PayoutCallbackPathTemplate);

    private string Build(string template) =>
        $"{CallbackDomain.TrimEnd('/')}{template.Replace("{tenantId}", TenantId.ToString())}";

    public static Pay247Options FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<Pay247Options>(json) ?? new Pay247Options();
        }
        catch
        {
            return new Pay247Options();
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
            return (false, "Currency is required (per-row; e.g. \"VND\", \"IDR\", \"MYR\", \"PHP\").");
        if (string.IsNullOrWhiteSpace(PayMethod))
            return (false, "PayMethod is required (e.g. \"BANK\", \"QRIS\", \"DANA\", \"GCASH\", \"MAYA\", \"EWALLET\", \"VIETQR\").");
        if (string.IsNullOrWhiteSpace(CallbackDomain))
            return (false, "CallbackDomain is required.");
        if (!Uri.TryCreate(CallbackDomain, UriKind.Absolute, out _))
            return (false, "CallbackDomain must be a valid absolute URL.");
        if (TenantId <= 0)
            return (false, "TenantId must be set before signing.");
        if (string.IsNullOrWhiteSpace(Version))
            return (false, "Version is required (Pay247 spec says \"v3.0\").");
        if (PayTheme is not ("link" or "custom"))
            return (false, $"PayTheme must be \"link\" or \"custom\" (was: \"{PayTheme}\").");

        return (true, string.Empty);
    }

    // ---- Bank helpers (DB-first, code-seed fallback; mirrors RivoOptions) -------------------

    /// <summary>
    /// Returns this row's configured banks for <paramref name="currency"/>, or empty if the
    /// row doesn't override that currency. <b>Does not</b> fall back to the seed table — use
    /// <see cref="GetBanksForCurrencyOrSeed"/> when you want fallback behaviour.
    /// </summary>
    public Pay247BankEntry[] GetBanksForCurrency(string currency)
        => Banks.TryGetValue(currency ?? string.Empty, out var banks)
            ? banks
            : Array.Empty<Pay247BankEntry>();

    /// <summary>
    /// Returns this row's configured banks for <paramref name="currency"/>; if the row
    /// hasn't overridden that currency, falls back to <see cref="Pay247BankCodes.GetSeedBanks"/>
    /// (the vendor snapshot). Used by tests and back-office UI when the row's <see cref="Banks"/>
    /// hasn't been populated yet.
    /// </summary>
    public IReadOnlyList<Pay247BankEntry> GetBanksForCurrencyOrSeed(string currency)
    {
        var local = GetBanksForCurrency(currency);
        return local.Length > 0 ? local : Pay247BankCodes.GetSeedBanks(currency);
    }

    /// <summary>
    /// Soft membership check. Looks at <see cref="Banks"/> first (per-row override); if that
    /// currency has no entries on this row, falls back to <see cref="Pay247BankCodes.IsKnown"/>.
    /// Non-BANK rows (e.g. <see cref="PayMethod"/> = <c>GCASH</c>) treat the method name itself
    /// as the bank_code and short-circuit to <c>true</c>.
    /// </summary>
    public bool IsBankSupported(string currency, string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode)) return false;

        // For non-BANK methods Pay247 expects bank_code == method name (GCASH, MAYA, QRIS, ...).
        if (!string.Equals(PayMethod, "BANK", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(bankCode, PayMethod, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var local = GetBanksForCurrency(currency);
        if (local.Length > 0)
            return local.Any(b => string.Equals(b.Code, bankCode, StringComparison.OrdinalIgnoreCase));

        return Pay247BankCodes.IsKnown(currency, bankCode);
    }
}

/// <summary>
/// One entry in the per-currency Pay247 bank whitelist. Display "Code - Name" on the back-office
/// payout dropdown; the submitted value on the wire is always <see cref="Code"/>.
/// </summary>
public sealed class Pay247BankEntry
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Accepts both the canonical object-array shape
/// <c>"VND": [{ "Code": "201", "Name": "VietcomBank" }, ...]</c>
/// and a legacy code-only shape <c>"VND": ["201", "203"]</c>. Legacy entries become
/// <see cref="Pay247BankEntry"/> with <c>Name = Code</c>.
/// </summary>
internal sealed class Pay247BanksConverter : JsonConverter<Dictionary<string, Pay247BankEntry[]>>
{
    public override Dictionary<string, Pay247BankEntry[]> ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<string, Pay247BankEntry[]>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var result = new Dictionary<string, Pay247BankEntry[]>();
        if (reader.TokenType == JsonToken.Null) return result;
        if (reader.TokenType != JsonToken.StartObject) return result;

        var root = JObject.Load(reader);
        foreach (var (currency, value) in root)
        {
            if (value is not JArray arr) continue;

            var entries = new List<Pay247BankEntry>(arr.Count);
            foreach (var token in arr)
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                    {
                        var code = token.Value<string>() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(code))
                            entries.Add(new Pay247BankEntry { Code = code, Name = code });
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
                            entries.Add(new Pay247BankEntry { Code = code, Name = name });
                        break;
                    }
                }
            }

            result[currency] = entries.ToArray();
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, Pay247BankEntry[]>? value, JsonSerializer serializer)
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
