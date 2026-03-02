using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.Poli.Models;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Poli;

public class PoliPay
{
    public sealed class RequestClient
    {
        private const string InitiateEndpoint = "Transaction/Initiate";

        public decimal Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; } = CurrencyTypes.AUD;
        public string PaymentNumber { get; set; } = null!;
        public string ReturnUrl { get; set; } = string.Empty;

        public string MerchantData { get; set; } = string.Empty;
        public string MerchantHomepageUrl => ReturnUrl;
        public string FailureUrl => ReturnUrl;
        public string CancellationUrl => ReturnUrl;
        public string NotificationUrl => ReturnUrl;
        public string SelectedFiCode { get; set; } = string.Empty;
        public string Token { get; private set; } = string.Empty;
        public int Timeout { get; set; } = 900;
        public PoliOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;


        public Dictionary<string, string> BuildForm()
            => new()
            {
                { "amount", $"{Amount:0.00}" },
                { "currencyCode", Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? "AUD" },
                { "merchantReference", PaymentNumber },
                { "merchantData", MerchantData },
                { "merchantHomepageURL", MerchantHomepageUrl },
                { "successURL", ReturnUrl },
                { "failureURL", FailureUrl },
                { "cancellationURL", CancellationUrl },
                { "notificationURL", NotificationUrl },
                { "selectedFICode", SelectedFiCode },
                { "token", Token },
                { "timeout", Timeout.ToString() }
            };

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Options.MerchantCode}:{Options.SecurityCode}"));
            var form = BuildForm();
            var json = JsonConvert.SerializeObject(form);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // using var httpClient = new HttpClient();
            Client.BaseAddress = new Uri(Options.EndPoint);
            Client.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");
            var response = await Client.PostAsync(InitiateEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                return new DepositCreatedResponseModel
                {
                    IsSuccess = false,
                    Message = response.StatusCode.ToString(),
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeDynamic(responseContent);
            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = obj.navigateURL ?? string.Empty,
                Reference = obj.transactionRefNo ?? string.Empty,
                PaymentNumber = PaymentNumber
            };
        }
    }
}