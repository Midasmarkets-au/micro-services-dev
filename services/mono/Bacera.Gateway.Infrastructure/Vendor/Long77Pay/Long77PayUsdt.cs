using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayUsdt
{
    public sealed class RequestClient
    {
        public Long77PayOptions Options { get; set; } = new();
        public long Amount { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string PaymentNumber { get; set; } = string.Empty;

        public long TargetAccountUid { get; set; }
        public HttpClient Client { get; set; } = null!;

        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "partner_id", Options.MerchantId },
                { "timestamp", Utils.ToTimestamp(DateTime.UtcNow).ToString() },
                { "random", Guid.NewGuid().ToString() },
                { "partner_order_code", PaymentNumber.Replace("-", "_") },
                { "amount", $"{Amount:0.00}" },
                { "return_url", ReturnUrl },
                { "notify_url", Options.CallbackUri },
                { "order_currency", "0" },
                { "order_language", "en_ww" },
                { "guest_id", Utils.Md5Hash("BCR" + TargetAccountUid) },
                { "extra_data", string.Empty }
            };

        private void SignForm(Dictionary<string, string> form)
        {
            var url = form["partner_id"] + ":"
                                         + form["timestamp"] + ":"
                                         + form["random"] + ":"
                                         + form["partner_order_code"] + ":"
                                         + form["order_currency"] + ":"
                                         + form["order_language"] + ":"
                                         + form["guest_id"] + ":"
                                         + form["amount"] + ":"
                                         + form["notify_url"] + ":"
                                         + form["return_url"] + ":"
                                         + form["extra_data"] + ":"
                                         + Options.MerchantSecret;

            var signature = Utils.Md5Hash(url);
            form.Add("sign", signature.ToLower());
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildForm();
            SignForm(form);
            var content = new StringContent(JsonConvert.SerializeObject(form), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.Endpoint, content);
            if (!response.IsSuccessStatusCode) return DepositCreatedResponseModel.Fail("Failed to make payment");
            var responseJson = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeDynamic(responseJson);
            return new DepositCreatedResponseModel
            {
                IsSuccess = obj.code == 200,
                Action = PaymentResponseActionTypes.Redirect,
                Message = (string)obj.SelectToken("msg") ?? null,
                RedirectUrl = (string)obj.SelectToken("data.payment_url") ?? null,
                Reference = (string)obj.SelectToken("data.system_order_code") ?? string.Empty,
                PaymentNumber = PaymentNumber
            };
        }
    }
}