using Amazon;
using Amazon.SQS;
using Bacera.Gateway.Vendor.Amazon;
using Bacera.Gateway.Vendor.Amazon.Options;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupAws(this WebApplicationBuilder me)
    {
        var sesRegion = GetEnvValue("AWS_SES_REGION");
        var sesAccessKey = GetEnvValue("AWS_SES_ACCESS_KEY");
        var sesAccessSecret = GetEnvValue("AWS_SES_ACCESS_SECRET");
        var sesFromAddress = GetEnvValue("AWS_SES_FROM_ADDRESS");
        var sesFromName = GetEnvValue("AWS_SES_FROM_NAME");

        // Only setup SES if all required values are present
        if (!string.IsNullOrEmpty(sesRegion) && !string.IsNullOrEmpty(sesAccessKey) &&
            !string.IsNullOrEmpty(sesAccessSecret) && !string.IsNullOrEmpty(sesFromAddress))
        {
            var sesOptions = Options.Create(AwsSesOptions.Of(
                sesAccessKey,
                sesAccessSecret,
                sesRegion,
                sesFromAddress,
                sesFromName)
            );
            me.Services.AddSingleton(_ => sesOptions);
            me.Services.AddSingleton<IEmailSender, AwsEmailSender>();
            me.Services.AddSingleton<AwsEmailClientV2>();
        }

        var s3Region = GetEnvValue("AWS_S3_REGION");
        var s3AccessKey = GetEnvValue("AWS_S3_ACCESS_KEY");
        var s3AccessSecret = GetEnvValue("AWS_S3_ACCESS_SECRET");
        var s3Bucket = GetEnvValue("AWS_S3_BUCKET");
        var s3PublicBucket = GetEnvValue("AWS_S3_PUBLIC_BUCKET");

        // Only setup S3 if all required values are present
        if (!string.IsNullOrEmpty(s3Region) && !string.IsNullOrEmpty(s3AccessKey) &&
            !string.IsNullOrEmpty(s3AccessSecret) && !string.IsNullOrEmpty(s3Bucket))
        {
            var s3Options = Options.Create(AwsS3Options.Of(
                s3AccessKey,
                s3AccessSecret,
                s3Region,
                s3Bucket,
                s3PublicBucket
            ));
            me.Services.AddSingleton(_ => s3Options);
            me.Services.AddTransient<IStorageService, AwsStorageService>();
        }

        // [MIGRATED] SQS infrastructure removed — all queues have been migrated to NATS JetStream:
        //   BCREventTrade  → NATS BCR_EVENT_TRADE  (scheduler/src/jobs/event_trade_handler.rs)
        //   BCRSendMessage → NATS BCR_SEND_MESSAGE  (scheduler/src/jobs/send_message_handler.rs)
        //   BCRTrade / BCRSalesRebateTrade → NATS BCR_TRADE (scheduler/src/jobs/trade_handler.rs)
        // AmazonSQSClient and IMessageQueueService are no longer needed.
        // var awsSqsOptions = GetAwsSqsOptions();
        // me.Services.AddSingleton(_ => Options.Create(awsSqsOptions));
        // me.Services.AddSingleton(service =>
        // {
        //     var option = service.GetRequiredService<IOptions<AmazonSQSOptions>>().Value;
        //     return new AmazonSQSClient(option.AccessKey, option.AccessSecret, RegionEndpoint.GetBySystemName(option.Region));
        // });
    }
}