using Bacera.Gateway.Auth.Db;
using Bacera.Gateway.Auth.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

// Load .env file (same logic as mono's LoadEnvironmentConfigure).
// Existing env vars (K8s/Docker/Compose) take priority and are NOT overwritten.
LoadEnvFile();

// ASPNETCORE_URLS in the shared .env is for mono (https://localhost:5001).
// Always override it with HTTP_ADDR so this service binds to the correct port.
var httpAddr = Env("HTTP_ADDR", "http://[::]:8081");
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", httpAddr);

var builder = WebApplication.CreateBuilder(args);

// Bind env vars into configuration (mirrors mono's GetEnvValue pattern)
builder.Configuration.AddEnvironmentVariables();

var dbHost = Env("DB_HOST", "localhost");
var dbPort = Env("DB_PORT", "5432");
var dbUser = Env("DB_USERNAME", "postgres");
var dbPassword = Env("DB_PASSWORD", "");
var dbDatabase = Env("DB_DATABASE", "portal_central");
var connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbDatabase}";

builder.Services.AddDbContext<AuthDbContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddDbContext<CentralDbContext>(opt => opt.UseNpgsql(connectionString));

// Redis — used by LoginSecurityService for account lockout (same keys as mono)
var redisConn = Env("REDIS_CONNECTION", "localhost:6379");
var redisPassword = Env("REDIS_PASSWORD", "");
var redisConfigOpts = ConfigurationOptions.Parse(redisConn);
if (!string.IsNullOrEmpty(redisPassword))
    redisConfigOpts.Password = redisPassword;
redisConfigOpts.AbortOnConnectFail = false;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfigOpts));

builder.Services.AddSingleton<LoginSecurityService>();
builder.Services.AddSingleton<TenantDbContextFactory>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddHealthChecks();
builder.Services.AddControllers();

var allowedOrigins = Env("CORS_ALLOWED_ORIGINS", "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
    {
        if (allowedOrigins.Length > 0)
            p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        else
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }));

builder.WebHost.UseUrls(httpAddr);

var app = builder.Build();

app.UseCors();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

static void LoadEnvFile()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    string? path = null;
    while (dir != null)
    {
        var candidate = Path.Combine(dir.FullName, ".env");
        if (File.Exists(candidate)) { path = candidate; break; }
        dir = dir.Parent;
    }
    if (path is null) return;

    foreach (var line in File.ReadLines(path))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var idx = trimmed.IndexOf('=');
        if (idx <= 0) continue;
        var key = trimmed[..idx].Trim();
        var value = trimmed[(idx + 1)..].Trim().Trim('"');
        if (string.IsNullOrEmpty(key)) continue;
        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

static string Env(string key, string fallback = "") =>
    Environment.GetEnvironmentVariable(key) ?? fallback;
