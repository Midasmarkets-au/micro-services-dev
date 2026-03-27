using System.Reflection;
using Bacera.Gateway.Web;
using Bacera.Gateway.Web.Middleware;
using Bacera.Gateway.Web.Middlewares;
using Bacera.Gateway.Web.WebSocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

Startup.LoadEnvironmentConfigure();
IdentityModelEventSource.ShowPII = true; //DEBUG for Identity

var builder = WebApplication.CreateBuilder(args);

// Add services to the container Yixuan.
builder.SetupAws();
builder.SetupRedis();
builder.SetupLogging();
builder.SetupOpenTelemetry();

// builder.SetupAuthentication();
// builder.SetupIdentity();
// builder.SetupIdentityServer();

// builder.Services.SetupMiddlewareAndFilter();
// builder.Services.SetupDbContext();
builder.SetupTenancy();
builder.SetupMiddlewareAndFilter();
builder.SetupDbContext();

builder.SetupAuthentication();
builder.SetupIdentity();
builder.SetupIdentityServer();
// AddIdentity() overrides DefaultAuthenticateScheme/DefaultChallengeScheme to the Identity
// cookie scheme. PostConfigure runs after all Configure calls so OpenIddict wins regardless
// of registration order, ensuring API endpoints challenge with 401 not a cookie redirect.
builder.Services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
{
    options.DefaultAuthenticateScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});
builder.Services.SetupDataProtection(builder.Configuration); // Add Data Protection with persistent key storage

// builder.Services.SetupAuthentication();
// builder.Services.SetupIdentity();
// builder.Services.SetupIdentityServer();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.SetupTwilio();
builder.Services.SetupIpInfo();
builder.Services.SetupSignalR();
builder.Services.SetupHangFire();
builder.Services.SetupHttpClientHandlers();
builder.Services.SetupApplicationServices();
builder.Services.SetupCors();
builder.Services.SetupGeoIp();
builder.Services.SetupChatGpt();
builder.Services.SetupHangFireServer();
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcReflection();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        //options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenOptions();

if (Environment.GetCommandLineArgs().Contains("cmd"))
{
    builder.DoCommandJobs();
    Log.CloseAndFlush();  // Ensure all logs are written before exit
    Environment.Exit(0);
}

// Application Level
var app = builder.Build();

// *** Use environment-appropriate CORS policy ***
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
var isStaging = environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);
var isTesting = environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);
var corsPolicy = (isProduction || isStaging || isTesting) ? "AllowSpecificOrigins" : "AllowAll";

app.UseCors(corsPolicy);
app.BuildSwagger();
// Configure the HTTP request pipeline.

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
    {
        if (httpContext.Request.ContentType?.StartsWith("application/grpc") == true)
            return LogEventLevel.Verbose; // gRPC has its own logging; skip here
        if (httpContext.Response.StatusCode >= 500 || ex != null)
            return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;
        return LogEventLevel.Information;
    };
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    // Pick up business fields stored by ApiLogMiddleware
    options.EnrichDiagnosticContext = (diag, ctx) =>
    {
        if (ctx.Items.TryGetValue("log.TenantId", out var tenantId)) diag.Set("TenantId", tenantId);
        if (ctx.Items.TryGetValue("log.IsTenantSet", out var isTenantSet)) diag.Set("IsTenantSet", isTenantSet);
        if (ctx.Items.TryGetValue("log.PartyId", out var partyId)) diag.Set("PartyId", partyId);
        if (ctx.Items.TryGetValue("log.GodPartyId", out var godPartyId)) diag.Set("GodPartyId", godPartyId);
        if (ctx.Items.TryGetValue("log.Ip", out var ip)) diag.Set("Ip", ip);
        if (ctx.Items.TryGetValue("log.UserAgent", out var userAgent)) diag.Set("UserAgent", userAgent);
        if (ctx.Items.TryGetValue("log.Referer", out var referer)) diag.Set("Referer", referer);
        if (ctx.Items.TryGetValue("log.QueryParams", out var queryParams)) diag.Set("QueryParams", queryParams);
        if (ctx.Items.TryGetValue("log.RequestBody", out var requestBody)) diag.Set("RequestBody", requestBody);
    };
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

// *** Add origin validation middleware early in the pipeline for production security ***
app.UseMiddleware<OriginValidationMiddleware>();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<MultiTenantServiceMiddleware>();
app.UseMiddleware<PermissionMiddleware>();
if (Startup.IsApiLogEnable())
{
    app.UseMiddleware<ApiLogMiddleware>();
}


// web socket
app.MapHub<NotificationHub>("/hub/client");
app.MapHub<NotificationHub>("/hub/tenant");
app.MapHub<MarketHub>("/live/trade/symbol-group-hub");

// MVC
app.MapControllerRoute(
    name: "MyAreas",
    pattern: "api/{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ─── gRPC JSON Transcoding services (replace RESTful controllers incrementally) ───
// Discovery: public endpoints, no authentication required
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Discovery.DiscoveryGrpcService>().AllowAnonymous();

// Report: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Report.TenantReportGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Report.TenantAccountReportGrpcService>().RequireAuthorization();

// Symbol + ExchangeRate: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Symbol.TenantSymbolGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Symbol.TenantExchangeRateGrpcService>().RequireAuthorization();

// Config: tenant-scoped, requires authentication
// ConfigurationController is kept for site-specific toggle endpoints not yet in proto.
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Config.TenantConfigurationGrpcService>().RequireAuthorization();

// User / Role / Permission: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.User.TenantUserGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.User.TenantRoleGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.User.TenantPermissionGrpcService>().RequireAuthorization();

// KYC: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.User.TenantKycGrpcService>().RequireAuthorization();

// Verification: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.User.TenantVerificationGrpcService>().RequireAuthorization();

// Account: tenant/client/sales/agent/rep-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Account.TenantAccountGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Account.TenantAccountV2GrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Account.ClientAccountGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Account.SalesAccountGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Account.AgentAccountGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Account.RepAccountGrpcService>().RequireAuthorization();

// Notification: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Notification.TenantMessageGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Notification.TenantEmailGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Notification.TenantTopicGrpcService>().RequireAuthorization();

// Admin: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantAuditGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantIpBlacklistGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantUserBlacklistGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantApiLogGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantStatisticsGrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantStatisticsV2GrpcService>().RequireAuthorization();
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Admin.TenantAdminGrpcService>().RequireAuthorization();

// Lead: tenant-scoped, requires authentication
app.MapGrpcService<Bacera.Gateway.Web.HttpServices.Lead.TenantLeadGrpcService>().RequireAuthorization();

// MonoCallbackService: called by the Rust scheduler via standard gRPC (tonic, HTTP/2).
// No UseGrpcWeb: gRPC-Web is for browsers; tonic uses standard gRPC and is incompatible with gRPC-Web encoding.
// AllowAnonymous: internal cluster call from scheduler, no JWT token.
app.MapGrpcService<Bacera.Gateway.Web.Grpc.MonoCallbackGrpcService>().AllowAnonymous();

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

// HTTP REST endpoints mirroring proto's google.api.http annotations
app.MapGet("/api/v1/health", () => Results.Ok(new { status = "SERVING" }));
app.MapGet("/api/v1/hello", (string? name) => Results.Ok(new { message = $"Hello, {(string.IsNullOrEmpty(name) ? "World" : name)}!" }));
app.MapGet("/api/v1/generateid", async (Api.V1.ApiService.ApiServiceClient idgenClient, uint workID = 0) =>
{
    var reply = await idgenClient.GenerateIDAsync(new Api.V1.GenerateIDRequest { WorkID = workID });
    return Results.Ok(new { id = reply.Id });
});
app.MapGet("/api/v1/rpc/test", async (Api.V1.ApiService.ApiServiceClient idgenClient, uint workId = 0) =>
{
    var reply = await idgenClient.GenerateIDAsync(new Api.V1.GenerateIDRequest { WorkID = workId });
    return Results.Ok(new { id = reply.Id });
});

app.UseHangfireJobSetup();

app.InitializeApplicationServices();
app.Run();
