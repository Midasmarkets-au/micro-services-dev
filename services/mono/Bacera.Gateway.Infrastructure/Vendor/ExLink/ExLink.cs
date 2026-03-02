using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ExLink.Models;

public class ExLink
{
    public static async Task<object> TestAsync(HttpClient client, string config, ILogger logger)
    {
        var options = ExLinkOptions.FromJson(config);

        var request = new RequestClient
        {
            UniqueCode = Utils.GenerateUniqueId().ToString(),
            PaymentNumber = Payment.GenerateNumber(),
            Amount = 2000,
            PayerName = "东阿法",
            Options = options,
            Client = client,
        };

        var response = await request.RequestAsync();
        return response;
    }

    public class RequestClient
    {
        public string UniqueCode { get; set; } = null!;
        public string PaymentNumber { get; set; } = null!;
        public long Amount { get; set; }
        public string PayerName { get; set; } = "";
        public ExLinkOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;


        private Dictionary<string, string> BuildForm()
        {
            var form = new Dictionary<string, string>
            {
                { "uid", Options.Uid.ToString() },
                { "uniqueCode", UniqueCode },
                { "payType", Options.PayType.ToString() },
                { "orderId", PaymentNumber },
                { "money", Amount.ToString() },
                { "payerName", PayerName }
            };
            return form;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildForm();
            var signature = GenerateSignature(form, Options.SecretKey);
            form.Add("signature", signature);

            var content = new StringContent(JsonConvert.SerializeObject(form), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.RequestUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeObjectWithDefault<dynamic>(json);
            return new DepositCreatedResponseModel
            {
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = obj.data ?? "",
                IsSuccess = obj.success == true,
                PaymentNumber = PaymentNumber
            };
        }
    }

    public sealed class DFRequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public string Currency { get; set; } = "";
        public string BankName { get; set; } = null!;
        public string BankBranchName { get; set; } = null!;
        public string BankNumber { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string BankCode { get; set; } = null!;
        public string NotifyUrl { get; set; } = null!;
        public ExLinkOptions Options { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;


        private Dictionary<string, string> BuildForm()
            => new()
            {
                { "uid", Options.Uid.ToString() },
                { "merchantOrderNo", PaymentNumber },
                { "currencyCoinName", Currency },
                { "channelCode", "channelCode!!!!!!!!!" },
                { "money", Amount.ToString(CultureInfo.InvariantCulture) },
                { "bankCode", BankCode },
                { "bankName", BankName },
                { "bankBranchName", BankBranchName },
                { "bankUserName", AccountName },
                { "bankNumber", BankNumber },
                { "memo", "memo!!!!!!!!!" },
            };

        public async Task<PayoutResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            var signature = GenerateSignature(form, Options.SecretKey);
            form.Add("signature", signature);

            var content = new StringContent(JsonConvert.SerializeObject(form), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.RequestUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeObjectWithDefault<dynamic>(json);
            return new PayoutResponseModel
            {
                IsSuccess = obj.success == true,
                Form = form,
                Action = PaymentResponseActionTypes.None,
            };
        }
    }

    private static string GenerateSignature(Dictionary<string, string> spec, string secretKey, ILogger? logger = null)
    {
        var aggregatedString = spec
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .Where(x => x.Key != "signature")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={x.Value}")
            .Aggregate((x, y) => $"{x}&{y}");

        logger?.LogInformation("ExLink.GenerateSignature: {aggregatedString}", aggregatedString);

        aggregatedString = aggregatedString.Trim() + $"&key={secretKey}";
        logger?.LogInformation("ExLink.GenerateSignature: {aggregatedString}", aggregatedString);

        return Utils.Md5Hash(aggregatedString).ToLower();
    }

    public static bool ValidateCallbackSignature(Dictionary<string, string> spec, string callbackSecretKey)
    {
        if (!spec.TryGetValue("signature", out var signature))
            return false;

        var calculatedSignature = GenerateSignature(spec, callbackSecretKey);
        return calculatedSignature == signature;
    }
}