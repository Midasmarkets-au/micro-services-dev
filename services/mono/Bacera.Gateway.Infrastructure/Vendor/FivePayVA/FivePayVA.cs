using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Vendor.FivePayVA;

public class FivePayVA
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = FivePayVAOptions.FromJson(config);

        var request = new RequestClient
        {
            TenantId = 10004,
            AccountUid = 32410542,
            Amount = 500000,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = "https://example.com/return",
            Options = options,
            Logger = logger
        };

        var response = await request.RequestAsync(true);
        return response;
    }
    
    public sealed class RequestClient
    {
        public long TenantId { get; set; }
        public long Amount { get; set; }
        public long AccountUid { get; set; }
        public string ReturnUrl { get; set; } = null!;
        public string PaymentNumber { get; set; } = null!;
        public CurrencyTypes CurrencyId { get; set; }
        public HttpClient? Client { get; set; }

        private string Currency => CurrencyId switch
        {
            CurrencyTypes.CNY => "CNY",
            CurrencyTypes.IDR => "IDR",
            CurrencyTypes.VND => "VND",
            CurrencyTypes.THB => "THB",
            CurrencyTypes.MYR => "MYR",
            _ => Options.CurrencyCode
        };

        public FivePayVAOptions Options { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        public Dictionary<string, string> BuildForm()
        {
            var form = new Dictionary<string, string>
            {
                { "merchantId", Options.MerchantId.ToString() },
                { "memberId", AccountUid.ToString() },
                { "currencyCode", Currency },
                // { "currencyCode", Options.CurrencyCode },
                { "orderAmount", $"{Amount:00}" },
                { "merchantOrderNo", PaymentNumber },
                { "notifyUrl", $"{Options.CallbackDomain}/payment/callback/{TenantId}/fivepay-va" },
                { "returnUrl", ReturnUrl }
            };

            return form;
        }

        private string Encryptor(string value) => Utils.TripleDESEncrypt(value, Options.SecretKey);
        public string GetEncryptPaymentNumber() => Encryptor(PaymentNumber);

        private Dictionary<string, string> EncryptAndSignForm(Dictionary<string, string> form)
        {
            var keysNeedToEncrypt = new[] { "memberId", "currencyCode", "orderAmount", "merchantOrderNo" };
            var encryptedForm = form
                .Where(x => keysNeedToEncrypt.Contains(x.Key))
                .ToDictionary(x => x.Key, x => Encryptor(x.Value));
            var notEncryptedForm = form
                .Where(x => !keysNeedToEncrypt.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
            encryptedForm = encryptedForm.Concat(notEncryptedForm).ToDictionary(x => x.Key, x => x.Value);
            var aggregateString = encryptedForm.OrderBy(x => x.Key, StringComparer.Ordinal)
                .Aggregate(new StringBuilder(), (sb, x) => sb.Append(x.Value), sb => sb.ToString());
            var sign = Utils.MD5(Utils.SHA1(aggregateString));
            encryptedForm.Add("sign", sign);
            return encryptedForm;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);
            var form = BuildForm();
            var encryptedForm = EncryptAndSignForm(form);

            if (isTest)
            {
                var response = await Client!.PostAsync(Options.EndPoint, new FormUrlEncodedContent(encryptedForm));
                var body = await response.Content.ReadAsStringAsync();
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Form = encryptedForm,
                Action = PaymentResponseActionTypes.Post,
                EndPoint = Options.EndPoint,
                PaymentNumber = PaymentNumber,
                Reference = GetEncryptPaymentNumber()
            };
        }
    }

    public static bool ValidateCallbackSignature(Dictionary<string, string> spec)
    {
        if (new[]
            {
                "orderNo", "currencyCode", "merchantId", "memberId", "channelName",
                "orderAmount", "merchantOrderNo", "status", "sign"
            }.Any(x => !spec.ContainsKey(x))) return false;

        var aggregateString = spec
            .Where(x => x.Key != "sign")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Aggregate(new StringBuilder(), (sb, x) => sb.Append(x.Value), sb => sb.ToString());
        var sign = Utils.MD5(Utils.SHA1(aggregateString));
        return sign == spec["sign"];
    }
}