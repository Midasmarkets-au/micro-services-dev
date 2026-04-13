using System.Net;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.QrCodeTunnel;

public class QrCodeTunnel
{
    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;

        public QrCodeTunnelOptions Options { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            if (!Options.IsValid())
                return DepositCreatedResponseModel.Fail("Invalid QrCodeTunnel configuration");

            var endpoint = $"{Options.BaseUrl.TrimEnd('/')}/api/v1/payment/request";
            var payload = new { amount = Amount, thirdPartyRef = PaymentNumber };
            var json = JsonConvert.SerializeObject(payload);

            Logger.LogInformation("QrCodeTunnel request to {Endpoint}: {Payload}", endpoint, json);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-API-Key", Options.ApiKey);
            request.Headers.Add("X-API-Secret", Options.ApiSecret);

            HttpResponseMessage response;
            try
            {
                response = await Client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "QrCodeTunnel HTTP request failed");
                return DepositCreatedResponseModel.Fail("Failed to connect to QR code service");
            }

            var body = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("QrCodeTunnel response {StatusCode}: {Body}", (int)response.StatusCode, body);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return DepositCreatedResponseModel.Fail("No available payment codes", true);

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(body);
                string errorMsg = errorObj?.error ?? $"Request failed with status {(int)response.StatusCode}";
                return DepositCreatedResponseModel.Fail(errorMsg, true);
            }

            var result = JsonConvert.DeserializeObject<dynamic>(body)!;
            string transactionId = result.transactionId;
            string qrCodeUrl = result.qrCodeUrl;
            string payeeName = result.payeeName;
            string expiresAt = result.expiresAt;

            Logger.LogInformation(
                "QrCodeTunnel created transaction {TransactionId} for thirdPartyRef {ThirdPartyRef}",
                transactionId,
                PaymentNumber);

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.QrCode,
                TextForQrCode = qrCodeUrl,
                PaymentNumber = PaymentNumber,
                Reference = transactionId,
                TransactionId = transactionId,
                Message = expiresAt,
                Info = new
                {
                    transactionId,
                    payeeName,
                    amount = Amount,
                    expiresAt,
                    qrCodeUrl
                }
            };
        }
    }

    /// <summary>POST /api/v1/payment/:id/paid on the QR tunnel provider.</summary>
    public static async Task<(bool Success, int StatusCode, string Body)> NotifyPaidAsync(
        HttpClient client,
        QrCodeTunnelOptions options,
        string transactionId,
        ILogger logger)
    {
        if (!options.IsValid() || string.IsNullOrWhiteSpace(transactionId))
            return (false, 400, """{"error":"Invalid request"}""");

        var url = $"{options.BaseUrl.TrimEnd('/')}/api/v1/payment/{Uri.EscapeDataString(transactionId.Trim())}/paid";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-API-Key", options.ApiKey);
        request.Headers.Add("X-API-Secret", options.ApiSecret);

        logger.LogInformation("QrCodeTunnel paid notify POST {Url}", url);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QrCodeTunnel paid notify HTTP failed for {TransactionId}", transactionId);
            return (false, 0, JsonConvert.SerializeObject(new { error = ex.Message }));
        }

        var body = await response.Content.ReadAsStringAsync();
        logger.LogInformation(
            "QrCodeTunnel paid notify {TransactionId} status {StatusCode}: {Body}",
            transactionId,
            (int)response.StatusCode,
            body);

        return (response.IsSuccessStatusCode, (int)response.StatusCode, body);
    }
}
