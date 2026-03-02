using Amazon.Runtime;
using AWS.Logger;
using AWS.Logger.SeriLog;
using Bacera.Gateway.Web.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupLogging(this WebApplicationBuilder me)
    {
        var dir = Directory.GetCurrentDirectory();
        var logPath = Path.Combine(GetEnvValue("LOG_DIR", Path.Combine(dir, "logs")));
        if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);

        var isSqlLogEnabled = IsSqlLogEnable();

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(me.Configuration)
            .WriteTo.Logger(
                isSqlLogEnabled
                    ? x => x.WriteTo.Console()
                    : x => x.WriteTo.Console().Filter.ByExcluding(e => e.Level == LogEventLevel.Information &&
                                                                       e.Properties.ContainsKey("SourceContext") &&
                                                                       e.Properties["SourceContext"].ToString()
                                                                           .Contains(
                                                                               "Microsoft.EntityFrameworkCore.Database.Command"))
            )
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(new JsonFormatter(), $"{logPath}/portal-error-.log",
                        rollingInterval: RollingInterval.Day)
            )
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(new JsonFormatter(), $"{logPath}/portal-warning-.log",
                        rollingInterval: RollingInterval.Day)
            )
            .WriteTo.Logger(
                x => x
                    .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("Slack"))
                    .WriteTo.ToSlack(GetEnvValue("LOG_SLACK_WEBHOOK_URL"))
            )
            .WriteTo.Logger(
                x => x
                    // Filter out noisy ASP.NET Core framework logs to reduce log size
                    .Filter.ByExcluding(e =>
                        e.Level == LogEventLevel.Information &&
                        e.Properties.ContainsKey("SourceContext") &&
                        (
                            // Exclude routing/endpoint logs (every HTTP request generates these)
                            e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Routing") ||
                            e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Mvc.Infrastructure") ||
                            e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Hosting.Diagnostics") ||
                            
                            // Exclude EF Core command logs (if SQL logging is disabled)
                            (!isSqlLogEnabled && e.Properties["SourceContext"].ToString().Contains("Microsoft.EntityFrameworkCore.Database.Command"))
                        )
                    )
                    .WriteTo.File(new JsonFormatter(), $"{logPath}/portal-.log", 
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 1204 * 1024 * 1024)
            );

        // Add Seq logging if configured
        var seqServerUrl = GetEnvValue("SEQ_SERVER_URL");
        var seqApiKey = GetEnvValue("SEQ_API_KEY");
        if (!string.IsNullOrEmpty(seqServerUrl))
        {
            if (!string.IsNullOrEmpty(seqApiKey))
            {
                loggerConfiguration.WriteTo.Seq(seqServerUrl, apiKey: seqApiKey);
            }
            else
            {
                loggerConfiguration.WriteTo.Seq(seqServerUrl);
            }
        }

        // Add OpenTelemetry logging if configured
        var otelEndpoint = GetEnvValue("OTEL_EXPORTER_OTLP_ENDPOINT");
        var otelHeaders = GetEnvValue("OTEL_EXPORTER_OTLP_HEADERS");
        
        if (!string.IsNullOrEmpty(otelEndpoint))
        {
            try
            {
                Console.WriteLine($"[SERILOG OTEL DEBUG] Configuring with endpoint: {otelEndpoint}");
                loggerConfiguration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otelEndpoint;
                    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf; // Changed from Grpc to HttpProtobuf
                    
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = "bacera-gateway",
                        ["service.version"] = "1.0.0",
                        ["environment"] = GetEnvValue("ASPNETCORE_ENVIRONMENT", "Development")
                    };
                    
                    if (!string.IsNullOrEmpty(seqApiKey))
                    {
                        options.Headers = new Dictionary<string, string>
                        {
                            ["X-Seq-ApiKey"] = seqApiKey
                        };
                        Console.WriteLine($"[SERILOG OTEL DEBUG] Using Seq API key");
                    }
                    
                    // Set batch options for better performance
                    options.BatchingOptions.BatchSizeLimit = 1000;
                    options.BatchingOptions.QueueLimit = 100000;
                });
                Console.WriteLine("[SERILOG OTEL DEBUG] Configuration completed successfully");
            }
            catch (Exception ex)
            {
                // Log configuration error but don't fail startup
                Console.WriteLine($"[WARNING] Failed to configure OpenTelemetry logging: {ex.Message}");
            }
        }

        if (
            !string.IsNullOrEmpty(GetEnvValue("AWS_CW_ACCESS_KEY"))
            && !string.IsNullOrEmpty(GetEnvValue("AWS_CW_ACCESS_SECRET"))
            && !string.IsNullOrEmpty(GetEnvValue("AWS_CW_REGION"))
            && !string.IsNullOrEmpty(GetEnvValue("AWS_CW_LOGS_NAME"))
        )
        {
            var awsLoggerConfig =
                new AWSLoggerConfig(GetEnvValue("AWS_CW_LOGS_NAME") + "." + me.Environment.EnvironmentName)
                {
                    Region = GetEnvValue("AWS_CW_REGION"),
                    Credentials = new BasicAWSCredentials(GetEnvValue("AWS_CW_ACCESS_KEY"),
                        GetEnvValue("AWS_CW_ACCESS_SECRET"))
                };
            loggerConfiguration.WriteTo.AWSSeriLog(
                awsLoggerConfig,
                textFormatter: new JsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Information
            );
        }


        Log.Logger = loggerConfiguration.CreateLogger();
        me.Configuration.SetBasePath(dir).AddJsonFile("appsettings.json");
        me.Host.UseSerilog();
        me.Services.AddLogging();
    }
}