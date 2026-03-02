using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PaymentAsia;

public partial class DragonPayPHP
{
    public static async Task<object> TestAsync(HttpClient cc, string config, ILogger logger)
    {
        var options = DragonPayPHPOptions.FromJson(config);
        var obj = new
        {
            returnSuccessUrl = "www.google.com",
            email = "someone@gmail.com"
        };
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));

        var client = new RequestClient
        {
            Amount = 43464,
            PaymentNumber = Payment.GenerateNumber(),
            // Email = request.email,
            // Email = "kay.wu+shopreward@bacera.com",
            Email = "kay.wu@bacera.com",
            // Email = "",
            FirstName = "Kay Shop",
            LastName = "Reward",
            Phone = "16267662856",
            IpAddress = "137.25.19.180",
            ReturnUrl = "http://client.localhost/dfdf/dfdf",
            // ReturnSuccessUrl = request.returnSuccessUrl,
            Options = options,
            Logger = logger,
            Client = cc,
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public sealed partial class RequestClient
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
        public DragonPayPHPOptions Options { get; set; } = null!;
        public HttpClient? Client { get; set; }

        private Dictionary<string, string> BuildForm()
            => new()
            {
                { "merchant_reference", PaymentNumber },
                { "currency", Options.Currency },
                { "amount", $"{Amount:0.00}" },
                { "customer_ip", IpAddress },
                { "customer_first_name", FirstName },
                { "customer_last_name", LastName },
                { "customer_phone", Phone },
                { "customer_email", EmailPlusRegex().Replace(Email, "") },
                { "return_url", ExtractHostname(ReturnUrl) },
                { "network", Options.Network },
            };

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("DragonPay.SendRequestAsync.form: {form}", form);
            Console.WriteLine($"key: {Options.MerchantSecret}");
            var signature = GenerateSignature(form, Options.MerchantSecret);
            Logger.LogInformation("DragonPay.SendRequestAsync.signature: {signature}", signature);
            form.Add("sign", signature);

            if (isTest)
            {
                var content = new FormUrlEncodedContent(form);
                var response = await Client!.PostAsync(Options.EndPoint, content);
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine("body => " + body);
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

        [GeneratedRegex("\\+[^@]*")]
        private static partial Regex EmailPlusRegex();


        [GeneratedRegex(":\\d+")]
        private static partial Regex HostRemoveRegex();

        [GeneratedRegex("^https?://([^:/]+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex HostNameRegex();

        private static string ExtractHostname(string url)
        {
            var match = HostNameRegex().Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return url; // 如果没有匹配，返回原始字符串
        }
    }
}