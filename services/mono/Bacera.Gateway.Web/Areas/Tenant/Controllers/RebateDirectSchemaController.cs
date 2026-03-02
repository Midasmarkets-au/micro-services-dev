using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Rebate Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/rebate-direct-schema")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RebateRuleController : TenantBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly TenantDbContext _tenantDbContext;
    private readonly UserManager<Auth.User> _userManager;
    private readonly ILogger<RebateRuleController> _logger;

    public RebateRuleController(
        TradingService tradingService
        , UserManager<Auth.User> userManager
        , TenantDbContext tenantDbContext
        , ILogger<RebateRuleController> logger)
    {
        _logger = logger;
        _userManager = userManager;
        _tradingSvc = tradingService;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Pagination Rebate Direct Schema
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateDirectSchema.ResponseModel>, RebateDirectSchema.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> QueryRebateDirectSchema([FromQuery] RebateDirectSchema.Criteria? criteria)
    {
        criteria ??= new RebateDirectSchema.Criteria();
        var result = await _tenantDbContext.RebateDirectSchemas
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        return Ok(Result<List<RebateDirectSchema.ResponseModel>, RebateDirectSchema.Criteria>.Of(result, criteria));
    }

    /// <summary>
    /// Rebate Direct Schema list 
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(Result<List<KeyValuePair<string, long>>, RebateDirectSchema.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> QueryRebateDirectSchemaList([FromQuery] RebateDirectSchema.Criteria? criteria)
    {
        criteria ??= new RebateDirectSchema.Criteria();
        var result = await _tenantDbContext.RebateDirectSchemas
            .PagedFilterBy(criteria)
            .Select(x => new KeyValuePair<string, long>(x.Name, x.Id))
            .ToListAsync();
        return Ok(Result<List<KeyValuePair<string, long>>, RebateDirectSchema.Criteria>.Of(result, criteria));
    }

    /// <summary>
    /// Get Rebate Direct Schema
    /// </summary>
    /// <param name="schemaId"></param>
    /// <returns></returns>
    [HttpGet("{schemaId:long}")]
    [ProducesResponseType(typeof(Result<List<RebateDirectSchema>, RebateDirectSchema.Criteria>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRebateDirectSchema(long schemaId)
    {
        var result = await _tenantDbContext.RebateDirectSchemas
            .Include(x => x.RebateDirectSchemaItems)
            .SingleOrDefaultAsync(x => x.Id == schemaId);
        return result == null ? NotFound() : Ok(result);
    }

    private Task<List<string>> GetClientCodes() =>
        _tenantDbContext.Symbols.ToRebateSymbols(400).Select(x => x.Code).ToListAsync();
    /// <summary>
    /// Create Rebate Direct Schema
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRebateDirectSchema([FromBody] RebateDirectSchema.CreateSpec spec)
    {
        var clientCodes = await GetClientCodes();
        if (!spec.Items.All(x => clientCodes.Contains(x.SymbolCode)))
            return BadRequest(Result.Error(ResultMessage.RebateRule.SymbolIdNotExists));

        var items = spec.Items
            .Where(x => x.Rate > 0 || x.Pips > 0 || x.Commission > 0)
            .Select(x => new RebateDirectSchemaItem
            {
                SymbolCode = x.SymbolCode, Rate = x.Rate, Pips = x.Pips, Commission = x.Commission
            })
            .ToList();
        if (!items.Any())
            return BadRequest(Result.Error(ResultMessage.RebateRule.ItemsShouldNotBeEmpty));

        if (await _tenantDbContext.RebateDirectSchemas.Where(x => x.Name == spec.Name).AnyAsync())
            return BadRequest(Result.Error(ResultMessage.RebateRule.NameExists));

        var item = new RebateDirectSchema
        {
            Name = spec.Name,
            Note = spec.Note,
            CreatedBy = GetPartyId(),
            RebateDirectSchemaItems = items
        };

        await _tenantDbContext.RebateDirectSchemas.AddAsync(item);
        await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        return Ok(item);
    }

    /// <summary>
    /// Confirm Rebate Direct Schema
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}/confirm")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmRebateDirectSchema(long schemaId)
    {
        var item = await _tenantDbContext.RebateDirectSchemas
            .Where(x => x.Id == schemaId)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        if (item.CreatedBy == GetPartyId())
            return BadRequest(Result.Error(ResultMessage.RebateRule.CanNotConfirmByCreator));

        item.ConfirmedOn = DateTime.UtcNow;
        item.ConfirmedBy = GetPartyId();
        item.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.RebateDirectSchemas.Update(item);
        await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        return Ok(item);
    }

    /// <summary>
    /// Update Rebate Direct Schema (Remark only)
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateDirectSchemaUpdate(long schemaId,
        [FromBody] RebateDirectSchema.UpdateSpec spec)
    {
        var item = await _tenantDbContext.RebateDirectSchemas
            .Where(x => x.Id == schemaId)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        item.Name = spec.Name;
        item.Note = spec.Note;
        item.UpdatedOn = DateTime.UtcNow;

        try
        {
            _tenantDbContext.RebateDirectSchemas.Update(item);
            await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
            return Ok(item);
        }
        catch (Exception e)
        {
            _logger.LogError("Update Rebate Rule error. Id:{Id}  PartyId: {PartyId}", item.Id, GetPartyId());
            return BadRequest(Result.Error(e.Message));
        }
    }

    // /// <summary>
    // /// Create Rebate Direct Schema
    // /// </summary>
    // /// <returns></returns>
    // [HttpDelete("{schemaId:long}")]
    // [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    // public async Task<IActionResult> RebateDirectSchemaDelete(long schemaId)
    // {
    //     var item = await _tenantDbContext.RebateDirectSchemas
    //         .Where(x => x.Id == schemaId)
    //         .SingleOrDefaultAsync();
    //     if (item == null)
    //         return NotFound();
    //
    //     try
    //     {
    //         _tenantDbContext.RebateDirectSchemas.Remove(item);
    //         await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
    //         return NoContent();
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError("Delete Rebate Rule error. Id:{Id}  PartyId: {PartyId}", item.Id, GetPartyId());
    //         return BadRequest(Result.Error(e.Message));
    //     }
    // }

    // long tradeServiceId,
    //     RebateDirectSchemaItem.CreateSpec directSchemaItem, int volume,
    //     CurrencyTypes sourceCurrencyId = CurrencyTypes.USD
    /// <summary>
    /// Create Rebate Direct Schema Item
    /// </summary>
    /// <returns></returns>
    [HttpPost("{schemaId:long}/item")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateDirectSchemaItemCreate(long schemaId,
        [FromBody] RebateDirectSchemaItem.CreateSpec spec)
    {
        var clientCodes = await GetClientCodes();
        if (!clientCodes.Contains(spec.SymbolCode))
            return BadRequest(Result.Error(ResultMessage.RebateRule.SymbolIdNotExists));

        var item = new RebateDirectSchemaItem
        {
            RebateDirectSchemaId = schemaId,
            SymbolCode = spec.SymbolCode,
            Rate = spec.Rate,
            Pips = spec.Pips,
            Commission = spec.Commission
        };

        try
        {
            await _tenantDbContext.RebateDirectSchemaItems.AddAsync(item);
            await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
            return Ok(item);
        }
        catch (Exception e)
        {
            _logger.LogError("Create Rebate Rule Item error. Id:{Id}  PartyId: {PartyId}", item.Id, GetPartyId());
            return BadRequest(Result.Error(e.Message));
        }
    }

    /// <summary>
    /// Update Rebate Direct Schema Item
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}/item/{schemaItemId:long}")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateDirectSchemaItemUpdate(long schemaId, long schemaItemId,
        [FromBody] RebateDirectSchemaItem.UpdateSpec spec)
    {
        var item = await _tenantDbContext.RebateDirectSchemaItems
            .Where(x => x.Id == schemaItemId && x.RebateDirectSchemaId == schemaId)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        item.Rate = spec.Rate;
        item.Pips = spec.Pips;
        item.Commission = spec.Commission;
        item.UpdatedOn = DateTime.UtcNow;

        try
        {
            _tenantDbContext.RebateDirectSchemaItems.Update(item);
            await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
            return Ok(item);
        }
        catch (Exception e)
        {
            _logger.LogError("Update Rebate Rule Item error. Id:{Id}  PartyId: {PartyId}", item.Id, GetPartyId());
            return BadRequest(Result.Error(e.Message));
        }
    }

    /// <summary>
    /// Batch update Rebate Direct Schema Items
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}/items")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateDirectSchemaItemBatchUpdate(long schemaId,
        [FromBody] List<RebateDirectSchemaItem.CreateSpec> spec)
    {
        spec = spec.Where(x => x.Rate > 0 || x.Pips > 0 || x.Commission > 0).ToList();
        var items = await _tenantDbContext.RebateDirectSchemaItems
            .Where(x => x.RebateDirectSchemaId == schemaId)
            .Where(x => spec.Select(y => y.SymbolCode).Contains(x.SymbolCode))
            .ToListAsync();

        var newRuleItems = new List<RebateDirectSchemaItem>();
        foreach (var item in spec)
        {
            var ruleItem = items.FirstOrDefault(x => x.SymbolCode == item.SymbolCode);
            if (ruleItem == null)
            {
                var newRuleItem = new RebateDirectSchemaItem
                {
                    RebateDirectSchemaId = schemaId, SymbolCode = item.SymbolCode, Rate = item.Rate, Pips = item.Pips,
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
                _tenantDbContext.RebateDirectSchemaItems.Update(ruleItem);
            }
        }

        if (newRuleItems.Any())
            await _tenantDbContext.RebateDirectSchemaItems.AddRangeAsync(newRuleItems);

        await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        items.AddRange(newRuleItems);
        return Ok(items);
    }

    /// <summary>
    /// Get Rebate Direct Schema Item
    /// </summary>
    /// <returns></returns>
    [HttpGet("{schemaId:long}/item/{schemaItemId:long}")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRebateDirectSchemaItem(long schemaId, long schemaItemId)
    {
        var item = await _tenantDbContext.RebateDirectSchemaItems
            .Where(x => x.Id == schemaItemId && x.RebateDirectSchemaId == schemaId)
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Get Rebate Schema Used by Direct Rules
    /// </summary>
    /// <returns></returns>
    [HttpGet("{schemaId:long}/used-direct-rule-schema")]
    [ProducesResponseType(typeof(Result<List<AccountViewModel>, Account.Criteria>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetDirectRuleRebateSchema(long schemaId, [FromQuery] Account.Criteria? criteria)
    {
        criteria ??= new Account.Criteria();
        criteria.Ids = await _tenantDbContext.RebateDirectRules
            .Where(x => x.RebateDirectSchemaId == schemaId)
            .Select(x => x.SourceTradeAccountId)
            .ToListAsync();

        if (criteria.Ids.IsEmpty())
            return Ok(Result<List<AccountViewModel>, Account.Criteria>.Of(new List<AccountViewModel>(), criteria));

        var hideEmail = ShouldHideEmail();
        return Ok(await _tradingSvc.AccountQueryForTenantAsync(criteria, hideUserEmail: hideEmail));
    }

    /// <summary>
    /// Get Rebate Schema Used by Client Rules
    /// </summary>
    /// <returns></returns>
    [HttpGet("{schemaId:long}/used-client-rule-schema")]
    [ProducesResponseType(typeof(Result<List<AccountViewModel>, Account.Criteria>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetClientRuleRebateSchema(long schemaId, [FromQuery] Account.Criteria? criteria)
    {
        criteria ??= new Account.Criteria();
        criteria.Ids = await _tenantDbContext.RebateClientRules
            .Where(x => x.RebateDirectSchemaId == schemaId)
            .Select(x => x.ClientAccountId)
            .ToListAsync();

        if (criteria.Ids.IsEmpty())
            return Ok(Result<List<AccountViewModel>, Account.Criteria>.Of(new List<AccountViewModel>(), criteria));
        var hideEmail = ShouldHideEmail();
        return Ok(await _tradingSvc.AccountQueryForTenantAsync(criteria, hideUserEmail: hideEmail));
    }

    // /// <summary>
    // /// Delete Rebate Direct Schema Item
    // /// </summary>
    // /// <returns></returns>
    // [HttpDelete("{schemaId:long}/item/{schemaItemId:long}")]
    // [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    // public async Task<IActionResult> DeleteRebateDirectSchemaItem(long schemaId, long schemaItemId)
    // {
    //     var item = await _tenantDbContext.RebateDirectSchemaItems
    //         .Where(x => x.Id == schemaItemId && x.RebateDirectSchemaId == schemaId)
    //         .SingleOrDefaultAsync();
    //     if (item == null)
    //         return NotFound();
    //     try
    //     {
    //         _tenantDbContext.RebateDirectSchemaItems.Remove(item);
    //         await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
    //         return NoContent();
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError("Delete Rebate Rule Item error. Id:{Id}  PartyId: {PartyId}", item.Id, GetPartyId());
    //         return BadRequest(Result.Error(e.Message));
    //     }
    // }
}