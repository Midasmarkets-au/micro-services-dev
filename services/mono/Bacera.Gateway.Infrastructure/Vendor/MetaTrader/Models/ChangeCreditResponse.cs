using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class ChangeCreditResponse : BaseResponse
{
    // ReSharper disable once StringLiteralTypo
    [JsonProperty("newcredit")] public decimal NewCredit { get; set; }

    // ReSharper disable once StringLiteralTypo
    [JsonProperty("order")] public long OrderId { get; set; }
}