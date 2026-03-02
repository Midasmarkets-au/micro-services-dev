using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class ChangeLeverageRequest : BaseRequest
{
    [JsonProperty("leverage")] public int Leverage { get; set; }

    public static ChangeLeverageRequest Build(long login, int leverage)
        => new() { Login = login, Leverage = leverage };

    public override string RequestQuery()
        // ReSharper disable once StringLiteralTypo
        => $"action=modifyaccount&request_id={Guid.NewGuid()}&" + this.ToQueryString();

    public object ToMt5Spec()
        => new
        {
            login = Login,
            leverage = Leverage,
        };
}