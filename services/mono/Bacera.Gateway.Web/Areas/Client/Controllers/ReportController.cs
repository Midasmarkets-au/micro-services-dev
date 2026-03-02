using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using M = Bacera.Gateway.ReportRequest;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Area("Client")]
[Route("api/" + VersionTypes.V1 + "/[Area]/report")]
[Tags("Client/Report")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class ReportController : ClientBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly TenantDbContext _tenantDbContext;

    public ReportController(TenantDbContext tenantDbContext, TradingService tradingService)
    {
        _tradingSvc = tradingService;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Request Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("request")]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M>, M.Criteria>>> RequestPagination(
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await _tenantDbContext.ReportRequests
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Get a query sample for a report
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("query/{type:int}/sample")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuerySample(int type)
    {
        await Task.Delay(0);
        switch ((ReportRequestTypes)type)
        {
            case ReportRequestTypes.Rebate:
                return Ok(new Rebate.Criteria());
            default:
                return NotFound();
        }
    }

    /// <summary>
    /// Query preview
    /// </summary>
    /// <param name="type"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpPost("query/{type:int}/preview")]
    [ProducesResponseType(typeof(Result<List<object>, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QueryPreview(int type, [FromBody] dynamic query)
    {
        await Task.Delay(0);
        switch ((ReportRequestTypes)type)
        {
            case ReportRequestTypes.Rebate:
                Rebate.Criteria criteria =
                    JsonConvert.DeserializeObject<Rebate.Criteria>(JsonConvert.SerializeObject(query));
                criteria.PartyId = GetPartyId();
                criteria.Page = 1;
                criteria.Size = 20;
                var items = await _tenantDbContext.Rebates
                    .PagedFilterBy(criteria)
                    .ToListAsync();
                criteria.PartyId = null;
                return Ok(Result<List<Rebate>, Rebate.Criteria>.Of(items, criteria));
            default:
                return NotFound();
        }
    }

    /// <summary>
    /// Create Report Request
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("request")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M>> CreateRequest([FromBody] M.CreateSpec spec)
    {
        var item = new M
        {
            PartyId = GetPartyId(),
            Type = (int)spec.Type,
            Name = spec.Name.Trim(),
            CreatedOn = DateTime.UtcNow,
            FileName = string.Empty,
            ExpireOn = null,
            GeneratedOn = null,
        };
        if (spec.Type == ReportRequestTypes.Rebate)
        {
            var criteria = JsonConvert.DeserializeObject<Rebate.Criteria>(JsonConvert.SerializeObject(spec.Query));
            if (criteria == null)
                return BadRequest(ResultMessage.Common.InvalidInput);
            criteria.PartyId = GetPartyId();
            item.Query = JsonConvert.SerializeObject(criteria);
        }
        else
        {
            return BadRequest(ResultMessage.Common.InvalidType);
        }

        _tenantDbContext.ReportRequests.Add(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Get Report Request
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("request/{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M>> GetRequest(long id)
    {
        var item = await _tenantDbContext.ReportRequests
            .Where(x => x.PartyId.Equals(GetPartyId()))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }
}