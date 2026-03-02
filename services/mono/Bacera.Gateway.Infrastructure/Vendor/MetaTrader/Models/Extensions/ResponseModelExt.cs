using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public static class ResponseModelExt
{
    public static ApiResult<T> FromQueryString<T>(this ApiResult<T> me, string responseString)
        where T : class, IResponse, new()
    {
        var dict = HttpUtility.ParseQueryString(responseString);
        if (!dict.AllKeys.Contains("result") || dict["result"] == null)
        {
            return me.SetStatus(status: ApiResultStatus.SystemError, "Empty response");
        }

        var status = (ApiResultStatus)int.Parse(dict["result"] ?? "-5");
        var message = dict.AllKeys.Contains("reason") && dict["reason"] != null
            ? dict["reason"] ?? string.Empty
            : string.Empty;
        me.SetStatus(status, message);

        if (dict.AllKeys.Contains("request_id") && dict["request_id"] != null)
            me.SetRequestId(dict["request_id"] ?? string.Empty);

        var json = JsonConvert.SerializeObject(dict.Cast<string>()
            .Where(x => x != null)
            .ToDictionary(k => k, v => dict[v]));
        var data = JsonConvert.DeserializeObject<T>(json);

        return me.SetData(data ?? new T());
    }

    // public static ApiResult<T> FromJsonResult<T>(this ApiResult<T> me, string responseJson)
    //     where T : class, IResponse, new()
    // {
    //     try
    //     {
    //         var testParse = JsonConvert.DeserializeObject<JObject>(responseJson);
    //         if (testParse == null) return me.SetStatus(status: ApiResultStatus.SystemError, responseJson);
    //     }
    //     catch
    //     {
    //         return me.SetStatus(status: ApiResultStatus.SystemError, responseJson);
    //     }
    //     var jObject = JsonConvert.DeserializeObject<JObject>(responseJson);
    //     if (jObject == null || jObject.ContainsKey("retcode") == false ||
    //         !string.IsNullOrEmpty((string?)jObject["retcode"]))
    //     {
    //         return me.SetStatus(status: ApiResultStatus.SystemError, "Empty response");
    //     }
    //
    //     var ret = (string)jObject["retcode"]!;
    //     if (ret.StartsWith("0") == false) return me.SetStatus(ApiResultStatus.SystemError, responseJson);
    //
    //     if (jObject.ContainsKey("answer") == false &&
    //         string.IsNullOrEmpty((string?)jObject["answer"])) return me.SetData(new T());
    //
    //     var dict = jObject["answer"]!.ToObject<Dictionary<string, string>>();
    //
    //
    //     if (dict == null || !dict.Any()) return me.SetData(new T());
    //
    //     var json = JsonConvert.SerializeObject(dict.Cast<string>()
    //         .Where(x => x != null)
    //         .ToDictionary(k => k, v => dict[v]));
    //     var data = JsonConvert.DeserializeObject<T>(json);
    //
    //     return me.SetData(data ?? new T());
    //
    // }
}