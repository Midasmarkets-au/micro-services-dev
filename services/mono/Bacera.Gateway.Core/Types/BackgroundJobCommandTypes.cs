namespace Bacera.Gateway.Core.Types;

public class BackgroundJobCommandTypes
{
    public const string TradeSymbolCheck = "TradeSymbolCheck";
    public const string TrackTradeTransaction = "TrackTradeTransaction";


    public static readonly List<string> All = new()
    {
        TradeSymbolCheck,
        TrackTradeTransaction,
    };

    public static bool IsExists(string command) => All.Contains(command);
}