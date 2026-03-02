using Bacera.Gateway.Msg;
using Bacera.Gateway.Msg.Services;
using Bacera.Gateway.Services;
using Bacera.LiveTrade.MyHub;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

Startup.LoadEnvironmentConfigure();
IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);
builder.SetupUrls();
builder.SetupRedis();
builder.SetupLogging();
builder.SetupTenancy();
builder.SetupDbContext();

builder.Services.AddControllers();
builder.Services.SetupSignalR();
builder.Services.SetupCors();
builder.Services.AddWsAuthentication();

builder.Services.AddScoped<ChatService>();
builder.Services.AddSingleton<RedisPubSubWithAckService>();

var app = builder.Build();

// Use environment-appropriate CORS policy
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
var corsPolicy = isProduction ? "AllowSpecificOrigins" : "AllowAll";

app.UseCors(corsPolicy);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chat-hub");
app.InitServices();
app.Run();