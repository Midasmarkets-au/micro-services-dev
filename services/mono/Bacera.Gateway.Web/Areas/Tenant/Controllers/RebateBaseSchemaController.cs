using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSG = Bacera.Gateway.ResultMessage.RebateRule;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Rebate Base Schema")]
[Route("api/" + VersionTypes.V1 + "/[Area]/rebate-base-schema")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RebateBaseSchemaController : TenantBaseController
{
    private readonly TenantDbContext _tenantDbContext;

    public RebateBaseSchemaController(
        TenantDbContext tenantDbContext
    )
    {
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Pagination BaseSchema
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateBaseSchema.ResponseModel>, RebateBaseSchema.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> BaseSchemaPagination([FromQuery] RebateBaseSchema.Criteria? criteria)
    {
        criteria ??= new RebateBaseSchema.Criteria();
        var result = await _tenantDbContext.RebateBaseSchemas
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        return Ok(Result<List<RebateBaseSchema.ResponseModel>, RebateBaseSchema.Criteria>.Of(result, criteria));
    }

    /// <summary>
    /// Base schema list for ID/Name
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(Result<List<KeyValuePair<string, long>>, RebateBaseSchema.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> BaseSchemaList([FromQuery] RebateBaseSchema.Criteria? criteria)
    {
        criteria ??= new RebateBaseSchema.Criteria();
        criteria.Size = 1000;
        var result = await _tenantDbContext.RebateBaseSchemas
            .PagedFilterBy(criteria)
            .Select(x => new KeyValuePair<string, long>(x.Name, x.Id))
            .ToListAsync();
        return Ok(Result<List<KeyValuePair<string, long>>, RebateBaseSchema.Criteria>.Of(result, criteria));
    }

    /// <summary>
    /// Get Base Schema
    /// </summary>
    /// <param name="baseSchemaId"></param>
    /// <returns></returns>
    [HttpGet("{baseSchemaId:long}")]
    [ProducesResponseType(typeof(Result<List<RebateBaseSchema>, RebateBaseSchema.Criteria>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaseSchemaGet(long baseSchemaId)
    {
        var result = await _tenantDbContext.RebateBaseSchemas
            .Include(x => x.RebateBaseSchemaItems)
            .SingleOrDefaultAsync(x => x.Id == baseSchemaId);
        return result == null ? NotFound() : Ok(result);
    }

    private Task<List<string>> GetClientSymbolCodes() => _tenantDbContext.Symbols
        .ToRebateSymbols(400)
        .Select(x => x.Code)
        .ToListAsync();
    
    /// <summary>
    /// Create Base Schema
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(RebateBaseSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaseSchemaCreate([FromBody] RebateBaseSchema.CreateSpec spec)
    {
        var clientCodes = await GetClientSymbolCodes();

        if (!spec.Items.All(x => clientCodes.Contains(x.SymbolCode)))
            return BadRequest(Result.Error(MSG.SymbolIdNotExists));

        var items = spec.Items
            .Where(x => x.Rate > 0 || x.Pips > 0 || x.Commission > 0)
            .Select(x => new RebateBaseSchemaItem
            {
                SymbolCode = x.SymbolCode, Rate = x.Rate, Pips = x.Pips, Commission = x.Commission
            })
            .ToList();
        if (!items.Any())
            return BadRequest(Result.Error(MSG.ItemsShouldNotBeEmpty));

        if (await _tenantDbContext.RebateBaseSchemas.Where(x => x.Name == spec.Name).AnyAsync())
            return BadRequest(Result.Error(MSG.NameExists));

        var item = new RebateBaseSchema
        {
            Name = spec.Name,
            Note = spec.Note,
            CreatedBy = GetPartyId(),
            RebateBaseSchemaItems = items
        };

        await _tenantDbContext.RebateBaseSchemas.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Update Base Schema
    /// </summary>
    /// <returns></returns>
    [HttpPut("{baseSchemaId:long}")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaseSchemaUpdate(long baseSchemaId, [FromBody] RebateBaseSchema.UpdateSpec spec)
    {
        var item = await _tenantDbContext.RebateBaseSchemas
            .Where(x => x.Id == baseSchemaId)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        item.Name = spec.Name;
        item.Note = spec.Note;
        item.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.RebateBaseSchemas.Update(item);
        await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        return Ok(item);
    }

    /// <summary>
    /// Delete Direct Schema
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{baseSchemaId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaseSchemaDelete(long baseSchemaId)
    {
        var item = await _tenantDbContext.RebateBaseSchemas
            .Include(x => x.TradeAccounts.Select(ta => new TradeAccount { Id = x.Id }))
            .Where(x => x.Id == baseSchemaId)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        if (item.TradeAccounts.Any())
            return BadRequest(Result.Error(MSG.SchemaInUse, item.TradeAccounts.Select(x => x.Id).ToList()));

        try
        {
            _tenantDbContext.RebateBaseSchemas.Remove(item);
            return await _tenantDbContext.SaveChangesAsync() > 0
                ? NoContent()
                : BadRequest(Result.Error(ResultMessage.Common.DeleteFail));
        }
        catch (Exception)
        {
            return BadRequest(Result.Error(ResultMessage.Common.DeleteFail));
        }
    }

    /// <summary>
    /// Create BaseSchema Item
    /// </summary>
    /// <returns></returns>
    [HttpPost("{baseSchemaId:long}/item")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaseSchemaItemCreate(long baseSchemaId,
        [FromBody] RebateDirectSchemaItem.CreateSpec spec)
    {
        var clientCodes = await GetClientSymbolCodes();
        if (!clientCodes.Contains(spec.SymbolCode))
            return BadRequest(Result.Error(MSG.SymbolIdNotExists));

        var item = new RebateBaseSchemaItem
        {
            RebateBaseSchemaId = baseSchemaId,
            SymbolCode = spec.SymbolCode,
            Rate = spec.Rate,
            Pips = spec.Pips,
            Commission = spec.Commission
        };
        await _tenantDbContext.RebateBaseSchemaItems.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Update BaseSchema Item
    /// </summary>
    /// <returns></returns>
    [HttpPut("{baseSchemaId:long}/item/{baseSchemaItemId:long}")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> BaseSchemaItemUpdate(long baseSchemaId, long baseSchemaItemId,
        [FromBody] RebateDirectSchemaItem.UpdateSpec spec)
    {
        var item = await _tenantDbContext.RebateBaseSchemaItems
            .Where(x => x.Id == baseSchemaItemId && x.RebateBaseSchemaId == baseSchemaId)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        item.Rate = spec.Rate;
        item.Pips = spec.Pips;
        item.Commission = spec.Commission;
        item.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.RebateBaseSchemaItems.Update(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Batch update BaseSchema Items
    /// </summary>
    /// <returns></returns>
    [HttpPut("{baseSchemaId:long}/items")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> BaseSchemaItemBatchUpdate(long baseSchemaId,
        [FromBody] List<RebateDirectSchemaItem.CreateSpec> spec)
    {
        // spec = spec.Where(x => x.Rate > 0 || x.Pips > 0 || x.Commission > 0).ToList();
        var items = await _tenantDbContext.RebateBaseSchemaItems
            .Where(x => x.RebateBaseSchemaId == baseSchemaId)
            .Where(x => spec.Select(y => y.SymbolCode).Contains(x.SymbolCode))
            .ToListAsync();

        var newRuleItems = new List<RebateBaseSchemaItem>();
        foreach (var item in spec)
        {
            var ruleItem = items.FirstOrDefault(x => x.SymbolCode == item.SymbolCode);
            if (ruleItem == null)
            {
                var newRuleItem = new RebateBaseSchemaItem
                {
                    RebateBaseSchemaId = baseSchemaId, SymbolCode = item.SymbolCode, Rate = item.Rate, Pips = item.Pips,
                    Commission = item.Commission
                };
                newRuleItems.Add(newRuleItem);
            }
            else
            {
                ruleItem.Rate = item.Rate;
                ruleItem.Pips = item.Pips;
                ruleItem.Commission = item.Commission;
                ruleItem.UpdatedOn = DateTime.UtcNow;
                _tenantDbContext.RebateBaseSchemaItems.Update(ruleItem);
            }
        }

        if (newRuleItems.Any())
            await _tenantDbContext.RebateBaseSchemaItems.AddRangeAsync(newRuleItems);

        await _tenantDbContext.SaveChangesAsync();
        items.AddRange(newRuleItems);
        return Ok(items);
    }

    /// <summary>
    /// Get BaseSchema Item
    /// </summary>
    /// <returns></returns>
    [HttpGet("{baseSchemaId:long}/item/{baseSchemaItemId:long}")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaseSchemaItemGet(long baseSchemaId, long baseSchemaItemId)
    {
        var item = await _tenantDbContext.RebateBaseSchemaItems
            .Where(x => x.Id == baseSchemaItemId && x.RebateBaseSchemaId == baseSchemaId)
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }
}