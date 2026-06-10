using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.TwelveGroup;

/// <summary>
/// Per-row (tenant, currency, MethodType) configuration for the 12Group (thesolix.com) platform.
/// Thailand <b>THB-only</b> gateway: Thai QR / PromptPay deposit (redirect) + bank payout.
///
/// Auth is a static JWT in the <c>authorization</c> header plus <c>partner_code</c> / <c>channel</c>
/// headers — there is NO HMAC request signing and NO callback signature. Because callbacks are
/// unsigned, deposits are confirmed by calling the <c>inquiry-deposit</c> API server-side before
/// crediting (see <see cref="TwelveGroup.InquiryDepositAsync"/>).
///
/// Endpoint segment quirk (driven by <see cref="Env"/>):
///   • Deposit:        test → <c>/test/create-qr-code</c>,   prod → <c>/v1/create-qr-code</c>
///   • Payout/inquiry: test → <c>/test/payout</c>,           prod → <c>/payout</c> (no segment)
///
/// Bank-code ownership mirrors <c>RivoOptions</c>: runtime authority lives in <see cref="Banks"/>;
/// <see cref="TwelveGroupBankCodes"/> is the seed/fallback only.
///
/// Doc: Requirement/12Group/ (deposit-api.md, withdrawal-api.md).
/// </summary>
public class TwelveGroupOptions
{
    /// <summary>3-letter partner code (e.g. <c>MDA</c>). Sent in the partner-code header. REQUIRED.</summary>
    [JsonProperty("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;

    /// <summary>Static JWT sent verbatim in the <c>authorization</c> header. REQUIRED.</summary>
    [JsonProperty("authorizationToken")]
    public string AuthorizationToken { get; set; } = string.Empty;

    /// <summary>Sent in the <c>channel</c> header. Default <c>WEB</c>.</summary>
    [JsonProperty("channel")]
    public string Channel { get; set; } = "WEB";

    /// <summary>Optional <c>device</c> header. Default <c>WEB</c>.</summary>
    [JsonProperty("device")]
    public string Device { get; set; } = "WEB";

    /// <summary>
    /// <c>test</c> or <c>prod</c>. Drives the URL segment for all three base URLs:
    /// deposit (<c>test</c>/<c>v1</c>) and payout/inquiry (<c>test</c>/empty).
    /// </summary>
    [JsonProperty("env")]
    public string Env { get; set; } = "test";

    /// <summary>Deposit host. Test: <c>https://api-test.thesolix.com</c>; prod sent via email.</summary>
    [JsonProperty("depositBaseUrl")]
    public string DepositBaseUrl { get; set; } = "https://api-test.thesolix.com";

    /// <summary>Payout host. Test: <c>https://api-test-payout.thesolix.com</c>; prod sent via email.</summary>
    [JsonProperty("payoutBaseUrl")]
    public string PayoutBaseUrl { get; set; } = "https://api-test-payout.thesolix.com";

    /// <summary>Inquiry host (shared deposit + withdraw). <c>https://inquiry.thesolix.com</c>.</summary>
    [JsonProperty("inquiryBaseUrl")]
    public string InquiryBaseUrl { get; set; } = "https://inquiry.thesolix.com";

    /// <summary>Currency for this row. 12Group is THB-only today. REQUIRED.</summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = "THB";

    /// <summary>
    /// Fallback <c>mobileno</c> for payouts when the payout record / party carries none.
    /// 12Group requires a non-empty <c>mobileno</c> on the payout request.
    /// </summary>
    [JsonProperty("defaultMobileNo")]
    public string DefaultMobileNo { get; set; } = "0000000000";

    /// <summary>
    /// Per-currency bank whitelist + display-name table for payout flows. Authoritative at runtime;
    /// the back-office bank dropdown is populated from this. Same shape/converter as Rivo.
    /// Empty / missing currency → falls back to <see cref="TwelveGroupBankCodes"/>.
    /// </summary>
    [JsonProperty("banks")]
    [JsonConverter(typeof(TwelveGroupBanksConverter))]
    public Dictionary<string, TwelveGroupBankEntry[]> Banks { get; set; } = new();

    // ---- Callback wiring (12Group configures these out-of-band, but we expose them for the seed) ----

    /// <summary>Public base URL of this gateway (e.g. <c>https://gateway.example.com</c>).</summary>
    [JsonProperty("callbackDomain")]
    public string CallbackDomain { get; set; } = string.Empty;

    [JsonProperty("payCallbackPathTemplate")]
    public string PayCallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/twelvegroup/pay";

    [JsonProperty("payoutCallbackPathTemplate")]
    public string PayoutCallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/twelvegroup/payout";

    /// <summary>Populated by the caller (Deposit/Payout service) before use — not stored in JSON.</summary>
    [JsonIgnore]
    public long TenantId { get; set; }

    public string PayCallbackUri => Build(PayCallbackPathTemplate);
    public string PayoutCallbackUri => Build(PayoutCallbackPathTemplate);

    private string Build(string template) =>
        $"{CallbackDomain.TrimEnd('/')}{template.Replace("{tenantId}", TenantId.ToString())}";

    // ---- URL builders -----------------------------------------------------------------------

    private bool IsTest => string.Equals(Env, "test", StringComparison.OrdinalIgnoreCase);

    /// <summary>Deposit URL segment: <c>test</c> in test, <c>v1</c> in prod.</summary>
    private string DepositSeg => IsTest ? "test" : "v1";

    /// <summary>Payout/inquiry URL segment: <c>test</c> in test, empty in prod.</summary>
    private string PayoutInquirySeg => IsTest ? "test" : string.Empty;

    public string CreateQrUrl => Combine(DepositBaseUrl, DepositSeg, "create-qr-code");
    public string ViewQrUrl(string transId) => Combine(DepositBaseUrl, DepositSeg, "view-qr-code", transId);
    public string PayoutUrl => Combine(PayoutBaseUrl, PayoutInquirySeg, "payout");
    public string InquiryDepositUrl(string transId) => Combine(InquiryBaseUrl, PayoutInquirySeg, "inquiry-deposit", transId);
    public string InquiryWithdrawUrl(string ref1) => Combine(InquiryBaseUrl, PayoutInquirySeg, "inquiry-withdraw", ref1);

    /// <summary>Join a base URL and path segments, dropping empty segments (so a blank prod segment collapses cleanly).</summary>
    private static string Combine(string baseUrl, params string[] segments)
    {
        var parts = segments
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim('/'));
        return baseUrl.TrimEnd('/') + "/" + string.Join("/", parts);
    }

    public static TwelveGroupOptions FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<TwelveGroupOptions>(json) ?? new TwelveGroupOptions();
        }
        catch
        {
            return new TwelveGroupOptions();
        }
    }

    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(PartnerCode))
            return (false, "PartnerCode is required (3-letter code, e.g. \"MDA\").");
        if (string.IsNullOrWhiteSpace(AuthorizationToken))
            return (false, "AuthorizationToken (static JWT) is required.");
        if (string.IsNullOrWhiteSpace(DepositBaseUrl) || !Uri.TryCreate(DepositBaseUrl, UriKind.Absolute, out _))
            return (false, "DepositBaseUrl must be a valid absolute URL.");
        if (string.IsNullOrWhiteSpace(PayoutBaseUrl) || !Uri.TryCreate(PayoutBaseUrl, UriKind.Absolute, out _))
            return (false, "PayoutBaseUrl must be a valid absolute URL.");
        if (string.IsNullOrWhiteSpace(InquiryBaseUrl) || !Uri.TryCreate(InquiryBaseUrl, UriKind.Absolute, out _))
            return (false, "InquiryBaseUrl must be a valid absolute URL.");
        if (string.IsNullOrWhiteSpace(Currency))
            return (false, "Currency is required (THB).");
        if (Env is not ("test" or "prod"))
            return (false, $"Env must be \"test\" or \"prod\" (was: \"{Env}\").");

        return (true, string.Empty);
    }

    // ---- Bank helpers (DB-first, code-seed fallback) -----------------------------------------

    /// <summary>Row-configured banks for <paramref name="currency"/>; no seed fallback.</summary>
    public TwelveGroupBankEntry[] GetBanksForCurrency(string currency)
        => Banks.TryGetValue(currency ?? string.Empty, out var banks)
            ? banks
            : Array.Empty<TwelveGroupBankEntry>();

    /// <summary>Row-configured banks for <paramref name="currency"/>, falling back to the THB seed table.</summary>
    public IReadOnlyList<TwelveGroupBankEntry> GetBanksForCurrencyOrSeed(string currency)
    {
        var local = GetBanksForCurrency(currency);
        return local.Length > 0 ? local : TwelveGroupBankCodes.GetSeedBanks(currency);
    }

    /// <summary>
    /// Soft membership check. <see cref="Banks"/> first, then <see cref="TwelveGroupBankCodes.IsKnown"/>.
    /// Returns <c>false</c> when neither layer recognises the code — caller warn-logs and continues
    /// (the vendor's master list is non-authoritative and image-only).
    /// </summary>
    public bool IsBankSupported(string currency, string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode)) return false;

        var local = GetBanksForCurrency(currency);
        if (local.Length > 0)
            return local.Any(b => string.Equals(b.Code, bankCode, StringComparison.OrdinalIgnoreCase));

        return TwelveGroupBankCodes.IsKnown(currency, bankCode);
    }
}

/// <summary>One entry in the per-currency 12Group bank whitelist. Wire value is always <see cref="Code"/>.</summary>
public sealed class TwelveGroupBankEntry
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Accepts both the canonical object-array shape
/// <c>"THB": [{ "Code": "002", "Name": "Bangkok Bank" }, ...]</c>
/// and a legacy code-only shape <c>"THB": ["002", "004"]</c> (entries become <c>Name = Code</c>).
/// Mirrors <c>RivoBanksConverter</c>.
/// </summary>
internal sealed class TwelveGroupBanksConverter : JsonConverter<Dictionary<string, TwelveGroupBankEntry[]>>
{
    public override Dictionary<string, TwelveGroupBankEntry[]> ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<string, TwelveGroupBankEntry[]>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var result = new Dictionary<string, TwelveGroupBankEntry[]>();
        if (reader.TokenType == JsonToken.Null) return result;
        if (reader.TokenType != JsonToken.StartObject) return result;

        var root = JObject.Load(reader);
        foreach (var (currency, value) in root)
        {
            if (value is not JArray arr) continue;

            var entries = new List<TwelveGroupBankEntry>(arr.Count);
            foreach (var token in arr)
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                    {
                        var code = token.Value<string>() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(code))
                            entries.Add(new TwelveGroupBankEntry { Code = code, Name = code });
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
                            entries.Add(new TwelveGroupBankEntry { Code = code, Name = name });
                        break;
                    }
                }
            }

            result[currency] = entries.ToArray();
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, TwelveGroupBankEntry[]>? value, JsonSerializer serializer)
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
