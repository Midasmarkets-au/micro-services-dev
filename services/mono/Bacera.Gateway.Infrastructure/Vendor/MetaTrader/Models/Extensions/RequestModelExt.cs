using System.Web;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public static class RequestModelExt
{
    private const string AddressString = "address";
    public static string ToQueryString<T>(this T model) where T : class, IRequest, new()
    {
        var json = JsonConvert.SerializeObject(model);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null || dict.Count == 0) return string.Empty;

        // for mt4 only, move address to the end of the query string
        // if (addressValue != null) dict.Remove(AddressString);
        dict.Remove(AddressString);

        var queryString = dict
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .Select(x => x.Key + "=" + x.Value)
            .Aggregate((x, y) => x + "&" + y);

        var addressValue = dict.GetValueOrDefault(AddressString);
        queryString += $"&{AddressString}={addressValue}";
        return queryString;
    }
}