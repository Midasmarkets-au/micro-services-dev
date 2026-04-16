namespace Bacera.Gateway.Web;

public partial class Startup
{
    private static AmazonSQSOptions GetAwsSqsOptions()
    {
        var accessKey = GetEnvValue("AWS_SQS_ACCESS_KEY");
        var accessSecret = GetEnvValue("AWS_SQS_ACCESS_SECRET");
        var region = GetEnvValue("AWS_SQS_REGION");
        var prefix = GetEnvValue("AWS_SQS_PREFIX");
        var bcrEventTrade = GetEnvValue("AWS_SQS_BCR_EVENT_TRADE_QUEUE");
        var bcrSendMessage = GetEnvValue("AWS_SQS_BCR_SEND_MESSAGE_QUEUE");
        // [MIGRATED] BCRTrade and BCRSalesRebateTrade env vars removed — queues no longer in use.
        // var bcrSalesRebateTrade = GetEnvValue("AWS_SQS_BCR_SALES_REBATE_TRADE_QUEUE");
        // var bcrTrade = GetEnvValue("AWS_SQS_BCR_TRADE_QUEUE");
        return AmazonSQSOptions.Create(accessKey, accessSecret, region, prefix, bcrEventTrade, bcrSendMessage);
    }

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