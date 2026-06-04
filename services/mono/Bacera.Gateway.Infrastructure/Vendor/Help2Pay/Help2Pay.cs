using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Help2Pay;

public class Help2Pay
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = Help2PayOptions.FromJson(config);
        var obj = new
        {
            currency = (int)CurrencyTypes.INR,
            frontUri = "https://www.google.com",
        };
        var (result, message) = EnsureRequest(obj);
        if (!result) return message;
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));

        var client = new RequestClient
        {
            Amount = 120000,
            AccountUid = 123456789,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.frontUri,
            Currency = request.currency,
            Ip = "47.241.6.29",
            Language = "en-us",
            Bank = "",
            Options = options,
            Logger = logger,
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public class RequestClient
    {
        // public string MerchantCode { get; set; } = null!;
        public CurrencyTypes Currency { get; set; } = CurrencyTypes.Invalid;
        public long AccountUid { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.AddHours(8); // base on UTC+8 Hong Kong
        public string? Bank { get; set; }
        public string Language { get; set; } = LanguageTypes.English;
        public string Ip { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public Help2PayOptions Options { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        public HttpClient? Client { get; set; }

        private string SignSignature()
            => Utils.Md5Hash($"{Options.MerchantCode}" +
                             $"{PaymentNumber}" +
                             $"{AccountUid}" +
                             $"{Amount:0.00}" +
                             $"{Enum.GetName(Currency)}" +
                             $"{CreatedOn:yyyyMMddHHmmss}" +
                             $"{Options.SecurityCode}" +
                             $"{Ip}");


        public Dictionary<string, string> BuildForm()
            => new()
            {
                { "Merchant", Options.MerchantCode },
                { "Currency", Enum.GetName(Currency)! },
                { "Customer", AccountUid.ToString() },
                { "Reference", PaymentNumber },
                { "Amount", Amount.ToString("0.00") },
                { "Datetime", CreatedOn.ToString("yyyy-MM-dd HH:mm:sstt") },
                { "FrontURI", ReturnUrl },
                { "BackURI", Options.CallbackUri },
                { "Bank", Bank ?? "" },
                { "Language", Language },
                { "ClientIP", Ip },
            };

        public async Task<ResponseModel> SendAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("Help2Pay Request: {form}", form);
            var key = SignSignature();
            Logger.LogInformation("Help2Pay Key: {key}", key);
            form.Add("Key", key);
            // var content = new FormUrlEncodedContent(form);
            // using var client = new HttpClient();
            // var response = await client.PostAsync(Options.EndPoint, content);
            // var body = await response.Content.ReadAsStringAsync();
            // return !response.IsSuccessStatusCode
            //     ? new ResponseModel { IsSuccess = false }
            //     : ResponseModel.FromJson(Options.EndPoint, form, body);
            return ResponseModel.FromJson(Options.EndPoint, form, "");
        }

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("Help2Pay Request: {form}", form);
            var key = SignSignature();
            Logger.LogInformation("Help2Pay Key: {key}", key);
            form.Add("Key", key);
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
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public Dictionary<string, string> Form { get; set; } = new();
        public string RedirectUrl { get; set; } = null!;

        public static ResponseModel FromJson(string endPoint, Dictionary<string, string> form, string body)
            => new()
            {
                IsSuccess = true,
                RedirectUrl = endPoint,
                Form = form,
            };
    }

    public static (bool, string) ValidateCallbackSpec(Dictionary<string, string> spec, string securityCode)
    {
        var requiredKeys = new[] { "Merchant", "Reference", "Customer", "Amount", "Currency", "Status", "Key" };
        var missedKeys = requiredKeys.Where(x => !spec.ContainsKey(x)).ToList();
        if (missedKeys.Count != 0) return (false, $"Missing required keys: {string.Join(", ", missedKeys)}");

        var baseString = spec["Merchant"] + spec["Reference"] + spec["Customer"] + spec["Amount"] + spec["Currency"] + spec["Status"] + securityCode;
        var calculatedKey = Utils.Md5Hash(baseString).ToUpper();
        var result = calculatedKey == spec["Key"];
        if (!result)
        {
            BcrLog.Slack($"Help2Pay Callback Validate Signature Failed: {JsonConvert.SerializeObject(spec)}");
        }

        return (result, result ? string.Empty : "Invalid key");
    }

    public static (bool, string) EnsureRequest(object obj)
    {
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));
        if (request == null) return (false, "Invalid request object");

        var currency = request.currency != null ? (CurrencyTypes)request.currency : CurrencyTypes.Invalid;
        if (currency == CurrencyTypes.Invalid) return (false, "Invalid currency type");

        if (request.frontUri == null && request.returnUrl == null) return (false, "Invalid returnUrl or frontUri");
        return (true, string.Empty);
    }

    public sealed class RequestSupplement
    {
        [Required] public CurrencyTypes Currency { get; set; }
        [Required] public string FrontUri { get; set; } = null!;
        [Required] public string ReturnUrl { get; set; } = null!;

        public string GetReturnUrl() => string.IsNullOrEmpty(ReturnUrl) ? FrontUri : ReturnUrl;

        public string? GetBank()
        {
            if (!QrCode) return null;
            return Currency switch
            {
                CurrencyTypes.MYR => "5",
                CurrencyTypes.THB => "1",
                CurrencyTypes.VND => "4",
                CurrencyTypes.IDR => "6",
                CurrencyTypes.PHP => "7",
                CurrencyTypes.INR => "2",
                _ => null
            };
        }

        public string ValidationMessage { get; set; } = string.Empty;
        public bool QrCode { get; set; } = false;

        public static bool TryParse(string json, out RequestSupplement request)
        {
            request = new RequestSupplement();
            try
            {
                request = JsonConvert.DeserializeObject<RequestSupplement>(json)!;
                return true;
            }
            catch (Exception e)
            {
                request.ValidationMessage = e.Message;
                return false;
            }
        }
    }
}