namespace Bacera.Gateway;

public class AmazonSQSOptions
{
    public string AccessKey { get; set; } = "";
    public string AccessSecret { get; set; } = "";
    public string Region { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string BCREventTrade { get; set; } = "";
    // [MIGRATED] BCRTrade and BCRSalesRebateTrade queues are no longer used.
    // MT5 trade monitoring has been moved to scheduler/src/jobs/trade_monitor.rs (NATS JetStream).
    // public string BCRTrade { get; set; } = "";
    // public string BCRSalesRebateTrade { get; set; } = "";
    public string BCRSendMessage { get; set; } = "";

    // public IEnumerable<string> QueueUrls =>
    //     new List<string> { BCREventTradeQueueUrl, BCRRebateTradeQueueUrl }.Where(x => !string.IsNullOrEmpty(x));
    //
    // [MIGRATED] bcrTrade and bcrSalesRebateTrade parameters removed — queues no longer in use.
    public static AmazonSQSOptions Create(string accessKey, string accessSecret, string region, string prefix
        , string bcrEventTrade, string bcrSendEmail)
        => new()
        {
            AccessKey = accessKey,
            AccessSecret = accessSecret,
            Region = region,
            Prefix = prefix,
            BCREventTrade = bcrEventTrade,
            BCRSendMessage = bcrSendEmail
        };
}