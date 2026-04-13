
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.Event;

[Tags("Client/Event")]
[Area("Client")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/[Area]/event")]
[Route("api/" + VersionTypes.V1 + "/[Area]/event")]
public partial class EventController(
    TenantDbContext tenantDbContext,
    UserManager<User> userManager,
    CentralDbContext centerDbContext,
    IMyCache myCache,
    UserService userSvc,
    ConfigService cfgSvc,
    IMediator mediator,
    EventService eventService)
    : ClientBaseController
{
    /// <summary>
    /// Get Event List, return event list according to the language of the logged in management user
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Gateway.Event.TenantPageModel>, Gateway.Event.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] Gateway.Event.Criteria? criteria)
    {
        criteria ??= new Gateway.Event.Criteria();
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound("__USER_NOT_FOUND__");
        var roles = await userManager.GetRolesAsync(user);
        var party = await userSvc.GetPartyAsync(user.PartyId);
        var items = await tenantDbContext.Events
            .FromSqlInterpolated(
                $"""
                 select * from event."_Event" where 
                                                  exists (select 1 from jsonb_array_elements_text("AccessRoles") as elem where elem::text = any(array[{roles}]))
                                                  and "AccessSites" @> cast({JsonConvert.SerializeObject(new[] { party.SiteId })} AS jsonb)
                 """
            )
            .PagedFilterBy(criteria)
            .ToClientPageModel(user.Language)
            .ToListAsync();

        return Ok(Result<List<Gateway.Event.ClientPageModel>, Gateway.Event.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Set the last check event
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpPut("{key}/last-check")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetLastCheckEvent(string key)
    {
        var id = await tenantDbContext.Events
            .Where(x => x.Key == key)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        if (id == 0) return NotFound();

        var value = ApplicationConfigure.LongValue.Of(id);
        await cfgSvc.SetAsync(nameof(Party), GetPartyId(), ConfigKeys.UserLastCheckedEventId, value);
        return Ok();
    }

    /// <summary>
    /// Get Event
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("{key}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Gateway.Event.TenantDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string key)
    {
        var lang = await GetLanguage();
        var item = await tenantDbContext.Events
            .Where(x => x.Key == key)
            .ToClientDetailModel(lang)
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Apply to participate in the event
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpPost("{key}/apply")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Apply(string key)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return BadRequest();
        var isParticipated = await tenantDbContext.EventParties
            .AnyAsync(x => x.PartyId == user.PartyId && x.Event.Key == key);
        if (isParticipated) return BadRequest("__ALREADY_PARTICIPATED__");

        var item = await tenantDbContext.Events.SingleOrDefaultAsync(x => x.Key == key);
        if (item == null) return NotFound();

        var userRoles = await userManager.GetRolesAsync(user);
        if (item.GetAccessRoles().All(x => !userRoles.Contains(x)))
            return BadRequest("__NO_ACCESS__");

        // create new Role
        if (userRoles.All(x => x != item.Key))
            await userManager.AddToRoleAsync(user, item.Key);

        item.EventParties.Add(EventParty.Build(user.PartyId, EventPartyStatusTypes.Approved));
        await tenantDbContext.SaveChangesAsync();

        if (!await userManager.IsInRoleAsync(user, UserRoleTypesString.EventShop))
        {
            await userManager.AddToRoleAsync(user, UserRoleTypesString.EventShop);
        }

        return NoContent();
    }

    /// <summary>
    /// Apply to participate in the event
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("{key}/user")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetEventParty(string key)
    {
        var partyId = GetPartyId();
        var lang = await GetLanguage();
        var item = await tenantDbContext.EventParties
            .Where(x => x.PartyId == partyId && x.Event.Key == key)
            .ToClientDetailModel(lang)
            .SingleOrDefaultAsync();
        return Ok(item);
    }

    private async Task<string> GetLanguage()
    {
        var user = await userSvc.GetPartyAsync(GetPartyId());
        return user.Language;
    }
}