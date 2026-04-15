using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Bacera.Gateway.Auth.User;

[Tags("Tenant/User")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class UserController(TenantDbContext tenantDbContext, AuthDbContext authDbContext) : TenantBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.TenantPageModel>, Party.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] Party.Criteria? criteria = null)
    {
        criteria ??= new Party.Criteria();
        var hideEmail = ShouldHideEmail();
        var result = await tenantDbContext.Parties
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();
        return Ok(Result<List<M.TenantPageModel>, Party.Criteria>.Of(result, criteria));
    }

    /// <summary>
    /// Get all roles (excluding SuperAdmin)
    /// </summary>
    [HttpGet("role")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await authDbContext.Roles
            .Where(x => x.Name != UserRoleTypesString.SuperAdmin)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Get user detail
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(User.TenantDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Detail(long id)
    {
        var hideEmail = ShouldHideEmail();
        var detail = await tenantDbContext.Parties
            .Where(x => x.Id == id)
            .ToTenantDetailModel(hideEmail)
            .SingleOrDefaultAsync();

        if (detail == null) return NotFound();

        var lockoutEnd = await authDbContext.Users
            .Where(x => x.PartyId == id)
            .Select(x => x.LockoutEnd)
            .SingleOrDefaultAsync();
        detail.LockoutEnd = lockoutEnd?.UtcDateTime;

        return Ok(detail);
    }

    /// <summary>
    /// Get social media information
    /// </summary>
    [HttpGet("{partyId:long}/social-media-info")]
    [ProducesResponseType(typeof(List<User.SocialMediaType>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SocialMediaInfo(long partyId)
    {
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.RowId == partyId)
            .Where(x => x.Type == (long)SupplementTypes.SocialMediaRecord)
            .SingleOrDefaultAsync();

        return supplement == null
            ? Ok(new List<User.SocialMediaType>())
            : Ok(JsonConvert.DeserializeObject<List<User.SocialMediaType>>(supplement.Data));
    }

    /// <summary>
    /// Update social media information
    /// </summary>
    [HttpPut("{partyId:long}/social-media-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSocialMediaInfo(long partyId, [FromBody] User.SocialMediaType spec)
    {
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.RowId == partyId)
            .Where(x => x.Type == (long)SupplementTypes.SocialMediaRecord)
            .SingleOrDefaultAsync();

        if (supplement == null)
        {
            var entry = tenantDbContext.Supplements.Add(
                Supplement.Build(SupplementTypes.SocialMediaRecord, partyId,
                    JsonConvert.SerializeObject(new List<User.SocialMediaType>())));
            supplement = entry.Entity;
            await tenantDbContext.SaveChangesAsync();
        }

        var data = JsonConvert.DeserializeObject<List<User.SocialMediaType>>(supplement.Data)
                   ?? new List<User.SocialMediaType>();

        var index = data.FindIndex(item => item.Name == spec.Name);
        if (index != -1)
            data[index] = spec;
        else
            data.Add(spec);

        supplement.Data = JsonConvert.SerializeObject(data);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }
}
