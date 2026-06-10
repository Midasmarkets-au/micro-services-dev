using System.Globalization;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.TwelveGroup;

/// <summary>
/// 12Group (thesolix.com) payment gateway client. Thailand <b>THB-only</b>: Thai QR / PromptPay
/// deposit (renders the base64 <c>code_image</c> inline as a QR; falls back to redirecting to the
/// hosted <c>code_url</c> only when no image is returned) + host-to-host bank payout.
///
/// Auth = static JWT in the <c>authorization</c> header + a partner-code header (note: deposit uses
/// <c>partner_code</c>, payout/inquiry use <c>Partnercode</c>) + <c>channel</c>. There is NO request
/// signature and NO callback signature, so deposit callbacks are confirmed server-side via
/// <see cref="InquiryDepositAsync"/> before crediting.
///
/// Endpoints (host + segment from <see cref="TwelveGroupOptions"/>):
///   POST {deposit}/{seg}/create-qr-code           (deposit; seg = test|v1)
///   GET  {deposit}/{seg}/view-qr-code/{trans_id}   (hosted QR page; fallback only — we render code_image inline)
///   GET  {inquiry}/{seg}/inquiry-deposit/{trans_id}(seg = test|empty)
///   POST {payout}/{seg}/payout                     (seg = test|empty)
///   GET  {inquiry}/{seg}/inquiry-withdraw/{ref1}
///
/// Doc: Requirement/12Group/ (deposit-api.md, withdrawal-api.md).
/// </summary>
public static class TwelveGroup
{
    // Deposit response: HTTP 201 + resp_code 201. Inquiry/callback: resp_code 200.
    private const int RespCodeCreated = 201;

    // Payout status codes (per withdrawal doc).
    public const int PayoutSuccess = 1000;
    public const int PayoutPending = -2000; // Zero-Drop: completed but waiting manual transfer; callback follows.
    public const int PayoutCancel  = 9092;  // replaces "Failed" under Zero-Drop.

    // ============================================================================================
    // Header helper
    // ============================================================================================

    /// <summary>
    /// Applies the common 12Group headers. <paramref name="partnerCodeHeader"/> differs by endpoint:
    /// <c>partner_code</c> for deposit, <c>Partnercode</c> for payout/inquiry.
    /// </summary>
    private static void ApplyHeaders(HttpRequestMessage req, TwelveGroupOptions options, string partnerCodeHeader,
        bool includeChannel = true, bool includeDevice = false)
    {
        // authorization is a restricted header → add without validation. Token is the raw JWT (no "Bearer ").
        req.Headers.TryAddWithoutValidation("authorization", options.AuthorizationToken);
        req.Headers.TryAddWithoutValidation(partnerCodeHeader, options.PartnerCode);
        if (includeChannel)
            req.Headers.TryAddWithoutValidation("channel", options.Channel);
        if (includeDevice && !string.IsNullOrWhiteSpace(options.Device))
            req.Headers.TryAddWithoutValidation("device", options.Device);
    }

    // ============================================================================================
    // Deposit (Create QR code)
    // ============================================================================================

    /// <summary>Create-QR-code client used by <c>DepositService.ProcessTwelveGroupAsync</c>.</summary>
    public sealed class RequestClient
    {
        public string PaymentNumber { get; set; } = null!;

        /// <summary>THB amount (integer; vendor accepts integer only, 300–900,000 THB/transaction).</summary>
        public decimal Amount { get; set; }

        /// <summary>Bank reference, numeric, ≤18 digits. Echoed on the callback.</summary>
        public string Ref1 { get; set; } = string.Empty;

        /// <summary>Client first + last name. REQUIRED by the vendor for AML.</summary>
        public string Ref3 { get; set; } = string.Empty;

        /// <summary>Free echo field; we put our PaymentNumber here for reconciliation (returned on callback).</summary>
        public string Ref4 { get; set; } = string.Empty;

        public string MobileNo { get; set; } = string.Empty;

        public TwelveGroupOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var bodyObj = new Dictionary<string, object?>
            {
                ["amount"] = (int)Math.Round(Amount, 0, MidpointRounding.AwayFromZero),
                ["ref1"]   = Ref1,
                ["ref3"]   = Ref3,
                ["ref4"]   = Ref4,
            };
            if (!string.IsNullOrWhiteSpace(MobileNo))
                bodyObj["mobile_no"] = MobileNo;

            var jsonBody = JsonConvert.SerializeObject(bodyObj);
            var url = Options.CreateQrUrl;

            Logger.LogInformation("TwelveGroup.RequestAsync POST {Url} body={Body}", url, jsonBody);

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            ApplyHeaders(req, Options, "partner_code", includeChannel: true, includeDevice: true);

            var response = await Client.SendAsync(req);
            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("TwelveGroup.RequestAsync -> {Status} {Body}", (int)response.StatusCode, responseBody);

            JObject jo;
            try
            {
                jo = JObject.Parse(responseBody);
            }
            catch
            {
                return DepositCreatedResponseModel.Fail($"12Group: unparseable response ({(int)response.StatusCode})");
            }

            var respCode = (int?)jo["resp_code"] ?? 0;
            var data = jo["data"] as JObject;
            if (respCode != RespCodeCreated || data == null)
            {
                var msg = (string?)jo["resp_msg"] ?? responseBody;
                return DepositCreatedResponseModel.Fail($"12Group error ({respCode}): {msg}");
            }

            var codeUrl = (string?)data["code_url"];
            var codeImage = (string?)data["code_image"];
            var transId = (string?)data["trans_id"];
            var one2payRef = (string?)data["one2pay_ref"];

            // Prefer rendering the QR inline from the base64 PNG (code_image): the hosted code_url page
            // has been observed to fail validation server-side ("g could not have been validated, or is
            // invalid"), so the inline image is the reliable path. Fall back to the code_url redirect only
            // when no image is returned. TransactionId is intentionally left null so the FE doesn't surface
            // the "complete payment" button (POST /payment/{id}/paid) — 12Group is inquiry/callback-confirmed,
            // not paid-notification driven. Reference still carries trans_id for inquiry reconciliation.
            if (!string.IsNullOrWhiteSpace(codeImage))
            {
                return new DepositCreatedResponseModel
                {
                    IsSuccess     = true,
                    Action        = PaymentResponseActionTypes.QrCode,
                    TextForQrCode = codeImage,
                    Reference     = transId,        // stored into Payment.ReferenceNumber for inquiry
                    PaymentNumber = PaymentNumber,
                    Info          = new { trans_id = transId, one2pay_ref = one2payRef, ref1 = Ref1, ref4 = Ref4, code_url = codeUrl }
                };
            }

            if (string.IsNullOrWhiteSpace(codeUrl))
            {
                Logger.LogError("TwelveGroup.RequestAsync: 201 but no code_image or code_url. Body={Body}", responseBody);
                return DepositCreatedResponseModel.Fail("12Group: no code_image or code_url in response");
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess     = true,
                Action        = PaymentResponseActionTypes.Redirect,
                RedirectUrl   = codeUrl,
                Reference     = transId,        // stored into Payment.ReferenceNumber for inquiry
                TransactionId = transId,
                PaymentNumber = PaymentNumber,
                Info          = new { trans_id = transId, one2pay_ref = one2payRef, ref1 = Ref1, ref4 = Ref4 }
            };
        }
    }

    // ============================================================================================
    // Payout (Transfer Out)
    // ============================================================================================

    /// <summary>Transfer-out client used by <c>PayoutService.PayoutAsync</c>.</summary>
    public sealed class PayoutRequestClient
    {
        /// <summary>We send <c>PayoutRecord.HashId</c> here → echoed back as <c>ref1</c> on the pending callback.</summary>
        public string PaymentNumber { get; set; } = null!;

        public decimal Amount { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public string BankNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string TransactionBy { get; set; } = string.Empty;

        public TwelveGroupOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        public async Task<PayoutResponseModel> RequestAsync()
        {
            var bodyObj = new Dictionary<string, object?>
            {
                ["bankcode"]       = BankCode,
                ["bankacc"]        = BankNumber,
                ["bankname"]       = BankName,
                ["accname"]        = AccountName,
                ["amount"]         = decimal.Parse(Amount.ToString("0.00", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                ["mobileno"]       = string.IsNullOrWhiteSpace(MobileNo) ? Options.DefaultMobileNo : MobileNo,
                ["transaction_by"] = string.IsNullOrWhiteSpace(TransactionBy) ? $"Bacera_{Options.PartnerCode}" : TransactionBy,
                ["ref1"]           = PaymentNumber,
                ["ref3"]           = AccountName,
            };

            var jsonBody = JsonConvert.SerializeObject(bodyObj);
            var url = Options.PayoutUrl;

            Logger.LogInformation("TwelveGroup.PayoutRequestClient POST {Url} body={Body}", url, jsonBody);

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            ApplyHeaders(req, Options, "Partnercode", includeChannel: true);

            var response = await Client.SendAsync(req);
            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("TwelveGroup.PayoutRequestClient -> {Status} {Body}", (int)response.StatusCode, responseBody);

            int status;
            string message;
            try
            {
                var jo = JObject.Parse(responseBody);
                status = (int?)jo["status"] ?? 0;
                message = (string?)jo["message"] ?? responseBody;
            }
            catch
            {
                return PayoutResponseModel.Fail($"12Group payout: unparseable response ({(int)response.StatusCode})");
            }

            // Money moves only on 1000. -2000 (Zero-Drop pending) is NOT a terminal success — keep the
            // record out of Completed and wait for the pending callback (which sends 1000 or 9092).
            var isSuccess = status == PayoutSuccess;

            return new PayoutResponseModel
            {
                IsSuccess    = isSuccess,
                Message      = $"status={status}: {message}",
                ResponseJson = responseBody,
            };
        }
    }

    // ============================================================================================
    // Inquiry helpers (callback confirmation + test tooling)
    // ============================================================================================

    /// <summary>
    /// GET <c>inquiry-deposit/{trans_id}</c>. Returns the raw <c>data</c> object so callers can read
    /// <c>status</c> / <c>aml_status</c> / <c>aml_check</c>. Used to confirm deposits before crediting
    /// (callbacks are unsigned).
    /// </summary>
    public static async Task<JObject?> InquiryDepositAsync(
        TwelveGroupOptions options, string transId, HttpClient client, ILogger? logger = null)
    {
        var url = options.InquiryDepositUrl(transId);
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyHeaders(req, options, "Partnercode", includeChannel: false);

        var response = await client.SendAsync(req);
        var body = await response.Content.ReadAsStringAsync();
        logger?.LogInformation("TwelveGroup.InquiryDepositAsync GET {Url} -> {Status} {Body}",
            url, (int)response.StatusCode, body);

        try
        {
            var jo = JObject.Parse(body);
            return jo["data"] as JObject;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>GET <c>inquiry-withdraw/{ref1}</c>. Returns the parsed body (top-level <c>status</c> / <c>message</c>).</summary>
    public static async Task<JObject?> InquiryWithdrawAsync(
        TwelveGroupOptions options, string ref1, HttpClient client, ILogger? logger = null)
    {
        var url = options.InquiryWithdrawUrl(ref1);
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyHeaders(req, options, "Partnercode", includeChannel: true);

        var response = await client.SendAsync(req);
        var body = await response.Content.ReadAsStringAsync();
        logger?.LogInformation("TwelveGroup.InquiryWithdrawAsync GET {Url} -> {Status} {Body}",
            url, (int)response.StatusCode, body);

        try
        {
            return JObject.Parse(body);
        }
        catch
        {
            return null;
        }
    }
}
