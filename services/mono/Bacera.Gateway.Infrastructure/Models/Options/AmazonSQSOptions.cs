namespace Bacera.Gateway;

public class AmazonSQSOptions
{
    public string AccessKey { get; set; } = "";
    public string AccessSecret { get; set; } = "";
    public string Region { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string BCREventTrade { get; set; } = "";
    public string BCRTrade { get; set; } = "";
    public string BCRSendMessage { get; set; } = "";
    public string BCRSalesRebateTrade { get; set; } = "";

    // public IEnumerable<string> QueueUrls =>
    //     new List<string> { BCREventTradeQueueUrl, BCRRebateTradeQueueUrl }.Where(x => !string.IsNullOrEmpty(x));
    //
    public static AmazonSQSOptions Create(string accessKey, string accessSecret, string region, string prefix
        , string bcrTrade, string bcrEventTrade, string bcrSalesRebateTrade, string bcrSendEmail)
        => new()
        {
            AccessKey = accessKey,
            AccessSecret = accessSecret,
            Region = region,
            Prefix = prefix,
            BCREventTrade = bcrEventTrade,
            BCRTrade = bcrTrade,
            BCRSalesRebateTrade = bcrSalesRebateTrade,
            BCRSendMessage = bcrSendEmail
        };
}