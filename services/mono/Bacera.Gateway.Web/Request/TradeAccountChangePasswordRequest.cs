namespace Bacera.Gateway.Web.Request;

public class TradeAccountChangePasswordRequest
{
    public string Token { get; set; } = null!;
    public string Password { get; set; } = null!;
}