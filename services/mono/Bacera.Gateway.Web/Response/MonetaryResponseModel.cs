namespace Bacera.Gateway.Web.Response;

public class MonetaryResponseModel
{
    public CurrencyTypes CurrencyId { get; set; }
    public long Amount { get; set; }

    public static MonetaryResponseModel Of(CurrencyTypes currencyId, long amount) => new()
    {
        Amount = amount,
        CurrencyId = currencyId,
    };

    public static MonetaryResponseModel Of(KeyValuePair<int, long> kv) => new()
    {
        Amount = kv.Value,
        CurrencyId = (CurrencyTypes)kv.Key,
    };
}