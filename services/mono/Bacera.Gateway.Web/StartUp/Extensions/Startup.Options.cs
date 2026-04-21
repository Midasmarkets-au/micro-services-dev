namespace Bacera.Gateway.Web;

public partial class Startup
{
    // [MIGRATED] GetAwsSqsOptions removed — all SQS queues have been migrated to NATS JetStream.
    //   BCRTrade / BCRSalesRebateTrade → NATS BCR_TRADE  (scheduler/src/jobs/trade_handler.rs)
    //   BCREventTrade                  → NATS BCR_EVENT_TRADE (scheduler/src/jobs/event_trade_handler.rs)
    //   BCRSendMessage                 → NATS BCR_SEND_MESSAGE (scheduler/src/jobs/send_message_handler.rs)
    // AmazonSQSClient + IMessageQueueService registration also removed in SetupAws() / SetupApplicationServices().
    // Env vars no longer needed: AWS_SQS_ACCESS_KEY, AWS_SQS_ACCESS_SECRET, AWS_SQS_REGION, AWS_SQS_PREFIX,
    //   AWS_SQS_BCR_TRADE_QUEUE, AWS_SQS_BCR_SALES_REBATE_TRADE_QUEUE,
    //   AWS_SQS_BCR_EVENT_TRADE_QUEUE, AWS_SQS_BCR_SEND_MESSAGE_QUEUE.
    // private static AmazonSQSOptions GetAwsSqsOptions()
    // {
    //     var accessKey = GetEnvValue("AWS_SQS_ACCESS_KEY");
    //     var accessSecret = GetEnvValue("AWS_SQS_ACCESS_SECRET");
    //     var region = GetEnvValue("AWS_SQS_REGION");
    //     var prefix = GetEnvValue("AWS_SQS_PREFIX");
    //     return AmazonSQSOptions.Create(accessKey, accessSecret, region, prefix);
    // }

    private static CentralDatabaseOptions GetCentralDatabaseOptions()
    {
        var host = GetEnvValue("DB_HOST", "localhost");
        var port = int.Parse(GetEnvValue("DB_PORT", "5432"));
        var user = GetEnvValue("DB_USERNAME", "postgres");
        var password = GetEnvValue("DB_PASSWORD");
        var database = GetEnvValue("DB_DATABASE", "portal");
        return CentralDatabaseOptions.Create(host, database, user, password, port);
    }

    private static WebsiteDatabaseOptions GetWebsiteDatabaseOptions()
    {
        var host = GetEnvValue("DB_WEBSITE_HOST", "localhost");
        var port = int.Parse(GetEnvValue("DB_WEBSITE_PORT", "3306"));
        var user = GetEnvValue("DB_WEBSITE_USERNAME", "root");
        var password = GetEnvValue("DB_WEBSITE_PASSWORD");
        var database = GetEnvValue("DB_WEBSITE_DATABASE", "website");
        var item = WebsiteDatabaseOptions.Create(host, database, user, password, port);
        item.ConvertZeroDateTime = true;
        return item;
    }

    private static MybcrDatabaseOptions GetMybcrDatabaseOptions()
    {
        var host = GetEnvValue("DB_WEBSITE_HOST", "localhost");
        var port = int.Parse(GetEnvValue("DB_WEBSITE_PORT", "3306"));
        var user = GetEnvValue("DB_WEBSITE_USERNAME", "root");
        var password = GetEnvValue("DB_WEBSITE_PASSWORD");
        var database = GetEnvValue("DB_MYBCR_DATABASE", "website");
        var item = MybcrDatabaseOptions.Create(host, database, user, password, port);
        item.ConvertZeroDateTime = true;
        return item;
    }

    private static TenantDatabaseOptions GetTenantDatabaseOptions()
    {
        var host = GetEnvValue("DB_HOST", "localhost");
        var port = int.Parse(GetEnvValue("DB_PORT", "5432"));
        var user = GetEnvValue("DB_USERNAME", "postgres");
        var password = GetEnvValue("DB_PASSWORD");
        return TenantDatabaseOptions.Create(host, "", user, password, port);
    }

    private static HangfireDatabaseOptions GetHangfireDatabaseOptions()
    {
        var host = GetEnvValue("DB_HOST", "localhost");
        var port = int.Parse(GetEnvValue("DB_PORT", "5432"));
        var user = GetEnvValue("DB_USERNAME", "postgres");
        var password = GetEnvValue("DB_PASSWORD");
        var database = GetEnvValue("HANGFIRE_DATABASE");
        return HangfireDatabaseOptions.Create(host, database, user, password, port);
    }
}