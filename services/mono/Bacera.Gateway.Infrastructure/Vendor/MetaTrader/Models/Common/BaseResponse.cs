using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class BaseResponse : IResponse
{
    [JsonProperty("login")] public long Login { get; set; }
}