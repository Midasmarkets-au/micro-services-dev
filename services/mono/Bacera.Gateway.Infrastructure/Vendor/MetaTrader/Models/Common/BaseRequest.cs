using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public abstract class BaseRequest : IRequest, IHasRequestQuery
{
    [JsonProperty("login")] public long Login { get; set; }
    public abstract string RequestQuery();
}