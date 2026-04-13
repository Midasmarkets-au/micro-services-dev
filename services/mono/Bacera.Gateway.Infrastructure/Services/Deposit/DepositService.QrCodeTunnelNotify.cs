using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.QrCodeTunnel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Services;

public partial class DepositService
{
    /// <summary>
    /// Verifies the current party owns a QrCodeTunnel payment with this <paramref name="transactionId"/>
    /// (<c>Payment.ReferenceNumber</c>), then calls the provider <c>POST /api/v1/payment/:id/paid</c>.
    /// Audits the HTTP response in <see cref="Payment.CallbackBody"/>.
    /// </summary>
    public async Task<QrTunnelNotifyPaidResult> NotifyQrCodeTunnelPaidAsync(long partyId, string transactionId)
    {
        if (partyId <= 0)
            return new QrTunnelNotifyPaidResult { Success = false, HttpStatusCode = 401, Message = "Unauthorized" };

        if (string.IsNullOrWhiteSpace(transactionId))
            return new QrTunnelNotifyPaidResult { Success = false, HttpStatusCode = 400, Message = "Transaction id required" };

        transactionId = transactionId.Trim();

        var payment = await tenantCtx.Payments
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.PartyId == partyId
                && p.ReferenceNumber == transactionId
                && p.PaymentMethod.Platform == (int)PaymentPlatformTypes.QrCodeTunnel);

        if (payment == null)
        {
            return new QrTunnelNotifyPaidResult
            {
                Success = false,
                HttpStatusCode = 404,
                Message = "Transaction not found or not in pending status"
            };
        }

        var options = QrCodeTunnelOptions.FromJson(payment.PaymentMethod.Configuration);
        if (!options.IsValid())
        {
            return new QrTunnelNotifyPaidResult
            {
                Success = false,
                HttpStatusCode = 400,
                Message = "Invalid QrCodeTunnel configuration"
            };
        }

        var client = clientFactory.CreateClient();
        var (ok, statusCode, body) = await QrCodeTunnel.NotifyPaidAsync(client, options, transactionId, logger);

        try
        {
            JObject jo;
            if (string.IsNullOrWhiteSpace(payment.CallbackBody))
                jo = new JObject();
            else
                jo = JObject.Parse(payment.CallbackBody);

            jo["qrTunnelPaidNotifiedAt"] = DateTime.UtcNow;
            jo["qrTunnelPaidNotifyHttpStatus"] = statusCode;
            jo["qrTunnelPaidNotifyResponseRaw"] = body;
            payment.CallbackBody = jo.ToString(Formatting.None);
            payment.UpdatedOn = DateTime.UtcNow;
            await tenantCtx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "NotifyQrCodeTunnelPaidAsync: failed to persist audit for payment {PaymentId}",
                payment.Id);
        }

        if (ok)
        {
            return new QrTunnelNotifyPaidResult
            {
                Success = true,
                HttpStatusCode = statusCode,
                ResponseBody = body
            };
        }

        var msg = body;
        try
        {
            var err = JsonConvert.DeserializeObject<dynamic>(body);
            if (err?.error != null)
                msg = (string)err.error;
        }
        catch
        {
            // keep raw body
        }

        return new QrTunnelNotifyPaidResult
        {
            Success = false,
            HttpStatusCode = statusCode,
            Message = msg,
            ResponseBody = body
        };
    }
}
