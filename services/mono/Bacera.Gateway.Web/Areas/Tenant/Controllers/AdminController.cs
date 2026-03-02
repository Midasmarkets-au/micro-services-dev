using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Permission;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Admin")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class AdminController(
    UserService userService,
    AuthDbContext authDbContext,
    ITenantGetter tenantGetter,
    PermissionService permissionSvc,
    UserManager<User> userManager,
    IServiceProvider serviceProvider)
    : TenantBaseController
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    
    [HttpGet("tokens")]
    public async Task<IActionResult> GetTokensForAllTenancies()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var users = await userManager.Users
            .Where(x => x.Status == 0 && x.Email == user.Email)
            .ToListAsync();

        var tokens = await Task.WhenAll(users.Select(async u =>
        {
            using var scope = serviceProvider.CreateScope();
            var tokenSvc = scope.ServiceProvider.GetRequiredService<BcrTokenService>();
            var userToken = await tokenSvc.GetUserTokenAsync(u);
            return (u.TenantId, userToken.AccessToken);
        }));

        return Ok(tokens.Select(x => new { x.TenantId, token = x.AccessToken }));
    }

    /// <summary>
    /// Admin Users
    /// </summary>
    /// <returns></returns>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<UserAdminViewModel>>))]
    public async Task<IActionResult> Index()
    {
        var result = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .Where(u => u.UserRoles.Any(ur => ur.RoleId < 100))
            .ToUserAdminViewModel()
            .ToListAsync();
      
        return Ok(Result<List<UserAdminViewModel>>.Of(result));
    }
   
    /// <summary>
    /// Get User Detail
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpGet("users/{partyId:long}")]
    public async Task<IActionResult> UserDetail(long partyId)
    {
        var user = await authDbContext.Users
            .Where(x => x.TenantId == _tenantId && x.PartyId == partyId)
            .ToUserAdminViewModel()
            .SingleOrDefaultAsync();
        if (user == null) return NotFound();
        
        var quizLockedOut = await userService.IsUserLockedOutAsync(user.PartyId);
        var roles = await userService.GetUserRolesAsync(user.PartyId);
        var userPermissions = await permissionSvc.GetUserPermissionIdsAsync(_tenantId, user.PartyId);
        var rolePermissions = await permissionSvc.GetRolePermissionIdsAsync(roles.ToArray());

        return Ok(new { user, userPermissions, rolePermissions, quizLockedOut });
    }


    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [HttpGet("roles")]
    public async Task<IActionResult> Roles()
    {
        var items = await userService.GetAllRolesAsync();
        items = items.Where(x => x.Id < 100).ToList();
        return Ok(Result.Of(items));
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    /// <returns></returns>
    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions()
    {
        var items = await permissionSvc.GetAllAsync();
        return Ok(Result.Of(items));
    }

    /// <summary>
    /// Create Permission
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("permissions")]
    public async Task<IActionResult> Create([FromBody] Permission.CreateSpec spec)
    {
        var result = await permissionSvc.CreateAsync(spec.Auth, spec.Action, spec.Method, spec.Category, spec.Key);
        if (!result) return BadRequest(ToErrorResult("Permission already exists"));
        return Ok();
    }

    /// <summary>
    /// Toggle Permission Auth
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("permissions/{id:long}/toggle")]
    public async Task<IActionResult> PermissionAuthToggle(long id)
    {
        var result = await permissionSvc.ToggleAuthAsync(id);
        if (!result) BadRequest(ToErrorResult("Permission not found"));
        return Ok();
    }

    /// <summary>
    /// Get Role Permission
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("roles/{id:long}")]
    public async Task<IActionResult> Role(long id)
    {
        var role = await authDbContext.ApplicationRoles
            .Where(r => r.Id == id)
            .Select(r => new ApplicationRole.BasicModel { Id = r.Id, Name = r.Name })
            .FirstOrDefaultAsync();
        if (role?.Name == null) return NotFound();

        var rolePermissions = await permissionSvc.GetRolePermissionIdsAsync(role.Name);
        return Ok(rolePermissions);
    }

    /// <summary>
    /// Toggle User Role
    /// </summary>
    /// <param name="id"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpPut("users/{id:long}/role/{roleId:long}/toggle")]
    public async Task<IActionResult> ToggleRole(long id, int roleId)
    {
        var partyId = await authDbContext.Users
            .Where(x => x.Id == id)
            .Select(x => x.PartyId)
            .SingleOrDefaultAsync();
        if (partyId == 0) return NotFound();

        var roleName = await authDbContext.ApplicationRoles
            .Where(x => x.Id == roleId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync();
        if (roleName == null) return NotFound();

        var result = await userService.HasRoleAsync(partyId, roleName)
            ? await userService.RemoveRoleAsync(partyId, roleName, GetPartyId())
            : await userService.AddRoleAsync(partyId, roleName, GetPartyId());

        return result ? NoContent() : BadRequest();
    }

    /// <summary>
    /// Toggle User Permission
    /// </summary>
    /// <param name="id"></param>
    /// <param name="permissionId"></param>
    /// <returns></returns>
    [HttpPut("users/{id:long}/permission/{permissionId:long}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleUserPermission(long id, int permissionId)
    {
        var partyId = await authDbContext.Users
            .Where(x => x.Id == id)
            .Select(x => x.PartyId)
            .SingleOrDefaultAsync();
        if (partyId == 0) return NotFound();

        var result = await permissionSvc.UserHasPermissionAsync(_tenantId, partyId, permissionId)
            ? await permissionSvc.RemoveUserPermissionAsync(_tenantId, partyId, permissionId)
            : await permissionSvc.AddUserPermissionAsync(_tenantId, partyId, permissionId);

        return result ? NoContent() : BadRequest();
    }

    /// <summary>
    /// Toggle Role Permission
    /// </summary>
    /// <param name="id"></param>
    /// <param name="permissionId"></param>
    /// <returns></returns>
    [HttpPut("roles/{id:long}/permission/{permissionId:long}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RoleTogglePermission(long id, int permissionId)
    {
        var roleName = await authDbContext.ApplicationRoles
            .Where(x => x.Id == id)
            .Select(x => x.Name)
            .SingleOrDefaultAsync();
        if (roleName == null) return NotFound();

        var result = await permissionSvc.RoleHasPermissionAsync(roleName, permissionId)
            ? await permissionSvc.RemoveRolePermissionAsync(roleName, permissionId)
            : await permissionSvc.AddRolePermissionAsync(roleName, permissionId);

        return result ? NoContent() : BadRequest();
    }
}