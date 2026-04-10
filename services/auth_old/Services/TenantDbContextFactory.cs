using Bacera.Gateway.Auth.Db;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Auth.Services;

/// <summary>
/// Creates per-tenant TenantDbContext instances by looking up the tenant's database name
/// from the central DB (core."_Tenant" table), mirroring mono's MyDbContextPool behaviour.
/// </summary>
public class TenantDbContextFactory(IConfiguration config, ILogger<TenantDbContextFactory> logger)
{
    private readonly string _baseConnectionString = BuildBaseConnectionString(config);

    public async Task<TenantDbContext?> CreateAsync(long tenantId, CancellationToken ct = default)
    {
        try
        {
            var dbName = await GetTenantDatabaseNameAsync(tenantId, ct);
            if (string.IsNullOrEmpty(dbName))
            {
                logger.LogWarning("No database found for tenantId {TenantId}", tenantId);
                return null;
            }

            var connStr = ReplaceDatabase(_baseConnectionString, dbName);
            var opts = new DbContextOptionsBuilder<TenantDbContext>()
                .UseNpgsql(connStr)
                .Options;
            return new TenantDbContext(opts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create TenantDbContext for tenantId {TenantId}", tenantId);
            return null;
        }
    }

    private async Task<string?> GetTenantDatabaseNameAsync(long tenantId, CancellationToken ct)
    {
        var opts = new DbContextOptionsBuilder<CentralDbContext>()
            .UseNpgsql(_baseConnectionString)
            .Options;
        await using var db = new CentralDbContext(opts);
        // SqlQueryRaw<string> requires the column to be aliased as "Value"
        return await db.Database
            .SqlQueryRaw<string>(
                """SELECT "DatabaseName" AS "Value" FROM core."_Tenant" WHERE "Id" = {0} LIMIT 1""",
                tenantId)
            .FirstOrDefaultAsync(ct);
    }

    private static string BuildBaseConnectionString(IConfiguration config)
    {
        var host = config["DB_HOST"] ?? "localhost";
        var port = config["DB_PORT"] ?? "5432";
        var user = config["DB_USERNAME"] ?? "postgres";
        var password = config["DB_PASSWORD"] ?? "";
        var database = config["DB_DATABASE"] ?? "portal_central";
        return $"Host={host};Port={port};Username={user};Password={password};Database={database}";
    }

    private static string ReplaceDatabase(string connectionString, string newDatabase)
    {
        // Replace the Database= segment in the connection string
        var parts = connectionString.Split(';');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                parts[i] = $"Database={newDatabase}";
        }
        return string.Join(';', parts);
    }
}
