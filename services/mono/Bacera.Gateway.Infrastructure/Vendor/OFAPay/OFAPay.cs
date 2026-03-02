using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.OFAPay;

public class OFAPay
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = OFAPayOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 15756,
            PartyUid = 51463502,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = "https://portal.thebcr.com/portal/",
            NativeName = "強大人",
            Options = options,
            Logger = logger,
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public sealed class DFRequestClient
    {
        public decimal Amount { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string BankNumber { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string BankCode { get; set; } = null!;
        public string NotifyUrl { get; set; } = null!;
        public OFAPayOptions Options { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;


        private Dictionary<string, string> BuildForm()
            => new()
            {
                { "scode", Options.MerchantId },
                { "orderid", PaymentNumber },
                { "money", $"{Amount:0.00}" },
                { "bankname", BankName },
                { "accountno", BankNumber },
                { "accountname", AccountName },
                { "bankno", BankCode },
                { "notifyurl", NotifyUrl },
            };

        public async Task<PayoutResponseModel> RequestAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("OFAPay.DFRequestClient.form: {form}", form);
            var signature = GenerateSignature(form, Options.SecretKey, Logger);
            Logger.LogInformation("OFAPay.DFRequestClient.signature: {signature}", signature);
            form.Add("sign", signature);


            var json = JsonConvert.SerializeObject(form);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(Options.EndPoint, content);
            var body = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("OFAPay.RequestAsync.body: {body}", body);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(body)!;
            var isSuccess = dict["prc"] == "1" && dict["errcode"] == "00";
            var msg = dict["msg"];
            if (!isSuccess) return PayoutResponseModel.Fail(msg);
            
            var orderNo = dict.GetValueOrDefault("orderno", "");
            var result = new PayoutResponseModel
            {
                IsSuccess = true,
                Message = $"{dict["msg"]}, Order No.: {orderNo}",
                Form = dict,
                Action = PaymentResponseActionTypes.None,
            };

            return result;
        }
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public long PartyUid { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public string ReturnUrl { get; set; } = "";
        public string NativeName { get; set; } = "";

        public OFAPayOptions Options { get; set; } = new();
        public ILogger Logger { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;


        private Dictionary<string, string> BuildForm()
        {
            var form = new Dictionary<string, string>
            {
                { "scode", Options.MerchantId },
                { "orderid", PaymentNumber },
                { "paytype", Options.PayType },
                { "amount", $"{Amount:0.00}" },
                { "currency", Options.Currency },
                { "productname", Options.ProductName },
                { "userid", PartyUid.ToString() },
                { "accountname", NativeName },
                { "noticeurl", Options.CallbackUrl },
                // { "redirectpage", "0" },
            };
            if (!string.IsNullOrEmpty(Options.RedirectPage))
            {
                form.Add("redirectpage", Options.RedirectPage);
            }

            return form;
        }
            

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("OFAPay.RequestAsync.form: {form}", form);
            var signature = GenerateSignature(form, Options.SecretKey);
            form.Add("sign", signature);
            Logger.LogInformation("OFAPay.RequestAsync.signed_form: {form}", form);

            if (isTest)
            {
                var content = new FormUrlEncodedContent(form);
                var response = await Client.PostAsync(Options.EndPoint, content);
                var body = await response.Content.ReadAsStringAsync();
                Logger.LogInformation("OFAPay.RequestAsync.body: {body}", body);
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

    public static string GenerateSignature(Dictionary<string, string> form, string secretKey, ILogger? logger = null)
    {
        var baseString = form
            .Where(x => x.Key != "sign")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Aggregate("", (current, pair) => $"{current}{pair.Key}={pair.Value}&");

        var signString = $"{baseString}key={secretKey}";
        logger?.LogInformation("OFAPay.GenerateSignature.signString: {signString}", signString);
        return Utils.Md5Hash(signString).ToLower();
    }
}