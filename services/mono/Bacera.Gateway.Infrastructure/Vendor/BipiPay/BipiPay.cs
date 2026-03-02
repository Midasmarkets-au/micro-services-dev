using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Vendor.BipiPay;

public class BipiPay
{
    public static async Task<object> TestAsync(HttpClient httpClient, string config, ILogger logger)
    {
        var options = BipiPayOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 5000,
            UsdAmount = 0,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = "www.google.com",
            CurrencyId = CurrencyTypes.CNY,
            NativeName = "強大人",
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

        public string NativeName { get; set; } = "";

        // public long AccountUid { get; set; }
        public CurrencyTypes CurrencyId { get; set; } = CurrencyTypes.CNY;
        public HttpClient? Client { get; set; }

        private string Currency => CurrencyId switch
        {
            CurrencyTypes.CNY => "RMB",
            CurrencyTypes.IDR => "IDR",
            CurrencyTypes.VND => "VND",
            CurrencyTypes.THB => "THB",
            CurrencyTypes.MYR => "MYR",
            _ => "RMB"
        };

        public BipiPayOptions Options { get; set; } = new();

        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "txcode", PaymentNumber },
                { "merchant", Options.MerchantId },
                { "customer", NativeName },
                { "currency", Currency },
                // { "paymethod", PayMethod },
                { "amount", $"{Amount:0.00}" },
                { "usdamount", $"{UsdAmount:0.00}" }
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
            SignForm(form);
            // var list = form.Select(item => item.Key + "=" + item.Value).ToList();
            // parse the form to a query string
            // var query = Uri.EscapeDataString(string.Join("&", form.Select(item => $"{item.Key}={item.Value}")));
            var query = string.Join("&", form.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
            var url = Options.EndPoint + "?" + query;

            if (isTest)
            {
                // using var client = new HttpClient();
                var dd = await Client!.GetAsync(url);
                var ddBody = await dd.Content.ReadAsStringAsync();
                Console.WriteLine(ddBody);
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = url,
                PaymentNumber = PaymentNumber
            };
        }
    }
}