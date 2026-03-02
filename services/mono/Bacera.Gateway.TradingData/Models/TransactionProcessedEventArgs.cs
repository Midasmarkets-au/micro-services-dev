namespace Bacera.Gateway.TradingData;

[Serializable]
public class TransactionProcessedEventArgs
{
    public long ServiceId { get; set; }
    public int Updated { get; set; }
    public int Inserted { get; set; }
}