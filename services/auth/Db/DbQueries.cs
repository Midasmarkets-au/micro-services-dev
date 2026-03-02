using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Auth.Db;

public static class DbQueries
{
    public static Task<List<User>> FindUsersByEmailAsync(AuthDbContext db, string email, CancellationToken ct = default)
        => db.Users
            .Where(u => u.Status == 0 && u.Email != null && u.Email.ToLower() == email)
            .ToListAsync(ct);

    public static Task<bool> IsIpBlockedAsync(CentralDbContext db, string ip, CancellationToken ct = default)
        => db.IpBlackLists.AnyAsync(x => x.Ip == ip, ct);

    public static async Task<List<string>> GetUserRolesAsync(AuthDbContext db, long userId, CancellationToken ct = default)
    {
        var roles = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT r."Name"
                FROM auth."_UserRole" ur
                JOIN auth."_Role" r ON ur."RoleId" = r."Id"
                WHERE ur."UserId" = {0}
                """, userId)
            .ToListAsync(ct);
        return roles;
    }

    public static Task UpdateLastLoginAsync(AuthDbContext db, long userId, string ip, CancellationToken ct = default)
    {
        return db.Database.ExecuteSqlRawAsync(
            """UPDATE auth."_User" SET "LastLoginOn" = NOW(), "LastLoginIp" = {0} WHERE "Id" = {1}""",
            [ip, userId], ct);
    }

    /// <summary>
    /// Retrieves the TOTP authenticator key from auth."_UserToken" (ASP.NET Identity storage).
    /// The key is Base32-encoded and stored by the Authenticator token provider.
    /// </summary>
    public static async Task<string?> GetAuthenticatorKeyAsync(AuthDbContext db, long userId, CancellationToken ct = default)
    {
        var results = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT "Value"
                FROM auth."_UserToken"
                WHERE "UserId" = {0}
                  AND "LoginProvider" = '[AspNetUserStore]'
                  AND "Name" = 'AuthenticatorKey'
                LIMIT 1
                """, userId)
            .ToListAsync(ct);
        return results.FirstOrDefault();
    }
}
