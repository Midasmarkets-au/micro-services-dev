using OpenIddict.Validation.AspNetCore;
using System.Net;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = UserBlackList;

[Area("Tenant")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/User Black List")]
[Route("api/" + VersionTypes.V1 + "/[Area]/user-black-list")]
public class UserBlackListController(
    CentralDbContext centralDbContext,
    AuthDbContext authDbContext,
    IMyCache myCache)
    : TenantBaseController
{
    private readonly string _nameKey = CacheKeys.GetBlackedUserNameHashKey();
    private readonly string _phoneKey = CacheKeys.GetBlackedUserPhoneHashKey();
    private readonly string _emailKey = CacheKeys.GetBlackedUserEmailHashKey();
    private readonly string _idNumberKey = CacheKeys.GetBlackedUserIdNumberHashKey();

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await centralDbContext.UserBlackLists
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<M.TenantPageModel>, M.Criteria>.Of(items, criteria));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var exists = await centralDbContext.UserBlackLists.AnyAsync(x =>
            x.Phone == spec.Phone || x.Email == spec.Email || x.IdNumber == spec.IdNumber);
        if (exists) return BadRequest("__USER_ALREADY_EXISTS__");
        var entity = spec.ToEntity(await GetOperatorNameAsync());
        centralDbContext.UserBlackLists.Add(entity);
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity);
        return Ok(entity);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await centralDbContext.UserBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        centralDbContext.UserBlackLists.Remove(entity);
        await centralDbContext.SaveChangesAsync();
        await RemoveCacheAsync(entity);
        return Ok();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var entity = await centralDbContext.UserBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        await RemoveCacheAsync(entity);
        spec.ApplyTo(entity, await GetOperatorNameAsync());
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity);
        return Ok(entity);
    }


    [HttpPut("reload-cache")]
    public async Task<IActionResult> ReloadCache() => Ok(await ReloadCacheAsync());

    private async Task RemoveCacheAsync(M entity)
    {
        await myCache.HSetDeleteByKeyFieldAsync(_nameKey, entity.Name);
        await myCache.HSetDeleteByKeyFieldAsync(_phoneKey, entity.Phone);
        await myCache.HSetDeleteByKeyFieldAsync(_emailKey, entity.Email);
        await myCache.HSetDeleteByKeyFieldAsync(_idNumberKey, entity.IdNumber);
    }

    private async Task AddCacheAsync(M entity)
    {
        await myCache.HSetStringAsync(_nameKey, entity.Name, "1");
        await myCache.HSetStringAsync(_phoneKey, entity.Phone, "1");
        await myCache.HSetStringAsync(_emailKey, entity.Email, "1");
        await myCache.HSetStringAsync(_idNumberKey, entity.IdNumber, "1");
    }

    private async Task<long> ReloadCacheAsync()
    {
        var nameKey = CacheKeys.GetBlackedUserNameHashKey();
        var phoneKey = CacheKeys.GetBlackedUserPhoneHashKey();
        var emailKey = CacheKeys.GetBlackedUserEmailHashKey();
        var idNumberKey = CacheKeys.GetBlackedUserIdNumberHashKey();
        await myCache.HSetDeleteByKeyAsync(nameKey);
        await myCache.HSetDeleteByKeyAsync(phoneKey);
        await myCache.HSetDeleteByKeyAsync(emailKey);
        await myCache.HSetDeleteByKeyAsync(idNumberKey);

        var items = await centralDbContext.UserBlackLists
            .Select(x => new { x.Name, x.Phone, x.Email, x.IdNumber })
            .ToListAsync();

        foreach (var item in items)
        {
            await myCache.HSetStringAsync(nameKey, item.Name, "1");
            await myCache.HSetStringAsync(phoneKey, item.Phone, "1");
            await myCache.HSetStringAsync(emailKey, item.Email, "1");
            await myCache.HSetStringAsync(idNumberKey, item.IdNumber, "1");
        }

        return items.Count;
    }

    private async Task<string> GetOperatorNameAsync()
    {
        long tenantId = GetTenantId(), partyId = GetPartyId();
        var user = await authDbContext.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .ToUserNameModel()
            .SingleAsync();
        return user.GuessNativeName();
    }
}