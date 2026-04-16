using System.Security.Claims;
using Api.V1;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Grpc.Core;
using Http.V1;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoUser        = Http.V1.User;
using ProtoRole        = Http.V1.Role;
using ProtoAddress     = Http.V1.Address;
using ProtoUserAudit   = Http.V1.UserAudit;

namespace Bacera.Gateway.Web.HttpServices.User;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantUserService.
/// Replaces Areas/Tenant/Controllers/UserController.cs.
/// Routes defined via google.api.http annotations in user.proto.
/// </summary>
public class TenantUserGrpcService(
    AuthDbContext authDb,
    TenantDbContext tenantDb,
    ITenantGetter tenantGetter,
    UserManager<Bacera.Gateway.Auth.User> userManager,
    UserService userSvc,
    TagService tagSvc,
    TradingService tradingSvc,
    AccountingService accountingSvc,
    BcrTokenService bcrTokenSvc,
    IMyCache cache,
    AuthValidationService.AuthValidationServiceClient authGrpcClient,
    ILogger<TenantUserGrpcService> logger)
    : TenantUserService.TenantUserServiceBase
{
    // ─── List / Get ───────────────────────────────────────────────────────────

    public override async Task<ListUsersResponse> ListUsers(
        ListUsersRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Party.Criteria
        {
            Page       = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size       = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            SearchText = request.HasKeywords ? request.Keywords : null,
        };

        var items = await tenantDb.Parties
            .PagedFilterBy(criteria)
            .ToTenantPageModel(false)
            .ToListAsync();

        var response = new ListUsersResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Parties
            .Where(x => x.Id == request.PartyId)
            .ToTenantDetailModel(false)
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var proto = MapDetailToProto(item);
        if (await userSvc.IsUserLockedOutAsync(request.PartyId))
            proto.LockoutEnd = DateTime.MaxValue.ToString("O");
        return new GetUserResponse { Data = proto };
    }

    public override async Task<GetUserByPidResponse> GetUserByPid(GetUserByPidRequest request, ServerCallContext context)
    {
        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new GetUserByPidResponse { Data = result.Data };
    }

    public override async Task<GetUserSummaryResponse> GetUserSummary(
        GetUserSummaryRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var criteria = new Bacera.Gateway.Auth.User.Criteria
        {
            Page     = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size     = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            TenantId = tenantId,
            Keywords = request.HasKeywords ? request.Keywords : null,
        };

        var items = await authDb.Users
            .Where(x => x.TenantId == tenantId)
            .PagedFilterBy(criteria)
            .ToUserInfo()
            .ToListAsync();

        var response = new GetUserSummaryResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(u => new ProtoUser
        {
            Id        = u.Id,
            PartyId   = u.PartyId,
            Email     = u.Email     ?? "",
            FirstName = u.FirstName ?? "",
            LastName  = u.LastName  ?? "",
        }));
        return response;
    }

    public override async Task<GetUserByUidResponse> GetUserByUid(GetUserByUidRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await authDb.Users
            .Where(x => x.TenantId == tenantId && x.Uid == request.Uid)
            .SingleOrDefaultAsync();
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        return new GetUserByUidResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await authDb.Users
            .Where(x => x.TenantId == tenantId && x.Id == request.Id)
            .SingleOrDefaultAsync();
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        return new GetUserByIdResponse { Data = MapAuthUserToProto(user) };
    }

    // ─── Tags ─────────────────────────────────────────────────────────────────

    public override async Task<AddUserTagResponse> AddUserTag(AddUserTagRequest request, ServerCallContext context)
    {
        await tagSvc.AddPartyTagAsync(request.PartyId, request.Tag);
        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new AddUserTagResponse { Data = result.Data };
    }

    public override async Task<RemoveUserTagResponse> RemoveUserTag(RemoveUserTagRequest request, ServerCallContext context)
    {
        await tagSvc.RemovePartyTagAsync(request.PartyId, request.Tag);
        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new RemoveUserTagResponse { Data = result.Data };
    }

    // ─── Roles (on User) ──────────────────────────────────────────────────────

    public override async Task<GetAllRolesResponse> GetAllRoles(GetAllRolesRequest request, ServerCallContext context)
    {
        var roles = await authDb.Roles
            .Where(x => x.Name != UserRoleTypesString.SuperAdmin)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();
        var response = new GetAllRolesResponse();
        response.Roles.AddRange(roles.Select(r => new ProtoRole { Id = r.Id, Name = r.Name ?? "" }));
        return response;
    }

    public override async Task<AddRoleResponse> AddRole(AddRoleRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var role = await authDb.Roles.SingleOrDefaultAsync(x => x.Id == request.RoleId);
        if (role?.Name == null) throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        var result = await userManager.AddToRoleAsync(user, role.Name);
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new AddRoleResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<RemoveRoleResponse> RemoveRole(RemoveRoleRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var role = await authDb.Roles.SingleOrDefaultAsync(x => x.Id == request.RoleId);
        if (role?.Name == null) throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        var result = await userManager.RemoveFromRoleAsync(user, role.Name);
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new RemoveRoleResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<GetUserPermissionsResponse> GetUserPermissions(
        GetUserPermissionsRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var userId = await authDb.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == request.PartyId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (userId == 0) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var perms = await authDb.UserClaims
            .Where(x => x.UserId == userId && x.ClaimType == UserClaimTypes.Permission && x.ClaimValue != null)
            .Select(x => x.ClaimValue!)
            .ToListAsync();

        var response = new GetUserPermissionsResponse();
        response.Permissions.AddRange(perms);
        return response;
    }

    public override async Task<GetUserClaimsResponse> GetUserClaims(
        GetUserClaimsRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await authDb.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var claims = await userManager.GetClaimsAsync(user);
        var response = new GetUserClaimsResponse();
        foreach (var c in claims)
            if (c.Type != null && c.Value != null)
                response.Claims[c.Type] = c.Value;
        return response;
    }

    public override async Task<AddRoleByNameResponse> AddRoleByName(AddRoleByNameRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new AddRoleByNameResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<RemoveRoleByNameResponse> RemoveRoleByName(RemoveRoleByNameRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.PartyId == request.PartyId && x.TenantId == tenantId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.RemoveFromRoleAsync(user, request.Role);
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new RemoveRoleByNameResponse { Data = MapAuthUserToProto(user) };
    }

    // ─── Permissions ──────────────────────────────────────────────────────────

    public override async Task<AddPermissionResponse> AddPermission(AddPermissionRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.AddClaimAsync(user,
            new Claim(UserClaimTypes.Permission, request.Permission));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new AddPermissionResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<RemovePermissionResponse> RemovePermission(RemovePermissionRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.RemoveClaimAsync(user,
            new Claim(UserClaimTypes.Permission, request.Permission));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new RemovePermissionResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<DisableWholesalePermissionResponse> DisableWholesalePermission(
        DisableWholesalePermissionRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.RemoveClaimAsync(user,
            new Claim(UserClaimTypes.Permission, UserPermissionTypes.ApplicationWholesaleDisabled));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        var operatorPartyId = GetPartyId(context);
        tenantDb.Comments.Add(Bacera.Gateway.Comment.Build(
            request.PartyId, operatorPartyId, CommentTypes.User,
            "Remove-" + UserPermissionTypes.ApplicationWholesaleDisabled));
        await tenantDb.SaveChangesAsync();

        return new DisableWholesalePermissionResponse { Data = MapAuthUserToProto(user) };
    }

    // ─── Account links ────────────────────────────────────────────────────────

    public override async Task<AddSalesAccountResponse> AddSalesAccount(AddSalesAccountRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var hasSalesUid = await tradingSvc.IsSalesUidExists(request.AccountUid, user.PartyId);
        if (!hasSalesUid) throw new RpcException(new Status(StatusCode.InvalidArgument, "Sales account not found for user"));

        var result = await userManager.AddClaimAsync(user,
            new Claim(UserClaimTypes.SalesAccount, request.AccountUid.ToString()));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new AddSalesAccountResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<RemoveSalesAccountResponse> RemoveSalesAccount(RemoveSalesAccountRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.RemoveClaimAsync(user,
            new Claim(UserClaimTypes.SalesAccount, request.AccountUid.ToString()));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new RemoveSalesAccountResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<AddAgentAccountResponse> AddAgentAccount(AddAgentAccountRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var hasUid = await tradingSvc.IsAgentUidExists(request.AccountUid, user.PartyId);
        if (!hasUid) throw new RpcException(new Status(StatusCode.InvalidArgument, "Agent account not found for user"));

        var result = await userManager.AddClaimAsync(user,
            new Claim(UserClaimTypes.AgentAccount, request.AccountUid.ToString()));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new AddAgentAccountResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<RemoveAgentAccountResponse> RemoveAgentAccount(RemoveAgentAccountRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var result = await userManager.RemoveClaimAsync(user,
            new Claim(UserClaimTypes.AgentAccount, request.AccountUid.ToString()));
        if (!result.Succeeded)
            throw new RpcException(new Status(StatusCode.Internal, string.Join("; ", result.Errors.Select(e => e.Description))));

        return new RemoveAgentAccountResponse { Data = MapAuthUserToProto(user) };
    }

    // ─── Audits ───────────────────────────────────────────────────────────────

    public override async Task<ListUserAuditsResponse> ListUserAudits(
        ListUserAuditsRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await userManager.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var criteria = new Bacera.Gateway.Auth.UserAudit.Criteria
        {
            Page  = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size  = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            RowId = user.Id,
            Type  = AuditTypes.User,
        };

        var rawItems = await authDb.UserAudits
            .PagedFilterBy(criteria)
            .Select(x => new { x.Id, x.PartyId, x.Action, x.CreatedOn })
            .ToListAsync();

        var response = new ListUserAuditsResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(rawItems.Select(a => new ProtoUserAudit
        {
            Id        = a.Id,
            PartyId   = a.PartyId,
            Action    = a.Action.ToString(),
            CreatedAt = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<GetUserAuditResponse> GetUserAudit(
        GetUserAuditRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var userId = await userManager.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == request.PartyId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (userId == 0) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var item = await authDb.UserAudits
            .Where(x => x.Type == (short)AuditTypes.User && x.RowId == userId && x.Id == request.Id)
            .Select(x => new { x.Id, x.PartyId, x.Action, x.CreatedOn })
            .FirstOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Audit not found"));

        return new GetUserAuditResponse
        {
            Data = new ProtoUserAudit
            {
                Id        = item.Id,
                PartyId   = item.PartyId,
                Action    = item.Action.ToString(),
                CreatedAt = item.CreatedOn.ToString("O"),
            }
        };
    }

    // ─── Payment Service ──────────────────────────────────────────────────────

    public override async Task<GetUserPaymentServiceResponse> GetUserPaymentService(
        GetUserPaymentServiceRequest request, ServerCallContext context)
    {
        var item = await accountingSvc.GetPaymentServiceAccessForTenantAsync(request.PartyId, null, null);
        return new GetUserPaymentServiceResponse
        {
            Settings = JsonConvert.SerializeObject(item)
        };
    }

    public override async Task<UpdateUserPaymentServiceResponse> UpdateUserPaymentService(
        UpdateUserPaymentServiceRequest request, ServerCallContext context)
    {
        var accesses = JsonConvert.DeserializeObject<Bacera.Gateway.PaymentService.Accesses>(request.Spec)
            ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid spec JSON"));

        await accountingSvc.SetPaymentServiceAccessAsync(request.PartyId, accesses);

        var updated = await accountingSvc.GetPaymentServiceAccessForTenantAsync(request.PartyId, null, null);
        return new UpdateUserPaymentServiceResponse
        {
            Settings = JsonConvert.SerializeObject(updated)
        };
    }

    // ─── Profile / Status / Site ──────────────────────────────────────────────

    public override async Task<UpdateUserProfileResponse> UpdateUserProfile(
        UpdateUserProfileRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var spec = JsonConvert.DeserializeObject<Bacera.Gateway.Auth.User.TenantUpdateSpec>(request.Spec)
            ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid spec JSON"));

        var user = await authDb.Users
            .SingleAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);

        user.Gender      = spec.Gender;
        user.Address     = spec.Address;
        user.Birthday    = spec.Birthday;
        user.Citizen     = spec.Citizen;
        user.FirstName   = spec.FirstName;
        user.LastName    = spec.LastName;
        user.IdType      = spec.IdType;
        user.IdNumber    = spec.IdNumber;
        user.IdIssuer    = spec.IdIssuer;
        user.IdIssuedOn  = spec.IdIssuedOn ?? DateOnly.MinValue;
        user.IdExpireOn  = spec.IdExpireOn ?? DateOnly.MinValue;
        user.NativeName  = spec.NativeName;
        user.ReferCode   = spec.ReferCode;
        user.UpdatedOn   = DateTime.UtcNow;
        user.CCC         = spec.CCC;
        user.PhoneNumber = spec.PhoneNumber;
        user.Email       = spec.Email.ToLower();
        user.NormalizedEmail    = spec.Email.ToUpper();
        user.UserName           = $"{tenantId}_{spec.Email.ToLower()}";
        user.NormalizedUserName = $"{tenantId}_{spec.Email.ToUpper()}";

        authDb.Users.Update(user);
        await authDb.SaveChangesWithAuditAsync(request.PartyId);

        return new UpdateUserProfileResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<UpdateUserStatusResponse> UpdateUserStatus(
        UpdateUserStatusRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var newStatus = (short)request.Status;

        await Task.WhenAll(
            Task.Run(async () =>
            {
                var u = await authDb.Users.SingleAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
                u.Status    = newStatus;
                u.UpdatedOn = DateTime.UtcNow;
                authDb.Users.Update(u);
                await authDb.SaveChangesAsync();
            }),
            Task.Run(async () =>
            {
                var party = await tenantDb.Parties.SingleAsync(x => x.Id == request.PartyId);
                party.Status    = newStatus;
                party.UpdatedOn = DateTime.UtcNow;
                tenantDb.Parties.Update(party);
                await tenantDb.SaveChangesAsync();
            }));

        var user = await authDb.Users
            .SingleAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        return new UpdateUserStatusResponse { Data = MapAuthUserToProto(user) };
    }

    public override async Task<UpdateUserSiteResponse> UpdateUserSite(
        UpdateUserSiteRequest request, ServerCallContext context)
    {
        var party = await tenantDb.Parties.SingleOrDefaultAsync(x => x.Id == request.PartyId);
        if (party == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        party.SiteId   = request.SiteId;
        party.UpdatedOn = DateTime.UtcNow;
        tenantDb.Parties.Update(party);
        await tenantDb.SaveChangesAsync();

        var tenantId = tenantGetter.GetTenantId();
        var user = await authDb.Users
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PartyId == request.PartyId);
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "Auth user not found"));
        return new UpdateUserSiteResponse { Data = MapAuthUserToProto(user) };
    }

    // ─── Lock / Unlock ────────────────────────────────────────────────────────

    public override async Task<LockUserResponse> LockUser(LockUserRequest request, ServerCallContext context)
    {
        await userSvc.LockUserAsync(request.PartyId, GetPartyId(context));

        // Revoke all active tokens via auth service (party-level revocation timestamp)
        try
        {
            await authGrpcClient.RevokeUserAsync(new RevokeUserRequest { PartyId = request.PartyId });
        }
        catch (Exception ex)
        {
            // Non-fatal: legacy Redis lockout cache still provides protection
            logger.LogWarning("LockUser: RevokeUser gRPC failed for partyId={PartyId}: {Error}",
                request.PartyId, ex.Message);
        }

        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new LockUserResponse { Data = result.Data };
    }

    public override async Task<UnlockUserResponse> UnlockUser(UnlockUserRequest request, ServerCallContext context)
    {
        await userSvc.UnlockUserAsync(request.PartyId, GetPartyId(context));
        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new UnlockUserResponse { Data = result.Data };
    }

    public override async Task<UnlockByQuizResponse> UnlockByQuiz(UnlockByQuizRequest request, ServerCallContext context)
    {
        await userSvc.AllowVerificationQuizAsync(request.PartyId);
        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new UnlockByQuizResponse { Data = result.Data };
    }

    // ─── God Mode ─────────────────────────────────────────────────────────────

    public override async Task<EnableGodModeResponse> EnableGodMode(EnableGodModeRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var targetUser = await userManager.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == request.PartyId)
            .SingleOrDefaultAsync();
        if (targetUser == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var userRoles = await userManager.GetRolesAsync(targetUser);
        if (userRoles.Contains(UserRoleTypesString.SuperAdmin) ||
            userRoles.Contains(UserRoleTypesString.TenantAdmin))
            throw new RpcException(new Status(StatusCode.PermissionDenied, "God mode not allowed for this user"));

        var res = await bcrTokenSvc.GetUserTokenAsync(targetUser, godPartyId: GetPartyId(context));

        if (string.IsNullOrEmpty(res.AccessToken))
            throw new RpcException(new Status(StatusCode.Internal, "IssueToken returned empty access token"));

        var oneTimeKey = Guid.NewGuid().ToString("N");
        await cache.SetStringAsync($"godmode:key:{oneTimeKey}", res.AccessToken, TimeSpan.FromSeconds(60));

        var operatorPartyId = GetPartyId(context);
        authDb.UserAudits.Add(new Bacera.Gateway.Auth.UserAudit
        {
            PartyId     = operatorPartyId,
            RowId       = targetUser.Id,
            Type        = (int)AuditTypes.User,
            Action      = (int)AuditActionTypes.Update,
            CreatedOn   = DateTime.UtcNow,
            Environment = System.Net.Dns.GetHostName(),
            Data        = $"{{\"action\":\"GodModeEnabled\",\"operatorPartyId\":{operatorPartyId},\"targetPartyId\":{targetUser.PartyId},\"targetUserId\":{targetUser.Id}}}",
        });
        await authDb.SaveChangesAsync();

        var proto = MapAuthUserToProto(targetUser);
        proto.Token = oneTimeKey;
        return new EnableGodModeResponse { Data = proto };
    }

    // ─── Addresses ────────────────────────────────────────────────────────────

    public override async Task<ListUserAddressesResponse> ListUserAddresses(
        ListUserAddressesRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Address.Criteria
        {
            Page    = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size    = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            PartyId = request.HasPartyId ? request.PartyId : null,
        };

        var items = await tenantDb.Addresses
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new ListUserAddressesResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(a => new ProtoAddress
        {
            Id       = a.Id,
            HashId   = Bacera.Gateway.Address.HashEncode(a.Id),
            PartyId  = a.PartyId,
            Country  = a.Country ?? "",
            Address_ = a.Name    ?? "",
        }));
        return response;
    }

    public override async Task<GetUserAddressResponse> GetUserAddress(GetUserAddressRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Addresses
            .ToTenantDetailModel()
            .SingleOrDefaultAsync(x => x.Id == request.AddressId);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Address not found"));

        return new GetUserAddressResponse
        {
            Data = new ProtoAddress
            {
                Id       = item.Id,
                HashId   = Bacera.Gateway.Address.HashEncode(item.Id),
                PartyId  = item.PartyId,
                Country  = item.Country ?? "",
                Address_ = item.Name    ?? "",
            }
        };
    }

    // ─── Migrate / Duplicate ──────────────────────────────────────────────────

    public override async Task<MigrateUserResponse> MigrateUser(MigrateUserRequest request, ServerCallContext context)
    {
        var currentTenantId = tenantGetter.GetTenantId();
        if (request.TenantId == currentTenantId)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Same tenant"));

        var (res, msg) = await userSvc.MigrateUserAsync(request.PartyId, request.TenantId);
        if (!res) throw new RpcException(new Status(StatusCode.Internal, msg?.Count > 0 ? string.Join("; ", msg) : "Migration failed"));

        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new MigrateUserResponse { Data = result.Data };
    }

    public override async Task<DuplicateUserResponse> DuplicateUser(DuplicateUserRequest request, ServerCallContext context)
    {
        var currentTenantId = tenantGetter.GetTenantId();
        if (request.TenantId == currentTenantId)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Same tenant"));

        var (res, msg) = await userSvc.DuplicateUserToOtherTenantAsync(
            request.PartyId, request.TenantId, false, GetPartyId(context));
        if (!res) throw new RpcException(new Status(StatusCode.Internal, msg ?? "Duplication failed"));

        var result = await GetUser(new GetUserRequest { PartyId = request.PartyId }, context);
        return new DuplicateUserResponse { Data = result.Data };
    }

    // ─── Legacy info ──────────────────────────────────────────────────────────

    public override async Task<GetLegacyPersonalInfoResponse> GetLegacyPersonalInfo(
        GetLegacyPersonalInfoRequest request, ServerCallContext context)
    {
        var info = await GetLegacyInfo(request.PartyId, SupplementTypes.MigrationPersonalInfo);
        return new GetLegacyPersonalInfoResponse { Data = info };
    }

    public override async Task<GetLegacyFinancialInfoResponse> GetLegacyFinancialInfo(
        GetLegacyFinancialInfoRequest request, ServerCallContext context)
    {
        var info = await GetLegacyInfo(request.PartyId, SupplementTypes.MigrationFinancialInfo);
        return new GetLegacyFinancialInfoResponse { Data = info };
    }

    public override async Task<GetLegacyKycFormResponse> GetLegacyKycForm(
        GetLegacyKycFormRequest request, ServerCallContext context)
    {
        var info = await GetLegacyInfo(request.PartyId, SupplementTypes.MigrationKycForm);
        return new GetLegacyKycFormResponse { Data = info };
    }

    public override async Task<GetLegacyKycCorrectionResponse> GetLegacyKycCorrection(
        GetLegacyKycCorrectionRequest request, ServerCallContext context)
    {
        var info = await GetLegacyInfo(request.PartyId, SupplementTypes.MigrationKycCorrection);
        return new GetLegacyKycCorrectionResponse { Data = info };
    }

    // ─── Social media ─────────────────────────────────────────────────────────

    public override async Task<GetSocialMediaInfoResponse> GetSocialMediaInfo(
        GetSocialMediaInfoRequest request, ServerCallContext context)
    {
        var supplement = await tenantDb.Supplements
            .Where(x => x.RowId == request.PartyId && x.Type == (long)SupplementTypes.SocialMediaRecord)
            .SingleOrDefaultAsync();

        if (supplement == null) return new GetSocialMediaInfoResponse { Data = "[]" };

        return new GetSocialMediaInfoResponse { Data = supplement.Data ?? "[]" };
    }

    public override async Task<UpdateSocialMediaInfoResponse> UpdateSocialMediaInfo(
        UpdateSocialMediaInfoRequest request, ServerCallContext context)
    {
        var supplement = await tenantDb.Supplements
            .Where(x => x.RowId == request.PartyId && x.Type == (long)SupplementTypes.SocialMediaRecord)
            .SingleOrDefaultAsync();

        if (supplement == null)
        {
            var entry = tenantDb.Supplements.Add(
                Bacera.Gateway.Supplement.Build(SupplementTypes.SocialMediaRecord, request.PartyId,
                    JsonConvert.SerializeObject(new List<Bacera.Gateway.Auth.User.SocialMediaType>())));
            supplement = entry.Entity;
            await tenantDb.SaveChangesAsync();
        }

        var data = JsonConvert.DeserializeObject<List<Bacera.Gateway.Auth.User.SocialMediaType>>(supplement.Data)
                   ?? new List<Bacera.Gateway.Auth.User.SocialMediaType>();

        var spec = new Bacera.Gateway.Auth.User.SocialMediaType
        {
            Name      = request.Name,
            Account   = request.Account,
            ConnectId = request.HasConnectId  ? request.ConnectId  : null,
            StaffName = request.HasStaffName  ? request.StaffName  : null,
        };
        var index = data.FindIndex(x => x.Name == spec.Name);
        if (index >= 0) data[index] = spec; else data.Add(spec);

        supplement.Data = JsonConvert.SerializeObject(data);
        await tenantDb.SaveChangesAsync();

        return new UpdateSocialMediaInfoResponse { Data = supplement.Data };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<string> GetLegacyInfo(long partyId, SupplementTypes type)
    {
        var supplement = await tenantDb.Supplements
            .Where(x => x.Type == (int)type && x.RowId == partyId)
            .SingleOrDefaultAsync();
        return supplement?.Data ?? "null";
    }

    private static PaginationMeta BuildMeta(int page, int size, int total)
        => new PaginationMeta
        {
            Page      = page,
            Size      = size,
            Total     = total,
            PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
            HasMore   = page * size < total,
        };

    private static ProtoUser MapToProto(Bacera.Gateway.Auth.User.TenantPageModel u)
        => new ProtoUser
        {
            Id        = u.Id,
            PartyId   = u.PartyId,
            Email     = u.Email     ?? "",
            FirstName = u.FirstName ?? "",
            LastName  = u.LastName  ?? "",
            Status    = (int)u.Status,
            CreatedAt = u.CreatedOn.ToString("O"),
        };

    private static ProtoUser MapDetailToProto(Bacera.Gateway.Auth.User.TenantDetailModel u)
        => new ProtoUser
        {
            Id        = u.Id,
            PartyId   = u.PartyId,
            Email     = u.Email     ?? "",
            FirstName = u.FirstName ?? "",
            LastName  = u.LastName  ?? "",
            Status    = u.Status,
            CreatedAt = u.CreatedOn.ToString("O"),
        };

    private static ProtoUser MapAuthUserToProto(Bacera.Gateway.Auth.User u)
        => new ProtoUser
        {
            Id        = u.Id,
            PartyId   = u.PartyId,
            Email     = u.Email     ?? "",
            FirstName = u.FirstName ?? "",
            LastName  = u.LastName  ?? "",
            Status    = u.Status,
            CreatedAt = u.CreatedOn.ToString("O"),
        };

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}

/// <summary>
/// gRPC JSON Transcoding implementation of TenantRoleService.
/// Replaces Areas/Tenant/Controllers/RoleController.cs.
/// Routes defined via google.api.http annotations in user.proto.
/// </summary>
public class TenantRoleGrpcService(
    AuthDbContext authDb,
    RoleManager<ApplicationRole> roleManager)
    : TenantRoleService.TenantRoleServiceBase
{
    public override async Task<ListRolesResponse> ListRoles(ListRolesRequest request, ServerCallContext context)
    {
        var roles = await roleManager.Roles.OrderByDescending(x => x.Id).ToListAsync();
        var response = new ListRolesResponse();
        response.Roles.AddRange(roles.Select(r => new ProtoRole { Id = r.Id, Name = r.Name ?? "" }));
        return response;
    }

    public override async Task<GetRoleResponse> GetRole(GetRoleRequest request, ServerCallContext context)
    {
        var role = await authDb.Roles.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (role == null) throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
        return new GetRoleResponse { Data = new ProtoRole { Id = role.Id, Name = role.Name ?? "" } };
    }
}

/// <summary>
/// gRPC JSON Transcoding implementation of TenantPermissionService.
/// Replaces Areas/Tenant/Controllers/PermissionController.cs.
/// Routes defined via google.api.http annotations in user.proto.
/// </summary>
public class TenantPermissionGrpcService : TenantPermissionService.TenantPermissionServiceBase
{
    public override Task<ListPermissionsResponse> ListPermissions(ListPermissionsRequest request, ServerCallContext context)
    {
        var response = new ListPermissionsResponse();
        response.Permissions.AddRange(UserPermissionTypes.All.OrderBy(x => x));
        return Task.FromResult(response);
    }
}
