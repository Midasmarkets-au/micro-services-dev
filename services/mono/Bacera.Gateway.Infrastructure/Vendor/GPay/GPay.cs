using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Vendor.GPay;

public class GPay
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = GPayOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 1900,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = "www.google.com",
            Ip = "192.168.1.1",
            NativeName = "",
            Options = options,
            Logger = logger,
        };

        var response = await client.RequestAsync();
        return response;
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string PaymentNumber { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public GPayOptions Options = new();
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;

        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "bank_id", "" },
                { "amount", $"{Amount:0.00}" },
                { "company_id", Options.MerchantId },
                { "company_order_num", PaymentNumber },
                { "company_user", NativeName },
                { "estimated_payment_bank", "" },
                { "group_id", "0" },
                { "web_url", ReturnUrl },
                { "memo", PaymentNumber },
                { "note", PaymentNumber },
                { "note_model", "2" },
                { "deposit_mode", Options.MethodId.ToString() },
                { "terminal", "1" },
                { "client_ip", Ip }
            };

        private Dictionary<string, string> Sign(Dictionary<string, string> form)
        {
            var baseString = Utils.Md5Hash(Options.MerchantSecret).ToLower()
                             + form["company_id"]
                             + form["bank_id"]
                             + form["amount"]
                             + form["company_order_num"]
                             + form["company_user"]
                             + form["estimated_payment_bank"]
                             + form["deposit_mode"]
                             + form["group_id"]
                             + form["web_url"]
                             + form["memo"]
                             + form["note"]
                             + form["note_model"]
                ;
            var key = Utils.Md5Hash(baseString).ToLower();
            form.Add("key", key);
            return form;
        }
        
        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildForm();
            Logger.LogInformation("GPay.SendRequestAsync.form: {form}", form);
            form = Sign(form);
            Logger.LogInformation("GPay.SendRequestAsync.signedForm: {form}", form);

            var query = string.Join("&", form.Select(item => Uri.EscapeDataString(item.Key) + "=" + Uri.EscapeDataString(item.Value)));
            var url = Options.EndPoint + "?" + query;
            Logger.LogInformation("GPay.SendRequestAsync.url: {url}", url);
            var response = await Client.PostAsync(url, null);
            var body = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeDynamic(body);
            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = obj["break_url"]?.ToString() ?? string.Empty,
                Reference = obj["mownecum_order_num"]?.ToString() ?? string.Empty,
                PaymentNumber = PaymentNumber
            };
        }
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public string RedirectUrl { get; set; } = "";
        public string ReferenceNumber { get; set; } = "";
        public string Message { get; set; } = "";

        public static ResponseModel Build(string content)
        {
            var obj = Utils.JsonDeserializeDynamic(content);
            var response = new ResponseModel();
            if (obj["status"] == null || obj["status"] != "1")
            {
                response.IsSuccess = false;
                return response;
            }

            response.IsSuccess = true;
            response.ReferenceNumber = obj["mownecum_order_num"]?.ToString() ?? string.Empty;
            response.RedirectUrl = obj["break_url"]?.ToString() ?? string.Empty;
            return response;
        }
    }

    public static bool CallbackValidator(Dictionary<string, string> spec, string secretKey)
    {
        if (!spec.TryGetValue("key", out var key))
            return false;
        if (!spec.TryGetValue("amount", out _) && !long.TryParse(spec["amount"], out _))
            return false;
        var keys = new[]
        {
            "pay_time", "bank_id", "amount", "company_order_num", "mownecum_order_num", "pay_card_num", "pay_card_name",
            "channel", "area", "fee", "transaction_charge", "deposit_mode"
        };
        var values = keys
            .Where(x => spec.GetValueOrDefault(x, "null") != "null")
            .Select(x => spec.GetValueOrDefault(x, ""))
            .Aggregate("", (current, value) => current + value)
            .ToLower();
        var baseString = Utils.Md5Hash(Utils.Md5Hash(secretKey).ToLower() + values).ToLower();
        return key == baseString;
    }
}