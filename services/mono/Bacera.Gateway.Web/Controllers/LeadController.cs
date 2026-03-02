using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Controllers;

using M = Lead;
using MSG = ResultMessage.Lead;

[Tags("Public/Lead")]
public class LeadController(ILeadService leadSvc) : BaseController
{
    /// <summary>
    /// Create a lead
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var specValidator = new LeadCreateSpecValidator();
        var specValidationResult = await specValidator.ValidateAsync(spec);
        if (!specValidationResult.IsValid) return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));
        var result = await leadSvc.CreateAsync(spec);
        return Ok(result);
    }
}