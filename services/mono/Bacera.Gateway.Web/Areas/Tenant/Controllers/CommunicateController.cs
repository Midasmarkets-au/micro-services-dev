
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = CommunicateHistory;

[Tags("Tenant/Communicate")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]

public class CommunicateController : TenantBaseController
{
    private readonly TenantDbContext _ctx;

    public CommunicateController(
        TenantDbContext tenantDbContext
    )
    {
        _ctx = tenantDbContext;
    }

    /// <summary>
    /// Communicate pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await _ctx.CommunicateHistories
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        return Ok(Result<List<M.ResponseModel>, M.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Communicate history
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] long id)
    {
        var item = await _ctx.CommunicateHistories
            .SingleOrDefaultAsync(x => x.Id == id);
        return item != null ? Ok(item) : NotFound();
    }
}