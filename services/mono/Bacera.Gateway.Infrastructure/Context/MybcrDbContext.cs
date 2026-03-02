using System;
using System.Collections.Generic;
using Bacera.Gateway;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Bacera.Gateway.Context;

public partial class MybcrDbContext : DbContext
{
    public MybcrDbContext()
    {
    }

    public MybcrDbContext(DbContextOptions<MybcrDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Server> Servers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("servers")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Creator)
                .HasMaxLength(255)
                .HasColumnName("creator");
            entity.Property(e => e.Eip)
                .HasMaxLength(255)
                .HasComment("eg. 1.0.0.01")
                .HasColumnName("eip");
            entity.Property(e => e.Instance)
                .HasMaxLength(255)
                .HasComment("eg. t3.small")
                .HasColumnName("instance");
            entity.Property(e => e.Instanceid)
                .HasMaxLength(255)
                .HasComment("eg. i-0ae40f12f7ec1cac5")
                .HasColumnName("instanceid");
            entity.Property(e => e.Ip)
                .HasMaxLength(255)
                .HasComment("eg. 18.163.55.174")
                .HasColumnName("ip");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("last_update");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("eg. ap-east-1")
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasColumnType("text")
                .HasColumnName("note");
            entity.Property(e => e.Provider)
                .HasMaxLength(255)
                .HasComment("eg. aws do")
                .HasColumnName("provider");
            entity.Property(e => e.Region)
                .HasMaxLength(255)
                .HasComment("eg. ap-east-1")
                .HasColumnName("region");
            entity.Property(e => e.Stat)
                .HasMaxLength(20)
                .HasDefaultValueSql("'none'")
                .HasColumnName("stat");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Sudo)
                .HasMaxLength(255)
                .HasColumnName("sudo");
            entity.Property(e => e.System)
                .HasMaxLength(64)
                .HasDefaultValueSql("'Ubuntu'")
                .HasColumnName("system");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasComment("eg. ec2 rds ...")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasComment("eg. it@bacera.com")
                .HasColumnName("username");
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}