
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Quiz")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class QuizController(
    UserService userService,
    ConfigurationService configurationService,
    TagService tagSvc,
    TenantDbContext tenantDbContext)
    : ClientBaseController
{
    /// <summary>
    /// Wholesale Quiz
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("wholesale")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Wholesale([FromBody] dynamic spec)
    {
        if (spec.answerw == null || spec.answerw <= 0) return Ok();
        var partyId = GetPartyId();
        await tagSvc.AddPartyTagAsync(partyId, "WholesaleApplicationBanned");
        return NoContent();
    }

    /// <summary>
    /// User Quiz Step 1
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("verification/step1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Verification([FromBody] dynamic spec)
    {
        if (spec.answerw == null || spec.answerw <= 0)
            return Ok();

        if (await configurationService.GetQuizFailLockEnabledToggleSwitchAsync() == false)
            return Ok();

        await userService.LockUserAsync(GetPartyId(), GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// User Quiz Step 2
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("verification/step2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VerificationStep2([FromBody] dynamic spec)
    {
        if (spec.answerw == null || spec.answerw <= 0)
            return Ok();

        if (await configurationService.GetQuizFailLockEnabledToggleSwitchAsync() == false)
            return Ok();

        await userService.BanVerificationQuizAsync(GetPartyId());
        return NoContent();
    }
}