using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class ChangeBalanceResponse : BaseResponse
{
    // ReSharper disable once StringLiteralTypo
    [JsonProperty("newbalance")] public decimal NewBalance { get; set; }

    // ReSharper disable once StringLiteralTypo
    [JsonProperty("orderid")] public long OrderId { get; set; }
}