using Serilog;

namespace Bacera.Gateway.Web.Services;

public static class BcrLog
{
    public static void Slack(string message) => Log.Logger.ForContext("Slack", true).Information(message);
}