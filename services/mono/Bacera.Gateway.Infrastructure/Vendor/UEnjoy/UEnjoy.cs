using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor;

public class UEnjoy
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = UEnjoyOptions.FromJson(config);
        var request = new RequestClient
        {
            Amount = 5000,
            PaymentNumber = Payment.GenerateNumber(),
            Logger = logger,
            Options = options,
            ReturnUrl = "https://www.google.com"
        };

        var response = await request.RequestAsync(true);
        logger.LogInformation("UEnjoy.TestAsync.response: {response}", response);
        return response;
    }

    public sealed class RequestClient
    {
        public UEnjoyOptions Options { get; set; } = new();

        public long Amount { get; set; }
        public long TenantId { get; set; }
        public string PaymentNumber { get; set; } = "";
        public string ReturnUrl { get; set; } = "";
        public string KycName { get; set; } = null!;
        public string UserAreaCode { get; set; } = null!;
        public string UserMobile { get; set; } = null!;

        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;

        private Dictionary<string, string> BuildForm()
            => new()
            {
                { "apiKey", Options.ApiKey },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") },
                { "signVersion", Options.SignVersion },
                { "currency", Options.Currency },
                { "legalCurrency", Options.LegalCurrency },
                { "userAreaCode", UserAreaCode },
                { "userMobile", UserMobile },
                { "kycName", KycName },
                { "language", "zh-cn" },
                { "kycLevel", Options.KycLevel },
                { "callbackUrl", $"{Options.CallbackDomain}/payment/callback/{TenantId}/uenjoy" },
                { "redirectUrl", ReturnUrl },
                { "amount", $"{Amount:0.00}" },
                { "orderNo", PaymentNumber },
            };

        private Dictionary<string, string> BuildFormTest()
            => new()
            {
                { "apiKey", Options.ApiKey },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") },
                { "signVersion", Options.SignVersion },
                { "currency", Options.Currency },
                { "legalCurrency", Options.LegalCurrency },
                { "userAreaCode", "86" },
                { "userMobile", "18171242125" },
                { "kycName", "滕磊" },
                { "language", "zh-cn" },
                { "kycLevel", Options.KycLevel },
                { "callbackUrl", $"{Options.CallbackDomain}/payment/callback/{TenantId}/uenjoy" },
                { "redirectUrl", ReturnUrl },
                { "amount", $"{Amount:0.00}" },
                { "orderNo", PaymentNumber },
            };

        private Dictionary<string, string> SignForm(Dictionary<string, string> form)
        {
            var signKeys = new[] { "apiKey", "timestamp", "signVersion" };
            var aggregatedString = form
                .Where(x => signKeys.Contains(x.Key))
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((x, y) => $"{x}&{y}");

            var sign = Uri.EscapeDataString(aggregatedString);
            using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(Options.SecretKey));
            var bytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(sign));
            sign = Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes)));
            sign = Uri.EscapeDataString(sign);
            form.Add("sign", sign);
            return form;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync(bool test = false)
        {
            var form = test ? BuildFormTest() : BuildForm();
            Logger.LogInformation("UEnjoy.SendRequestAsync.form: {form}", form);
            form = SignForm(form);
            Logger.LogInformation("UEnjoy.SendRequestAsync.sign: {sign}",
                form.GetValueOrDefault("sign", "Failed to get sign."));
            var content = new StringContent(JsonConvert.SerializeObject(form), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.EndPoint, content);
            var body = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeDynamic(body);
            return new DepositCreatedResponseModel
            {
                IsSuccess = obj.code == 0,
                Message = obj.msg,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = obj.code == 0 ? obj.data?.url ?? "" : "",
                PaymentNumber = PaymentNumber
            };
        }
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public string RedirectUrl { get; set; } = "";

        public static ResponseModel Build(string content)
        {
            var obj = Utils.JsonDeserializeDynamic(content);
            var response = new ResponseModel();
            if (obj.code != 0)
            {
                response.IsSuccess = false;
                response.Message = obj.msg;
                return response;
            }

            response.IsSuccess = true;
            response.RedirectUrl = obj.data?.url ?? "";
            return response;
        }
    }

    public sealed class KycInfos
    {
        public string NativeName { get; set; } = "";
        public string UserAreaCode { get; set; } = "";
        public string UserMobile { get; set; } = "";

        public static KycInfos FromDynamic(dynamic obj)
        {
            return new KycInfos
            {
                NativeName = obj.nativeName ?? "",
                UserAreaCode = obj.userAreaCode ?? "",
                UserMobile = obj.userMobile ?? ""
            };
        }

        public static KycInfos FromDict(Dictionary<string, string> dict)
            => Utils.JsonDeserializeObjectWithDefault<KycInfos>(Utils.JsonSerializeObject(dict));

        public bool IsValid()
            => !string.IsNullOrWhiteSpace(NativeName) && !string.IsNullOrWhiteSpace(UserAreaCode) &&
               !string.IsNullOrWhiteSpace(UserMobile);
    }
}