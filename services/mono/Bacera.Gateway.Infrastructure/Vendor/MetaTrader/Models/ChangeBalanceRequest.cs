using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class ChangeBalanceRequest : BaseRequest
{
    [JsonProperty("value")] public decimal Value { get; set; }

    [JsonProperty("comment")] public string Comment { get; set; } = string.Empty;

    public static ChangeBalanceRequest Build(long login, decimal value, string comment)
        => new() { Login = login, Value = value, Comment = comment };

    public override string RequestQuery()
        // ReSharper disable once StringLiteralTypo
        => $"action=changebalance&request_id={Guid.NewGuid()}&" + this.ToQueryString();
}