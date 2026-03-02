using Bacera.Gateway.Connection;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Permission;

using M = Gateway.Permission;

public class PermissionService(AuthDbContext authCtx, AuthDbConnection authCon, IMyCache myCache)
{
    private static readonly string CacheKey = CacheKeys.GetPermissionKey();

    public Task<List<M>> GetAllAsync() => myCache.GetOrSetAsync(CacheKey
        , () => authCtx.Permissions.ToListAsync()
        , TimeSpan.FromHours(12)
    );

    public async Task<bool> AddUserPermissionAsync(long tenantId, long partyId, long permissionId)
    {
        var permission = await authCtx.Permissions.FindAsync(permissionId);
        if (permission == null) return false;

        var user = await authCtx.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .Where(x => x.Permissions.All(y => y.Id != permissionId))
            .SingleOrDefaultAsync();

        if (user == null) return false;

        user.Permissions.Add(permission);
        await authCtx.SaveChangesAsync();
        return true;
    }

    // remove user permission
    public async Task<bool> RemoveUserPermissionAsync(long tenantId, long partyId, long permissionId)
    {
        var exists = await authCtx.Permissions.AnyAsync(x => x.Id == permissionId);
        if (!exists) return false;

        var user = await authCtx.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .Where(x => x.Permissions.Any(y => y.Id == permissionId))
            .Include(x => x.Permissions.Where(y => y.Id == permissionId))
            .SingleOrDefaultAsync();

        if (user == null) return false;
        var permission = user.Permissions.SingleOrDefault(x => x.Id == permissionId);
        if (permission == null) return true;

        user.Permissions.Remove(permission);
        await authCtx.SaveChangesAsync();
        return true;
    }

    public Task<bool> UserHasPermissionAsync(long tenantId, long partyId, long permissionId) => authCtx.Users
        .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
        .SelectMany(x => x.Permissions)
        .AnyAsync(x => x.Id == permissionId);


    public async Task<bool> AddRolePermissionAsync(string roleName, long permissionId)
    {
        var permission = await authCtx.Permissions.FindAsync(permissionId);
        if (permission == null) return false;

        var item = await authCtx.ApplicationRoles
            .Where(x => x.Name == roleName)
            .Where(x => x.Permissions.All(y => y.Id != permissionId))
            .SingleOrDefaultAsync();

        if (item == null) return false;

        item.Permissions.Add(permission);
        await authCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRolePermissionAsync(string roleName, long permissionId)
    {
        var exists = await authCtx.Permissions.AnyAsync(x => x.Id == permissionId);
        if (!exists) return false;

        var item = await authCtx.ApplicationRoles
            .Where(x => x.Name == roleName)
            .Where(x => x.Permissions.Any(y => y.Id == permissionId))
            .Include(x => x.Permissions.Where(y => y.Id == permissionId))
            .SingleOrDefaultAsync();

        if (item == null) return false;
        var permission = item.Permissions.SingleOrDefault(x => x.Id == permissionId);
        if (permission == null) return true;

        item.Permissions.Remove(permission);
        await authCtx.SaveChangesAsync();
        return true;
    }

    public Task<bool> RoleHasPermissionAsync(string roleName, long permissionId) => authCtx.ApplicationRoles
        .Where(x => x.Name == roleName)
        .SelectMany(x => x.Permissions)
        .AnyAsync(x => x.Id == permissionId);

    public async Task<List<long>> GetUserPermissionIdsAsync(long tenantId, long partyId)
    {
        var items = await authCtx.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .SelectMany(x => x.Permissions)
            .Select(x => x.Id)
            .ToListAsync();

        return items;
    }

    public async Task<List<long>> GetRolePermissionIdsAsync(params string[] roles)
    {
        var items = await authCtx.UserRoles
            .Where(x => roles.Contains(x.ApplicationRole.Name))
            .SelectMany(x => x.ApplicationRole.Permissions)
            .Select(x => x.Id)
            .ToListAsync();

        return items;
    }
    

    public async Task<List<string>> GetUserWebPermissions(long tenantId, long partyId)
    {
        var userPermissions = await authCtx.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .SelectMany(x => x.Permissions)
            .Where(x => x.Category == "WEB")
            .Select(x => x.Action)
            .Distinct()
            .ToListAsync();

        var rolePermissions = await authCtx.UserRoles
            .Where(x => x.User.TenantId == tenantId && x.User.PartyId == partyId)
            .SelectMany(x => x.ApplicationRole.Permissions)
            .Where(x => x.Category == "WEB")
            .Select(x => x.Action)
            .Distinct()
            .ToListAsync();

        return userPermissions.Concat(rolePermissions).Distinct().ToList();
    }

    public async Task<bool> CreateAsync(bool auth, string action, string method, string category, string key)
    {
        var exists = await authCtx.Permissions.AnyAsync(x => x.Action == action && x.Method == method && x.Category == category);
        if (exists) return false;

        var item = new M
        {
            Auth = auth,
            Action = action,
            Method = method,
            Category = category,
            Key = key
        };
        authCtx.Permissions.Add(item);
        await authCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAuthAsync(long id)
    {
        var item = await authCtx.Permissions.FindAsync(id);
        if (item == null) return false;

        item.Auth = !item.Auth;
        await authCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsActionAllowForUser(long tenantId, long partyId, string url, string method)
    {
        // return true;
        var item = await GetActionAsync(url, method);
        if (item == null) return true;
        if (item.Auth == false) return true;

        var authFromUserOrRole = await authCtx.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .AnyAsync(x => x.Permissions.Any(y => y.Id == item.Id)
                           || x.UserRoles.Any(y => y.ApplicationRole.Permissions.Any(z => z.Id == item.Id)));

        return authFromUserOrRole;
    }

    private async Task<M.BasicModel?> GetActionAsync(string url, string method)
    {
        var item = await authCon.FirstOrDefaultAsync<M.BasicModel>($"""
                                                                    SELECT "Id", "Auth", "Action", "Method", "Category", "Key"
                                                                    FROM auth."_Permission"
                                                                    WHERE '{url}' ~ "Action" and "Method" = '{method.ToUpper()}';
                                                                    """);
        return item;
    }

}
