using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Role")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RoleController : TenantBaseController
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AuthDbContext _authDbContext;

    public RoleController(
        AuthDbContext authDbContext
        , RoleManager<ApplicationRole> roleManager
    )
    {
        _roleManager = roleManager;
        _authDbContext = authDbContext;
    }

    /// <summary>
    /// Get role list
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApplicationRole>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles
            .OrderByDescending(x => x.Id)
            .ToListAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Get role detail with claims
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApplicationRole), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Detail(long id)
    {
        var role = await _authDbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == id);
        if (role == null) return NotFound();
        var claims = await _authDbContext.RoleClaims
            .Where(x => x.RoleId == role.Id)
            .ToListAsync();

        return Ok(new { Id = role.Id, Name = role.Name, Claims = claims });
    }
}