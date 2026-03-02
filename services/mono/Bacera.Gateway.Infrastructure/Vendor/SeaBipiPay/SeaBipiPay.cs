using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Vendor.SeaBipiPay;

public class SeaBipiPay
{
    public static async Task<object> TestAsync(HttpClient httpClient, string config, ILogger logger)
    {
        var options = SeaBipiPayOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 1500,
            UsdAmount = 0,
            // PaymentNumber = Payment.GenerateNumber(),
            PaymentNumber = "TX3888",
            ReturnUrl = "www.google.com",
            CurrencyId = CurrencyTypes.THB,
            AccountUid = 10001,
            Options = options,
            Client = httpClient
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public decimal UsdAmount { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string PayMethod { get; set; } = "def";
        public long AccountUid { get; set; }
        public CurrencyTypes CurrencyId { get; set; } = CurrencyTypes.CNY;
        public SeaBipiPayOptions Options { get; set; } = new();
        public HttpClient? Client { get; set; }

        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "txcode", PaymentNumber },
                { "merchant", Options.MerchantId },
                { "customer", AccountUid.ToString() },
                { "currency", $"{Enum.GetName(CurrencyId)}" },
                { "paymethod", PayMethod },
                // { "paymethod", $"{Amount:0.00}" },
                // { "amount", $"{Amount:0.00}" },
                { "amount", $"{Amount}" },
                // { "usdamount", $"{UsdAmount:0.00}" }
            };

        public void SignForm(Dictionary<string, string> form)
        {
            var url = form
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");

            url += Options.MerchantSecret;
            var signature = Utils.Sha512Hash(url);
            form.Add("signed", signature);
        }

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);
            var form = BuildForm();

            var baseString = form
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                // .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}")
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((x, y) => $"{x}&{y}");

            var withSecretAppended = baseString + Options.MerchantSecret;

            var signature = Utils.Sha512Hash(withSecretAppended);
            var query = baseString + "&signed=" + signature;
            // convert it to hex string
            var url = Options.EndPoint + "?" + query;

            if (isTest)
            {
                var dd = await Client!.GetAsync(url);
                var ddBody = await dd.Content.ReadAsStringAsync();
                Console.WriteLine(ddBody);
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Get,
                EndPoint = url,
                PaymentNumber = PaymentNumber
            };
        }
    }
}