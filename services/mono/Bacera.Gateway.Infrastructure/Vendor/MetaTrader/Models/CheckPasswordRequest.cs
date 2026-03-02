using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class CheckPasswordRequest : BaseRequest
{
    [JsonProperty("pass")] public string Password { get; set; } = string.Empty;
    private string Type { get; init; } = "main";
    public static CheckPasswordRequest Build(long login, string password, bool forInvestor = false)
        => new() { Login = login, Password = password, Type = forInvestor ? "investor" : "main" };

    public override string RequestQuery()
        // ReSharper disable once StringLiteralTypo
        => $"action=checkpassword&request_id={Guid.NewGuid()}&" + this.ToQueryString();

    public string GetPasswordType() => Type;

    public object ToMt5Spec()
        => new
        {
            login = Login,
            password = Password,
            type = Type,
        };
}