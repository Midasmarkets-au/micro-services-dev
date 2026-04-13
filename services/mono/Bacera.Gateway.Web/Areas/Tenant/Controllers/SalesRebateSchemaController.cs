
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Sales Rebate")]
[Route("api/" + VersionTypes.V1 + "/[Area]/sales-rebate-schema")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class SalesRebateSchemaController(
    TenantDbContext tenantDbContext,
    AccountManageService accManageSvc
) : TenantBaseController
{
    /// <summary>
    /// Get Sales Rebate Schema List
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<SalesRebateSchema.TenantPageModel>, SalesRebateSchema.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] SalesRebateSchema.Criteria? criteria)
    {
        criteria ??= new SalesRebateSchema.Criteria();
        if (criteria.Status == SalesRebateSchemaStatusTypes.Pending)
        {
            criteria.Status = null;
            criteria.SalesAccountIds = await tenantDbContext.SalesRebateSchemas
                .Where(x => x.Status == (short)SalesRebateSchemaStatusTypes.Pending)
                .Select(x => x.SalesAccountId)
                .ToListAsync();
            if (criteria.SalesAccountIds.Count == 0)
            {
                return Ok(Result<List<List<SalesRebateSchema.TenantPageModel>>, SalesRebateSchema.Criteria>.Of([], criteria));
            }
        }

        var items = await tenantDbContext.SalesRebateSchemas
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .GroupBy(x => x.SalesAccountId)
            .Select(g => g.ToList())
            .ToListAsync();
        
        return Ok(Result<List<List<SalesRebateSchema.TenantPageModel>>, SalesRebateSchema.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Add New Sales Rebate Schema
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SalesRebateSchemaCreate([FromBody] SalesRebateSchema.CreateSpec spec)
    {
        var salesId = await accManageSvc.GetAccountIdByUidAsync(spec.SalesAccountUid);
        if (salesId == 0)
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        var rebateId = await accManageSvc.GetAccountIdByUidAsync(spec.RebateAccountUid);
        if (rebateId == 0)
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        var schema = await tenantDbContext.SalesRebateSchemas
            .Where(x => x.SalesAccountId == salesId 
                        && x.RebateAccountId == rebateId)
            .FirstOrDefaultAsync();
        if (schema != null)
            return BadRequest(Result.Error(ResultMessage.Common.RecordExists));

        var item = new SalesRebateSchema
        {
            SalesType = (int)spec.SalesType,
            Schedule = (short)spec.Schedule,
            SalesAccountId = salesId,
            RebateAccountId = rebateId,
            ExcludeAccount = Utils.JsonSerializeObject(spec.ExcludeAccount.Split(',')),
            ExcludeSymbol = Utils.JsonSerializeObject(spec.ExcludeSymbol.Split(',')),
            Rebate = spec.Rebate,
            AlphaRebate = spec.AlphaRebate,
            ProRebate = spec.ProRebate,
            Note = spec.Note,
            Status = (short)SalesRebateSchemaStatusTypes.Pending,
            OperatorPartyId = GetPartyId(),
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
        };

        tenantDbContext.SalesRebateSchemas.Add(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Update Sales Rebate Schema
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}")]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SalesRebateSchemaUpdate(long schemaId,
        [FromBody] SalesRebateSchema.UpdateSpec spec)
    {
        var item = await tenantDbContext.SalesRebateSchemas
            .Where(x => x.Id == schemaId)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();

        var isRebateChanged = item.Rebate != spec.Rebate || item.AlphaRebate != spec.AlphaRebate ||
                              item.ProRebate != spec.ProRebate;
        if (item.Status == (short)SalesRebateSchemaStatusTypes.Active && isRebateChanged)
        {
            var pendingRecord = await tenantDbContext.SalesRebateSchemas
                .Where(x => x.SalesAccountId == item.SalesAccountId 
                            && x.RebateAccountId == item.RebateAccountId 
                            && x.Status == (short)SalesRebateSchemaStatusTypes.Pending)
                .SingleOrDefaultAsync();
            if (pendingRecord != null) return BadRequest(Result.Error(ResultMessage.Common.PendingRecordExists));
            
            var newItem = new SalesRebateSchema
            {
                SalesType = item.SalesType,
                Schedule = (short)spec.Schedule,
                SalesAccountId = item.SalesAccountId,
                RebateAccountId = item.RebateAccountId,
                ExcludeAccount = Utils.JsonSerializeObject(spec.ExcludeAccount.Split(',')),
                ExcludeSymbol = Utils.JsonSerializeObject(spec.ExcludeSymbol.Split(',')),
                Rebate = spec.Rebate,
                AlphaRebate = spec.AlphaRebate,
                ProRebate = spec.ProRebate,
                Note = spec.Note,
                Status = (short)SalesRebateSchemaStatusTypes.Pending,
                OperatorPartyId = GetPartyId(),
                UpdatedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
            };
            tenantDbContext.SalesRebateSchemas.Add(newItem);
        }
        else
        {
            item.SalesType = (int)spec.SalesType;
            item.ExcludeAccount = Utils.JsonSerializeObject(spec.ExcludeAccount.Split(','));
            item.ExcludeSymbol = Utils.JsonSerializeObject(spec.ExcludeSymbol.Split(','));
            item.Rebate = spec.Rebate;
            item.AlphaRebate = spec.AlphaRebate;
            item.ProRebate = spec.ProRebate;
            item.Note = spec.Note;
            item.UpdatedOn = DateTime.UtcNow;
            item.OperatorPartyId = GetPartyId();
            item.UpdatedOn = DateTime.UtcNow;

            if (item.Status == (short)SalesRebateSchemaStatusTypes.Active && item.Schedule != (int)spec.Schedule)
            {
                var records = await tenantDbContext.SalesRebateSchemas
                    .Where(x => x.RebateAccountId == item.RebateAccountId && x.Status == (short)SalesRebateSchemaStatusTypes.Active)
                    .ToListAsync();

                records.ForEach(record => record.Schedule = (short)spec.Schedule);
                tenantDbContext.SalesRebateSchemas.UpdateRange(records);
            }
            else if (item.Schedule != (int)spec.Schedule)
            {
                item.Schedule = (short)spec.Schedule;
            }
            
            tenantDbContext.SalesRebateSchemas.Update(item);
        }

        await tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }
    
    /// <summary>
    /// Approve Sales Rebate Schema Status
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}/approve")]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SalesRebateSchemaUpdateStatus(long schemaId)
    {
        var item = await tenantDbContext.SalesRebateSchemas
            .Where(x => x.Id == schemaId && x.Status == (short)SalesRebateSchemaStatusTypes.Pending)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();
        
        item.Status = (short)SalesRebateSchemaStatusTypes.Active;
        tenantDbContext.SalesRebateSchemas.Update(item);

        var oldRule = await tenantDbContext.SalesRebateSchemas
            .Where(x => x.SalesAccountId == item.SalesAccountId
                        && x.RebateAccountId == item.RebateAccountId
                        && x.Status == (short)SalesRebateSchemaStatusTypes.Active)
            .SingleOrDefaultAsync();
        if (oldRule != null)
        {
            tenantDbContext.SalesRebateSchemas.Remove(oldRule);
            await tenantDbContext.SaveChangesAsync();
        }

        var existingSchemas = await tenantDbContext.SalesRebateSchemas
            .Where(x => x.RebateAccountId == item.RebateAccountId && x.Status == (short)SalesRebateSchemaStatusTypes.Active)
            .ToListAsync();

        if (existingSchemas.Count != 0 && existingSchemas.First().Schedule != item.Schedule)
        {
            existingSchemas.ForEach(record => record.Schedule = item.Schedule);
        }
        
        tenantDbContext.SalesRebateSchemas.UpdateRange(existingSchemas);
        await tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }
    
    /// <summary>
    /// Delete Sales Rebate Schema
    /// </summary>
    /// <returns></returns>
    [HttpPut("{schemaId:long}/delete")]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SalesRebateSchemaDelete(long schemaId)
    {
        var item = await tenantDbContext.SalesRebateSchemas
            .Where(x => x.Id == schemaId && x.Status == (short)SalesRebateSchemaStatusTypes.Pending)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();
        
        tenantDbContext.SalesRebateSchemas.Remove(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }
}