using System.Globalization;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.Pay247;

/// <summary>
/// Pay247 (gateway.pay247.io) payment client. Multi-currency by design — IDR/MYR/VND/PHP today,
/// extra currencies are config-only additions (new <c>_PaymentMethod</c> row per
/// (<see cref="Pay247Options.Currency"/>, <see cref="Pay247Options.PayMethod"/>) pair).
///
/// Endpoints (all <b>POST JSON</b>; prefixed by <see cref="Pay247Options.ApiBaseUrl"/>):
///   POST /gateway/payin/create     (collection / pay-in)
///   POST /gateway/payin/query      (collection status; param: mch_order_no)
///   POST /gateway/payin/update     (push UTR — INR only; not used for our 4 currencies)
///   POST /gateway/payout/create    (disbursement / pay-out)
///   POST /gateway/payout/query     (disbursement status; param: mch_order_no — UNIFIED, unlike Rivo's mchOrderId/transNo trap)
///   POST /gateway/payout/approve   (optional manual approval step; not used today)
///   POST /gateway/balance          (multi-currency balance)
///
/// Signature: MD5 hex lowercase. Sort by full ASCII key (case-sensitive), drop empty/null/sign,
/// join <c>k=v&amp;...</c>, append <see cref="Pay247Options.SecretKey"/> <b>raw</b> (no <c>&amp;key=</c>
/// prefix — different from Rivo), MD5.
///
/// Callback reply: literal <c>SUCCESS</c> (uppercase) as <c>text/plain</c>. Anything else triggers
/// Pay247's retry loop (5× at 3/9/27/81/243s).
///
/// Doc: https://docs.pay247.io/api-reference/en/common.md
/// </summary>
public static class Pay247
{
    private const string PathPayinCreate   = "/gateway/payin/create";
    private const string PathPayinQuery    = "/gateway/payin/query";
    private const string PathPayinUpdate   = "/gateway/payin/update";
    private const string PathPayoutCreate  = "/gateway/payout/create";
    private const string PathPayoutQuery   = "/gateway/payout/query";
    private const string PathPayoutApprove = "/gateway/payout/approve";
    private const string PathBalance       = "/gateway/balance";

    // ============================================================================================
    // Signer
    // ============================================================================================

    /// <summary>
    /// MD5 signer per Pay247 doc §Signature.
    /// 1. Drop <c>null</c> / <c>sign</c> entries (and empty-string entries when <paramref name="dropEmpty"/> is true).
    /// 2. Sort by full ASCII key (case-sensitive — <see cref="StringComparer.Ordinal"/>).
    /// 3. Join as <c>k=v&amp;...</c>.
    /// 4. Append <see cref="Pay247Options.SecretKey"/> <b>raw</b> (no <c>&amp;key=</c> prefix).
    /// 5. MD5 hex lowercase (32 chars).
    ///
    /// <para>
    /// <paramref name="dropEmpty"/> exists because Pay247 signs the two directions differently:
    /// our <b>outbound requests</b> drop empty-string fields (and Pay247's request-verifier agrees,
    /// since requests carrying empty <c>payer_*</c>/<c>return_url</c> are accepted), but Pay247's
    /// <b>callback</b> signer KEEPS empty-string fields (observed: it signs <c>error=</c>). Callback
    /// verification therefore must pass <c>dropEmpty: false</c>, otherwise the empty <c>error</c>
    /// field is silently stripped and every callback fails with a false signature mismatch.
    /// </para>
    /// </summary>
    public static string GenerateSignature(IDictionary<string, object?> spec, string secretKey, ILogger? logger = null, bool dropEmpty = true)
    {
        var sorted = spec
            .Where(kv => kv.Key != "sign")
            .Where(kv => kv.Value is not null && (!dropEmpty || !string.IsNullOrEmpty(Stringify(kv.Value))))
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .ToList();

        var plain = string.Join("&", sorted.Select(kv => $"{kv.Key}={Stringify(kv.Value!)}"));
        var withSecret = plain + secretKey;

        logger?.LogDebug("Pay247.GenerateSignature plain: {Plain}", plain);
        var hash = Utils.Md5Hash(withSecret).ToLowerInvariant();
        logger?.LogDebug("Pay247.GenerateSignature hash: {Hash}", hash);
        return hash;
    }

    /// <summary>
    /// Verify a callback signature <b>from the raw JSON payload</b> (per doc: never deserialize
    /// first — Pay247 can add fields across versions, and any field we silently drop would
    /// produce a false-negative mismatch).
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
            logger?.LogWarning(ex, "Pay247.VerifySignatureFromRaw: invalid JSON");
            return false;
        }

        var providedSign = (string?)jo["sign"];
        if (string.IsNullOrWhiteSpace(providedSign))
        {
            logger?.LogWarning("Pay247.VerifySignatureFromRaw: sign field missing");
            return false;
        }

        var spec = ToSignSpec(jo);
        // Callbacks: Pay247 keeps empty-string fields in the signed string (e.g. error=""),
        // unlike the request direction. Do NOT drop empties here or every callback false-mismatches.
        var computed = GenerateSignature(spec, secretKey, logger, dropEmpty: false);
        var ok = string.Equals(computed, providedSign, StringComparison.OrdinalIgnoreCase);
        if (!ok)
        {
            logger?.LogWarning(
                "Pay247 signature mismatch. Computed={Computed} Provided={Provided}",
                computed, providedSign);
        }
        return ok;
    }

    /// <summary>
    /// Adds the four common request envelope fields (<c>mch_id</c>, <c>timestamp</c> in <b>milliseconds</b>,
    /// <c>version</c>, <c>uuid</c>) to <paramref name="spec"/> in-place. All four participate in signing.
    /// Caller may pre-set any of them to override defaults.
    /// </summary>
    public static void StampCommon(IDictionary<string, object?> spec, Pay247Options options)
    {
        if (!spec.ContainsKey("mch_id")    || spec["mch_id"]    is null) spec["mch_id"]    = options.MchId;
        if (!spec.ContainsKey("version")   || spec["version"]   is null) spec["version"]   = options.Version;
        if (!spec.ContainsKey("uuid")      || spec["uuid"]      is null) spec["uuid"]      = Guid.NewGuid().ToString();
        if (!spec.ContainsKey("timestamp") || spec["timestamp"] is null) spec["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
    /// Leaf values become their typed counterparts; nested objects (e.g. <c>pay_params</c>)
    /// are serialised back to JSON with <c>Formatting.None</c> so the byte sequence we sign
    /// matches what Pay247 signed.
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
    // Pay-In (Collection) — POST /gateway/payin/create
    // ============================================================================================

    /// <summary>Create-collection-order client used by <c>DepositService.ProcessPay247Async</c>.</summary>
    public sealed class RequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public long AccountUid { get; set; }
        public decimal Amount { get; set; }

        /// <summary>Per-row from <see cref="Pay247Options.Currency"/>.</summary>
        public string Currency { get; set; } = null!;

        /// <summary>Per-row from <see cref="Pay247Options.PayMethod"/>.</summary>
        public string PayMethod { get; set; } = null!;

        public string ClientIp { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;

        // Optional payer fields. Required only for some methods (e.g. THB BANK needs name/account/bank_code,
        // JPY BANK needs name/address). For our 4 currencies in phase 1 they're all optional, but populated
        // when available improves anti-fraud scoring on Pay247's side.
        public string PayerName { get; set; } = string.Empty;
        public string PayerPhone { get; set; } = string.Empty;
        public string PayerEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public Pay247Options Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private IDictionary<string, object?> BuildForm()
        {
            var spec = new Dictionary<string, object?>
            {
                ["mch_order_no"] = PaymentNumber,
                ["mch_user_id"]  = AccountUid.ToString(),
                ["currency"]     = Currency,
                // Doc: amount is a STRING with two decimals ("99.11"). Must be the exact same token
                // on the wire and in the signature, else Pay247 returns 4000 "Signature verification failed".
                ["amount"]       = Amount.ToString("0.00", CultureInfo.InvariantCulture),
                ["pay_method"]   = PayMethod,
                ["pay_theme"]    = Options.PayTheme,
                ["client_ip"]    = ClientIp,
                ["notify_url"]   = Options.PayCallbackUri,
                ["return_url"]   = ReturnUrl,
                ["payer_name"]   = PayerName,
                ["payer_phone"]  = PayerPhone,
                ["payer_email"]  = PayerEmail,
            };
            StampCommon(spec, Options);
            return spec;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var spec = BuildForm();
            spec["sign"] = GenerateSignature(spec, Options.SecretKey, Logger);

            var url = Options.ApiBaseUrl.TrimEnd('/') + PathPayinCreate;
            var jsonBody = JsonConvert.SerializeObject(
                spec.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value));

            Logger.LogInformation("Pay247.RequestAsync POST {Url} body={Body}", url, jsonBody);

            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Pay247.RequestAsync ({Currency}/{PayMethod}) -> {Status} {Body}",
                Currency, PayMethod, (int)response.StatusCode, responseBody);

            var envelope = SafeParse(responseBody);
            if (envelope == null || envelope.Code != 0 || envelope.Data == null)
            {
                var msg = envelope?.Message ?? responseBody;
                var code = envelope?.Code.ToString() ?? "?";
                return DepositCreatedResponseModel.Fail($"Pay247 error ({code}): {msg}");
            }

            var payUrl    = (string?)envelope.Data["pay_url"];
            var orderNo   = (string?)envelope.Data["order_no"];
            var payParams = envelope.Data["pay_params"] as JObject;
            var status    = (string?)envelope.Data["status"];

            // pay_theme=link → redirect to pay_url.
            if (!string.IsNullOrEmpty(payUrl))
            {
                return new DepositCreatedResponseModel
                {
                    IsSuccess = true,
                    Action = PaymentResponseActionTypes.Redirect,
                    RedirectUrl = payUrl,
                    Reference = orderNo,
                    PaymentNumber = PaymentNumber,
                    Info = new { order_no = orderNo, status }
                };
            }

            // pay_theme=custom → render the bank/QR details on our own deposit screen.
            // Phase 1 we ship with pay_theme=link only; surface a clear error if a row gets misconfigured.
            if (payParams != null)
            {
                Logger.LogWarning(
                    "Pay247.RequestAsync: pay_theme=custom returned pay_params but FE rendering isn't wired yet. " +
                    "Set Configuration.PayTheme=\"link\" to use the hosted cashier. pay_params={PayParams}",
                    payParams.ToString(Formatting.None));
                return DepositCreatedResponseModel.Fail("Pay247: custom theme not yet supported by the FE; switch this row to PayTheme=\"link\".");
            }

            Logger.LogError("Pay247.RequestAsync: 200 OK but no pay_url/pay_params. Body={Body}", responseBody);
            return DepositCreatedResponseModel.Fail("Pay247: no payment URL or QR params in response");
        }
    }

    // ============================================================================================
    // Pay-Out (Disbursement) — POST /gateway/payout/create
    // ============================================================================================

    /// <summary>Create-disbursement-order client used by <c>PayoutService.PayoutAsync</c>.</summary>
    public sealed class PayoutRequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;

        public string AccountName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankBranch { get; set; } = string.Empty;

        public Pay247Options Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private IDictionary<string, object?> BuildForm()
        {
            var spec = new Dictionary<string, object?>
            {
                ["mch_order_no"] = PaymentNumber,
                ["currency"]     = Currency,
                // Doc: amount is a STRING with two decimals ("99.11"). Keep wire and signature tokens identical.
                ["amount"]       = Amount.ToString("0.00", CultureInfo.InvariantCulture),
                ["pay_method"]   = Options.PayMethod,
                ["account_name"] = AccountName,
                ["account_no"]   = AccountNo,
                ["bank_code"]    = BankCode,
                ["bank_branch"]  = BankBranch,
                ["notify_url"]   = Options.PayoutCallbackUri,
            };
            StampCommon(spec, Options);
            return spec;
        }

        public async Task<PayoutResponseModel> RequestAsync()
        {
            var spec = BuildForm();
            spec["sign"] = GenerateSignature(spec, Options.SecretKey, Logger);

            var url = Options.ApiBaseUrl.TrimEnd('/') + PathPayoutCreate;
            var jsonBody = JsonConvert.SerializeObject(
                spec.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value));

            Logger.LogInformation("Pay247.PayoutRequestClient POST {Url} body={Body}", url, jsonBody);

            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Pay247.PayoutRequestClient ({Currency}/{PayMethod}) -> {Status} {Body}",
                Currency, Options.PayMethod, (int)response.StatusCode, responseBody);

            var envelope = SafeParse(responseBody);
            // code=5000 is the special "unknown — you MUST query later" error per doc §System Code.
            // We mark these as success=false so BatchPayout doesn't auto-flip to Completed,
            // but log loudly so ops know to query, not retry.
            if (envelope?.Code == 5000)
            {
                Logger.LogWarning(
                    "Pay247.PayoutRequestClient: code 5000 (system unknown) for {PaymentNumber} — caller must call payout/query later",
                    PaymentNumber);
            }

            bool isSuccess = envelope?.Code == 0 && envelope.Data != null;
            string message = envelope?.Message ?? responseBody;

            return new PayoutResponseModel
            {
                IsSuccess = isSuccess,
                Message = message,
                ResponseJson = responseBody,
            };
        }
    }

    // ============================================================================================
    // Query helpers — all POST JSON (unlike Rivo's GET queries)
    // ============================================================================================

    /// <summary>POST <c>/gateway/payin/query</c>. Lookup by <c>mch_order_no</c> (our <see cref="PaymentNumber"/>).</summary>
    public static async Task<Pay247Envelope?> QueryPayinAsync(
        Pay247Options options, string paymentNumber, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mch_order_no"] = paymentNumber,
        };
        return await PostSignedAsync(options.ApiBaseUrl + PathPayinQuery, spec, options, client, logger);
    }

    /// <summary>
    /// POST <c>/gateway/payout/query</c>. Lookup by <c>mch_order_no</c> — <b>unified</b> with
    /// payin query (unlike Rivo's <c>mchOrderId</c>/<c>transNo</c> trap).
    /// </summary>
    public static async Task<Pay247Envelope?> QueryPayoutAsync(
        Pay247Options options, string paymentNumber, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mch_order_no"] = paymentNumber,
        };
        return await PostSignedAsync(options.ApiBaseUrl + PathPayoutQuery, spec, options, client, logger);
    }

    /// <summary>
    /// POST <c>/gateway/payout/approve</c> with <c>action=confirm|reject</c>. Only relevant if
    /// the merchant has Pay247-side manual approval enabled (default off). Not wired into the
    /// auto-flow today — provided as an admin-tooling helper.
    /// </summary>
    public static async Task<Pay247Envelope?> ApprovePayoutAsync(
        Pay247Options options, string paymentNumber, string action, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mch_order_no"] = paymentNumber,
            ["action"]       = action,
        };
        return await PostSignedAsync(options.ApiBaseUrl + PathPayoutApprove, spec, options, client, logger);
    }

    /// <summary>POST <c>/gateway/balance</c>. Returns the multi-currency <c>wallets[]</c>.</summary>
    public static async Task<Pay247Envelope?> QueryBalanceAsync(
        Pay247Options options, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>();
        return await PostSignedAsync(options.ApiBaseUrl + PathBalance, spec, options, client, logger);
    }

    /// <summary>POST <c>/gateway/payin/update</c> with a <c>transaction_id</c> (UTR). INR only; not used for IDR/MYR/VND/PHP.</summary>
    public static async Task<Pay247Envelope?> UpdatePayinAsync(
        Pay247Options options, string paymentNumber, string transactionId, HttpClient client, ILogger? logger = null)
    {
        var spec = new Dictionary<string, object?>
        {
            ["mch_order_no"]   = paymentNumber,
            ["transaction_id"] = transactionId,
        };
        return await PostSignedAsync(options.ApiBaseUrl + PathPayinUpdate, spec, options, client, logger);
    }

    private static async Task<Pay247Envelope?> PostSignedAsync(
        string url, IDictionary<string, object?> spec, Pay247Options options, HttpClient client, ILogger? logger)
    {
        StampCommon(spec, options);
        spec["sign"] = GenerateSignature(spec, options.SecretKey, logger);

        var body = JsonConvert.SerializeObject(
            spec.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value));
        logger?.LogInformation("Pay247 POST {Url} body={Body}", url, body);

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        logger?.LogInformation("Pay247 POST response: {Status} {Body}", (int)response.StatusCode, responseBody);

        return SafeParse(responseBody);
    }

    private static Pay247Envelope? SafeParse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var jo = JObject.Parse(json);
            return new Pay247Envelope
            {
                Code = (int?)jo["code"] ?? -1,
                Message = (string?)jo["message"] ?? string.Empty,
                Uuid = (string?)jo["uuid"] ?? string.Empty,
                Timestamp = (long?)jo["timestamp"] ?? 0L,
                Data = jo["data"] as JObject,
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>Lightweight envelope around a Pay247 response. <c>Data</c> kept as <see cref="JObject"/> so callers can pull whatever they need.</summary>
public sealed class Pay247Envelope
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public JObject? Data { get; set; }

    public bool IsSuccess => Code == 0;
}
