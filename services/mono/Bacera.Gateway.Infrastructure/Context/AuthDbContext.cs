#nullable disable
using Bacera.Gateway.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Bacera.Gateway;

public partial class AuthDbContext : IdentityDbContext<
    User, ApplicationRole, long,
    UserClaim,
    UserRole,
    IdentityUserLogin<long>,
    RoleClaim,
    IdentityUserToken<long>
>
{
    public virtual DbSet<UserAudit> UserAudits { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }

    // public virtual DbSet<PermissionRoleAccess> PermissionRoleAccesses { get; set; }

    // public virtual DbSet<PermissionUserAccess> PermissionUserAccesses { get; set; }
    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

    // OpenIddict tables
    public virtual DbSet<OpenIddictEntityFrameworkCoreApplication> OpenIddictApplications { get; set; }
    public virtual DbSet<OpenIddictEntityFrameworkCoreAuthorization> OpenIddictAuthorizations { get; set; }
    public virtual DbSet<OpenIddictEntityFrameworkCoreScope> OpenIddictScopes { get; set; }
    public virtual DbSet<OpenIddictEntityFrameworkCoreToken> OpenIddictTokens { get; set; }

    public AuthDbContext(
        DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=portal_central;Username=postgres;Password=dev");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("_User", "auth");
            b.HasIndex(u => u.Uid).IsUnique();
            b.HasIndex(e => e.PartyId, "_User_PartyId_index");
            b.HasIndex(e => e.TenantId, "_User_TenantId_index");
            b.HasIndex(e => e.Status, "_User_Status_index");

            b.HasIndex(e => new { e.TenantId, e.PartyId }, "_User_TenantId_PartyId_index").IsUnique();

            b.Property(x => x.LastLoginIp).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            b.HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<ApplicationRole>(entity => { entity.ToTable("_Role", "auth"); });

        modelBuilder.Entity<RoleClaim>(entity => { entity.ToTable("_RoleClaim", "auth"); });

        modelBuilder.Entity<UserClaim>(entity => { entity.ToTable("_UserClaim", "auth"); });


        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("_UserRole", "auth");

            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            entity.HasOne(ur => ur.ApplicationRole)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            entity.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityUserLogin<long>>(entity => { entity.ToTable("_UserLogin", "auth"); });
        modelBuilder.Entity<IdentityUserToken<long>>(entity => { entity.ToTable("_UserToken", "auth"); });

        // OpenIddict — store all 4 tables in the "identity" schema
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication>(b => b.ToTable("OpenIddictApplications", "identity"));
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization>(b => b.ToTable("OpenIddictAuthorizations", "identity"));
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope>(b => b.ToTable("OpenIddictScopes", "identity"));
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken>(b => b.ToTable("OpenIddictTokens", "identity"));


        modelBuilder.Entity<UserAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_UserAudit_pk");

            entity.ToTable("_UserAudit", "auth");

            entity.HasIndex(e => e.PartyId, "_UserAudit_PartyId_index");

            entity.HasIndex(e => new { e.Type, e.RowId }, "_UserAudit_Type_RowId_index");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_permission_pk");

            entity.ToTable("_Permission", "auth");

            entity.HasIndex(e => e.Action, "_Permission_Action_index");

            entity.HasIndex(e => e.Category, "_Permission_Category_index");

            entity.HasIndex(e => e.Method, "_Permission_Method_index");

            entity.Property(e => e.Action)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Key)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Method)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasMany(d => d.Users).WithMany(p => p.Permissions)
                .UsingEntity<Dictionary<string, object>>(
                    "PermissionUserAccess",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("_PermissionUserAccess__UserId_fk"),
                    l => l.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("_PermissionUserAccess__Permission_fk"),
                    j =>
                    {
                        j.HasKey("UserId", "PermissionId").HasName("_PermissionUserAccess_pk");
                        j.ToTable("_PermissionUserAccess", "auth");
                    });

            entity.HasMany(d => d.ApplicationRoles).WithMany(p => p.Permissions)
                .UsingEntity<Dictionary<string, object>>(
                    "PermissionRoleAccess",
                    r => r.HasOne<ApplicationRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("_PermissionUserAccess__RoleId_fk"),
                    l => l.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("_PermissionUserAccess__Permission_fk"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("_PermissionRoleAccess_pk");
                        j.ToTable("_PermissionRoleAccess", "auth");
                    });
        });

        // modelBuilder.Entity<PermissionRoleAccess>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_PermissionRoleAccess_pk");
        //
        //     entity.ToTable("_PermissionRoleAccess", "auth");
        //
        //     entity.HasIndex(e => e.PermissionId, "_PermissionRoleAccess_PermissionId_index");
        //
        //     entity.HasIndex(e => e.RoleId, "_PermissionRoleAccess_RoleId_index");
        //
        //     entity.HasOne(d => d.Permission).WithMany(p => p.PermissionRoleAccesses)
        //         .HasForeignKey(d => d.PermissionId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_PermissionRoleAccess_PermissionId_fk");
        //
        //     entity.HasOne(d => d.Role).WithMany(p => p.PermissionRoleAccesses)
        //         .HasForeignKey(d => d.RoleId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_PermissionRoleAccess_RoleId_fk");
        // });
        //
        // modelBuilder.Entity<PermissionUserAccess>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_PermissionUserAccess_pk");
        //     entity.ToTable("_PermissionUserAccess", "auth");
        //     entity.HasIndex(e => e.PermissionId, "_PermissionUserAccess_PermissionId_index");
        //     entity.HasIndex(e => e.UserId, "_PermissionUserAccess_UserId_index");
        //
        //     entity.HasOne(d => d.Permission).WithMany(p => p.PermissionUserAccesses)
        //         .HasForeignKey(d => d.PermissionId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_PermissionUserAccess_PermissionId_fk");
        //
        //     entity.HasOne(d => d.User).WithMany(p => p.PermissionUserAccesses)
        //         .HasForeignKey(d => d.UserId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_PermissionUserAccess_UserId_fk");
        // });
    }
}