using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otelEndpoint = GetEnvValue("OTEL_EXPORTER_OTLP_ENDPOINT");
        var otelHeaders = GetEnvValue("OTEL_EXPORTER_OTLP_HEADERS");
        var seqApiKey = GetEnvValue("SEQ_API_KEY");

        if (string.IsNullOrEmpty(otelEndpoint))
        {
            Console.WriteLine("[INFO] OTEL_EXPORTER_OTLP_ENDPOINT not configured, skipping OpenTelemetry setup");
            return;
        }

        var headers = BuildOtlpHeaders(otelHeaders, seqApiKey);

        void ConfigureOtlpExporter(OtlpExporterOptions options)
        {
            options.Endpoint = new Uri(otelEndpoint);
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            if (!string.IsNullOrEmpty(headers))
                options.Headers = headers;
        }

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: "BaceraGateway.Api",
                serviceVersion: "1.0.0",
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["environment"] = GetEnvValue("ASPNETCORE_ENVIRONMENT", "Development"),
                ["deployment.environment"] = GetEnvValue("ASPNETCORE_ENVIRONMENT", "Development"),
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.SetResourceBuilder(resourceBuilder);
            logging.AddOtlpExporter(ConfigureOtlpExporter);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddAttributes(new Dictionary<string, object>
            {
                ["environment"] = GetEnvValue("ASPNETCORE_ENVIRONMENT", "Development"),
            }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("Bacera.Gateway.Web")
                .AddOtlpExporter(ConfigureOtlpExporter))
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                    tracing.SetSampler(new AlwaysOnSampler());

                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                            activity.SetTag("http.request.body.size", request.ContentLength);
                        options.EnrichWithHttpResponse = (activity, response) =>
                            activity.SetTag("http.response.body.size", response.ContentLength);
                    })
                    .AddHttpClientInstrumentation(options => options.RecordException = true)
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource("Bacera.Gateway.Web")
                    .AddOtlpExporter(ConfigureOtlpExporter);
            });

        Console.WriteLine($"[INFO] OpenTelemetry configured → {otelEndpoint}");
    }

    private static string BuildOtlpHeaders(string otelHeaders, string seqApiKey)
    {
        // Parse existing headers into a dict to deduplicate keys.
        // OTEL_EXPORTER_OTLP_HEADERS may already contain X-Seq-ApiKey=xxx.
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in otelHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = part.IndexOf('=');
            if (idx > 0)
                dict[part[..idx].Trim()] = part[(idx + 1)..].Trim();
        }

        if (!string.IsNullOrEmpty(seqApiKey))
            dict["X-Seq-ApiKey"] = seqApiKey;

        return string.Join(",", dict.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}
