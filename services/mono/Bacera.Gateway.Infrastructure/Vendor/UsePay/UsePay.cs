using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.DTO;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Vendor.UsePay;

public class UsePay
{
    public sealed class RequestClient
    {
        public long TenantId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public string ReturnUrl { get; set; } = "";
        public UsePayOptions Options { get; set; } = new();
        public ILogger Logger { get; set; } = null!;

        private string? GenerateCipherText()
        {
            var orderInfo = new Dictionary<string, string>
            {
                { "amount", $"{Amount:0.00}" },
                { "payid", Options.PayId },
                { "requestid", PaymentNumber.Replace("pm-", "") },
                { "callfront", ReturnUrl },
                { "callback", $"{Options.CallbackDomain}/{TenantId}/usepay" },
                { "customize1", TenantId.ToString() }
            };

            var orderInfoJson = JsonConvert.SerializeObject(orderInfo);
            var rsaHelper = new RSAHelper(Options.MerchantPublicKey, null, 2048, RSAHelper.SecretKeyModel.PEM);
            var ciphertext = rsaHelper.PublicKeyEncrypt(orderInfoJson);
            return ciphertext;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            var ciphertext = GenerateCipherText();
            if (ciphertext == null)
                return DepositCreatedResponseModel.Fail("Failed to generate ciphertext.");

            var form = new Dictionary<string, string>
            {
                { "access_token", Options.MerchantNo },
                { "ciphertext", ciphertext }
            };
            //
            // var content = new FormUrlEncodedContent(form);
            // using var client = new HttpClient();
            // var response = await client.PostAsync(Options.EndPoint, content);
            // var body = await response.Content.ReadAsStringAsync();

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Post,
                Form = form,
                EndPoint = Options.EndPoint,
                PaymentNumber = PaymentNumber
            };
        }
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public Dictionary<string, string> Form { get; set; } = new();
        public string Html { get; set; } = "";
        public string EndPoint { get; set; } = "";
        public string Message { get; set; } = "";

        public static ResponseModel BuildWithFailed(string message)
            => new()
            {
                Message = message,
                IsSuccess = false
            };

        public static ResponseModel BuildWithSuccess(Dictionary<string, string> form, string json, string endPoint)
            => new()
            {
                IsSuccess = true,
                Html = json,
                EndPoint = endPoint,
                Form = form
            };
    }
}

// public Dictionary<string, string> BuildForm()
// {
// var form = new Dictionary<string, string>
// {
//     { "payid", Options.PayId },
//     { "requestid", PaymentNumber },
//     { "amount", $"{Amount:0.00}" },
//     { "callfront", ReturnUrl },
//     { "callback", $"{Options.CallbackDomain}/{TenantId}/usepay" },
// };
//
//
// return form;
// }