using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers;

[ApiController]
[Route("/api/configuration")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.Guest + "," + UserRoleTypesString.ClientOrTenantAdmin)]
[Tags("Public/Configuration")]
public class ConfigurationController(
    // ConfigurationService configurationService,
    ConfigService cfgSvc,
    TenantDbContext tenantDbContext,
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
        if (User.Identity is { IsAuthenticated: true })
        {
            var partyId = GetPartyId();
            var party = await tenantDbContext.Parties.SingleOrDefaultAsync(x => x.Id == partyId);
            if (party == null)
            {
                logger.LogError($"Party not found for partyId, regenerate token or check UserClaim : {partyId}");
                return Unauthorized();
            }
        }

        // var data = await configurationService.GetPublicConfigurationBySiteAsync(site);
        var data = await cfgSvc.GetPartyConfigurationBySiteAsync(GetPartyId());
        return Ok(data);
    }
}