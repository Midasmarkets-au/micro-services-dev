
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Equity Report")]
[Route("api/" + VersionTypes.V1 + "/[Area]/equity-report")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class EquityReportController(TenantDbContext tenantCtx) : TenantBaseController
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AccountReportGroup.Criteria? criteria)
    {
        criteria ??= new AccountReportGroup.Criteria();

        var items = await tenantCtx.AccountReportGroups
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="size"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("login")]
    public async Task<IActionResult> GetLoginByGroupId([FromQuery] long id, [FromQuery] int size, [FromQuery] int page)
    {
        var items = await tenantCtx.AccountReportGroups
            .Where(x => x.Id == id)
            .SelectMany(x => x.AccountReportGroupLogins)
            .Select(x => x.Login)
            .OrderBy(x => x)
            .Skip(size * page)
            .Take(size)
            .ToListAsync();

        return Ok(Result.Of(items, new { id, size, page }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("login")]
    public async Task<IActionResult> AppendAccountNumber([FromBody] AccountReportGroup.UpdateAccountNumberSpec spec)
    {
        var item = await tenantCtx.AccountReportGroups
            .Where(x => x.Id == spec.Id)
            .Include(x => x.AccountReportGroupLogins
                .Where(y => spec.AccountNumbers.Contains(y.Login)))
            .FirstOrDefaultAsync();
        if (item == null) return NotFound();

        var logins = spec.AccountNumbers
            .Where(x => item.AccountReportGroupLogins.All(y => y.Login != x))
            .Select(x => new AccountReportGroupLogin { AccountReportGroupId = item.Id, Login = x, });

        tenantCtx.AccountReportGroupLogins.AddRange(logins);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpDelete("login")]
    public async Task<IActionResult> DeleteAccountNumber([FromBody] AccountReportGroup.UpdateAccountNumberSpec spec)
    {
        var item = await tenantCtx.AccountReportGroups
            .Where(x => x.Id == spec.Id)
            .Include(x => x.AccountReportGroupLogins
                .Where(y => spec.AccountNumbers.Contains(y.Login)))
            .FirstOrDefaultAsync();
        if (item == null) return NotFound();

        tenantCtx.AccountReportGroupLogins.RemoveRange(item.AccountReportGroupLogins);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("schema")]
    public async Task<IActionResult> CreateSchema([FromBody] AccountReportGroup.CreateReportSchemaSpec spec)
    {
        var partyId = GetPartyId();
        var metaData = AccountReportGroup.MetaDataModel.FromCreateSpec(spec);
        var metaDataJson = metaData.ToJson();

        var entity = new AccountReportGroup
        {
            Category = spec.Category,
            Group = spec.Group,
            OperatorPartyId = partyId,
            MetaData = metaDataJson,
        };

        tenantCtx.AccountReportGroups.Add(entity);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("schema")]
    public async Task<IActionResult> UpdateSchema([FromBody] AccountReportGroup.UpdateReportSchemaSpec spec)
    {
        var partyId = GetPartyId();
        var metaData = AccountReportGroup.MetaDataModel.FromUpdateSpec(spec);
        var metaDataJson = metaData.ToJson();

        var item = await tenantCtx.AccountReportGroups
            .Where(x => x.Id == spec.Id && x.ParentId == null)
            .FirstOrDefaultAsync();

        if (item == null) return NotFound();

        item.OperatorPartyId = partyId;
        item.MetaData = metaDataJson;
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("group-category")]
    public async Task<IActionResult> AddGroupCategory([FromBody] AccountReportGroup.AddGroupCategorySpec spec)
    {
        var partyId = GetPartyId();
        var parent = await tenantCtx.AccountReportGroups
            .Where(x => x.Id == spec.ParentId)
            .FirstOrDefaultAsync();
        if (parent == null) return NotFound("Parent not found");

        var entity = new AccountReportGroup
        {
            Category = spec.Category,
            Group = spec.Group,
            OperatorPartyId = partyId,
            ParentId = parent.Id,
        };

        tenantCtx.AccountReportGroups.Add(entity);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("group-category")]
    public async Task<IActionResult> UpdateGroupCategory([FromBody] AccountReportGroup.UpdateGroupCategorySpec spec)
    {
        var partyId = GetPartyId();
        var item = await tenantCtx.AccountReportGroups.FindAsync(spec.Id);
        if (item == null) return NotFound();

        item.OperatorPartyId = partyId;
        item.Group = spec.Group;
        item.Category = spec.Category;
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }
}