
using Bacera.Gateway.Vendor.MetaTrader;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = CopyTrade;

[Area("Tenant")]
[Tags("Tenant/Copy Trade")]
[Route("api/" + VersionTypes.V1 + "/[Area]/copy-trade")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]

public class CopyTradeController : TenantBaseController
{
    private readonly ICopyTradeService _svc;

    public CopyTradeController(ICopyTradeService svc)
    {
        _svc = svc;
    }

    /// <summary>
    /// Copy Trade pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Query([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var result = await _svc.QueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Copy Trade
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await _svc.GetAsync(id);
        if (result.Id == 0)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create Copy Trade
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var result = await _svc.CreateAsync(spec.Source, spec.Target, spec.Mode, spec.Value);
        return result.IsSuccess() ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// delete Copy Trade
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.IsSuccess() ? NoContent() : BadRequest(result);
    }
}