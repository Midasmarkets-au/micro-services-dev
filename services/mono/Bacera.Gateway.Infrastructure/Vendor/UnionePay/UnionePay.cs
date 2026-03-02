using System.Text;
using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;

namespace Bacera.Gateway.Vendor.UnionePay;

public static class UnionePay
{
    public class RequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public long TargetAccountUid { get; set; }
        public string IdNumber { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public UnionePayOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;

        private Dictionary<string, string> BuildAndSignForm()
        {
            var form = new Dictionary<string, string>
            {
                { "merchantNo", Options.MerchantId },
                { "orderNo", PaymentNumber },
                { "currency", Options.Currency },
                { "amount", $"{Amount:0.00}" },
                { "payTime", DateTime.UtcNow.ToString("yyyyMMddHHmmss") },
                { "renderType", Options.RenderType.ToString() },
                { "payChannel", Options.PaymentChannel.ToString() },
                { "additionalInfo", UrlValueEncoder($"{NativeName}&{IdNumber}") }
            };

            var aggregatedString = form
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Where(x => x.Key != "renderType")
                .Where(x => x.Key != "payChannel")
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((x, y) => $"{x}&{y}");
            aggregatedString += Options.MerchantKey;
            var sign = Utils.Md5Hash(aggregatedString).ToLower();
            form.Add("signed", sign);
            return form;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            // using var client = new HttpClient();
            var form = BuildAndSignForm();
            // var response = await client.PostAsync(Options.Endpoint, new FormUrlEncodedContent(form));
            // var body = await response.Content.ReadAsStringAsync();
            // var obj = Utils.JsonDeserializeDynamic(body);
            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Post,
                Form = form,
                EndPoint = Options.Endpoint,
                PaymentNumber = PaymentNumber
            };
        }
    }
    
    

    public sealed class Response
    {
        // {"success":true,"payUrl":"https://gateway.wangming.digital/D20240306123748001110"}
        public bool IsSuccess { get; set; }

        public string Error { get; set; } = string.Empty;

        public Dictionary<string, string> Form { get; set; } = new();
        public string EndPoint { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;

        public static Response FromJson(string json, Dictionary<string, string> form, string endPoint)
        {
            var obj = Utils.JsonDeserializeDynamic(json);
            var response = new Response
            {
                IsSuccess = obj.success == true,
                RedirectUrl = obj?.payUrl ?? string.Empty,
                Error = UrlValueDecoder(obj?.error?.ToString() ?? string.Empty),
                Form = form,
                EndPoint = endPoint
            };
            return response;
        }
    }

    public sealed class CallbackRequest
    {
        public string OrderNo { get; set; } = string.Empty;
        public string MerchantNo { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;

        public string AdditionalInfo { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public decimal RealAmount { get; set; }

        public string PayTime { get; set; } = string.Empty;

        public string PayNo { get; set; } = string.Empty;

        public string Result { get; set; } = string.Empty; // "OK" or "Fail"


        public string Error { get; set; } = string.Empty;

        public string Signed { get; set; } = string.Empty;

        public static CallbackRequest FromJson(string json) =>
            Utils.JsonDeserializeObjectWithDefault<CallbackRequest>(json);


        public UnionePayOptions Options { get; set; } = new();

        public bool ValidateSign()
        {
            var form = new Dictionary<string, string>
            {
                { "orderNo", OrderNo },
                { "merchantNo", MerchantNo },
                { "additionalInfo", AdditionalInfo },
                { "currency", Currency },
                { "amount", $"{Amount:0.00}" },
                { "realAmount", $"{RealAmount:0.00}" },
                { "payTime", PayTime },
                { "payNo", PayNo },
                { "result", Result },
                { "error", Error },
                { "signed", Signed },
            };

            var aggregatedString = form
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Where(x => x.Key != "signed")
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((x, y) => $"{x}&{y}");
            aggregatedString += Options.MerchantKey;
            var sign = Utils.Md5Hash(aggregatedString).ToLower();
            return Signed == sign;
        }
    }

    private static string UrlValueDecoder(string value)
    {
        try
        {
            return HttpUtility.UrlDecode(HttpUtility.UrlDecode(value, Encoding.UTF8), Encoding.UTF8);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string UrlValueEncoder(string value)
    {
        try
        {
            return HttpUtility.UrlEncode(HttpUtility.UrlEncode(value, Encoding.UTF8), Encoding.UTF8);
        }
        catch
        {
            return string.Empty;
        }
    }
}