using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PaymentAsia;

public sealed class PaymentAsiaRMB
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = PaymentAsiaRMBOptions.FromJson(config);
        var obj = new
        {
            returnSuccessUrl = "https://www.google.com",
            returnFailUrl = "https://www.google.com",
            email = "dealing@bacera.com",
            nativeName = "Bacera Test",
        };
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));

        var client = new RequestClient
        {
            Amount = 1000,
            PaymentNumber = Payment.GenerateNumber(),
            Language = "en-us",
            Email = request.email,
            ReturnUrl = request.returnSuccessUrl,
            ReturnFailUrl = request.returnFailUrl,
            NativeName = request.nativeName,
            RmbOptions = options,
            Logger = logger,
        };

        var response = await client.SendAsync();
        return response;
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public string Language { get; set; } = "en";
        public string ReturnUrl { get; set; } = "";
        public string ReturnFailUrl { get; set; } = "";
        public string NativeName { get; set; } = "";
        public string Email { get; set; } = "";

        public PaymentAsiaRMBOptions RmbOptions { get; set; } = new();
        public ILogger Logger { get; set; } = null!;

        private Dictionary<string, string> BuildForm()
            => new()
            {
                { "merchant_id", RmbOptions.MerchantId },
                { "success_url", ReturnUrl },
                { "fail_url", string.IsNullOrEmpty(ReturnFailUrl) ? ReturnUrl : ReturnFailUrl },
                { "merchant_ref", $"{RmbOptions.MerchantRefKey}_{PaymentNumber}" },
                { "amount", $"{Amount:0.00}" },
                { "currency", RmbOptions.Currency },
                { "customer_name", NativeName },
                // { "customer_name", "李善均" },
                { "customer_email", Email },
                { "customer_tel", RmbOptions.CustomerTel },
                { "lang", Language == "en-us" ? "en" : "sc" },
                { "gateway", RmbOptions.Gateway },
                { "charset", RmbOptions.CharSet },
                { "sign_type", RmbOptions.SignType },
            };

        public async Task<ResponseModel> SendAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("PaymentAsia.SendRequestAsync.form: {form}", form);
            var signature = GenerateSignature(form, RmbOptions.SecurityKey);
            Logger.LogInformation("PaymentAsia.SendRequestAsync.signature: {signature}", signature);
            form.Add("sign_msg", signature);
            
            return ResponseModel.FromJson(RmbOptions.EndPoint, form, "");

            // var content = new FormUrlEncodedContent(form);
            // using var client = new HttpClient();
            // var response = await client.PostAsync(RmbOptions.EndPoint, content);
            // var body = await response.Content.ReadAsStringAsync();
            // return !response.IsSuccessStatusCode
            //     ? new ResponseModel { IsSuccess = false }
            //     : ResponseModel.FromJson(RmbOptions.EndPoint, form, body);
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("PaymentAsia.SendRequestAsync.form: {form}", form);
            var signature = GenerateSignature(form, RmbOptions.SecurityKey);
            Logger.LogInformation("PaymentAsia.SendRequestAsync.signature: {signature}", signature);
            form.Add("sign_msg", signature);

            // var content = new FormUrlEncodedContent(form);
            // using var client = new HttpClient();
            // var response = await client.PostAsync(RmbOptions.EndPoint, content);
            // var body = await response.Content.ReadAsStringAsync();

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Form = form,
                Action = PaymentResponseActionTypes.Post,
                EndPoint = RmbOptions.EndPoint,
                PaymentNumber = PaymentNumber
            };

            // var content = new FormUrlEncodedContent(form);
            // using var client = new HttpClient();
            // var response = await client.PostAsync(RmbOptions.EndPoint, content);
            // var body = await response.Content.ReadAsStringAsync();
            // return !response.IsSuccessStatusCode
            //     ? new ResponseModel { IsSuccess = false }
            //     : ResponseModel.FromJson(RmbOptions.EndPoint, form, body);
        }
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public Dictionary<string, string> Form { get; set; } = new();
        public string RedirectUrl { get; set; } = "";
        public string Html { get; set; } = "";

        public static ResponseModel FromJson(string url, Dictionary<string, string> form, string html)
            => new()
            {
                IsSuccess = true,
                Form = form,
                RedirectUrl = url,
                Html = html,
            };
    }

    private static string GenerateSignature(Dictionary<string, string> form, string key)
    {
        var baseString = form
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={x.Value}")
            .Aggregate((x, y) => $"{x}&{y}");
        baseString += $"&security_key={key}";
        var signature = Utils.Sha512Hash(baseString).ToLower();
        return signature;
    }

    public static (bool, string) EnsureRequest(object obj)
    {
        // ensure it has ReturnSuccessUrl, ReturnFailUrl, Email, NativeName, PhoneNumber in dynamic
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));
        if (request.returnSuccessUrl == null)
            return (false, "returnSuccessUrl is required");
        // if (request.returnFailUrl == null)
        //     return (false, "returnFailUrl is required");
        // if (request.email == null)
        //     return (false, "email is required");
        // if (request.nativeName == null)
        //     return (false, "nativeName is required");
        return (true, "");
    }
}