using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor.MetaTrader;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Message;
using Bacera.Gateway.Services.Twilio;
using Bacera.Gateway.Vendor;
using Bacera.Gateway.Web.Middleware;
using Bacera.Gateway.Web.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.ChunkStorage;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.Permission;
using Bacera.Gateway.Services.Withdraw;
using Bacera.Gateway.Vendor.IPInfo;
using Bacera.Gateway.Web.BackgroundJobs.GeneralJob;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Middlewares;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.OpenApi.Models;
using System.Net.Security;
using Serilog;

namespace Bacera.Gateway.Web;

public static partial class Startup
{
    public static void SetupApplicationServices(this IServiceCollection me)
    {
        me.AddHttpContextAccessor()
            .AddTransient<ReportService>()
            .AddTransient<TradingService>()
            .AddTransient<CentralService>()
            .AddTransient<ShopService>()
            .AddTransient<ApplicationService>()
            .AddTransient<ITopicService, TopicService>()
            .AddTransient<ILeadService, LeadService>()
            // .AddTransient<ICopyTradeService, CopyTradeService>()
            .AddTransient<AccountingService>()
            .AddTransient<IApplicationTokenService, ApplicationTokenService>()
            .AddTransient<IChunkStorageService, RedisChunkStorageService>()
            .AddTransient<EventService>()
            .AddTransient<StartupService>()
            .AddTransient<AcctService>()
            .AddSingleton<IMessageQueueService, MessageQueueService>()
            .AddSingleton<WsMessageProcessor>()
            .AddSingleton<MonitorService>()
            // Security services
            .AddSingleton<RateLimiterService>()
            .AddSingleton<LoginSecurityService>()
            // Background service
            //.AddHostedService<MT5TickService>()
            // Interface for controllers  
            //.AddScoped<IMt5TickService, Mt5TickService>()
            .AddScoped<RebateService>()
            .AddScoped<BcrTokenService>()
            .AddScoped<ITenantService, TenantService>()
            .AddScoped<ISendMailService, SendMailService>()
            .AddScoped<ITradingApiService, TradingApiService>()
            // .AddScoped<IPaymentProxyService, PaymentProxyService>()
            .AddScoped<ISendMessageService, SendMessageService>()
            .AddScoped<IRebateJob, RebateJob>()
            .AddScoped<IReportJob, ReportJob>()
            .AddScoped<IProcessAccountStatJob, ProcessAccountStatJob>()
            .AddScoped<TagService>()
            .AddScoped<ITradeAccountJob, TradeAccountJob>()
            .AddScoped<ITradeJob, TradeJob>()
            .AddScoped<IGeneralJob, GeneralJob>()
            .AddScoped<IPaymentCallbackJob, PaymentCallbackJob>()
            .AddScoped<CryptoJob>()
            .AddScoped<UserService>()
            .AddScoped<ReferralCodeService>()
            .AddScoped<MessageService>()
            .AddScoped<ConfigurationService>()
            .AddScoped<ConfigurationSnapshotService>()
            .AddScoped<PaymentMethodService>()
            .AddScoped<AccountManageService>()
            .AddScoped<WalletService>()
            .AddScoped<ConfigService>()
            .AddScoped<DepositService>()
            .AddScoped<WithdrawalService>()
            .AddScoped<SupplementService>()
            .AddScoped<PermissionService>()
            .AddScoped<AutoCreateAccountService>()
            .AddScoped<CryptoService>()
            .AddScoped<BatchSendEmailService>()
            .AddScoped<PayoutService>()
            .AddScoped<ITradePasswordEncryptionService, TradePasswordEncryptionService>()
            .AddScoped<ITradePasswordValidationService, TradePasswordValidationService>()
            ;
    }

    public static void SetupHttpClientHandlers(this IServiceCollection me)
    {
        me.AddHttpClient();

        // Rust scheduler service gRPC client
        var schedulerGrpcAddr = GetEnvValue("SCHEDULER_GRPC_URL", "http://scheduler:50053");
        me.AddGrpcClient<Api.V1.SchedulerService.SchedulerServiceClient>(o =>
            o.Address = new Uri(schedulerGrpcAddr));
        me.AddScoped<IReportServiceClient, ReportServiceClient>();

        var idgenAddr = GetEnvValue("IDGEN_GRPC_ADDR", "http://idgen:50051");
        me.AddGrpcClient<Api.V1.ApiService.ApiServiceClient>(o => o.Address = new Uri(idgenAddr));

        var boardcastAddr = GetEnvValue("BOARDCAST_GRPC_ADDR", "http://boardcast:50052");
        me.AddGrpcClient<Api.V1.BoardcastService.BoardcastServiceClient>(o => o.Address = new Uri(boardcastAddr));

        me.AddHttpClient(HttpClientHandlerTypes.ManualCertificate, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                return handler;
            });

        var tronProCryptoApiKey = GetEnvValue("TORNSCAN_IO_KEY");
        me.AddHttpClient(HttpClientHandlerTypes.TronPro, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("TRON-PRO-API-KEY", tronProCryptoApiKey);
        });

        var sharedCookieContainer = new CookieContainer();
        // Named HTTP client for MT5
        me.AddHttpClient("mt5", client =>
        {
            //client.BaseAddress = new Uri("https://13.212.107.58/"); // Delay to init MT5TickService time
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            
            // MT5 requires explicit Connection: keep-alive header to maintain stateful connection
            client.DefaultRequestHeaders.ConnectionClose = false; 
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        { 
            SslOptions = new SslClientAuthenticationOptions {
                RemoteCertificateValidationCallback = (_, __, ___, ____) => true // dev only
            },
            CookieContainer = sharedCookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AllowAutoRedirect = false,
            EnableMultipleHttp2Connections = false
        });
    }

    public static void DoCommandJobs(this WebApplicationBuilder me)
    {
        // Add Serilog to ServiceCollection so ILogger<T> uses Serilog
        me.Services.AddSerilog(Log.Logger);
        
        me.Services.AddTransient<CommandJob>()
            .AddTransient<CmdTestService>()
            .AddTransient<TradeMonitorService>()
            .AddTransient<PollEventTradeHandler>()
            .AddTransient<PollMetaTradeHandler>()
            .AddTransient<PollSendMessageHandler>()
            .AddSingleton<MyDbContextPool>()
            ;
        var commandService = new CommandJob(me.Services.BuildServiceProvider(), me, Environment.GetCommandLineArgs());
        commandService.RunAsync().Wait();
    }

    public static void LoadEnvironmentConfigure()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        string? configureFile = null;
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate)) { configureFile = candidate; break; }
            dir = dir.Parent;
        }

        if (configureFile == null)
            return;

        foreach (var line in File.ReadAllLines(configureFile))
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


    private static string GetEnvValue(string key, string fallback = "")
        => Environment.GetEnvironmentVariable(key) ?? fallback;

    public static void SetupTenancy(this WebApplicationBuilder me)
    {
        me.Services.AddScoped<Tenancy>()
            .AddScoped(typeof(ITenantGetter), svc => svc.GetRequiredService<Tenancy>())
            .AddScoped(typeof(ITenantSetter), svc => svc.GetRequiredService<Tenancy>())
            .AddScoped<MultiTenantServiceMiddleware>()
            ;
    }

    public static void SetupMiddlewareAndFilter(this WebApplicationBuilder me)
    {
        me.Services.AddScoped<AgentAreaFilter>()
            .AddScoped<RepAreaFilter>()
            .AddScoped<SalesAreaFilter>()
            .AddScoped<ClientAccountFilter>()
            .AddScoped<ClientWalletFilter>()
            .AddScoped<ConfigurationFilter>()
            .AddScoped<PermissionMiddleware>()
            .AddScoped<ErrorHandlingMiddleware>()
            .AddScoped<OriginValidationMiddleware>() // Add origin validation middleware
            ;
        if (IsApiLogEnable()) me.Services.AddScoped<ApiLogMiddleware>();
    }


    public static void SetupRedis(this WebApplicationBuilder me)
    { 
        var raw = GetEnvValue("REDIS_CONNECTION");
        var redisPassword = GetEnvValue("REDIS_PASSWORD");
        var isClusterMode = GetEnvValue("REDIS_CLUSTER_MODE", "false").Equals("true", StringComparison.OrdinalIgnoreCase);

        // Normalize REDIS_CONNECTION → host and port
        // Supports: redis://host:port, rediss://host:port, host:port, host
        string hostPort = raw.Replace("redis://", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("rediss://", "", StringComparison.OrdinalIgnoreCase)
                             .Trim();

        var parts = hostPort.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var host = parts.Length > 0 ? parts[0] : "localhost";
        var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 6379;

        var configOptions = new ConfigurationOptions
        {
            Password = redisPassword,
            // ElastiCache optimized configuration
            SyncTimeout = 10000,        // Increased for ElastiCache
            KeepAlive = 60,
            ConnectRetry = 10,          // More retries for ElastiCache
            AsyncTimeout = 10000,       // Increased for ElastiCache
            ConnectTimeout = 15000,     // Increased for ElastiCache initial connection
            AllowAdmin = !isClusterMode, // Disable admin commands for cluster mode
            ReconnectRetryPolicy = new ExponentialRetry(1000, 10000), // Better backoff for ElastiCache
            AbortOnConnectFail = false, // Don't crash if ElastiCache is temporarily unavailable
        };

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
            // Single-node (local dev or non-cluster ElastiCache) tuning
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
            options.ConfigurationOptions = configOptions; // single source of truth
            options.InstanceName = $"portal:{me.Environment.EnvironmentName.ToLower()}_";
        });
    }

    public static void SetupChatGpt(this IServiceCollection me)
    {
        var gptOptions = GptServiceOptions.Build(GetEnvValue("MIX_CHATGPT_API_KEY"));
        me.AddSingleton(_ => Options.Create(gptOptions)).AddScoped<GptService>();
    }

    public static void SetupCors(this IServiceCollection me)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var isTesting = environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);
        var isStaging = environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);
        var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
        
        // Testing and Staging are treated as production-like (Staging IS production currently)
        var useSpecificOrigins = isTesting || isStaging || isProduction;

        me.AddCors(options =>
        {
            if (useSpecificOrigins)
            {
                // Testing/Staging/Production: Use specific allowed origins
                // This fixes CORS errors when Authorization headers (credentials) are sent, wildcard * may not work for some browsers
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS#requests_with_credentials
                var allowedOrigins = GetProductionAllowedOrigins();

                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains() // Allow subdomains
                        .WithExposedHeaders("Connection", "Upgrade") // Expose WebSocket headers
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(600)); // Cache preflight result for 10 min
                });

                // Also add the AllowAll policy for backwards compatibility but log usage
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Connection", "Upgrade") // Expose WebSocket headers
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(600)); // Cache preflight result for 10 min
                });
            }
            else
            {
                // Development only: Allow all origins but still support credentials (needed for cookies)
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
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
            "https://midasmarkets.net",
            "https://tenant.midasmkts.com",
            "https://midasmkts.com",
            "https://au.midasmkts.com",
            "https://bvi.midasmkts.com",
            "https://sea.midasmkts.com",
            "https://www.midasmkts.com",
            "https://mm-front-clent.vercel.app",
            "https://mm-front-tenant.vercel.app"
        };
    }


    public static void SetupGeoIp(this IServiceCollection me)
    {
        var root = Directory.GetCurrentDirectory();
        var dbPath = Path.Combine(root, @"Resources/GeoLite2-Country_20230606/GeoLite2-Country.mmdb");
        if (!File.Exists(dbPath))
            throw new FileNotFoundException($"GeoIP DB file not exists", dbPath);
        me.AddSingleton<IGeoIp, MaxMindGeoIp>(_ => new MaxMindGeoIp(dbPath));
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

                var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                connection.ConnectionFailed += (_, _) => Console.WriteLine("SignalR connection to Redis failed.");

                if (!connection.IsConnected) throw new Exception("SignalR can not connect to Redis.");

                return connection;
            };
        });
    }


    public static bool IsApiLogEnable()
    {
        return !string.IsNullOrEmpty(GetEnvValue("API_LOG_ENABLE")) &&
               GetEnvValue("API_LOG_ENABLE").Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }

    public static void SetupTwilio(this IServiceCollection me)
    {
        var accountSid = GetEnvValue("TWILIO_ACCOUNT_SID");
        var authToken = GetEnvValue("TWILIO_AUTH_TOKEN");
        var serviceSid = GetEnvValue("TWILIO_SERVICE_SID");
        me.AddSingleton<ISmsVerification, TwilioSmsVerificationService>(_ =>
            new TwilioSmsVerificationService(accountSid, authToken, serviceSid));
    }

    public static void SetupIpInfo(this IServiceCollection me)
    {
        me.AddSingleton(_ => Options.Create(new IPInfoOptions
        {
            Token = GetEnvValue("IPINFO_TOKEN"),
            Endpoint = GetEnvValue("IPINFO_ENDPOINT")
        }));
    }

    private static bool IsSqlLogEnable()
    {
        return !string.IsNullOrEmpty(GetEnvValue("SQL_LOG_ENABLE")) &&
               GetEnvValue("SQL_LOG_ENABLE").Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool StartWithHubPath(PathString path)
    {
        return path.StartsWithSegments("/hub/client")
               || path.StartsWithSegments("/hub/tenant")
               || path.StartsWithSegments("/live/trade/symbol-group-hub"); // Add MarketHub support
    }

    private static bool StartWithMediaPath(PathString path)
    {
        return path.StartsWithSegments("/api/v1/client/image")
               || path.StartsWithSegments("/api/v1/client/media")
               || path.StartsWithSegments("/api/v1/tenant/image")
               || path.StartsWithSegments("/api/v1/tenant/media");
    }

    public static void AddSwaggerGenOptions(this IServiceCollection me)
    {
        if (IsProduction()) return;
        me.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Bacera Gateway API", Version = "v1" });
            options.TagActionsBy(apiDesc =>
            {
                var areaName = apiDesc.GetAreaName();
                return areaName == null ? new[] { "Default" } : areaName;
            });
            options.EnableAnnotations();
            options.CustomSchemaIds(x => x.FullName?.Replace("+", "."));
            // Include 'SecurityScheme' to use JWT Authentication
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on text box below!",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });
            // Added Details for Enum Type
            //options.SchemaFilter<Startup.EnumSchemaFilter>();
            // using System.Reflection;
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }

    public static void BuildSwagger(this IApplicationBuilder me)
    {
        if (IsProduction()) return;
        me.UseSwagger();
        me.UseSwaggerUI();
    }

    private static string GetWebAppEnv() => GetEnvValue("ASPNETCORE_ENVIRONMENT", "unknown");

    public static bool IsProduction() => AppEnvironment.IsProduction();

    private static bool IsLocal() => Environment.GetCommandLineArgs().Contains("local-dev");

    public static void InitializeApplicationServices(this IApplicationBuilder me)
    {
        var pool = me.ApplicationServices.GetDbPool();
        var cache = me.ApplicationServices.GetRedisCache();
        var monitor = me.ApplicationServices.GetRequiredService<MonitorService>();
        _ = monitor.ClearOnlineAdminAsync();
        // var tasks = CacheKeys.GetResetCacheKeys(pool.GetTenantIds()).Select(x => cache.KeyDeleteAsync(x)).ToList();

        var keys = CacheKeys.GetResetCacheKeys(pool.GetTenantIds());
        foreach (var key in keys)
        {
            cache.KeyDeleteAsync(key).Wait();
        }

        if (IsLocal()) return;

        var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        var pacificTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificZone);
        var pacificTime24HourFormat = pacificTime.ToString("yyyy-MM-dd HH:mm:ss");
        var webAppEnv = GetWebAppEnv();
        // Task.WhenAll(tasks).Wait();
        if (IsLocal()) return;
        BcrLog.Slack(
            $"BCR Web Api, [{webAppEnv}] started on [{Environment.MachineName}], on LA Time: [{pacificTime24HourFormat}]");
    }
}

public static class StartupExt
{
    public static List<string>? GetAreaName(this ApiDescription description)
    {
        if (description.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var groupName = controllerActionDescriptor.ControllerTypeInfo
                .GetCustomAttributes(typeof(TagsAttribute), true)
                .Cast<TagsAttribute>().FirstOrDefault();
            if (groupName != null) return new List<string> { groupName.Tags[0] };
        }

        var areaName = description.ActionDescriptor.RouteValues["Area"];
        if (areaName == null) return null;
        var areaList = new List<string> { areaName };
        description.RelativePath = $"{description.RelativePath}";
        return areaList;
    }
}