using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Pay247;

public class Pay247
{
    public static async Task<object> TestAsync(HttpClient client, string config, ILogger logger)
    {
        var options = Pay247Options.FromJson(config);
        var request = new RequestClient
        {
            Amount = 100000,
            AccountUid = 325150104,
            PaymentNumber = Payment.GenerateNumber(),
            CurrencyId = CurrencyTypes.VND,
            Logger = logger,
            Options = options,
            Client = client,
            ReturnUrl = "https://www.google.com"
        };

        var response = await request.RequestAsync();
        logger.LogInformation("Pay247_TestAsync_response: {response}", response);
        return response;
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public long AccountUid { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string PaymentNumber { get; set; } = string.Empty;
        public CurrencyTypes CurrencyId { get; set; }
        public Pay247Options Options { get; set; } = new();
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;


        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "mch_id", Options.MerchantId },
                { "mch_order_no", PaymentNumber },
                { "mch_user_id", AccountUid.ToString() },
                { "currency", Enum.GetName(CurrencyId)! },
                { "amount", $"{Amount:0.00}" },
                // { "pay_method", Options.PayMethod },
                { "notify_url", Options.CallbackUrl },
                { "return_url", ReturnUrl },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "version", Options.Version },
                { "uuid", Guid.NewGuid().ToString() },
            };

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildForm();
            Logger.LogInformation("Pay247_RequestAsync_form: {form}", form);

            if (form["currency"] == "VND")
                form["pay_method"] = "BANK";
            
            var signature = GenerateSignature(form, Options.SecretKey);
            form.Add("sign", signature);
            Logger.LogInformation("Pay247_RequestAsync_signed_form: {form}", form);
            
            var content = new StringContent(JsonConvert.SerializeObject(form), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.GetPayinUrl(), content);
            var body = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Pay247_RequestAsync_body: {body}", body);

            var obj = Utils.JsonDeserializeDynamic(body);
            var isSuccess = (int)obj.code == 0 && (string)obj.message == "success";
            var payUrl = isSuccess ? (string?)obj.data?.pay_url : null;
            var message = (string)obj.message;

            return new DepositCreatedResponseModel
            {
                IsSuccess = isSuccess,
                Message = message,
                PaymentNumber = PaymentNumber,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = payUrl,
            };
        }
    }

    public static async Task<object> TestPayoutAsync(HttpClient client, string config, ILogger logger)
    {
        // VND,THB,PHP
        var options = Pay247Options.FromJson(config);
        var request = new PayoutRequestClient
        {
            Amount = 6000,
            AccountName = "东阿法",
            BankNumber = "4444333322221111",
            PaymentNumber = Payment.GenerateNumber(),
            BankCode = "GCASH",
            Currency = "PHP",
            Logger = logger,
            Options = options,
            Client = client,
        };

        var response = await request.RequestAsync();
        logger.LogInformation("Pay247out_TestAsync_response: {response}", response);
        return response;
    }

    public sealed class PayoutRequestClient
    {
        public decimal Amount { get; set; }
        public string AccountName { get; set; } = null!;
        public string BankNumber { get; set; } = null!;
        public string PaymentNumber { get; set; } = null!;
        public string BankCode { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public Pay247Options Options { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;

        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "mch_id", Options.MerchantId },
                { "mch_order_no", PaymentNumber },
                { "currency", Currency },
                { "amount", $"{Amount:0.00}" },
                { "pay_method", Options.PayMethod },
                { "account_name", AccountName },
                { "account_no", BankNumber },
                { "notify_url", Options.CallbackUrl },
                { "bank_code", BankCode },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "version", Options.Version },
                { "uuid", Guid.NewGuid().ToString() },
            };

        public async Task<PayoutResponseModel> RequestAsync()
        {
            var form = BuildForm();
            Logger.LogInformation("Pay247out_RequestAsync_form: {form}", form);
            var signature = GenerateSignature(form, Options.SecretKey);
            form.Add("sign", signature);
            Logger.LogInformation("Pay247out_RequestAsync_signed_form: {form}", form);

            var content = new StringContent(JsonConvert.SerializeObject(form), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.GetPayinUrl(), content);
            var body = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Pay247out_RequestAsync_body: {body}", body);

            var obj = Utils.JsonDeserializeDynamic(body);
            var isSuccess = (int)obj.code == 0;
            var message = (string)obj.message;

            return new PayoutResponseModel
            {
                IsSuccess = isSuccess,
                ResponseJson = body,
                Message = $"{message}",
                Form = obj,
                Action = PaymentResponseActionTypes.None,
            };
        }
    }

    public static string GenerateSignature(Dictionary<string, string> form, string secretKey)
    {
        var queryString = form
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={x.Value}")
            .Aggregate((x, y) => $"{x}&{y}");

        Console.WriteLine($"Pay247_GenerateSignature_queryString: {queryString}");
        var baseString = queryString + secretKey;
        Console.WriteLine($"Pay247_GenerateSignature_baseString: {baseString}");

        return Utils.Md5Hash(baseString);
    }

    public sealed class CreatePayoutSpec
    {
        public long WithdrawalId { get; set; }
        public string BankCode { get; set; } = null!;
        public CurrencyTypes CurrencyId { get; set; }
    }
}