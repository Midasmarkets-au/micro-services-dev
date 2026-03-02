using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Buy365;

public class NPay
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = NPayOptions.FromJson(config);
    
        var request = new RequestClient
        {
            Amount = 5000,
            PaymentNumber = Payment.GenerateNumber(),
            AccountUid = 500000009,
            NativeName = "东阿发",
            Options = options,
            Logger = logger,
        };
    
        var response = await request.RequestAsync();
        return response;
    }

    
    public sealed class RequestClient
    {
        public NPayOptions Options = new();
        public long AccountUid { get; set; }
        public string NativeName { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private Dictionary<string, string> BuildForm()
            => new Dictionary<string, string>
                {
                    { "sys_no", Options.MerchantId },
                    { "user_id", AccountUid.ToString() },
                    { "order_amount", Amount.ToString() },
                    { "order_ip", Options.ServerIp },
                    { "order_time", DateTime.UtcNow.ToString("yyyyMMddHHmmss") },
                    { "order_id", PaymentNumber },
                    { "pay_user_name", NativeName.Trim() }
                }
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            form.Add("sign", RequestSignature(form, Options.MerchantSecret));
            Logger.LogWarning("NPay_Request_Form {@Form}", form);

            var formContent = new FormUrlEncodedContent(form);
            var response = await Client.PostAsync(Options.Endpoint, formContent);
            var json = await response.Content.ReadAsStringAsync();
            Logger.LogWarning("Payment request result {@Result}", json);
            if (!response.IsSuccessStatusCode)
            {
                BcrLog.Slack($"NPay_request_failed: {json}");
                return DepositCreatedResponseModel.Fail("Payment request failed");
            }

            var model = ResponseModel.FromJson(json);
            
            if (!model.IsSuccess)
            {
                BcrLog.Slack($"NPay_request_failed: {model.Message}");
                return DepositCreatedResponseModel.Fail(model.Message);
            }
            
            return new DepositCreatedResponseModel
            {
                IsSuccess = model.IsSuccess,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = model.RedirectUrl,
                PaymentNumber = PaymentNumber
            };
        }
    }


    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        private string BaseUrl { get; set; } = string.Empty;

        private string AccountUid { get; set; } = string.Empty;


        public string RedirectUrl => string.IsNullOrEmpty(BaseUrl)
            ? string.Empty
            : $"{BaseUrl}?in_order_id={ReferenceNumber}&user_id={AccountUid}";

        public string? Message { get; set; }


        public static ResponseModel FromJson(string json)
        {
            var obj = Utils.JsonDeserializeDynamic(json);
            var item = new ResponseModel
            {
                IsSuccess = obj.status == "success",
                Message = obj.msg
            };
            if (!item.IsSuccess) return item;
            item.BaseUrl = obj.data?.send_url ?? string.Empty;
            item.AccountUid = obj.data?.user_id ?? string.Empty;
            item.ReferenceNumber = obj.data?.order_no ?? string.Empty;
            return item;
        }
    }

    public static bool ValidateCallbackSignature(Dictionary<string, string> spec, string callbackSecretKey)
    {
        if (!spec.TryGetValue("sign", out var sign)) return false;
        return sign == CallbackSignature(spec, callbackSecretKey);
    }

    private static string RequestSignature(Dictionary<string, string> form, string secretKey)
    {
        var baseString = form
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}")
            .Aggregate((x, y) => $"{x}&{y}");

        baseString = baseString.Trim('&') + secretKey;
        var signature = Utils.Md5Hash(baseString).ToLower();
        return signature;
    }

    private static string CallbackSignature(Dictionary<string, string> form, string secretKey)
    {
        var fieldsNotToSign = new[] { "sign", "amount", "amount_usdt", "sys_no" };
        var baseString = form
            .Where(x => !fieldsNotToSign.Contains(x.Key) && !string.IsNullOrEmpty(x.Value))
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => Uri.EscapeDataString(x.Value))
            .Aggregate((x, y) => $"{x}&{y}");

        baseString = baseString.Trim('&') + secretKey;
        var signature = Utils.Md5Hash(baseString).ToLower();
        return signature;
    }
}