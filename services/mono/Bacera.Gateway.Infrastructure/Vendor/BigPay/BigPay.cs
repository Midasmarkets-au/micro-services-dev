using System.Globalization;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.BigPay.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.BigPay;

public class BigPay
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = BigPayOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 80000,
            PaymentNumber = Payment.GenerateNumber(),
            AccountId = 32410525,
            ReturnUrl = "www.google.com",
            CancelUrl = "www.google.com",
            Options = options,
            Logger = logger
        };

        var response = await client.RequestAsync();
        return response;
    }
    
    public sealed class RequestClient
    {
        public long AccountId { get; set; }
        public long Amount { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public BigPayOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;

        public ILogger Logger { get; set; } = null!;

        public object BuildRequestBody() => new
        {
            @ref = $"{PaymentNumber}",
            mt5Id = $"{AccountId}",
            amount = Amount,
            callback = $"{Options.CallbackUri}",
        };

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildRequestBody();
            Logger.LogInformation("BigPay.RequestAsync.form: {form}", form);
            var json = JsonConvert.SerializeObject(form);

            Client.DefaultRequestHeaders.Add("X-API-Key", Options.ApiKey);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.EndPoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            Logger.LogInformation("BigPay.RequestAsync.response: {responseJson}", responseJson);
            if (!response.IsSuccessStatusCode) return DepositCreatedResponseModel.Fail(responseJson);

            var responseModel = Utils.JsonDeserializeObjectWithDefault<ResponseModel>(responseJson);
            var redirectUrl = $"{Options.RedirectPrefix}/?order={responseModel.HashId}";
            Logger.LogInformation("BigPay.RequestAsync.redirectUrl: {redirectUrl}", redirectUrl);
            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = redirectUrl,
                PaymentNumber = PaymentNumber
            };
        }
    }


    public sealed class ResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public string HashId { get; set; } = string.Empty;
        public string Ref { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

        public bool IsSuccess { get; set; }
        public object? Response { get; private set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        public string? RedirectUrl { get; set; }
        public string? Message { get; set; }
    }

    public static object CallbackSuccessResponse() => new
    {
        code = 200,
        msg = "success",
        success = true
    };

    public static object CallbackFailureResponse(string msg) => new
    {
        code = -2,
        msg,
        data = "",
        success = false
    };
}