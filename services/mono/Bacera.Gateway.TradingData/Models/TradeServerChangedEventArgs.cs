namespace Bacera.Gateway.TradingData;

[Serializable]
public class TradeServerChangedEventArgs : EventArgs
{
    public string Platform { get; set; }
    public int ServerId { get; set; }
    public string AdditionalInfo { get; set; }
    public int TotalAccount { get; set; }

    public TradeServerChangedEventArgs(string platform, int serverId, int totalAccount = 0, string additionalInfo = "")
    {
        Platform = platform;
        ServerId = serverId;
        TotalAccount = totalAccount;
        AdditionalInfo = additionalInfo;
    }
}