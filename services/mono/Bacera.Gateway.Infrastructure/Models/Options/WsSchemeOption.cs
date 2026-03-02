using Microsoft.AspNetCore.Authentication;

namespace Bacera.Gateway.Msg.MyOptions;

public class WsSchemeOption : AuthenticationSchemeOptions
{
    public string Password { get; set; } = string.Empty;
    public string PfxPath { get; set; } = string.Empty;
}