using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services;
using Force.Crc32;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PayPal;

public class PayPal(PayPalOptions options, ILogger logger, HttpClient client)
{
    public PayPalOptions Options { get; set; } = options;
    public ILogger Logger { get; set; } = logger;
    public HttpClient Client { get; set; } = client;

    public static async Task<object> TestAsync(HttpClient cc, string config, ILogger logger)
    {
        var options = PayPalOptions.FromJson(config);

        var client = new RequestClient
        {
            Amount = 1000,
            PaymentNumber = Payment.GenerateNumber(),
            AccountUid = 500000009,
            ReturnUrl = "https://www.google.com",
            CancelUrl = "https://www.google.com",
            CurrencyId = CurrencyTypes.USD,
            Options = options,
            Client = cc,
            Logger = logger,
        };

        var response = await client.RequestAsync();
        return response;
    }

    public async Task<string?> GetTokenAsync()
    {
        var authString = $"{Options.ClientId}:{Options.ClientSecret}";
        var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

        // Create the request content
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        ]);

        // Make the request
        var response = await Client.PostAsync(Options.TokenEndPoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            // Logger.LogError("PayPal.GetTokenAsync: {response}", response);
            BcrLog.Slack($"PayPal.GetTokenAsync.Error: {responseContent}");
            return null;
        }

        var token = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent)!;
        return token["access_token"];
    }

    public async Task<PayPalResponseModel?> CreateOrderAsync(PayPalRequestModel form)
    {
        var token = await GetTokenAsync();
        if (token == null) return null;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonConvert.SerializeObject(form);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync(Options.CheckOutEndPoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            BcrLog.Slack($"PayPal.CreateOrderAsync.Error: {responseContent}");
            return null;
        }

        return PayPalResponseModel.FromJson(responseContent);
    }

    public async Task<PayPalResponseModel?> CaptureOrderAsync(string orderId, string? captureUrl = null)
    {
        var token = await GetTokenAsync();
        if (token == null) return null;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        captureUrl ??= $"{Options.CheckOutEndPoint}/{orderId}/capture";
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await Client.PostAsync(captureUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        Logger.LogInformation("PayPal_CaptureOrderAsync: {responseContent}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            BcrLog.Slack($"PayPal_CaptureOrderAsync_Error: {responseContent}");
            return null;
        }

        return PayPalResponseModel.FromJson(responseContent);
    }

    public async Task<PayPalOrderResponseModel?> GetOrderAsync(string orderId)
    {
        var token = await GetTokenAsync();
        if (token == null) return null;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"{Options.CheckOutEndPoint}/{orderId}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            BcrLog.Slack($"PayPal.GetOrderAsync.Error: {responseContent}");
            return null;
        }

        return PayPalOrderResponseModel.FromJson(responseContent);
    }


    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = "";
        public long AccountUid { get; set; }
        public string ReturnUrl { get; set; } = "";
        public string CancelUrl { get; set; } = "";
        public CurrencyTypes CurrencyId { get; set; }
        public PayPalOptions Options { get; set; } = new();
        public ILogger Logger { get; set; } = null!;

        public HttpClient Client { get; set; } = null!;

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("PayPal.SendRequestAsync.form: {form}", form);

            var service = new PayPal(Options, Logger, Client);
            var token = await service.GetTokenAsync();
            if (token == null) return DepositCreatedResponseModel.Fail("Failed to get token");

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(form);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync(Options.CheckOutEndPoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                BcrLog.Slack($"PayPal.RequestAsync.Error: {responseContent}");
                return DepositCreatedResponseModel.Fail(responseContent);
            }

            var model = PayPalResponseModel.FromJson(responseContent);
            var result = new DepositCreatedResponseModel
            {
                Action = PaymentResponseActionTypes.PayPal,
                IsSuccess = true,
                PaymentNumber = PaymentNumber,
                Form = model,
            };
            

            return result;
        }

        private PayPalRequestModel BuildForm() => new()
            {
                Intent = "CAPTURE",
                PurchaseUnits =
                [
                    new PurchaseUnit
                    {
                        ReferenceId = PaymentNumber,
                        CustomId = AccountUid.ToString(),
                        Amount = new Amount
                        {
                            Value = Amount.ToString("0.00"),
                            CurrencyCode = Enum.GetName(CurrencyId) ?? "USD",
                        }
                    }
                ],
                // PaymentSource = new PaymentSource
                // {
                //     Paypal = new PayPalSource
                //     {
                //         ExperienceContext = new ExperienceContext
                //         {
                //             BrandName = Options.BrandName,
                //             ReturnUrl = ReturnUrl,
                //             CancelUrl = CancelUrl,
                //         }
                //     }
                //
                //     // Card = new CardSource
                //     // {
                //     //     AllowedPaymentMethods = ["DEBIT"],
                //     //     StoredCredential = new StoredCredential
                //     //     {
                //     //         PaymentInitiator = "MERCHANT",
                //     //         PaymentType = "ONE_TIME",
                //     //         Usage = "FIRST",
                //     //         ReturnUrl = ReturnUrl,
                //     //         CancelUrl = CancelUrl,
                //     //     }
                //     // }
                // }
            };
    }

    public sealed class WebhookRequestDetails
    {
        public string TransmissionId { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public string Signature { get; set; } = "";
        public string CertUrl { get; set; } = "";
        public string RawBody { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public string GetMessage()
        {
            var bodyBytes = Encoding.UTF8.GetBytes(RawBody);
            var crc32Value = Crc32Algorithm.Compute(bodyBytes);
            return $"{TransmissionId}|{Timestamp}|{WebHookId}|{crc32Value}";
        }

        public bool IsValidRequest() => TransmissionId != "" && Timestamp != "" && Signature != "" &&
                                        CertUrl != "" && RawBody != "";

        public static async Task<WebhookRequestDetails> FromRequestAsync(HttpRequest request)
        {
            try
            {
                var transmissionId = request.Headers["paypal-transmission-id"].FirstOrDefault()!;
                var timestamp = request.Headers["paypal-transmission-time"].FirstOrDefault()!;
                var signatureBase64 = request.Headers["paypal-transmission-sig"].FirstOrDefault()!;
                var certUrl = request.Headers["paypal-cert-url"].FirstOrDefault()!;
                var body = await new StreamReader(request.Body).ReadToEndAsync();
                
                return new WebhookRequestDetails
                {
                    TransmissionId = transmissionId,
                    Timestamp = timestamp,
                    Signature = signatureBase64,
                    CertUrl = certUrl,
                    RawBody = body,
                };
            }
            catch (Exception e)
            {
                return new WebhookRequestDetails
                {
                    ErrorMessage = e.Message,
                };
            }
        }
    }

    public const string CertPemCacheKey = "paypal-webhook-cert.pem";
    private const string WebHookId = "1Y782058YN068522L";

    public static bool VerifyWebhookRequestAsync(WebhookRequestDetails details, string certPem)
    {
        var signature = Convert.FromBase64String(details.Signature);

        var certBytes = Encoding.ASCII.GetBytes(certPem);
        using var cert = new X509Certificate2(certBytes);

        // Verify signature
        using var rsa = cert.GetRSAPublicKey();
        if (rsa == null)
            return false;

        var messageBytes = Encoding.UTF8.GetBytes(details.GetMessage());

        var result = rsa.VerifyData(
            messageBytes,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        return result;
    }
}