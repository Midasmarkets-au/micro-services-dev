using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers;

[ApiController]
[Route("/api/configuration")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.Guest + "," + UserRoleTypesString.ClientOrTenantAdmin)]
[Tags("Public/Configuration")]
public class ConfigurationController(
    // ConfigurationService configurationService,
    ConfigService cfgSvc,
    TenantDbContext tenantDbContext,
    UserService userService,
    ILogger<ConfigurationController> logger)
    : BaseController
{
    /// <summary>
    /// Get Public Configuration
    /// </summary>
    /// <returns></returns>
    [HttpGet("public")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApplicationConfigure.PublicSetting))]
    // public async Task<IActionResult> Public([FromQuery] int site = 0)
    public async Task<IActionResult> Public()
    {
        var partyId = GetPartyId();

        if (User.Identity is { IsAuthenticated: true })
        {
            var party = await tenantDbContext.Parties.SingleOrDefaultAsync(x => x.Id == partyId);
            if (party == null)
            {
                logger.LogError($"Party not found for partyId, regenerate token or check UserClaim : {partyId}");
                return Unauthorized();
            }
        }

        // var data = await configurationService.GetPublicConfigurationBySiteAsync(site);
        var data = await cfgSvc.GetPartyConfigurationBySiteAsync(partyId);
        data.PasswordChangedWithinLast24h = await userService.CheckRecentPasswordChangeAsync(partyId);
        data.EmailOrPhoneChangedWithinLast24h = await userService.CheckRecentEmailOrPhoneChangeAsync(partyId);
        return Ok(data);
    }
}