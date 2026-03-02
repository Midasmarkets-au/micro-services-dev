using System.Net;
using Amazon.Runtime;
using Bacera.Gateway.Context;
using Bacera.Gateway.Msg.Identity;
using Bacera.Gateway.Msg.MyOptions;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using StackExchange.Redis;

namespace Bacera.Gateway.Msg;

public static class Startup
{
    public static void SetupUrls(this WebApplicationBuilder me)
    {
        var httpPort = GetEnvValue("CHAT_HTTP_URL");
        var httpPorts = GetEnvValue("CHAT_HTTPS_URL");
        if (!string.IsNullOrEmpty(httpPort))
            me.WebHost.UseUrls(httpPort);

        if (!string.IsNullOrEmpty(httpPorts))
            me.WebHost.UseUrls(httpPorts);
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

    public static void SetupTenancy(this WebApplicationBuilder me)
    {
        me.Services.AddScoped<Tenancy>()
            .AddScoped(typeof(ITenantGetter), svc => svc.GetRequiredService<Tenancy>())
            .AddScoped(typeof(ITenantSetter), svc => svc.GetRequiredService<Tenancy>())
            ;
    }


    public static void SetupDbContext(this WebApplicationBuilder me)
    {
        var centralOptions = GetCentralDatabaseOptions();
        me.Services.AddSingleton(_ => Options.Create(centralOptions));

        me.Services.AddDbContext<CentralDbContext>(options => options.UseNpgsql(centralOptions.ConnectionString));

        me.Services.AddDbContext<TenantDbContext>((services, options) =>
        {
            var getter = services.GetRequiredService<ITenantGetter>();
            var pool = services.GetRequiredService<MyDbContextPool>();
            var tenantId = getter.GetTenantId();
            var connectionString = pool.GetConnectionStringByTenantId(tenantId);
            // options.UseNpgsql(connectionString).UseLazyLoadingProxies();
            // connectionString = $"{connectionString};Include Error Details=True";
            // connectionString += ";Include Error Details=True";
            options.UseNpgsql(connectionString);
        });

        me.Services.AddSingleton<MyDbContextPool>();
    }

    public static void SetupRedis(this WebApplicationBuilder me)
    {
        var raw = GetEnvValue("REDIS_CONNECTION");
        var redisPassword = GetEnvValue("REDIS_PASSWORD");
        var isClusterMode = GetEnvValue("REDIS_CLUSTER_MODE", "false").Equals("true", StringComparison.OrdinalIgnoreCase);

        // Normalize: redis://host:port | rediss://host:port | host:port | host
        string hostPort = raw.Replace("redis://", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("rediss://", "", StringComparison.OrdinalIgnoreCase)
                             .Trim();

        var parts = hostPort.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var host = parts.Length > 0 ? parts[0] : "localhost";
        var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 6379;

        var configOptions = new ConfigurationOptions
        {
            Password = redisPassword,
            SyncTimeout = 10000,
            KeepAlive = 60,
            ConnectRetry = 10,
            AsyncTimeout = 10000,
            ConnectTimeout = 15000,
            AllowAdmin = !isClusterMode,
            ReconnectRetryPolicy = new ExponentialRetry(1000, 10000),
            AbortOnConnectFail = false,
        };

        // Use TLS only outside Development
        if (!AppEnvironment.IsDevelopment())
        {
            configOptions.Ssl = true;
            configOptions.SslHost = host; // SNI
            configOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                                         | System.Security.Authentication.SslProtocols.Tls13;
        }

        configOptions.EndPoints.Add(host, port);

        if (!isClusterMode)
        {
            configOptions.SocketManager = new SocketManager("Redis", 1000);
        }

        me.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var multiplexer = ConnectionMultiplexer.Connect(configOptions);
            multiplexer.ConnectionFailed += (_, args) =>
            {
                BcrLog.Slack($"Redis connection failed: {args.Exception}");
            };

            multiplexer.ConnectionRestored += (_, args) =>
            {
                BcrLog.Slack($"Redis connection restored: {args.Exception}");
            };
            return multiplexer;
        });

        me.Services.AddSingleton<IMyCache, MyCache>();

        me.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = configOptions;
            options.InstanceName = $"portal:{me.Environment.EnvironmentName.ToLower()}_";
        });
    }

    public static void SetupCors(this IServiceCollection me)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

        me.AddCors(options =>
        {
            if (isProduction)
            {
                // Production: Use specific allowed origins
                var allowedOrigins = GetProductionAllowedOrigins();
                
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains(); // Allow subdomains
                });
                
                // Also add the AllowAll policy for backwards compatibility
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
            else
            {
                // Development/Staging: Allow all origins for easier testing
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            }
        });
    }

    private static string[] GetProductionAllowedOrigins()
    {
        // Get allowed origins from environment variables or configuration
        var originsEnv = GetEnvValue("CORS_ALLOWED_ORIGINS", "");
        
        if (!string.IsNullOrEmpty(originsEnv))
        {
            return originsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(o => o.Trim())
                           .Where(o => !string.IsNullOrEmpty(o))
                           .ToArray();
        }

        // Fallback to default production domains (update these with your actual domains)
        return new[]
        {
            "https://au.midasmarkets.org",
            "https://bvi.midasmarkets.org",
            "https://sea.midasmarkets.org",
            "https://www.tradebcr.com"
        };
    }

    public static void SetupSignalR(this IServiceCollection me)
    {
        me.AddSignalR().AddStackExchangeRedis(o =>
        {
            o.Configuration.ChannelPrefix = new RedisChannel("portal_signalr", RedisChannel.PatternMode.Literal);
            o.ConnectionFactory = async writer =>
            {
                var raw = GetEnvValue("REDIS_CONNECTION");
                var isClusterMode = GetEnvValue("REDIS_CLUSTER_MODE", "false").Equals("true", StringComparison.OrdinalIgnoreCase);

                string hostPort = raw.Replace("redis://", "", StringComparison.OrdinalIgnoreCase)
                                     .Replace("rediss://", "", StringComparison.OrdinalIgnoreCase)
                                     .Trim();

                var parts = hostPort.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var host = parts.Length > 0 ? parts[0] : "localhost";
                var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 6379;

                var config = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    Password = GetEnvValue("REDIS_PASSWORD"),
                    ConnectTimeout = 15000,
                    SyncTimeout = 10000,
                    AsyncTimeout = 10000,
                    ConnectRetry = 10,
                    KeepAlive = 60,
                    ReconnectRetryPolicy = new ExponentialRetry(1000, 10000),
                    AllowAdmin = !isClusterMode,
                };

                if (!AppEnvironment.IsDevelopment())
                {
                    config.Ssl = true;
                    config.SslHost = host;
                    config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                                          | System.Security.Authentication.SslProtocols.Tls13;
                }

                config.EndPoints.Add(host, port);

                var conn = await ConnectionMultiplexer.ConnectAsync(config, writer);
                conn.ConnectionFailed += (_, _) => Console.WriteLine("SignalR connection to Redis failed.");

                if (!conn.IsConnected) throw new Exception("SignalR can not connect to Redis.");

                return conn;
            };
        });
    }

    public static void SetupLogging(this WebApplicationBuilder me)
    {
        var dir = Directory.GetCurrentDirectory();
        var logPath = Path.Combine(GetEnvValue("LOG_DIR", Path.Combine(dir, "logs")));
        if (!File.Exists(logPath))
            Directory.CreateDirectory(logPath);

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(me.Configuration)
            .WriteTo.Logger(
                x => x.WriteTo.Console()
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
            .WriteTo.File(new JsonFormatter(), $"{logPath}/portal-.log", rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 1204 * 1024 * 1024);

        Log.Logger = loggerConfiguration.CreateLogger();
        me.Configuration.SetBasePath(dir).AddJsonFile("appsettings.json");
        me.Host.UseSerilog();
        me.Services.AddLogging();
    }


    public static void AddWsAuthentication(this IServiceCollection me)
    {
        me.AddAuthentication(options => { options.DefaultScheme = "WsScheme"; })
            .AddScheme<WsSchemeOption, WsSchemeHandler>("WsScheme", options =>
            {
                options.Password = GetEnvValue("PFX_PASSWORD");
                options.PfxPath = GetEnvValue("PFX");
            });

        me.AddSingleton(_ => Options.Create(new WsSchemeOption
        {
            PfxPath = GetEnvValue("PFX"),
            Password = GetEnvValue("PFX_PASSWORD")
        }));
    }


    public static void LoadEnvironmentConfigure()
    {
        var root = Directory.GetCurrentDirectory();
        var envFile = Path.Combine(root, ".env");

        if (!File.Exists(envFile))
            throw new FileNotFoundException(".env not found");

        AddEnvValue(envFile);
    }

    private static void AddEnvValue(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
            var idx = trimmed.IndexOf('=');
            if (idx <= 0) continue;
            var key = trimmed[..idx].Trim();
            var value = trimmed[(idx + 1)..].Trim().Trim('"');
            if (!string.IsNullOrEmpty(key))
                Environment.SetEnvironmentVariable(key, value);
        }
    }


    private static bool IsLocal() => Environment.GetCommandLineArgs().Contains("local-dev");

    private static string GetEnvValue(string key, string fallback = "")
        => Environment.GetEnvironmentVariable(key) ?? fallback;

    public static void InitServices(this WebApplication app)
    {
    }
}