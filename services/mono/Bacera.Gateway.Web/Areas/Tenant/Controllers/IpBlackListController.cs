using OpenIddict.Validation.AspNetCore;
using System.Net;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.IpBlackList;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/IP Black List")]
[Route("api/" + VersionTypes.V1 + "/[Area]/ip-black-list")]
public class IpBlackListController(CentralDbContext centralDbContext, AuthDbContext authDbContext, IMyCache myCache)
    : TenantBaseController
{
    private readonly string _ipKey = CacheKeys.GetBlackedIpHashKey();


    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await centralDbContext.IpBlackLists
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<IpBlackList.TenantPageModel>, M.Criteria>.Of(items, criteria));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        if (!IPAddress.TryParse(spec.Ip, out _)) return BadRequest("__INVALID_IP_ADDRESS__");
        var exists = await centralDbContext.IpBlackLists.AnyAsync(x => x.Ip == spec.Ip);
        if (exists) return BadRequest("__IP_ALREADY_EXISTS__");
        var entity = spec.ToEntity(await GetOperatorNameAsync());
        centralDbContext.IpBlackLists.Add(entity);
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity.Ip);
        return Ok(entity);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await centralDbContext.IpBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        centralDbContext.IpBlackLists.Remove(entity);
        await centralDbContext.SaveChangesAsync();
        await RemoveCacheAsync(entity.Ip);
        return Ok();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var entity = await centralDbContext.IpBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        await RemoveCacheAsync(entity.Ip);
        spec.ApplyTo(entity, await GetOperatorNameAsync());
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity.Ip);
        return Ok(entity);
    }

    [HttpPut("reload-cache")]
    public async Task<IActionResult> ReloadCache() => Ok(await ReloadCacheAsync());

    private Task RemoveCacheAsync(string ip) => myCache.HSetDeleteByKeyFieldAsync(_ipKey, ip);

    private Task AddCacheAsync(string ip) => myCache.HSetStringAsync(_ipKey, ip, "1");

    private async Task<long> ReloadCacheAsync()
    {
        await myCache.HSetDeleteByKeyAsync(_ipKey);

        var ips = await centralDbContext.IpBlackLists
            .Select(x => x.Ip)
            .ToListAsync();

        foreach (var ip in ips)
        {
            await myCache.HSetStringAsync(_ipKey, ip, "1");
        }

        return ips.Count;
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