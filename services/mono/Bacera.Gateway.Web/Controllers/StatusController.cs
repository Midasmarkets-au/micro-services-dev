
using System.Security.Claims;
using Bacera.Gateway.Web.WebSocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers;

[ApiController]
[Route("/api/status")]
[Tags("Public/Status")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class StatusController : Controller
{
    private readonly ITenantGetter _tenantGetter;
    private readonly Tenancy _tenancy;
    private readonly IServiceProvider _serviceProvider;
    private readonly CentralDbContext _centralDbContext;
    private readonly ILogger<StatusController> _logger;
    private readonly IHubContext<NotificationHub> _clientHubContext;

    public StatusController(
        ITenantGetter tenantGetter
        , Tenancy tenancy
        , IServiceProvider serviceProvider
        , CentralDbContext centralDbContext
        , IHubContext<NotificationHub> clientHubContext, ILogger<StatusController> logger)
    {
        _tenantGetter = tenantGetter;
        _serviceProvider = serviceProvider;
        _tenancy = tenancy;
        _centralDbContext = centralDbContext;
        _clientHubContext = clientHubContext;
        _logger = logger;
    }

    /// <summary>
    /// Get current User info
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Index() =>
        User.Identity is not ClaimsIdentity identity
            ? Ok(new { Message = "User not found" })
            : Ok(new
            {
                identity.Name,
                identity.IsAuthenticated,
                TenantId = _tenancy.GetTenantId(),
                Claims = identity.Claims.Select(x => new { x.Type, x.Value }).ToList()
            });

    /// <summary>
    /// Ping
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        // throw new Exception();
        _clientHubContext.Clients.All.SendAsync("ReceiveMessage", "Pong@All");
        _clientHubContext.Clients.Group("GROUP:TENANT:" + _tenantGetter.GetTenantId())
            .SendAsync("ReceiveMessage", "Pong@TENANT:" + _tenantGetter.GetTenantId());
        return Ok(new { Message = "Pong" });
    }

    /// <summary>
    /// Get current Domain and TenantId
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("domain")]
    public IActionResult Domain()
        => Ok(new { Host = Request.Host.Value, TenantId = _tenancy.GetTenantId() });

    /// <summary>
    /// Get server date time
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("datetime")]
    public IActionResult Datetime() => Ok(new { Datetime = DateTime.UtcNow });

    /// <summary>
    /// Get Tenant scope for testing
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("scope")]
    public async Task<IActionResult> Scope()
    {
        var count = new Dictionary<long, int>();
        var tenants = await _centralDbContext.Tenants.Select(x => new { x.Id }).ToListAsync();

        foreach (var t in tenants)
        {
            using var scope = _serviceProvider.CreateScope();
            var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
            s.SetTenantId(t.Id);
            var svc = scope.ServiceProvider.GetRequiredService<TradingService>();
            var c = await svc.GetAccountCountAsync();
            count.Add(t.Id, c);
        }

        return Ok(count);
    }
}