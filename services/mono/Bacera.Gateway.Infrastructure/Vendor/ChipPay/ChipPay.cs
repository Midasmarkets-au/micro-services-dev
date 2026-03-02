using System.Text;
using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ChipPay;

public class ChipPay
{
    public sealed class RequestClient
    {
        public CurrencyTypes CurrencyId { get; set; }
        public long Amount { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public string PaymentNumber { get; set; } = string.Empty;
        public HttpClient Client { get; set; } = null!;
        public ChipPayOptions Options = new();

        private Dictionary<string, string> BuildForm()
        {
            var form = CurrencyId != CurrencyTypes.VND
                ? new Dictionary<string, string>
                {
                    { "companyId", Options.MerchantId },
                    { "companyOrderNum", PaymentNumber },
                    { "totalAmount", Amount.ToString() },
                    { "syncUrl", Options.CallbackUri },
                    { "asyncUrl", !string.IsNullOrEmpty(RedirectUrl) ? RedirectUrl : Options.CallbackUri }
                }
                : new Dictionary<string, string>
                {
                    { "companyId", Options.MerchantId },
                    { "kyc", "2" },
                    { "total", Amount.ToString() },
                    { "phone", Options.Phone },
                    {
                        "coinAmount", "0"
                    }, // BigDecimal USDT下单数字货币数量 coinAmount参数换算后法币 金额若不为整数将无条件进位为 整数显示于收银台 (USDT下单数字货币数量(coinAmount和 total 两个字段二选一，当两个字段都填写的时候，优先处理total)) VND        
                    { "orderType", "1" }, // Integer 订单类型1、买单 2、卖单  VND
                    { "companyOrderNum", PaymentNumber },
                    { "coinSign", "USDT" },
                    { "payCoinSign", "VND" },
                    {
                        "orderPayChannel", Options.PaymentChannel == "BankCard" ? "3" : "1"
                    }, // Integer if VND, 1: Momo, 3: BankCard    
                    {
                        "orderTime",
                        ((long)Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds, 0)).ToString()
                    }, // 订单时间戳（使用当前时间戳，与当前时间相差5分钟视为无效）
                    { "syncUrl", Options.CallbackUri },
                    { "asyncUrl", !string.IsNullOrEmpty(RedirectUrl) ? RedirectUrl : Options.CallbackUri }
                };
            return form.Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        private Dictionary<string, string> SignForm()
        {
            var form = BuildForm();

            var url = form
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((x, y) => $"{x}&{y}");

            url = url.Trim('&');
            var signature = Utils.SignSha265HashWithPkcs8PrivateKey(url, Options.PrivateKey);
            form.Add("sign", signature);
            return form;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = SignForm();
            var str = JsonConvert.SerializeObject(form);
            var content = new StringContent(str, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.EndPoint, content);
            if (!response.IsSuccessStatusCode) return DepositCreatedResponseModel.Fail();
            var json = await response.Content.ReadAsStringAsync();
            var obj = Utils.JsonDeserializeObjectWithDefault<dynamic>(json);
            return new DepositCreatedResponseModel
            {
                Action = PaymentResponseActionTypes.Redirect,
                IsSuccess = obj.success == true,
                RedirectUrl = obj.data?.link ?? string.Empty,
                Reference = obj.data?.intentOrderNo ?? string.Empty,
                PaymentNumber = PaymentNumber
            };
        }
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

        public string? RedirectUrl { get; set; }

        public string? Message { get; set; }

        public static ResponseModel FromJson(string json)
        {
            var obj = Utils.JsonDeserializeObjectWithDefault<dynamic>(json);
            var response = new ResponseModel
            {
                IsSuccess = obj.success == true,
                RedirectUrl = obj.data?.link ?? string.Empty,
                ReferenceNumber = obj.data?.intentOrderNo ?? string.Empty
            };
            return response;
        }
    }

    public static bool ValidateCallbackDict(Dictionary<string, string> dict, string publicKey)
    {
        var sign = dict["sign"];
        var url = dict
            .Where(x => x.Key != "sign")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={x.Value}")
            .Aggregate((x, y) => $"{x}&{y}");
        url = url.Trim('&');
        return Utils.VerifySignatureForSha265Hash(url, sign, publicKey);
    }
}