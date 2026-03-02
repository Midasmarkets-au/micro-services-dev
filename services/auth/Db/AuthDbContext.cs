using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Auth.Db;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("_User", "auth");
            e.HasKey(x => x.Id);
        });
    }
}

public class CentralDbContext(DbContextOptions<CentralDbContext> options) : DbContext(options)
{
    public DbSet<IpBlackList> IpBlackLists => Set<IpBlackList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IpBlackList>(e =>
        {
            e.ToTable("_IpBlackList", "core");
            e.HasKey(x => x.Id);
        });
    }
}

/// <summary>
/// Per-tenant DB context — connects to the tenant schema of the same Postgres instance.
/// Uses a dynamic connection string keyed by tenantId.
/// </summary>
public class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public DbSet<AuthCode> AuthCodes => Set<AuthCode>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthCode>(e =>
        {
            e.ToTable("_AuthCode", "core");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Configuration>(e =>
        {
            e.ToTable("_Configuration", "core");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<LoginLog>(e =>
        {
            e.ToTable("_LoginLog", "core");
            e.HasKey(x => x.Id);
        });
    }
}

public class AuthCode
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string Code { get; set; } = "";
    public string Event { get; set; } = "";
    public string MethodValue { get; set; } = "";
    public short Method { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }

    public const string TwoFactorEvent = "TwoFactor";
    public const short MethodEmail = 1;
    public const short StatusValid = 0;
    public const short StatusInvalid = 1;
}

public class Configuration
{
    public long Id { get; set; }
    public string Category { get; set; } = "";
    public long RowId { get; set; }
    public string Key { get; set; } = "";
    public string? Value { get; set; }
}

public class LoginLog
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string UserAgent { get; set; } = "";
    public string Referer { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public DateTime CreatedOn { get; set; }
}
