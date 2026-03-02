using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Permission")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class PermissionController : TenantBaseController
{
    private readonly AuthDbContext _authDbContext;

    public PermissionController(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    /// <summary>
    /// Get all permission types for User
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public IActionResult Index()
        => Ok(UserPermissionTypes.All.OrderBy(x => x).ToList());
}