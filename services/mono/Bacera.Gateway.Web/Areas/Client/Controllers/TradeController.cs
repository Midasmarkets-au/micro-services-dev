
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Trade")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.AllClient + "," + UserRoleTypesString.TenantAdmin)]
public class TradeController : ClientBaseController
{
    private readonly TenantDbContext _ctx;

    public TradeController(TenantDbContext tenantDbContext)
    {
        _ctx = tenantDbContext;
    }

    /// <summary>
    /// Get all Services
    /// </summary>
    /// <returns></returns>
    [HttpGet("service")]
    [ProducesResponseType(typeof(List<TradeService>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TradeService>>> Index()
    {
        var items = await _ctx.TradeServices
            .Select(x =>
                new TradeService
                {
                    Id = x.Id,
                    Name = x.Name,
                    Platform = x.Platform,
                    Priority = x.Priority,
                    Description = x.Description,
                    IsAllowAccountCreation = x.IsAllowAccountCreation
                })
            .OrderBy(x => x.Platform)
            .ThenBy(x => x.Priority)
            .ToListAsync();
        return Ok(items.Select(GetServiceInfoForClient).ToList());
    }

    private static object GetServiceInfoForClient(TradeService item)
        => new
        {
            item.Id,
            item.Name,
            item.Platform,
            item.Priority,
            item.Description,
        };
}