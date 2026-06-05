using System.Globalization;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.Rivo;

/// <summary>
/// Rivo payment gateway client. Currently supports VND only at the merchant level, but every
/// piece of the surface is currency-agnostic so adding THB / IDR / etc. is a config-only change
/// (see <see cref="RivoOptions.Currency"/> + <see cref="RivoOptions.PaymentMethod"/>).
///
/// Endpoints (paths only; prefixed by <see cref="RivoOptions.ApiBaseUrl"/>):
///   POST /gateway/pay/first/order/create     (collection / pay-in)
///   GET  /gateway/pay/first/order/query      (collection status)
///   POST /gateway/payout/first/order/create  (disbursement / pay-out)
///   GET  /gateway/payout/first/order/query   (disbursement status; param names: mchOrderId / transNo)
///   GET  /gateway/pay/common/currentBalance  (multi-currency balance)
///
/// Doc: https://merchant.rivoglobal.io/rivo-api-docs.html
/// </summary>
public static class Rivo
{
    private const string PathPayCreate    = "/gateway/pay/first/order/create";
    private const string PathPayQuery     = "/gateway/pay/first/order/query";
    private const string PathPayoutCreate = "/gateway/payout/first/order/create";
    private const string PathPayoutQuery  = "/gateway/payout/first/order/query";
    private const string PathBalance      = "/gateway/pay/common/currentBalance";

    // ============================================================================================
    // Signer
    // ============================================================================================

    /// <summary>
    /// SHA-512 signer per Rivo doc §2.
    /// 1. Drop null / empty / <c>sign</c> entries.
    /// 2. Sort by full ASCII key (NOT by first letter).
    /// 3. Join as <c>k=v&amp;...</c>.
    /// 4. Append <c>&amp;key=&lt;secret&gt;</c>.
    /// 5. SHA-512 hex (lowercase).
    /// </summary>
    public static string GenerateSignature(IDictionary<string, object?> spec, string secretKey, ILogger? logger = null)
    {
        var sorted = spec
            .Where(kv => kv.Key != "sign")
            .Where(kv => kv.Value is not null && !string.IsNullOrEmpty(Stringify(kv.Value)))
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .ToList();

        var plain = string.Join("&", sorted.Select(kv => $"{kv.Key}={Stringify(kv.Value!)}"));
        plain = plain + "&key=" + secretKey;

        logger?.LogDebug("Rivo.GenerateSignature plain: {Plain}", plain);
        var hash = Utils.Sha512Hash(plain);
        logger?.LogDebug("Rivo.GenerateSignature hash: {Hash}", hash);
        return hash;
    }

    /// <summary>
    /// Verify a callback / response signature **from the raw JSON payload** (per doc: never deserialize first).
    /// Walks the top-level JObject; if the response carries an inner <c>data</c> object, pass that as <paramref name="rawJson"/>.
    /// Returns <c>false</c> when <c>sign</c> is missing or mismatched.
    /// </summary>
    public static bool VerifySignatureFromRaw(string rawJson, string secretKey, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return false;

        JObject jo;
        try
        {
            jo = JObject.Parse(rawJson);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Rivo.VerifySignatureFromRaw: invalid JSON");
            return false;
        }

        var providedSign = (string?)jo["sign"];
        if (string.IsNullOrWhiteSpace(providedSign))
        {
            logger?.LogWarning("Rivo.VerifySignatureFromRaw: sign field missing");
            return false;
        }

        var spec = ToSignSpec(jo);
        var computed = GenerateSignature(spec, secretKey, logger);
        var ok = string.Equals(computed, providedSign, StringComparison.OrdinalIgnoreCase);
        if (!ok)
        {
            logger?.LogWarning(
                "Rivo signature mismatch. Computed={Computed} Provided={Provided}",
                computed, providedSign);
        }
        return ok;
    }

    /// <summary>
    /// Adds <c>version</c>, optional <c>signType=SHA512</c>, and (for <c>1.1</c>) <c>timestamp</c> + <c>nonce</c>
    /// to <paramref name="spec"/> in-place. All present fields participate in signing.
    ///
    /// IMPORTANT: <c>signType</c> is NOT part of the documented parameter list for the query endpoints
    /// (<c>/gateway/pay/first/order/query</c> and <c>/gateway/payout/first/order/query</c>). Rivo's
    /// strict per-endpoint verification rebuilds the canonical sign string only from the documented
    /// fields, so including <c>signType</c> on a query produces a different SHA-512 on their side and
    /// fails with <c>40101</c>. Pass <paramref name="includeSignType"/>=<c>false</c> for those two
    /// endpoints (kept <c>true</c> for create/payout/balance, where the doc DOES require <c>signType</c>).
    /// </summary>
    public static void StampVersion11(IDictionary<string, object?> spec, string version = "1.1", bool includeSignType = true)
    {
        if (includeSignType) spec["signType"] = "SHA512";
        spec["version"] = version;
        if (version == "1.1")
        {
            spec["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            spec["nonce"] = Guid.NewGuid().ToString("N")[..16];
        }
    }

    private static string Stringify(object value)
    {
        return value switch
        {
            null => string.Empty,
            string s => s,
            bool b => b ? "true" : "false",
            decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
            double dd => dd.ToString("0.00", CultureInfo.InvariantCulture),
            float ff => ff.ToString("0.00", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Convert a JObject into the flat <c>{key -> object?}</c> map used by the signer.
    /// Leaf values become their typed counterparts; nested objects (e.g. <c>extraData</c> in a callback)
    /// are serialized back to JSON to preserve byte-equality with what Rivo signed.
    /// </summary>
    private static IDictionary<string, object?> ToSignSpec(JObject jo)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in jo.Properties())
        {
            if (prop.Name == "sign") continue;
            dict[prop.Name] = prop.Value.Type switch
            {
                JTokenType.Null => null,
                JTokenType.String => (string?)prop.Value ?? string.Empty,
                JTokenType.Integer => (long)prop.Value,
                JTokenType.Float => (decimal)prop.Value,
                JTokenType.Boolean => (bool)prop.Value,
                JTokenType.Object or JTokenType.Array => prop.Value.ToString(Formatting.None),
                _ => prop.Value.ToString()
            };
        }
        return dict;
    }

    // ============================================================================================
    // Pay-In (Collection)
    // ============================================================================================

    /// <summary>Create-collection-order client used by <c>DepositService.ProcessRivoAsync</c>.</summary>
    public sealed class RequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }

        /// <summary>Per-row from <see cref="RivoOptions.Currency"/>.</summary>
        public string Currency { get; set; } = null!;

        /// <summary>Per-row from <see cref="RivoOptions.PaymentMethod"/>.</summary>
        public string PaymentMethod { get; set; } = null!;

        public string PayerName { get; set; } = string.Empty;
        public string PayerPhone { get; set; } = string.Empty;
        public string PayerEmail { get; set; } = string.Empty;
        public string City { get; set; } = " ";
        public string Address { get; set; } = " ";
        public string ClientIp { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;

        public RivoOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private IDictionary<string, object?> BuildForm()
        {
            var spec = new Dictionary<string, object?>
            {
                ["mchId"]         = Options.MchId,
                ["tradeNo"]       = PaymentNumber,
                // Pre-stringify so the JSON body and signed string are byte-identical.
                // Newtonsoft serializes decimal 50_000m as "50000.0" but our signing format is "50000.00";
                // the mismatch makes Rivo reject with 40101. See Pay247 for the same pattern.
                ["amount"]        = Amount.ToString("0.00", CultureInfo.InvariantCulture),
                ["currency"]      = Currency,
                ["paymentMethod"] = PaymentMethod,
                ["orderDate"]     = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["clientIp"]      = ClientIp,
                ["name"]          = PayerName,
                ["phone"]         = PayerPhone,
                ["email"]         = PayerEmail,
                ["city"]          = City,
                ["address"]       = Address,
                ["notifyUrl"]     = Options.PayCallbackUri,
                ["returnUrl"]     = ReturnUrl,
                ["subject"]       = string.IsNullOrEmpty(Subject) ? Options.Subject : Subject,
                ["extraData"]     = ExtraData,
            };
            StampVersion11(spec, Options.Version);
            return spec;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var spec = BuildForm();
            spec["sign"] = GenerateSignature(spec, Options.SecretKey, Logger);

            var url = Options.ApiBaseUrl.TrimEnd('/') + PathPayCreate;
            var jsonBody = JsonConvert.SerializeObject(
                spec.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value));

            Logger.LogInformation("Rivo.RequestAsync POST {Url} body={Body}", url, jsonBody);

            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Rivo.RequestAsync ({Currency}) -> {Status} {Body}",
                Currency, (int)response.StatusCode, responseBody);

            var envelope = SafeParse(responseBody);
            if (envelope == null || envelope.Code != "200" || envelope.Data == null)
            {
                var msg = envelope?.Msg ?? responseBody;
                return DepositCreatedResponseModel.Fail($"Rivo error ({envelope?.Code ?? "?"}): {msg}");
            }

            var cashierUrl = (string?)envelope.Data["cashierUrl"];
            var qrcode = (string?)envelope.Data["qrcode"];
            var outTradeNo = (string?)envelope.Data["outTradeNo"];

            if (!string.IsNullOrEmpty(cashierUrl))
            {
                return new DepositCreatedResponseModel
                {
                    IsSuccess = true,
                    Action = PaymentResponseActionTypes.Redirect,
                    RedirectUrl = cashierUrl,
                    Reference = outTradeNo,
                    PaymentNumber = PaymentNumber,
                    Info = new { outTradeNo, qrcode }
                };
            }

            if (!string.IsNullOrEmpty(qrcode))
            {
                return new DepositCreatedResponseModel
                {
                    IsSuccess = true,
                    Action = PaymentResponseActionTypes.QrCode,
                    TextForQrCode = qrcode,
                    Reference = outTradeNo,
                    PaymentNumber = PaymentNumber,
                    Info = new { outTradeNo, qrcode }
                };
            }

            Logger.LogError("Rivo.RequestAsync: Rivo returned 200 but no cashierUrl/qrcode. Body={Body}", responseBody);
            return DepositCreatedResponseModel.Fail("Rivo: no payment URL or QR code in response");
        }
    }

    // ============================================================================================
    // Pay-Out (Disbursement)
    // ============================================================================================

    /// <summary>Create-disbursement-order client used by <c>PayoutService.PayoutAsync</c>.</summary>
    public sealed class WithdrawalRequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;

        public string AccountName { get; set; } = string.Empty;
        public string AccountNoUnified { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccTel { get; set; } = string.Empty;
        public string AccIdCard { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;

        public RivoOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private IDictionary<string, object?> BuildForm()
        {
            var spec = new Dictionary<string, object?>
            {
                ["mchId"]            = Options.MchId,
                ["tradeNo"]          = PaymentNumber,
                // See RequestClient.BuildForm for why amount is pre-stringified.
                ["amount"]           = Amount.ToString("0.00", CultureInfo.InvariantCulture),
                ["currency"]         = Currency,
                ["accType"]          = Options.AccType,
                ["paymentMethod"]    = Options.PayoutMethod,
                ["accountName"]      = AccountName,
                ["accountNoUnified"] = AccountNoUnified,
                ["bankCode"]         = BankCode,
                ["bankName"]         = BankName,
                ["accTel"]           = AccTel,
                ["accIdCard"]        = AccIdCard,
                ["purpose"]          = Purpose,
                ["notifyUrl"]        = Options.PayoutCallbackUri,
                ["extraData"]        = ExtraData,
            };
            StampVersion11(spec, Options.Version);
            return spec;
        }

        public async Task<PayoutResponseModel> RequestAsync()
        {
            var spec = BuildForm();
            spec["sign"] = GenerateSignature(spec, Options.SecretKey, Logger);

            var url = Options.ApiBaseUrl.TrimEnd('/') + PathPayoutCreate;
            var jsonBody = JsonConvert.SerializeObject(
                spec.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value));

            Logger.LogInformation("Rivo.WithdrawalRequestClient POST {Url} body={Body}", url, jsonBody);

            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Rivo.WithdrawalRequestClient ({Currency}) -> {Status} {Body}",
                Currency, (int)response.StatusCode, responseBody);

            var envelope = SafeParse(responseBody);
            bool isSuccess = envelope?.Code == "200" && envelope.Data != null;
            string message = envelope?.Msg ?? responseBody;

            return new PayoutResponseModel
            {
                IsSuccess = isSuccess,
                Message = message,
                ResponseJson = responseBody,
            };
        }
    }

    // ============================================================================================
    // Query helpers (used by CmdTestService + admin tooling)
    // ============================================================================================

    /// <summary>
    /// GET <c>/gateway/pay/first/order/query</c>. At least one of tradeNo / outTradeNo required.
    /// Per doc: <c>signType</c> is NOT part of this endpoint's parameter list — do NOT include it in
    /// signing or the URL, otherwise Rivo rejects with <c>40101</c>.
    /// </summary>
    public static async Task<RivoEnvelope?> QueryCollectionOrderAsync(
        RivoOptions options, string? tradeNo, string? outTradeNo, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mchId"]      = options.MchId,
            ["tradeNo"]    = tradeNo ?? string.Empty,
            ["outTradeNo"] = outTradeNo ?? string.Empty,
        };
        StampVersion11(spec, options.Version, includeSignType: false);
        spec["sign"] = GenerateSignature(spec, options.SecretKey, logger);
        return await GetSignedAsync(options.ApiBaseUrl + PathPayQuery, spec, client, logger);
    }

    /// <summary>
    /// GET <c>/gateway/payout/first/order/query</c>. Param names are <c>mchOrderId</c> / <c>transNo</c>,
    /// NOT <c>tradeNo</c> / <c>outTradeNo</c> — easy gotcha. Same <c>signType</c>-exclusion rule as
    /// the collection-query endpoint.
    /// </summary>
    public static async Task<RivoEnvelope?> QueryDisbursementOrderAsync(
        RivoOptions options, string? mchOrderId, string? transNo, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mchId"]      = options.MchId,
            ["mchOrderId"] = mchOrderId ?? string.Empty,
            ["transNo"]    = transNo ?? string.Empty,
        };
        StampVersion11(spec, options.Version, includeSignType: false);
        spec["sign"] = GenerateSignature(spec, options.SecretKey, logger);
        return await GetSignedAsync(options.ApiBaseUrl + PathPayoutQuery, spec, client, logger);
    }

    /// <summary>GET <c>/gateway/pay/common/currentBalance</c>. Returns multi-currency balances.</summary>
    public static async Task<RivoEnvelope?> QueryBalanceAsync(
        RivoOptions options, string? currencyFilter, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mchId"] = options.MchId,
        };
        if (!string.IsNullOrWhiteSpace(currencyFilter))
            spec["currency"] = currencyFilter;
        StampVersion11(spec, options.Version);
        spec["sign"] = GenerateSignature(spec, options.SecretKey, logger);
        return await GetSignedAsync(options.ApiBaseUrl + PathBalance, spec, client, logger);
    }

    private static async Task<RivoEnvelope?> GetSignedAsync(
        string url, IDictionary<string, object?> spec, HttpClient client, ILogger? logger)
    {
        var query = string.Join("&",
            spec.Where(kv => kv.Value is not null && !string.IsNullOrEmpty(Stringify(kv.Value!)))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(Stringify(kv.Value!))}"));
        var fullUrl = url + (url.Contains('?') ? "&" : "?") + query;
        logger?.LogInformation("Rivo GET {Url}", fullUrl);

        var response = await client.GetAsync(fullUrl);
        var body = await response.Content.ReadAsStringAsync();
        logger?.LogInformation("Rivo GET response: {Status} {Body}", (int)response.StatusCode, body);

        return SafeParse(body);
    }

    private static RivoEnvelope? SafeParse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var jo = JObject.Parse(json);
            return new RivoEnvelope
            {
                Code = (string?)jo["code"] ?? string.Empty,
                Msg = (string?)jo["msg"] ?? string.Empty,
                Data = jo["data"] as JObject,
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>Lightweight envelope around a Rivo response. <c>Data</c> kept as <see cref="JObject"/> so callers can pull whatever they need.</summary>
public sealed class RivoEnvelope
{
    public string Code { get; set; } = string.Empty;
    public string Msg { get; set; } = string.Empty;
    public JObject? Data { get; set; }

    public bool IsSuccess => Code == "200";
}
