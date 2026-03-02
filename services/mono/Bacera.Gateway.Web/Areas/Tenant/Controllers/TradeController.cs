using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using M = Bacera.Gateway.TradeService;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Trade")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TradeController(TenantDbContext tenantCtx, TradingService tradingService) : TenantBaseController
{
    /// <summary>
    /// Trade Transaction pagination (trade history)
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TradeViewModel>, TradeViewModel.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] TradeViewModel.Criteria? criteria)
    {
        criteria ??= new TradeViewModel.Criteria();
        if (criteria.ServiceId == null)
            return BadRequest(ResultMessage.Application.ServiceIdNotProvided);
        if (criteria.Commands == null && criteria.Command == null)
            criteria.Commands = new List<int> { 0, 1 };
        return Ok(await tradingService.QueryTrade(criteria));
    }

    /// <summary>
    /// Get service list
    /// </summary>
    /// <returns></returns>
    [HttpGet("service")]
    [ProducesResponseType(typeof(List<M>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index()
    {
        var items = await tenantCtx.TradeServices
            .Select(x => new M
            {
                Id = x.Id, Name = x.Name, Platform = x.Platform, Priority = x.Priority,
                Configuration = x.Configuration, Description = x.Description,
                IsAllowAccountCreation = x.IsAllowAccountCreation,
            })
            .OrderBy(x => x.Platform)
            .ThenBy(x => x.Priority)
            .ToListAsync();
        return Ok(items.Select(GetGroupsFromConfig).ToList());
    }

    [HttpGet("service/{id:int}/group")]
    public async Task<IActionResult> GetGroup(int id)
    {
        var item = await tenantCtx.TradeServices
            .Where(x => x.Id == id)
            .Select(x => x.Configuration)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound("__TRADE_SERVICE_NOT_FOUND__");

        if (!TradeServiceOptions.TryParse(item, out var config))
            return BadRequest("__TRADE_SERVICE_CONFIG_INVALID__");

        return Ok(config.Groups);
    }

    [HttpDelete("service/{id:int}/group")]
    public async Task<IActionResult> DeleteGroup(int id, M.AddOrDeleteGroupSpec spec)
    {
        var item = await tenantCtx.TradeServices.FindAsync(id);
        if (item == null)
            return NotFound("__TRADE_SERVICE_NOT_FOUND__");

        if (!TradeServiceOptions.TryParse(item.Configuration, out var config))
            return BadRequest("__TRADE_SERVICE_CONFIG_INVALID__");

        if (config.Groups == null || !config.Groups.Contains(spec.Group))
            return BadRequest("__TRADE_SERVICE_GROUP_ALREADY_EXISTS__");

        config.Groups.Remove(spec.Group);
        item.Configuration = JsonConvert.SerializeObject(config);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("service/{id:int}/group")]
    public async Task<IActionResult> AddGroup(int id, M.AddOrDeleteGroupSpec spec)
    {
        var item = await tenantCtx.TradeServices.FindAsync(id);
        if (item == null)
            return NotFound("__TRADE_SERVICE_NOT_FOUND__");

        if (!TradeServiceOptions.TryParse(item.Configuration, out var config))
            return BadRequest("__TRADE_SERVICE_CONFIG_INVALID__");

        config.Groups ??= [];
        if (config.Groups.Contains(spec.Group))
            return BadRequest("__TRADE_SERVICE_GROUP_ALREADY_EXISTS__");

        config.Groups.Add(spec.Group);
        item.Configuration = JsonConvert.SerializeObject(config);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("service/{id:int}/group")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] M.UpdateGroupSpec spec)
    {
        var item = await tenantCtx.TradeServices.FindAsync(id);
        if (item == null)
            return NotFound("__TRADE_SERVICE_NOT_FOUND__");

        if (!TradeServiceOptions.TryParse(item.Configuration, out var config))
            return BadRequest("__TRADE_SERVICE_CONFIG_INVALID__");

        config.Groups = spec.groups;
        item.Configuration = JsonConvert.SerializeObject(config);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    private static M.ClientResponseModel GetGroupsFromConfig(M item)
    {
        var configString = string.IsNullOrEmpty(item.Configuration) ? "{}" : item.Configuration;
        var config = JsonConvert.DeserializeObject<TradeServiceOptions>(configString);
        return new M.ClientResponseModel
        {
            Id = item.Id,
            Name = item.Name,
            Platform = item.Platform,
            Priority = item.Priority,
            Description = item.Description,
            IsAllowAccountCreation = item.IsAllowAccountCreation,
            Groups = config is { Groups: not null } ? config.Groups : new List<string>()
        };
    }
}