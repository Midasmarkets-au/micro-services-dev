using Microsoft.AspNetCore.DataProtection; // For AddDataProtection
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore; // For PersistKeysToDbContext

namespace Bacera.Gateway.Web;

public static partial class Startup
{
    /// <summary>
    /// Configure ASP.NET Core Data Protection to use persistent key storage
    /// This ensures encryption keys are shared across all environments (local, testing, production)
    /// Keys are stored in CentralDbContext (not TenantDbContext) because they are application-wide,
    /// not tenant-specific. All tenants share the same encryption keys.
    /// </summary>
    public static void SetupDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        // Default to "Database" storage if not specified
        var keyStorageType = configuration.GetValue<string>("DataProtection:KeyStorage", "Database");
        
        var dataProtection = services.AddDataProtection()
            .SetApplicationName("Bacera.Gateway"); // Critical: must be same across all environments
        
        switch (keyStorageType.ToLower())
        {
            case "database":
                // Store keys in PostgreSQL CENTRAL database (application-wide, shared by all tenants)
                // Keys will be stored in core."DataProtectionKeys" table
                dataProtection.PersistKeysToDbContext<CentralDbContext>();
                break;
                
            case "filesystem":
                // Store keys in a shared file system path
                var keysPath = configuration.GetValue<string>("DataProtection:KeysPath");
                if (!string.IsNullOrEmpty(keysPath))
                {
                    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
                }
                break;
                
            case "redis":
                // Store keys in Redis (if using Redis)
                //var redisConnection = configuration.GetConnectionString("Redis");
                //if (!string.IsNullOrEmpty(redisConnection))
                //{
                //    var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection);
                //    dataProtection.PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                //}
                break;
                
            default:
                // Default: local file system (NOT RECOMMENDED for multi-server deployments)
                // Keys will only work on the current server
                break;
        }
    }
}

