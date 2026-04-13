
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSG = Bacera.Gateway.ResultMessage.RebateRule;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Rebate Schema Bundle")]
[Route("api/" + VersionTypes.V1 + "/[Area]/rebate-schema-bundle")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class RebateSchemaBundleController : TenantBaseController
{
    private readonly TenantDbContext _tenantDbContext;

    public RebateSchemaBundleController(
        TenantDbContext tenantDbContext
    )
    {
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Get Bundle
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns></returns>
    [HttpGet("{bundleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateSchemaBundle))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BundleGet(long bundleId)
    {
        var result = await _tenantDbContext.RebateSchemaBundles
            .SingleOrDefaultAsync(x => x.Id == bundleId);
        return result == null ? NotFound() : Ok(result);
    }

    private Task<List<string>> GetClientCodes() =>
        _tenantDbContext.Symbols.ToRebateSymbols(400).Select(x => x.Code).ToListAsync();
    /// <summary>
    /// Create Bundle
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(RebateSchemaBundle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BundleCreate([FromBody] RebateSchemaBundle.CreateSpec spec)
    {
        var clientCodes = await GetClientCodes();
        if (!spec.Items.All(x => clientCodes.Contains(x.Key)))
            return BadRequest(Result.Error(MSG.SymbolIdNotExists));

        if (await _tenantDbContext.RebateSchemaBundles
                .Where(x => x.Name == spec.Name)
                .AnyAsync())
            return BadRequest(Result.Error(MSG.NameExists));

        var item = new RebateSchemaBundle
        {
            Name = spec.Name,
            Note = spec.Note ?? string.Empty,
            Type = (int)spec.Type,
            CreatedBy = GetPartyId(),
            Items = spec.Items,
        };

        await _tenantDbContext.RebateSchemaBundles.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Update Bundle
    /// </summary>
    /// <returns></returns>
    [HttpPut("{bundleId:long}")]
    [ProducesResponseType(typeof(RebateSchemaBundle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BundleUpdate(long bundleId, [FromBody] RebateSchemaBundle.CreateSpec spec)
    {
        var clientCodes = await GetClientCodes();
        if (!spec.Items.All(x => clientCodes.Contains(x.Key)))
            return BadRequest(Result.Error(MSG.SymbolIdNotExists));

        var item = await _tenantDbContext.RebateSchemaBundles
            .SingleOrDefaultAsync(x => x.Id == bundleId);
        if (item == null)
            return NotFound();

        item.Name = spec.Name;
        item.Note = spec.Note ?? string.Empty;
        item.Items = spec.Items;
        // item.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.RebateSchemaBundles.Update(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Pagination Bundle
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateSchemaBundle>, RebateSchemaBundle.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> BundlePagination([FromQuery] RebateSchemaBundle.Criteria? criteria)
    {
        criteria ??= new RebateSchemaBundle.Criteria();
        var result = await _tenantDbContext.RebateSchemaBundles
            .PagedFilterBy(criteria)
            .Select(x => new RebateSchemaBundle
            {
                Id = x.Id,
                Name = x.Name,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                Type = x.Type,
                Note = x.Note,
            })
            .ToListAsync();
        return Ok(Result<List<RebateSchemaBundle>, RebateSchemaBundle.Criteria>.Of(result, criteria));
    }

    /// <summary>
    /// Bundle Id Name List
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(Result<List<KeyValuePair<string, long>>, RebateSchemaBundle.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> BundleList([FromQuery] RebateSchemaBundle.Criteria? criteria)
    {
        criteria ??= new RebateSchemaBundle.Criteria();
        var result = await _tenantDbContext.RebateSchemaBundles
            .PagedFilterBy(criteria)
            .Select(x => new KeyValuePair<string, long>(x.Name, x.Id))
            .ToListAsync();
        return Ok(Result<List<KeyValuePair<string, long>>, RebateSchemaBundle.Criteria>.Of(result, criteria));
    }


    /// <summary>
    /// Delete Bundle
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns></returns>
    [HttpDelete("{bundleId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BundleDelete(long bundleId)
    {
        var result = await _tenantDbContext.RebateSchemaBundles
            .SingleOrDefaultAsync(x => x.Id == bundleId);
        if (result == null)
            return NotFound();
        try
        {
            _tenantDbContext.RebateSchemaBundles.Remove(result);

            return await _tenantDbContext.SaveChangesAsync() > 0
                ? NoContent()
                : BadRequest(ResultMessage.Common.DeleteFail);
        }
        catch (Exception)
        {
            return BadRequest(ResultMessage.Common.DeleteFail);
        }
    }
}