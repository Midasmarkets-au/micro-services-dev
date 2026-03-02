using System.Text.RegularExpressions;
using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Bakong;

public partial class Bakong
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = BakongOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 60,
            // PaymentNumber = Payment.GenerateNumber(),
            // PaymentNumber = "pm-24443124f767",
            PaymentNumber = "pm-24465945d458",
            FirstName = "Vichit",
            LastName = "Chum",
            Phone = "884310256",
            Email = "chumvichit007@gmail.com",
            ReturnUrl = "thebcr.com",
            IpAddress = "202.150.3.169",
            Logger = logger,
            Options = options,
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public partial class RequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public string ReturnUrl { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public ILogger Logger { get; set; } = null!;
        public CurrencyTypes CurrencyId { get; set; } = CurrencyTypes.USD;

        public BakongOptions Options { get; set; } = null!;
        public HttpClient? Client { get; set; }

        private Dictionary<string, string> BuildForm()
            => new()
            {
                { "merchant_reference", PaymentNumber },
                { "currency", Enum.GetName(CurrencyId)! },
                { "amount", $"{Amount:0.00}" },
                { "customer_ip", IpAddress == "::1" ? "192.168.1.1" : IpAddress },
                { "customer_first_name", FirstName },
                { "customer_last_name", LastName },
                { "customer_phone", Phone },
                { "customer_email", EmailPlusRegex().Replace(Email, "") },
                { "return_url", ExtractHostname(ReturnUrl) },
                // { "return_url", "http://client.localhost/dfdf/dfdf" },
                { "network", Options.Network },
            };

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("Bakong_SendRequestAsync_form: {form}", form);
            Console.WriteLine($"key: {Options.MerchantSecret}");
            var signature = GenerateSignature(form, Options.MerchantSecret);
            Logger.LogInformation("Bakong_SendRequestAsync_signature: {signature}", signature);
            form.Add("sign", signature);

            if (isTest)
            {
                var content = new FormUrlEncodedContent(form);
                var response = await Client!.PostAsync(Options.EndPoint, content);
                var body = await response.Content.ReadAsStringAsync();
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Form = form,
                Action = PaymentResponseActionTypes.Post,
                EndPoint = Options.EndPoint,
                PaymentNumber = PaymentNumber
            };
        }

        private static string ExtractHostname(string url)
        {
            var match = HostNameRegex().Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return url; // 如果没有匹配，返回原始字符串
        }

        [GeneratedRegex("\\+[^@]*")]
        private static partial Regex EmailPlusRegex();

        [GeneratedRegex("^https?://([^:/]+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex HostNameRegex();
    }

    private static string GenerateSignature(Dictionary<string, string> form, string key)
    {
        // 创建一个 NameValueCollection 并添加非空的参数
        var queryStringCollection = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kvp in form.OrderBy(x => x.Key, StringComparer.Ordinal)
                     .Where(kvp => !string.IsNullOrEmpty(kvp.Value)))
        {
            queryStringCollection[kvp.Key] = kvp.Value;
        }

        var queryString = queryStringCollection.ToString();

        Console.WriteLine("queryString => " + queryString);

        // 拼接密钥
        var baseString = queryString + key;
        Console.WriteLine("baseString => " + baseString);

        // 生成 SHA512 哈希
        var signature = Utils.Sha512Hash(baseString).ToLower();
        return signature;
    }

    public static (bool, string) ValidateCallbackSpec(Dictionary<string, string> spec, string key)
    {
        var requiredKeys = new[] { "merchant_reference", "request_reference", "currency", "amount", "status", "sign" };
        var missedKeys = requiredKeys.Where(x => !spec.ContainsKey(x)).ToList();
        if (missedKeys.Count != 0) return (false, $"Missing required keys: {string.Join(", ", missedKeys)}");

        var dictToSign = new Dictionary<string, string>
        {
            { "merchant_reference", spec["merchant_reference"] },
            { "request_reference", spec["request_reference"] },
            { "currency", spec["currency"] },
            { "amount", spec["amount"] },
            { "status", spec["status"] },
        };

        var signature = GenerateSignature(dictToSign, key);

        var result = signature == spec["sign"];
        if (!result)
        {
            BcrLog.Slack($"Bakong Callback Validate Signature Failed: {JsonConvert.SerializeObject(spec)}");
        }

        return (result, result ? string.Empty : "Invalid sign");
    }
}