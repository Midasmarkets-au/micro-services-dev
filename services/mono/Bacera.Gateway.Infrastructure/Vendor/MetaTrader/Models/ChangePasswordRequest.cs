using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class ChangePasswordRequest : BaseRequest
{
    [JsonProperty("pass")] public string Password { get; set; } = null!;
    private string Type { get; init; } = "main";
    public string GetPasswordType() => Type;

    public static ChangePasswordRequest Build(long login, string password, bool forInvestor = false)
        => new() { Login = login, Password = password, Type = forInvestor ? "investor" : "main" };

    public static ChangePasswordRequest Build(long login, string password, string passwordType)
        => new() { Login = login, Password = password, Type = passwordType };

    public override string RequestQuery()
        // ReSharper disable once StringLiteralTypo
        => $"action=changepassword&request_id={Guid.NewGuid()}&" + this.ToQueryString();

    public object ToMt5Spec()
        => new
        {
            login = Login,
            password = Password,
            type = Type,
        };
}