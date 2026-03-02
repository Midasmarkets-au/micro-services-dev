using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class PaymentMethod
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.BCRPaymentMethod, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.BCRPaymentMethod]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public string ToJson() => JsonConvert.SerializeObject(this);

    public class CallbackSetting
    {
        public int CallbackExpiredTimeInMinutes { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    public HashSet<CurrencyTypes> GetAvailableCurrencies(CurrencyTypes? accountCurrency = null)
    {
        var available = AvailableCurrenciesSpec.FromJson(AvailableCurrencies);
        if (accountCurrency != null && available.ApplyAccountCurrency)
        {
            return [accountCurrency.Value];
        }

        return available.Currencies;
    }

    public List<string> GetRequestKeys()
    {
        if (Group == "Union Pay")
        {
            return ["returnUrl", "nativeName", "userMobile"];
        }

        return (PaymentPlatformTypes)Platform switch
        {
            PaymentPlatformTypes.EuPay =>
            [
                "returnUrl",
                "currencyId",
                "billingLastName",
                "billingFirstName",
                "billingAddress",
                "billingCity",
                "billingState",
                "billingZipcode",
                "billingCountry",
                "billingPhone",
                "ccNumber",
                "ccMonth",
                "ccYear",
                "ccCvc",
            ],
            PaymentPlatformTypes.Pay247 => ["returnUrl", "currencyId"],
            PaymentPlatformTypes.Help2Pay => ["returnUrl", "currencyId"],
            PaymentPlatformTypes.Monetix => ["returnUrl", "currencyId"],
            PaymentPlatformTypes.Bakong => ["returnUrl", "currencyId"],
            PaymentPlatformTypes.Wire => MethodType == PaymentMethodTypes.Withdrawal
                ? ["returnUrl", "routingNumber", "accountNumber", "accountName"]
                : ["returnUrl",],
            PaymentPlatformTypes.GPay => ["returnUrl"],
            PaymentPlatformTypes.UEnjoy => ["returnUrl", "nativeName", "userMobile"],
            // public string NativeName { get; set; } = "";
            // public string UserAreaCode { get; set; } = "";
            // public string UserMobile { get; set; } = "";
            _ => ["returnUrl",]
        };
    }
}