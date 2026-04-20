namespace Bacera.Gateway;

public class AmazonSQSOptions
{
    public string AccessKey { get; set; } = "";
    public string AccessSecret { get; set; } = "";
    public string Region { get; set; } = "";
    public string Prefix { get; set; } = "";
    // [MIGRATED] BCREventTrade queue no longer used — events published to NATS BCR_EVENT_TRADE.
    // OpenAccount: AccountCreatedEventHandler → NatsPublisher
    // Deposit: GeneralJob.DepositCompletedAsync → NatsPublisher
    // Trade: scheduler/src/jobs/trade_handler.rs → NATS (already migrated)
    // Consumer: scheduler/src/jobs/event_trade_handler.rs
    // public string BCREventTrade { get; set; } = "";
    // [MIGRATED] BCRTrade and BCRSalesRebateTrade queues are no longer used.
    // MT5 trade monitoring has been moved to scheduler/src/jobs/trade_monitor.rs (NATS JetStream).
    // public string BCRTrade { get; set; } = "";
    // public string BCRSalesRebateTrade { get; set; } = "";
    public string BCRSendMessage { get; set; } = "";

    // [MIGRATED] bcrTrade, bcrSalesRebateTrade, and bcrEventTrade parameters removed — queues no longer in use.
    public static AmazonSQSOptions Create(string accessKey, string accessSecret, string region, string prefix
        , string bcrSendEmail)
        => new()
        {
            AccessKey = accessKey,
            AccessSecret = accessSecret,
            Region = region,
            Prefix = prefix,
            BCRSendMessage = bcrSendEmail
        };
}