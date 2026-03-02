using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

public partial class WebsiteDbContext(DbContextOptions<WebsiteDbContext> options) : DbContext(options)
{
    public virtual DbSet<EconomicCalendar> EconomicCalendars { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<EconomicCalendar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("economic_calendar")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Actual)
                .HasPrecision(8, 2)
                .HasComment("事件数值")
                .HasColumnName("actual");
            entity.Property(e => e.Change)
                .HasPrecision(8, 2)
                .HasComment("事件数值")
                .HasColumnName("change");
            entity.Property(e => e.ChangePercentage)
                .HasPrecision(8, 2)
                .HasComment("事件数值")
                .HasColumnName("change_percentage");
            entity.Property(e => e.Country)
                .HasMaxLength(255)
                .HasComment("事件国家")
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Data)
                .HasComment("api的整个数据整合的json")
                .HasColumnType("json")
                .HasColumnName("data");
            entity.Property(e => e.Date)
                .HasComment("事件日期")
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Estimate)
                .HasPrecision(8, 2)
                .HasComment("事件预期或预算")
                .HasColumnName("estimate");
            entity.Property(e => e.Event)
                .HasMaxLength(255)
                .HasComment("事件名称")
                .HasColumnName("event");
            entity.Property(e => e.Impact)
                .HasMaxLength(255)
                .HasComment("事件影响力,高,中,低,3个值")
                .HasColumnName("impact");
            entity.Property(e => e.Language)
                .HasMaxLength(255)
                .HasComment("语言")
                .HasColumnName("language");
            entity.Property(e => e.Previous)
                .HasPrecision(8, 2)
                .HasComment("事件数值")
                .HasColumnName("previous");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("news")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasComment("新闻类型")
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Data)
                .HasComment("整个api的数据")
                .HasColumnType("json")
                .HasColumnName("data");
            entity.Property(e => e.Intro)
                .HasComment("简介")
                .HasColumnType("text")
                .HasColumnName("intro");
            entity.Property(e => e.Language)
                .HasMaxLength(255)
                .HasComment("语言")
                .HasColumnName("language");
            entity.Property(e => e.Pid)
                .HasComment("parent id")
                .HasColumnName("pid");
            entity.Property(e => e.PublishedDate)
                .HasComment("发布时间")
                .HasColumnType("datetime")
                .HasColumnName("publishedDate");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasComment("新闻标题")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("posts")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Body)
                .HasColumnType("mediumtext")
                .HasColumnName("body");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("langauge_code");
            entity.Property(e => e.Languages)
                .HasColumnType("json")
                .HasColumnName("languages");
            entity.Property(e => e.PublishTime)
                .HasColumnType("timestamp")
                .HasColumnName("publish_time");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .HasColumnName("slug");
            entity.Property(e => e.Subtitle)
                .HasMaxLength(255)
                .HasColumnName("subtitle");
            entity.Property(e => e.Tag)
                .HasColumnType("json")
                .HasColumnName("tag");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}