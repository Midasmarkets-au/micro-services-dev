using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Response;
using LinqKit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = User;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/User")]
public class UserController(
    UserService userService,
    AuthDbContext authDbContext,
    UserManager<M> userManager,
    TradingService tradingService,
    AccountingService accountingService,
    TenantDbContext tenantDbContext,
    IMediator mediator,
    ITenantGetter tenantGetter,
    TagService tagSvc,
    BcrTokenService bcrTokenService)
    : TenantBaseController
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Party.Criteria? criteria)
    {
        criteria ??= new Party.Criteria();
        var hideEmail = ShouldHideEmail();
        var result = await tenantDbContext.Parties
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();
        return Ok(Result<List<M.TenantPageModel>, Party.Criteria>.Of(result, criteria));
    }


    [HttpGet("{partyId:long}")]
    [HttpGet("pid/{partyId:long}")]
    public async Task<IActionResult> Get(long partyId)
    {
        var hideEmail = ShouldHideEmail();
        var item = await tenantDbContext.Parties
            .Where(x => x.Id == partyId)
            .ToTenantDetailModel(hideEmail)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();

        if (await userService.IsUserLockedOutAsync(partyId))
            item.LockoutEnd = DateTime.MaxValue;

        return Ok(item);
    }

    /// <summary>
    /// Get User summaries
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Result<List<UserInfo>, User.Criteria>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPartyIds([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.TenantId = GetTenantId();
        var items = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .PagedFilterBy(criteria)
            .ToUserInfo()
            .ToListAsync();
        return Ok(Result<List<UserInfo>, User.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get User by UID
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("uid/{uid:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUid(long uid)
    {
        var tenantId = GetTenantId();
        var result = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .Where(x => x.Uid == uid && x.TenantId == tenantId)
            .SingleOrDefaultAsync();
        return result == null ? NotFound() : Ok(result.ToResponse());
    }

    /// <summary>
    /// Get User ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("id/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var tenantId = GetTenantId();
        var result = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .SingleOrDefaultAsync();
        return result == null ? NotFound() : Ok(result.ToResponse());
    }

    /// <summary>
    /// Add Tag
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/tag/{tag}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Tag))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTag(long partyId, string tag)
    {
        var result = await tagSvc.AddPartyTagAsync(partyId, tag);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Remove Tag
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    [HttpDelete("{partyId:long}/tag/{tag}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTag(long partyId, string tag)
    {
        var result = await tagSvc.RemovePartyTagAsync(partyId, tag);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("role")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await authDbContext.Roles
            .Where(x => x.Name != UserRoleTypesString.SuperAdmin)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();
        return Ok(roles);
    }


    [HttpPut("{partyId:long}/role/{roleId:long}/add")]
    public async Task<IActionResult> AddRole(long partyId, long roleId)
    {
        if (roleId <= 10 && !User.IsInRole(UserRoleTypesString.SuperAdmin))
        {
            return BadRequest(Result.Error(ResultMessage.User.AddRoleFailed, "Only SuperAdmin can assign admin roles"));
        }

        var user = await userManager.Users.SingleOrDefaultAsync(x => x.TenantId == GetTenantId() && x.PartyId == partyId);
        if (user == null) return NotFound();

        var role = await authDbContext.Roles.SingleOrDefaultAsync(x => x.Id == roleId);
        if (role?.Name == null) return NotFound();

        var result = await userManager.AddToRoleAsync(user, role.Name);
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.AddRoleFailed, result.Errors.Select(x => x.Description).ToList()));
    }

    [HttpPut("{partyId:long}/role/{roleId:long}/remove")]
    public async Task<IActionResult> RemoveRole(long partyId, long roleId)
    {
        if (roleId <= 10 && !User.IsInRole(UserRoleTypesString.SuperAdmin))
        {
            return BadRequest(Result.Error(ResultMessage.User.RemoveRoleFailed, "Only SuperAdmin can remove admin roles"));
        }

        var user = await userManager.Users.SingleOrDefaultAsync(x => x.TenantId == GetTenantId() && x.PartyId == partyId);
        if (user == null) return NotFound();

        var role = await authDbContext.Roles.SingleOrDefaultAsync(x => x.Id == roleId);
        if (role?.Name == null) return NotFound();

        var result = await userManager.RemoveFromRoleAsync(user, role.Name);
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.RemoveRoleFailed, result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Get User Permissions
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpGet("{partyId:long}/permission")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserClaim>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissions(long partyId)
    {
        var tenantId = GetTenantId();
        var user = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .Where(x => x.PartyId == partyId && x.TenantId == tenantId)
            .Select(x => new M { Id = x.Id, PartyId = x.PartyId, Uid = x.Uid })
            .SingleOrDefaultAsync();
        if (user == null) return NotFound();

        var result = await authDbContext.UserClaims
            .Where(x => x.UserId == user.Id)
            .Where(x => x.ClaimType == UserClaimTypes.Permission)
            .Where(x => x.ClaimValue != null)
            .ToListAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get User Permissions
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpGet("{partyId:long}/claim")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserClaim>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaims(long partyId)
    {
        var tenantId = GetTenantId();
        var user = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .SingleOrDefaultAsync(x => x.PartyId == partyId && x.TenantId == tenantId);
        if (user == null) return NotFound();

        var items = await userManager.GetClaimsAsync(user);
        return Ok(items);
    }

    /// <summary>
    /// Add role to user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    [HttpPost("{partyId:long}/role/{role}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddRole(long partyId, string role)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();

        var result = await userManager.AddToRoleAsync(user, role);
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.AddRoleFailed,
            result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    [HttpDelete("{partyId:long}/role/{role}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRole(long partyId, string role)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.PartyId == partyId);
        if (user == null) return NotFound();

        var result = await userManager.RemoveFromRoleAsync(user, role);
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.AddRoleFailed,
            result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Add permission to user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="permission"></param>
    /// <returns></returns>
    [HttpPost("{partyId:long}/permission/{permission}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddPermission(long partyId, string permission)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();

        var result = await userManager.AddClaimAsync(user, new Claim(UserClaimTypes.Permission, permission));
        if (result.Succeeded) return NoContent();

        return BadRequest(
            Result.Error(
                ResultMessage.User.AddPermissionFailed,
                result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Reset user permission by removing ApplicationWholesaleDisabled
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpDelete("{partyId:long}/permission/application-wholesale-disabled")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> WholesaleReset(long partyId)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();

        var result = await userManager.RemoveClaimAsync(user,
            new Claim(UserClaimTypes.Permission, UserPermissionTypes.ApplicationWholesaleDisabled));
        if (!result.Succeeded)
            return BadRequest(
                Result.Error(
                    ResultMessage.User.RemovePermissionFailed,
                    result.Errors.Select(x => x.Description).ToList()));

        var cmt = new Comment
        {
            PartyId = GetPartyId(),
            RowId = user.Id,
            Content = "Remove-" + UserPermissionTypes.ApplicationWholesaleDisabled,
            Type = (int)CommentTypes.User,
        };

        tenantDbContext.Comments.Add(cmt);
        await tenantDbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Add claim: Sales Account to user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("{partyId:long}/sales-account/{accountUid:long}")]
    public async Task<IActionResult> AddClaimForSalesAccount(long partyId, long accountUid)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();

        var hasSalesUid = await tradingService.IsSalesUidExists(accountUid, user.PartyId);
        if (!hasSalesUid) return BadRequest(Result.Error(ResultMessage.User.SalesUidNotFoundForUser));

        var result =
            await userManager.AddClaimAsync(user, new Claim(UserClaimTypes.SalesAccount, accountUid.ToString()));
        if (result.Succeeded) return NoContent();

        return BadRequest(
            Result.Error(
                ResultMessage.User.AddClaimFailed,
                result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// remove claim: Sales Account from user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    /// 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{partyId:long}/sales-account/{accountUid:long}")]
    public async Task<IActionResult> RemoveClaimForSalesAccount(long partyId, long accountUid)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();
        var result =
            await userManager.RemoveClaimAsync(user, new Claim(UserClaimTypes.SalesAccount, accountUid.ToString()));
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.RemoveClaimFailed,
            result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Add claim: Agent Account to user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    /// 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpPost("{partyId:long}/agent-account/{accountUid:long}")]
    public async Task<IActionResult> AddClaimForAgentAccount(long partyId, long accountUid)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();

        var hasUid = await tradingService.IsAgentUidExists(accountUid, user.PartyId);
        if (!hasUid) return BadRequest(Result.Error(ResultMessage.User.AgentUidNotFoundForUser));

        var result =
            await userManager.AddClaimAsync(user, new Claim(UserClaimTypes.AgentAccount, accountUid.ToString()));
        if (result.Succeeded) return NoContent();

        return BadRequest(
            Result.Error(
                ResultMessage.User.AddClaimFailed,
                result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// remove claim: Agent Account from user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    /// 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{partyId:long}/agent-account/{accountUid:long}")]
    public async Task<IActionResult> RemoveClaimForAgentAccount(long partyId, long accountUid)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();
        var result =
            await userManager.RemoveClaimAsync(user, new Claim(UserClaimTypes.AgentAccount, accountUid.ToString()));
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.RemoveClaimFailed,
            result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Remove permission from user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="permission"></param>
    /// <returns></returns>
    /// 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{partyId:long}/permission/{permission}")]
    public async Task<IActionResult> RemovePermission(long partyId, string permission)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null) return NotFound();

        var userClaim = await authDbContext.UserClaims
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id && x.ClaimType == UserClaimTypes.Permission && x.ClaimValue == permission);
        if (userClaim == null) return BadRequest(Result.Error(ResultMessage.User.PermissionNotFound));

        var result = await userManager.RemoveClaimAsync(user, new Claim(UserClaimTypes.Permission, permission));
        if (result.Succeeded) return NoContent();

        return BadRequest(Result.Error(ResultMessage.User.RemovePermissionFailed,
            result.Errors.Select(x => x.Description).ToList()));
    }

    /// <summary>
    /// Get User Audits
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    /// 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<AuditResponseModel>, UserAudit.Criteria>))]
    [HttpGet("{partyId:long}/audit")]
    public async Task<IActionResult> Index(long partyId, [FromQuery] UserAudit.Criteria? criteria)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null || user.IsEmpty()) return NotFound();

        criteria ??= new UserAudit.Criteria();
        criteria.RowId = user.Id;
        criteria.Type = AuditTypes.User;
        var items = await authDbContext.UserAudits
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        await FulfillUsersAsync(items);
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Get User Audit
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuditResponseModel))]
    [HttpGet("{partyId:long}/audit/{id:long}")]
    public async Task<IActionResult> UserAudit(long partyId, long id)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == GetTenantId());
        if (user == null || user.IsEmpty()) return NotFound();

        var item = await authDbContext.UserAudits
            .Where(x => x.Type == (short)AuditTypes.User)
            .Where(x => x.RowId == user.Id)
            .Where(x => x.Id == id)
            .ToResponseModel()
            .FirstOrDefaultAsync();

        return item == null ? NotFound() : Ok(await GetAuditUserAsync(item));
    }

    /// <summary>
    /// Get User's Payment Service Access
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="fundType"></param>
    /// <param name="currencyId"></param>
    /// <returns></returns>
    [HttpGet("{partyId:long}/payment-service")]
    [ProducesResponseType(typeof(PaymentService.AccessResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentService(long partyId,
        [FromQuery] FundTypes? fundType = null, [FromQuery] CurrencyTypes? currencyId = null)
    {
        var item = await accountingService.GetPaymentServiceAccessForTenantAsync(partyId, fundType, currencyId);
        return Ok(item);
    }


    /// <summary>
    /// Set User's Payment Service Access
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="accesses"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/payment-service")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<PaymentService.Accesses>> UpdatePaymentServiceAccess(long partyId,
        [FromBody] PaymentService.Accesses accesses)
    {
        if (accesses.CurrencyId <= 0)
            return BadRequest(Result.Error(ResultMessage.Deposit.CurrencyNotMatch));

        await accountingService.SetPaymentServiceAccessAsync(partyId, accesses);
        return NoContent();
    }

    /// <summary>
    /// Update User's profile
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M))]
    public async Task<ActionResult<M>> UpdateProfile(long partyId, [FromBody] M.TenantUpdateSpec spec)
    {
        var tenantId = GetTenantId();
        var user = await authDbContext.Users.SingleAsync(x => x.PartyId == partyId && x.TenantId == tenantId);
        var existingPartyId = await authDbContext.Users
            .Where(x => x.TenantId == tenantId && x.Email == spec.Email)
            .Where(x => x.PartyId != partyId)
            .Select(x => x.PartyId)
            .FirstOrDefaultAsync();

        if (existingPartyId != 0)
        {
            return BadRequest(Result.Error("Email with same address already exists"));
        }
        
        user.Gender = spec.Gender;
        user.Address = spec.Address;
        user.Birthday = spec.Birthday;
        user.Citizen = spec.Citizen;
        user.FirstName = spec.FirstName;
        user.LastName = spec.LastName;
        user.IdType = spec.IdType;
        user.IdNumber = spec.IdNumber;
        user.IdIssuer = spec.IdIssuer;
        user.IdIssuedOn = spec.IdIssuedOn ?? DateOnly.MinValue;
        user.IdExpireOn = spec.IdExpireOn ?? DateOnly.MinValue;
        user.NativeName = spec.NativeName;
        // user.Language = spec.Language;
        user.ReferCode = spec.ReferCode;
        user.UpdatedOn = DateTime.UtcNow;
        user.CCC = spec.CCC;
        user.PhoneNumber = spec.PhoneNumber;
        user.Email = spec.Email.ToLower();
        user.NormalizedEmail = spec.Email.ToUpper();

        // make sure to prepend the UserName with tenantId
        user.UserName = $"{tenantId}_{spec.Email.ToLower()}";
        user.NormalizedUserName = $"{tenantId}_{spec.Email.ToUpper()}";

        authDbContext.Users.Update(user);
        // Use the target user's partyId (not the operator's) for audit trail
        // This ensures we can track email/phone changes for the correct user
        await authDbContext.SaveChangesWithAuditAsync(partyId);

        // var party = await _tenantDbContext.Parties.SingleAsync(x => x.Id == partyId);
        // user.ApplyToParty(ref party);
        // party.UpdatedOn = DateTime.UtcNow;
        // _tenantDbContext.Parties.Update(party);
        // await _tenantDbContext.SaveChangesAsync();

        await mediator.Publish(new UserInfoUpdatedEvent(user));
        return Ok(user);
    }

    /// <summary>
    /// Update User's status
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserAndPartyStatus(long partyId, M.TenantUpdateStatusSpec spec)
    {
        await Task.WhenAll(Task.Run(async () =>
        {
            var tenantId = GetTenantId();
            var user = await authDbContext.Users.SingleAsync(x => x.TenantId == tenantId && x.PartyId == partyId);
            user.Status = (short)spec.Status;
            user.UpdatedOn = DateTime.UtcNow;
            authDbContext.Users.Update(user);
            await authDbContext.SaveChangesAsync();
        }), Task.Run(async () =>
        {
            var party = await tenantDbContext.Parties.SingleAsync(x => x.Id == partyId);
            party.Status = (short)spec.Status;
            party.UpdatedOn = DateTime.UtcNow;
            tenantDbContext.Parties.Update(party);
            if (!string.IsNullOrWhiteSpace(spec.Comment))
                tenantDbContext.Comments.Add(Comment.Build(partyId, GetPartyId(), CommentTypes.User, spec.Comment));

            await tenantDbContext.SaveChangesAsync();
        }));
        return NoContent();
    }

    /// <summary>
    /// Update User's Site Id
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/site/{siteId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Party))]
    public async Task<ActionResult<M>> SiteIdUpdate(long partyId, int siteId)
    {
        var party = await tenantDbContext.Parties.SingleOrDefaultAsync(x => x.Id == partyId);
        if (party == null)
            return NotFound();

        party.SiteId = siteId;
        party.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.Parties.Update(party);
        await tenantDbContext.SaveChangesAsync();
        var tenantId = GetTenantId();
        var user = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .SingleOrDefaultAsync(x => x.PartyId == partyId && x.TenantId == tenantId);
        if (user != null)
            await mediator.Publish(new UserInfoUpdatedEvent(user));
        return Ok(party);
    }

    /// <summary>
    /// Lock User
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/lock")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M))]
    public async Task<ActionResult<M>> LockUser(long partyId)
    {
        await userService.LockUserAsync(partyId, GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Unlock User
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns> 
    [HttpPut("{partyId:long}/unlock")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M))]
    public async Task<ActionResult<M>> UnlockUser(long partyId)
    {
        await userService.UnlockUserAsync(partyId, GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Unlock User Quiz
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/unlock/quiz")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<M>> UnlockUserQuiz(long partyId)
    {
        await userService.AllowVerificationQuizAsync(partyId);
        return NoContent();
    }

    /// <summary>
    /// Get user token (God Mode)
    /// </summary>
    /// <returns></returns>
    [HttpPost("{partyId:long}/god-mode")]
    public async Task<IActionResult> God(long partyId)
    {
        var tenantId = GetTenantId();

        var user = await userManager.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .SingleOrDefaultAsync();
        if (user == null)
            return NotFound();

        var userRoles = await userManager.GetRolesAsync(user);
        if (userRoles.Contains(UserRoleTypesString.SuperAdmin))
        {
            return BadRequest(ResultMessage.User.UserGodModeNotAllowed);
        }

        var operatorEmail = await userManager.Users
            .Where(x => x.PartyId == GetPartyId() && x.TenantId == tenantId)
            .Select(x => x.Email)
            .SingleAsync();

        if (userRoles.Contains(UserRoleTypesString.TenantAdmin))
        {
            return BadRequest(ResultMessage.User.UserGodModeNotAllowed);
        }
       
        var res = await bcrTokenService.GetUserTokenAsync(user, godPartyId: GetPartyId());
        return Ok(new { token = res.AccessToken });
    }

    [HttpGet("address")]
    public async Task<IActionResult> UserAddressIndex([FromQuery] Address.Criteria? criteria)
    {
        criteria ??= new Address.Criteria();
        var addresses = await tenantDbContext.Addresses
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();
        return Ok(Result<List<Address.TenantPageModel>, Address.Criteria>.Of(addresses, criteria));
    }

    [HttpGet("address/{addressId:long}")]
    public async Task<IActionResult> GetUserAddress(long addressId)
    {
        var item = await tenantDbContext.Addresses
            .ToTenantDetailModel()
            .SingleOrDefaultAsync(x => x.Id == addressId);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("{partyId:long}/migrate-to/{tenantId:long}")]
    public async Task<IActionResult> MigrateUser(long partyId, long tenantId)
    {
        if (tenantId == _tenantId) return BadRequest("Same tenant");
        var (res, msg) = await userService.MigrateUserAsync(partyId, tenantId);
        return res ? Ok(msg) : BadRequest(msg);
    }

    [HttpPost("{partyId:long}/duplicate-to/{tenantId:long}")]
    public async Task<IActionResult> DuplicateUser(long partyId, long tenantId,
        [FromQuery] bool includeVerification = false)
    {
        if (tenantId == _tenantId) return BadRequest("Same tenant");
        var operatorPartyId = GetPartyId();
        var (res, msg) = await userService.DuplicateUserToOtherTenantAsync(partyId
            , tenantId
            , includeVerification
            , operatorPartyId);
        return res ? Ok() : BadRequest(msg);
    }

    /// <summary>
    /// Get user legacy personal info
    /// </summary>
    /// <returns></returns>
    [HttpGet("{partyId:long}/legacy/personal-info")]
    public async Task<IActionResult> LegacyPersonalInfo(long partyId)
        => Ok(await GetLegacyInfo(partyId, SupplementTypes.MigrationPersonalInfo));

    /// <summary>
    /// Get user legacy financial info
    /// </summary>
    /// <returns></returns>
    [HttpGet("{partyId:long}/legacy/financial-info")]
    public async Task<IActionResult> LegacyFinancialInfo(long partyId)
        => Ok(await GetLegacyInfo(partyId, SupplementTypes.MigrationFinancialInfo));

    /// <summary>
    /// Get user legacy kyc form
    /// </summary>
    /// <returns></returns>
    [HttpGet("{partyId:long}/legacy/kyc-form")]
    public async Task<IActionResult> LegacyKycForm(long partyId)
        => Ok(await GetLegacyInfo(partyId, SupplementTypes.MigrationKycForm));

    /// <summary>
    /// Get user legacy kyc Correction
    /// </summary>
    /// <returns></returns>
    [HttpGet("{partyId:long}/legacy/kyc-correction")]
    public async Task<IActionResult> LegacyKycCorrection(long partyId)
        => Ok(await GetLegacyInfo(partyId, SupplementTypes.MigrationKycCorrection));

    /// <summary>
    /// Get social media information
    /// </summary>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [HttpGet("{partyId:long}/social-media-info")]
    public async Task<IActionResult> SocialMediaInfo(long partyId)
    {
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.RowId == partyId)
            .Where(x => x.Type == (long)SupplementTypes.SocialMediaRecord)
            .SingleOrDefaultAsync();

        return supplement == null
            ? Ok(new List<M.SocialMediaType>())
            : Ok(JsonConvert.DeserializeObject<List<M.SocialMediaType>>(supplement.Data));
    }

    /// <summary>
    /// Update social media information
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{partyId:long}/social-media-info")]
    public async Task<IActionResult> UpdateSocialMediaInfo(long partyId, [FromBody] M.SocialMediaType spec)
    {
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.RowId == partyId)
            .Where(x => x.Type == (long)SupplementTypes.SocialMediaRecord)
            .SingleOrDefaultAsync();

        if (supplement == null)
        {
            var entry = tenantDbContext.Supplements.Add(
                Supplement.Build(SupplementTypes.SocialMediaRecord, partyId,
                    JsonConvert.SerializeObject(new List<M.SocialMediaType>()))
            );
            supplement = entry.Entity;
            await tenantDbContext.SaveChangesAsync();
        }

        var data = JsonConvert.DeserializeObject<List<M.SocialMediaType>>(supplement.Data) ??
                   new List<M.SocialMediaType>();

        var index = data.FindIndex(item => item.Name == spec.Name);
        if (index != -1)
        {
            data[index] = spec;
        }
        else
        {
            data.Add(spec);
        }

        supplement.Data = JsonConvert.SerializeObject(data);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    private async Task<object?> GetLegacyInfo(long partyId, SupplementTypes type)
    {
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.Type == (int)type)
            .Where(x => x.RowId == partyId)
            .SingleOrDefaultAsync();
        return supplement == null ? null : JsonConvert.DeserializeObject(supplement.Data);
    }

    private async Task<AuditResponseModel> GetAuditUserAsync(AuditResponseModel audit)
    {
        var items = await FulfillUsersAsync(new List<AuditResponseModel> { audit });
        return items.Select(x => x.SetValue(x.Data)).First();
    }

    private async Task<ICollection<AuditResponseModel>> FulfillUsersAsync(ICollection<AuditResponseModel> audits)
    {
        var tenantId = GetTenantId();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantGetter.GetTenantId())
            .Where(x => audits.Select(a => a.PartyId).Contains(x.PartyId) && x.TenantId == tenantId)
            .ToUserInfo()
            .ToListAsync();

        foreach (var audit in audits)
        {
            var user = users.FirstOrDefault(x => x.PartyId == audit.PartyId);
            if (user != null) audit.SetUser(user).SetValue(audit.Data);
        }

        return audits;
    }

    private static string GetGodModeCacheKey(long tenantId, long partyId, long targetUserId) =>
        $"god_mode_T{tenantId}_P{partyId}_U{targetUserId}_util";
}