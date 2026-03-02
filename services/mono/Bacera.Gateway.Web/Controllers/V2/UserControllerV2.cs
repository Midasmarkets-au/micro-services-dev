using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Permission;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers.V2;

[Tags("User")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/user")]
public class UserControllerV2(
    TenantDbContext tenantDbContext,
    AuthDbContext authCtx,
    ITenantService tenantService,
    PermissionService permissionService)
    : BaseControllerV2
{
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { Message = "Pong" });

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        long tenantId = GetTenantId(), partyId = GetPartyId();
        if (tenantId <= 0 || partyId <= 0) return Unauthorized();
        var tenant = await tenantService.GetAsync(tenantId);
        if (!tenant.IsNotEmpty()) return Unauthorized();

        // var result = UserResponseModel.Of(user);

        var user = await authCtx.Users.FirstOrDefaultAsync(x => x.PartyId == partyId && x.TenantId == tenantId);
        if (user == null) return NotFound();

        var roles = await authCtx.UserRoles.Where(x => x.UserId == user.Id)
            .Select(x => x.ApplicationRole.Name)
            .ToListAsync();

        // var userClaims = await authCtx.UserClaims.Where(x => x.UserId == user.Id).ToListAsync();

        // if (userClaims.Count == 0) return Ok(result);
        var webPermissionsTask = Task.Run(() => permissionService.GetUserWebPermissions(user.TenantId, user.PartyId));

        var configurations = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Party))
            .Where(x => x.RowId == user.PartyId)
            .ToClientMeViewModel()
            .ToListAsync();

        // if (userClaims.Count == 0) return Ok(result);
        var result = UserResponseModel.Of(user);

        result.TwoFactorEnabled = user.TwoFactorEnabled;
        result.Roles = roles.ToArray();
        result.NativeName = user.NativeName;

        result.Permissions = (await webPermissionsTask).ToArray();

        var accounts = await tenantDbContext.Accounts
            .Where(x => x.PartyId == user.PartyId && x.Status == 0)
            .Select(x => new
            {
                x.Role, x.Uid, x.Group,
                Tags = x.Tags.Where(y => y.Name == AccountTagTypes.DefaultSalesAccount || y.Name == AccountTagTypes.DefaultAgentAccount)
                    .Select(y => y.Name),
            })
            .ToListAsync();

        if (accounts.Any(x => x.Group == "TMTM"))
        {
            configurations.Add(new Configuration.ClientMeViewModel
            {
                DataFormat = "bool",
                Key = "DisableTransfer",
                ValueString = "true",
            });
        }

        result.IbAccount = accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Agent)
            .Select(x => x.Uid.ToString())
            .ToArray();

        result.SalesAccount = accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .Select(x => x.Uid.ToString())
            .ToArray();

        result.RepAccount = accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Rep)
            .Select(x => x.Uid.ToString())
            .ToArray();

        var defaultAgentUid = accounts.Where(x => x.Role == (int)AccountRoleTypes.Agent)
            .Where(x => x.Tags.Contains(AccountTagTypes.DefaultAgentAccount))
            .Select(x => x.Uid)
            .SingleOrDefault();

        result.DefaultAgentAccount = defaultAgentUid == 0
            ? accounts.Where(x => x.Role == (int)AccountRoleTypes.Agent).Select(x => x.Uid.ToString()).FirstOrDefault() ?? "0"
            : defaultAgentUid.ToString();

        var defaultSalesUid = accounts.Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .Where(x => x.Tags.Contains(AccountTagTypes.DefaultSalesAccount))
            .Select(x => x.Uid)
            .SingleOrDefault();

        result.DefaultSalesAccount = defaultSalesUid == 0
            ? accounts.Where(x => x.Role == (int)AccountRoleTypes.Sales).Select(x => x.Uid.ToString()).FirstOrDefault() ?? "0"
            : defaultSalesUid.ToString();

        result.Configurations = configurations;
        result.Tenancy = user.TenantId switch
        {
            1 => "au",
            10000 => "bvi",
            10002 => "mn",
            10004 => "sea",
            10005 => "jp",
            _ => "bvi"
        };
        return Ok(result);
    }

    [HttpGet("address")]
    public async Task<IActionResult> UserAddressIndex([FromQuery] Address.ClientCriteria? criteria)
    {
        criteria ??= new Address.ClientCriteria();
        criteria.PartyId = GetPartyId();
        var addresses = await tenantDbContext.Addresses
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return Ok(Result<List<Address.ClientPageModel>, Address.ClientCriteria>.Of(addresses, criteria));
    }

    [HttpGet("address/{hashId}")]
    public async Task<IActionResult> GetUserAddress(string hashId)
    {
        var addressId = Address.HashDecode(hashId);
        var addresses = await tenantDbContext.Addresses
            .ToClientDetailModel()
            .SingleOrDefaultAsync(x => x.Id == addressId);

        return addresses == null ? NotFound() : Ok(addresses);
    }

    [HttpPost("address")]
    public async Task<IActionResult> CreateUserAddress([FromBody] Address.CreateSpecV2 spec)
    {
        var address = new Address
        {
            Name = spec.Name,
            CCC = spec.CCC,
            Phone = spec.Phone,
            Country = spec.Country,
            Content = spec.Content,
            PartyId = GetPartyId(),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        tenantDbContext.Addresses.Add(address);
        await tenantDbContext.SaveChangesAsync();
        return Ok(await tenantDbContext.Addresses.ToClientDetailModel().SingleOrDefaultAsync(x => x.Id == address.Id));
    }

    [HttpPut("address/{hashId}")]
    public async Task<IActionResult> UpdateUserAddress(string hashId, [FromBody] Address.UpdateSpecV2 spec)
    {
        var addressId = Address.HashDecode(hashId);
        var partyId = GetPartyId();
        var address = await tenantDbContext.Addresses
            .SingleOrDefaultAsync(x => x.Id == addressId && x.PartyId == partyId);
        if (address == null) return NotFound();

        address.Name = spec.Name;
        address.CCC = spec.CCC;
        address.Phone = spec.Phone;
        address.Country = spec.Country;
        address.Content = spec.Content;
        address.UpdatedOn = DateTime.UtcNow;
        await tenantDbContext.SaveChangesAsync();
        return Ok(await tenantDbContext.Addresses.ToClientDetailModel().SingleOrDefaultAsync(x => x.Id == address.Id));
    }
}