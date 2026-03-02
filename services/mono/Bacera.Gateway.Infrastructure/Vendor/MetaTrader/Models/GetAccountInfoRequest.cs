namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class GetAccountInfoRequest : BaseRequest
{
    public static GetAccountInfoRequest Build(long login) => new() { Login = login };

    public override string RequestQuery()
        // ReSharper disable once StringLiteralTypo
        => $"action=getaccountinfo&request_id={Guid.NewGuid()}&login=" + Login;
}