#nullable disable
using Bacera.Gateway.Central;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bacera.Gateway;

public partial class CentralDbContext(DbContextOptions<CentralDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public virtual DbSet<Domain> Domains { get; set; }
    public virtual DbSet<Tenant> Tenants { get; set; }
    public virtual DbSet<CentralParty> CentralParties { get; set; }
    public virtual DbSet<IpBlackList> IpBlackLists { get; set; }
    public virtual DbSet<UserBlackList> UserBlackLists { get; set; }

    public virtual DbSet<CentralAccount> CentralAccounts { get; set; }
    public virtual DbSet<CentralConfig> CentralConfigs { get; set; }
    public virtual DbSet<CentralReferralCode> CentralReferralCodes { get; set; }
    public virtual DbSet<CentralApiLog> CentralApiLogs { get; set; }
    public virtual DbSet<CentralTradeService> CentralTradeServices { get; set; }
    public virtual DbSet<MetaTrade> MetaTrades { get; set; }
    
    // Data Protection Keys (for encryption/decryption across all tenants)
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=portal_central;Username=postgres;Password=dev;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Data Protection Keys table
        modelBuilder.Entity<DataProtectionKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DataProtectionKeys_pkey");
            entity.ToTable("DataProtectionKeys", "core");
        });

        modelBuilder.Entity<CentralApiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("apiLog_pkey");
            entity.ToTable("_ApiLog", "core");
            entity.HasIndex(e => e.Method, "_ApiLog_Method_index");
            entity.HasIndex(e => e.PartyId, "_ApiLog_PartyId_index");
            entity.HasIndex(e => e.Ip, "_ApiLog_Ip_index");
            entity.HasIndex(e => e.Action, "_ApiLog_Action_index");
            entity.HasIndex(e => e.StatusCode, "_ApiLog_StatusCode_index");
            entity.HasIndex(e => e.ConnectionId, "_ApiLog_ConnectionId_index");
            
            entity.Property(e => e.Method).HasMaxLength(12).HasDefaultValueSql("''::character varying").HasColumnType("character varying");
            entity.Property(e => e.Action).HasMaxLength(256).HasDefaultValueSql("''::character varying").HasColumnType("character varying");
            entity.Property(e => e.ConnectionId).HasMaxLength(256).HasDefaultValueSql("''::character varying").HasColumnType("character varying");

            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Domain>(entity =>
        {
            entity.Property(e => e.Id).HasIdentityOptions(startValue: 10000);
            entity.HasKey(e => e.Id).HasName("domain_pkey");

            entity.ToTable("_Domain", "core");

            entity.HasIndex(e => e.TenantId, "IX_domains_tenant_id");
            entity.HasIndex(e => e.DomainName, "UX_Domain_DomainName").IsUnique();

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.DomainName).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Domains)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("core_domain_tenant_id_foreign");
        });

        modelBuilder.Entity<CentralTradeService>(entity =>
        {
            entity.ToTable("_CentralTradeService", "trd");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UserBlackList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_black_lists_pkey");
            entity.Property(e => e.Id).HasIdentityOptions(startValue: 10000);
            entity.ToTable("_UserBlackList", "core");

            entity.HasIndex(e => e.Name, "_UserBlackList_Name_Index");
            entity.HasIndex(e => e.Phone, "_UserBlackList_Phone_Index");
            entity.HasIndex(e => e.Email, "_UserBlackList_Email_Index");
            entity.HasIndex(e => e.IdNumber, "_UserBlackList_IdNumber_Index");
            entity.HasIndex(e => e.OperatorName, "_UserBlackList_OperatorName_Index");
            entity.Property(e => e.Name).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Phone).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Email).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.OperatorName).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<IpBlackList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ip_black_lists_pkey");
            entity.Property(e => e.Id).HasIdentityOptions(startValue: 10000);
            entity.ToTable("_IpBlackList", "core");

            entity.HasIndex(e => e.Ip, "_IpBlackList_Ip_Index").IsUnique();
            entity.HasIndex(e => e.OperatorName, "_IpBlackList_OperatorName_Index");

            entity.Property(e => e.Ip).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.OperatorName).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Note).HasMaxLength(255).HasDefaultValueSql("''::character varying");
            // entity.Property(e => e.Enabled).HasDefaultValueSql("true");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(e => e.Id).HasIdentityOptions(startValue: 10000);
            entity.HasKey(e => e.Id).HasName("tenant_pkey");

            entity.ToTable("_Tenant", "core");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.DatabaseName).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<CentralParty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CentralParty_PrimeKey");

            entity.ToTable("_CentralParty", "core");

            entity.HasIndex(e => e.SiteId, "_CentralParty_SiteId_Index");

            entity.HasIndex(e => e.Code, "_CentralParty_Code_Index");

            entity.HasIndex(e => e.NativeName, "_CentralParty_NativeName_Index");
            entity.HasIndex(e => e.Email, "_CentralParty_Email_Index");
            entity.HasIndex(e => e.Uid, "_CentralParty_Uid_Index");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CentralAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PrimeKey");
            entity.ToTable("_CentralAccount", "trd");
            entity.HasIndex(e => new
                {
                    e.TenantId,
                    e.ServiceId,
                    e.AccountId,
                    e.AccountNumber
                },
                "_CentralAccount_Ids_Index");
            entity.HasIndex(e => e.Uid, "_CentralAccount_Uid_Index");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Tenant).WithMany(p => p.CentralAccounts)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("_CentralAccount_TenantId_Foreign");
        });

        modelBuilder.Entity<CentralConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CentralConfig_pk");

            entity.ToTable("_CentralConfig", "core");

            entity.HasIndex(e => e.RowId, "IX__CentralConfig_RowId");
            entity.HasIndex(e => e.Key, "IX__CentralConfig_Key");
            entity.HasIndex(e => e.Category, "IX__CentralConfig_Category");
            entity.HasIndex(e => e.DataFormat, "IX__CentralConfig_DataFormat");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name).HasMaxLength(32);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });


        modelBuilder.Entity<CentralReferralCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CentralReferralCode_PrimeKey");
            entity.ToTable("_CentralReferralCode", "core");

            entity.HasIndex(e => e.Code, "_CentralReferralCode_Code_Index");
            entity.HasIndex(e => e.Name, "_CentralReferralCode_Name_Index");
            entity.HasIndex(e => e.TenantId, "_CentralReferralCode_TenantId_Index");
            entity.HasIndex(e => e.PartyId, "_CentralReferralCode_PartyId_Index");
            entity.HasIndex(e => e.AccountId, "_CentralReferralCode_AccountId_Index");

            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Tenant).WithMany(p => p.CentralReferralCodes)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("_CentralReferralCode_TenantId_Foreign");
        });

        modelBuilder.Entity<MetaTrade>(entity =>
        {
            entity.Property(e => e.Id).HasIdentityOptions(startValue: 10000);
            entity.HasKey(e => e.Id).HasName("meta_trade_trade_pkey");
            entity.ToTable("_MetaTrade", "trd");
            entity.HasIndex(e => e.TenantId, "IX_meta_trade_trade_tenant_id");
            entity.HasIndex(e => e.AccountNumber, "IX_meta_trade_trade_account_number");
            entity.HasIndex(e => e.ServiceId, "IX_meta_trade_trade_service_id");
            entity.HasIndex(e => e.Ticket, "IX_meta_trade_trade_ticket");
            entity.HasIndex(e => e.Symbol, "IX_meta_trade_trade_symbol");
            entity.HasIndex(e => e.Cmd, "IX_meta_trade_trade_cmd");
            entity.HasIndex(e => e.OpenAt, "IX_meta_trade_trade_open_at");
            entity.HasIndex(e => e.CloseAt, "IX_meta_trade_trade_close_at");
            entity.HasIndex(e => e.TimeStamp, "IX_meta_trade_trade_time_stamp");

            entity.HasIndex(e => new { e.ServiceId, e.Ticket }, "IX_meta_trade_trade_service_id_ticket")
                .IsUnique();

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Tenant).WithMany(p => p.CentralTrades)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("trd_meta_trade_trade_tenant_id_foreign");
        });

        OnModelCreatingPartial(modelBuilder);

        //seeding data
        modelBuilder.Entity<Tenant>().HasData(
            new Tenant
            {
                Id = 1,
                Name = "Demo Tenant",
                DatabaseName = "portal_tenant_1"
            });
        modelBuilder.Entity<Domain>().HasData(
            new Domain
            {
                Id = 1,
                TenantId = 1,
                DomainName = "demo.localhost"
            }
        );
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public bool IsDatabaseExists(string databaseName)
    {
        var dbExists = false;
        using var conn = new NpgsqlConnection(Database.GetConnectionString());
        conn.Open();
        var cmdText = "SELECT 1 FROM pg_catalog.pg_database WHERE lower(datname) = lower('" + databaseName + "')";
        using var cmd = new NpgsqlCommand(cmdText, conn);
        dbExists = cmd.ExecuteScalar() != null;
        return dbExists;
    }
}