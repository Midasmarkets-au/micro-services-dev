using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.Event;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/Event")]
public partial class EventController(
    TenantDbContext tenantDbContext,
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    CentralDbContext centralDbContext,
    EventService eventService,
    ConfigService cfgSvc,
    IMyCache myCache)
    : TenantBaseController
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
        var lang = await GetLanguage();

        criteria ??= new Gateway.Event.Criteria();
        var items = await tenantDbContext.Events
            .PagedFilterBy(criteria)
            .ToTenantPageModel(lang)
            .ToListAsync();

        return Ok(Result<List<Gateway.Event.TenantPageModel>, Gateway.Event.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Gateway.Event.TenantDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tenantDbContext.Events
            .Where(x => x.Id == id)
            .ToTenantDetailModel()
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create Event with the specified key and language
    /// </summary>
    /// <param name="createWithLanguageSpec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create([FromBody] Gateway.Event.CreateWithLanguageSpec createWithLanguageSpec)
    {
        if (await tenantDbContext.Events.AnyAsync(x => x.Key == createWithLanguageSpec.Key))
            return BadRequest("__KEY_ALREADY_EXISTS__");
        if (LanguageTypes.All.All(x => x != createWithLanguageSpec.Language))
            return BadRequest($"__LANGUAGE_SHOULD_BE_ONE_OF_THESE__{string.Join(",", LanguageTypes.All)}");

        var entity = createWithLanguageSpec.ToEntity();
        tenantDbContext.Events.Add(entity);
        await tenantDbContext.SaveChangesAsync();
        if (createWithLanguageSpec.Key != UserRoleTypes.EventShop.GetDescription()) return Ok();
            
        var hasRole = await authDbContext.Roles.AnyAsync(x => x.Name == entity.Key);
        if (hasRole) return Ok();
        var maxId = await authDbContext.Roles.MaxAsync(x => x.Id);
        var newRole = new ApplicationRole
        {
            Id = maxId < 500 ? 501 : maxId + 1,
            Name = UserRoleTypes.EventShop.GetDescription(),
            NormalizedName = UserRoleTypes.EventShop.GetDescription().ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        authDbContext.Roles.Add(newRole);
        await authDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Update Event with the specified id and language
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(long id, [FromBody] Gateway.Event.UpdateSpec spec)
    {
        var item = await tenantDbContext.Events.FindAsync(id);
        if (item == null) return NotFound();
        item.FromUpdateSpec(spec);
        tenantDbContext.Events.Update(item);
        await tenantDbContext.SaveChangesAsync();
        await myCache.KeyDeleteAsync(CacheKeys.EventKeyToIdCache);
        return Ok();
    }


    /// <summary>
    /// Draft the event
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/draft")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> DraftEvent(long id) => ChangeEventStatus(id, EventStatusTypes.Draft);


    /// <summary>
    /// Publish the event
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/publish")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> PublishEvent(long id) => ChangeEventStatus(id, EventStatusTypes.Published);


    /// <summary>
    /// Close the event
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/close")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> CloseEvent(long id) => ChangeEventStatus(id, EventStatusTypes.Closed);

    private async Task<IActionResult> ChangeEventStatus(long id, EventStatusTypes status)
    {
        var item = await tenantDbContext.Events.FindAsync(id);
        if (item == null) return NotFound();
        item.Status = (short)status;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.Events.Update(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Add or Update Event Language with the specified id and language
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/lang/{language}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContentByLanguage(long id, string language,
        [FromBody] Gateway.Event.UpdateLanguageSpec spec)
    {
        if (LanguageTypes.All.All(x => x != language))
            return BadRequest($"__LANGUAGE_SHOULD_BE_ONE_OF_THESE__{string.Join(",", LanguageTypes.All)}");

        var eventLanguage = await tenantDbContext.EventLanguages
                                .FirstOrDefaultAsync(x => x.EventId == id && x.Language == language)
                            ?? new EventLanguage();

        eventLanguage.Name = spec.Name;
        eventLanguage.Title = spec.Title;
        eventLanguage.Description = spec.Description;
        eventLanguage.Images = JsonConvert.SerializeObject(spec.Images);
        eventLanguage.Term = spec.Term;
        eventLanguage.Instruction = JsonConvert.SerializeObject(spec.Instruction);
        if (eventLanguage.Id == 0)
        {
            eventLanguage.EventId = id;
            eventLanguage.Language = language;
            tenantDbContext.EventLanguages.Add(eventLanguage);
        }
        else
        {
            tenantDbContext.EventLanguages.Update(eventLanguage);
        }

        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Event Party Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(Result<List<EventParty.TenantEventPartyPageModel>, EventParty.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventParties([FromQuery] EventParty.Criteria? criteria)
    {
        criteria ??= new EventParty.Criteria();
        var items = await tenantDbContext.EventParties
            .PagedFilterBy(criteria)
            .ToTenantEventPartyPageModel()
            .ToListAsync();

        return Ok(Result<List<EventParty.TenantEventPartyPageModel>, EventParty.Criteria>.Of(items, criteria));
    }


    /// <summary>
    /// Approve the user's participation in the event
    /// </summary>
    /// <param name="eventPartyId"></param>
    /// <returns></returns>
    [HttpPost("user/{eventPartyId:long}/approve")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> Approve(long eventPartyId)
        => ChangeEventPartyStatus(eventPartyId, EventPartyStatusTypes.Approved);

    /// <summary>
    /// Reject the user's participation in the event
    /// </summary>
    /// <param name="eventPartyId"></param>
    /// <returns></returns>
    [HttpPost("user/{eventPartyId:long}/reject")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> Reject(long eventPartyId)
        => ChangeEventPartyStatus(eventPartyId, EventPartyStatusTypes.Rejected);

    /// <summary>
    ///  Cancel the user's participation in the event
    /// </summary>
    /// <param name="eventPartyId"></param>
    /// <returns></returns>
    [HttpPost("user/{eventPartyId:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(long eventPartyId)
        => await ChangeEventPartyStatus(eventPartyId, EventPartyStatusTypes.Cancelled);

    private async Task<IActionResult> ChangeEventPartyStatus(long eventPartyId, EventPartyStatusTypes status)
    {
        var item = await tenantDbContext.EventParties
            .Include(x => x.Event)
            .SingleOrDefaultAsync(x => x.Id == eventPartyId);
        if (item == null) return NotFound();
        item.Status = (short)status;
        item.OperatorPartyId = GetPartyId();
        item.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.EventParties.Update(item);
        if (status == EventPartyStatusTypes.Rejected)
        {
            var user = await userManager.Users.SingleAsync(x =>
                x.PartyId == item.PartyId && x.TenantId == GetTenantId());
            if (await userManager.IsInRoleAsync(user, item.Event.Key))
                await userManager.RemoveFromRoleAsync(user, item.Event.Key);
        }

        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    private async Task<string> GetLanguage()
    {
        var (tenantId, partyId) = (GetTenantId(), GetPartyId());
        var lang = await userManager.Users.Where(x => x.TenantId == tenantId && x.PartyId == partyId)
                       .Select(x => x.Language)
                       .FirstOrDefaultAsync()
                   ?? "en-us";
        return lang;
    }
}