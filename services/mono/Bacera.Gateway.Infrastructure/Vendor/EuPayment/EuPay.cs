using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.GPay;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Vendor.EuPayment;

public class EuPay
{
    public static async Task<object> TestAsync(HttpClient httpClient, string config, ILogger logger)
    {
        var options = EuPayOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 2000,
            AccountUid = 123456,
            ReturnUrl = "https://example.com/return",
            PaymentNumber = Payment.GenerateNumber(),
            Email = "it@bacera.com",
            Ip = "137.25.19.180",
            Request = new EuPayRequestModel
            {
                BillingLastName = "Smith",
                BillingFirstName = "John",
                BillingAddress = "Ricardo Street",
                BillingCity = "San Francisco",
                BillingState = "CA",
                BillingZipcode = "94115",
                BillingCountry = "US",
                BillingPhone = "64-4844-8654",
                CcNumber = "4444333322221111",  // 16 digits - doc test card for Direct Process
                CcMonth = "06",
                CcYear = "2027",
                CcCvc = "123"
            },
            Options = options,
            Logger = logger,
            Client = httpClient
        };

        var response = await client.RequestAsync();
        return response;
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public long AccountUid { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string PaymentNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public EuPayRequestModel Request { get; set; } = null!;
        public EuPayOptions Options = new();
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;
        /// <summary>Strip quotes/spaces and keep only digits; gateway requires pure numeric values.</summary>
        private static string SanitizeNumeric(string? value) =>
            string.IsNullOrEmpty(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

        private Dictionary<string, string> BuildForm() =>
            new()
            {
                { "account_id", Options.AccountId },
                { "account_password", Options.Password },
                { "action_type", Options.ActionType },
                { "account_gateway", Options.AccountGateway },
                { "merchant_payment_id", PaymentNumber },
                { "cust_email", Email },
                { "cust_billing_last_name", Request.BillingLastName },
                { "cust_billing_first_name", Request.BillingFirstName },
                { "cust_billing_address", Request.BillingAddress },
                { "cust_billing_city", Request.BillingCity },
                { "cust_billing_zipcode", Request.BillingZipcode },
                { "cust_billing_state", Request.BillingState },
                { "cust_billing_country", Request.BillingCountry },
                { "cust_billing_phone", Request.BillingPhone },
                { "transac_products_name", $"Deposit:{AccountUid}" },
                { "transac_amount", $"{Amount:00}" },
                { "transac_currency_code", Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? "USD" },
                { "customer_ip", Ip },
                { "merchant_url_return", ReturnUrl },
                { "merchant_url_callback", Options.CallbackUrl },
                { "transac_cc_number", SanitizeNumeric(Request.CcNumber) },
                { "transac_cc_month", SanitizeNumeric(Request.CcMonth).PadLeft(2, '0') },
                { "transac_cc_year", SanitizeNumeric(Request.CcYear) },
                { "transac_cc_cvc", SanitizeNumeric(Request.CcCvc) },
            };


        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            if (string.IsNullOrWhiteSpace(Options.AccountId) || string.IsNullOrWhiteSpace(Options.Password) || string.IsNullOrWhiteSpace(Options.PassPhrase))
                return DepositCreatedResponseModel.Fail("EuPayment is not configured. Set AccountId, Password, and PassPhrase in payment method Configuration.", true);

            var form = BuildForm();
            // Logger.LogInformation("EuPay.SendRequestAsync.form: {form}", form);

            var shaString = Options.PassPhrase
                            + form["transac_amount"]
                            + form["account_id"]
                            + form["cust_email"]
                            + form["transac_cc_number"]
                            + form["customer_ip"];
            var accountSha = Utils.Sha256Hash(shaString);
            Logger.LogInformation("EuPay.SendRequestAsync.account_sha: {sha}", accountSha);
            form.Add("account_sha", accountSha);
            // Logger.LogInformation("EuPay.SendRequestAsync.shaedForm: {form}", form);

            var endpoint = !string.IsNullOrWhiteSpace(Options.Endpoint)
                ? Options.Endpoint
                : EuPayOptions.DefaultEndpoint;
            var response = await Client.PostAsync(endpoint, new FormUrlEncodedContent(form));
            var body = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("EuPay.SendRequestAsync.response: {body}", body);
            var obj = Utils.JsonDeserializeDynamic(body);
            if (obj.success != true) return DepositCreatedResponseModel.Fail("Failed to create deposit", true);

            var status = (string)obj.data.resp_trans_status;
            var description = (string)obj.data.resp_trans_description_status;
            BcrLog.Slack($"Eupay_error: {status} - {description} - AccountUid:{AccountUid}");
            var detail = (string)obj.data.resp_trans_detailled_status;
            if (status != "00000") return DepositCreatedResponseModel.Fail($"eupay_{status}_{detail}", true);

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.None,
                PaymentNumber = PaymentNumber
            };
        }
    }
}