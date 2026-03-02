using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Integration;

public partial class MetaTrade5DbContext : DbContext
{
    public MetaTrade5DbContext(DbContextOptions<MetaTrade5DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mt5Account> Mt5Accounts { get; set; }
    public virtual DbSet<Mt5Deals2025s> Mt5Deals2025s { get; set; }
    public virtual DbSet<Mt5Position> Mt5Positions { get; set; }
    public virtual DbSet<Mt5User> Mt5Users { get; set; }
    public virtual DbSet<Mt5Price> Mt5Prices { get; set; }

    public virtual DbSet<Mt5AntiddosServer> Mt5AntiddosServers { get; set; }

    public virtual DbSet<Mt5AntiddosSource> Mt5AntiddosSources { get; set; }

    public virtual DbSet<Mt5Client> Mt5Clients { get; set; }

    public virtual DbSet<Mt5Commission> Mt5Commissions { get; set; }

    public virtual DbSet<Mt5CommissionsTier> Mt5CommissionsTiers { get; set; }

    public virtual DbSet<Mt5Daily> Mt5Dailys { get; set; }

    //public virtual DbSet<Mt5Deals2023> Mt5Deals2023s { get; set; }
    //public virtual DbSet<Mt5Deals2024> Mt5Deals2024s { get; set; }

    public virtual DbSet<Mt5Document> Mt5Documents { get; set; }

    public virtual DbSet<Mt5Feeder> Mt5Feeders { get; set; }

    public virtual DbSet<Mt5FeederParam> Mt5FeederParams { get; set; }

    public virtual DbSet<Mt5FeederTranslate> Mt5FeederTranslates { get; set; }

    public virtual DbSet<Mt5Firewall> Mt5Firewalls { get; set; }

    public virtual DbSet<Mt5Gateway> Mt5Gateways { get; set; }

    public virtual DbSet<Mt5GatewayParam> Mt5GatewayParams { get; set; }

    public virtual DbSet<Mt5GatewayTranslate> Mt5GatewayTranslates { get; set; }

    public virtual DbSet<Mt5Group> Mt5Groups { get; set; }

    public virtual DbSet<Mt5GroupsSymbol> Mt5GroupsSymbols { get; set; }

    public virtual DbSet<Mt5Holiday> Mt5Holidays { get; set; }

    public virtual DbSet<Mt5Manager> Mt5Managers { get; set; }

    public virtual DbSet<Mt5Network> Mt5Networks { get; set; }

    public virtual DbSet<Mt5NetworkAccessServer> Mt5NetworkAccessServers { get; set; }

    public virtual DbSet<Mt5NetworkAntiddo> Mt5NetworkAntiddos { get; set; }

    public virtual DbSet<Mt5NetworkBackupFolder> Mt5NetworkBackupFolders { get; set; }

    public virtual DbSet<Mt5NetworkBackupServer> Mt5NetworkBackupServers { get; set; }

    public virtual DbSet<Mt5NetworkHistoryServer> Mt5NetworkHistoryServers { get; set; }

    public virtual DbSet<Mt5NetworkTradeServer> Mt5NetworkTradeServers { get; set; }

    public virtual DbSet<Mt5Order> Mt5Orders { get; set; }

    public virtual DbSet<Mt5Orders2023> Mt5Orders2023s { get; set; }

    public virtual DbSet<Mt5Plugin> Mt5Plugins { get; set; }

    public virtual DbSet<Mt5PluginParam> Mt5PluginParams { get; set; }


    public virtual DbSet<Mt5Report> Mt5Reports { get; set; }

    public virtual DbSet<Mt5ReportParam> Mt5ReportParams { get; set; }

    public virtual DbSet<Mt5Routing> Mt5Routings { get; set; }

    public virtual DbSet<Mt5RoutingCond> Mt5RoutingConds { get; set; }

    public virtual DbSet<Mt5RoutingDealer> Mt5RoutingDealers { get; set; }

    public virtual DbSet<Mt5Symbol> Mt5Symbols { get; set; }

    public virtual DbSet<Mt5SymbolsSession> Mt5SymbolsSessions { get; set; }

    public virtual DbSet<Mt5Time> Mt5Times { get; set; }

    public virtual DbSet<Mt5TimeWeekday> Mt5TimeWeekdays { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<Mt5Account>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_accounts")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.CurrencyDigits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MarginLeverage).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5AntiddosServer>(entity =>
        {
            entity.HasKey(e => e.ServerId).HasName("PRIMARY");

            entity
                .ToTable("mt5_antiddos_servers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Login, "IDX_mt5_antiddos_server_login");

            entity.Property(e => e.ServerId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Server_ID");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5AntiddosSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PRIMARY");

            entity
                .ToTable("mt5_antiddos_sources")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Login, "IDX_mt5_antiddos_source_login");

            entity.Property(e => e.SourceId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Source_ID");
            entity.Property(e => e.From)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.To)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PRIMARY");

            entity
                .ToTable("mt5_clients")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.ClientId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ClientID");
            entity.Property(e => e.AddressCity)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.AddressCountry)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.AddressPostcode)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.AddressState)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.AddressStreet)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.AssignedManager).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ClientExternalId)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("ClientExternalID");
            entity.Property(e => e.ClientOrigin).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ClientOriginLogin).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ClientStatus).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ClientType).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Comment)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyAddress)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyCountry)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyLei)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyLicenseAuthority)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyLicenseNumber)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyRegAuthority)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyRegDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.CompanyRegNumber)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyVat)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyWebsite)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ComplianceApprovedBy).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ComplianceClientCategory)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ComplianceDateApproval)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.ComplianceDateTermination)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ContactLanguage)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ContactLastDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.ContactMessengers)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ContactPreferred).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ContactSocialNetworks)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ExperienceCfd)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("ExperienceCFD");
            entity.Property(e => e.ExperienceFutures).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExperienceFx)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("ExperienceFX");
            entity.Property(e => e.ExperienceStocks).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Introducer)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.LeadCampaign)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.LeadSource)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonBirthDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.PersonCitizenship)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonDocumentDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.PersonDocumentExtra)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonDocumentNumber)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonDocumentType)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonEducation).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PersonEmployment).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PersonGender).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PersonIndustry).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PersonLastName)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonMiddleName)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonName)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonTaxId)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("PersonTaxID");
            entity.Property(e => e.PersonTitle)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PersonWealthSource).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5Commission>(entity =>
        {
            entity.HasKey(e => e.CommissionId).HasName("PRIMARY");

            entity
                .ToTable("mt5_commissions")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Name, "IDX_mt5_commissions_Name");

            entity.HasIndex(e => e.GroupId, "IDX_mt5_commissions_group_id");

            entity.Property(e => e.CommissionId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Commission_ID");
            entity.Property(e => e.Description)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.GroupId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Group_ID");
            entity.Property(e => e.Mode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ModeAction).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ModeCharge).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ModeEntry).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ModeProfit).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ModeRange).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ModeReason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Path)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.TurnoverCurrency)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5CommissionsTier>(entity =>
        {
            entity.HasKey(e => e.TierId).HasName("PRIMARY");

            entity
                .ToTable("mt5_commissions_tiers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.CommissionId, "IDX_mt5_tiers_commission_id");

            entity.Property(e => e.TierId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Tier_ID");
            entity.Property(e => e.CommissionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Commission_ID");
            entity.Property(e => e.Currency)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Mode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5Daily>(entity =>
        {
            entity.HasKey(e => new { e.Datetime, e.Login })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_daily")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Datetime).HasColumnType("bigint(20)");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Company)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Currency)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.DailySocompensation).HasColumnName("DailySOCompensation");
            entity.Property(e => e.DailySocompensationCredit).HasColumnName("DailySOCompensationCredit");
            entity.Property(e => e.DatetimePrev).HasColumnType("bigint(20)");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("EMail");
            entity.Property(e => e.Group)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.MarginLeverage).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5Deals2025s>(entity =>
        {
            entity.HasKey(e => e.Deal).HasName("PRIMARY");

            entity
                .ToTable("mt5_deals")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_deals_Timestamp");

            entity.Property(e => e.Deal)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Action).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Dealer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DigitsCurrency).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Entry).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpertId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertID");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ExternalID");
            entity.Property(e => e.Flags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Gateway)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ModifyFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Order).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.PositionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionID");
            entity.Property(e => e.PriceSl).HasColumnName("PriceSL");
            entity.Property(e => e.PriceTp).HasColumnName("PriceTP");
            entity.Property(e => e.Reason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Volume).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeClosed).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeClosedExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Deals2023>(entity =>
        {
            entity.HasKey(e => e.Deal).HasName("PRIMARY");

            entity
                .ToTable("mt5_deals_2023")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_deals_Timestamp");

            entity.Property(e => e.Deal)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Action).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Dealer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DigitsCurrency).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Entry).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpertId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertID");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ExternalID");
            entity.Property(e => e.Flags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Gateway)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ModifyFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Order).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.PositionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionID");
            entity.Property(e => e.PriceSl).HasColumnName("PriceSL");
            entity.Property(e => e.PriceTp).HasColumnName("PriceTP");
            entity.Property(e => e.Reason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Volume).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeClosed).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeClosedExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Deals2024>(entity =>
        {
            entity.HasKey(e => e.Deal).HasName("PRIMARY");

            entity
                .ToTable("mt5_deals_2024")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_deals_Timestamp");

            entity.Property(e => e.Deal)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Action).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Dealer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DigitsCurrency).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Entry).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpertId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertID");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ExternalID");
            entity.Property(e => e.Flags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Gateway)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ModifyFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Order).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.PositionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionID");
            entity.Property(e => e.PriceSl).HasColumnName("PriceSL");
            entity.Property(e => e.PriceTp).HasColumnName("PriceTP");
            entity.Property(e => e.Reason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Volume).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeClosed).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeClosedExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PRIMARY");

            entity
                .ToTable("mt5_documents")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.DocumentId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("DocumentID");
            entity.Property(e => e.ApprovedBy).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ApprovedDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.DateExpiration)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.DateIssue)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentComment)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.DocumentStatus).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DocumentType).HasColumnType("int(10) unsigned");
            entity.Property(e => e.RelatedClient).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5Feeder>(entity =>
        {
            entity.HasKey(e => e.Feeder).HasName("PRIMARY");

            entity
                .ToTable("mt5_feeders")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Feeder)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.AttempsSleep).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BooksCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BytesReceived).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BytesSent).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Company)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Enable).HasColumnType("int(11)");
            entity.Property(e => e.FeedServer)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.GatewayServer)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ID");
            entity.Property(e => e.Issuer)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Mode).HasColumnType("int(11)");
            entity.Property(e => e.Module)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.NewsCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.StateFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbols)
                .HasMaxLength(512)
                .HasDefaultValueSql("''");
            entity.Property(e => e.SysConnection).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysLastTime)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TickStatsCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TicksCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TimeoutReconnect).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TimeoutSleep).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5FeederParam>(entity =>
        {
            entity.HasKey(e => e.ParamId).HasName("PRIMARY");

            entity
                .ToTable("mt5_feeder_params")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Feeder, "IDX_mt5_feeder_params_name");

            entity.Property(e => e.ParamId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Param_ID");
            entity.Property(e => e.Feeder)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Value)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5FeederTranslate>(entity =>
        {
            entity.HasKey(e => new { e.Symbol, e.Feeder })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_feeder_translates")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Symbol)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Feeder)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.AskMarkup).HasColumnType("int(11)");
            entity.Property(e => e.BidMarkup).HasColumnType("int(11)");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Source)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5Firewall>(entity =>
        {
            entity.HasKey(e => new { e.Ipfrom, e.Ipto })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_firewall")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Ipfrom)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("IPFrom")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Ipto)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("IPTo")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Action).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5Gateway>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PRIMARY");

            entity
                .ToTable("mt5_gateways")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.AttempsSleep).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BooksCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BytesReceived).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BytesSent).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Company)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Enable).HasColumnType("int(11)");
            entity.Property(e => e.Flags).HasColumnType("int(11)");
            entity.Property(e => e.Gateway)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.GatewayServer)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ID");
            entity.Property(e => e.Issuer)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Module)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.StateFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbols)
                .HasMaxLength(512)
                .HasDefaultValueSql("''");
            entity.Property(e => e.SysConnection).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysLastTime)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TickStatsCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TicksCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TimeoutReconnect).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TimeoutSleep).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.TradeAverageTime).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TradeRequestsCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TradingServer)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5GatewayParam>(entity =>
        {
            entity.HasKey(e => e.ParamId).HasName("PRIMARY");

            entity
                .ToTable("mt5_gateway_params")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.GatewayName, "IDX_mt5_gate_params_name");

            entity.Property(e => e.ParamId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Param_ID");
            entity.Property(e => e.GatewayName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Value)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5GatewayTranslate>(entity =>
        {
            entity.HasKey(e => new { e.Symbol, e.GatewayName })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_gateway_translates")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Symbol)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.GatewayName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.AskMarkup).HasColumnType("int(11)");
            entity.Property(e => e.BidMarkup).HasColumnType("int(11)");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Source)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PRIMARY");

            entity
                .ToTable("mt5_groups")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Group, "IDX_mt5_groups_Group");

            entity.Property(e => e.GroupId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Group_ID");
            entity.Property(e => e.AuthMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.AuthOtpmode)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("AuthOTPMode");
            entity.Property(e => e.AuthPasswordMin).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Company)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyCatalog)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyDepositUrl)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .HasColumnName("CompanyDepositURL");
            entity.Property(e => e.CompanyEmail)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyPage)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanySupportEmail)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanySupportPage)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyWithdrawalUrl)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .HasColumnName("CompanyWithdrawalURL");
            entity.Property(e => e.Currency)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CurrencyDigits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DemoLeverage).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DemoTradesClean).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Group)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.LimitHistory).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LimitOrders).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LimitPositions).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LimitSymbols).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MailMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MarginFreeMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MarginFreeProfitMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MarginMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MarginSomode)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("MarginSOMode");
            entity.Property(e => e.NewsCategory)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.NewsMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PermissionsFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ReportsEmail)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ReportsFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ReportsMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.TradeFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.TransferMode).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5GroupsSymbol>(entity =>
        {
            entity.HasKey(e => e.SymbolId).HasName("PRIMARY");

            entity
                .ToTable("mt5_groups_symbols")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Path, "IDX_mt5_groups_symbols_Path");

            entity.HasIndex(e => e.GroupId, "IDX_mt5_symbols_group_id");

            entity.Property(e => e.SymbolId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Symbol_ID");
            entity.Property(e => e.ExecMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpirFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FillFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FreezeLevel).HasColumnType("int(11)");
            entity.Property(e => e.GroupId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Group_ID");
            entity.Property(e => e.IecheckMode)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IECheckMode");
            entity.Property(e => e.Ieflags)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IEFlags");
            entity.Property(e => e.IeslipLosing)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IESlipLosing");
            entity.Property(e => e.IeslipProfit)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IESlipProfit");
            entity.Property(e => e.Ietimeout)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IETimeout");
            entity.Property(e => e.IevolumeMax)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("IEVolumeMax");
            entity.Property(e => e.IevolumeMaxExt)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("IEVolumeMaxExt");
            entity.Property(e => e.MarginFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.OrderFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Path)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PermissionsBookdepth).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PermissionsFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Reflags)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("REFlags");
            entity.Property(e => e.Retimeout)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("RETimeout");
            entity.Property(e => e.SpreadDiff).HasColumnType("int(11)");
            entity.Property(e => e.SpreadDiffBalance).HasColumnType("int(11)");
            entity.Property(e => e.StopsLevel).HasColumnType("int(11)");
            entity.Property(e => e.SwapFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SwapMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SwapYearDay).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.TradeMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.VolumeLimit).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeLimitExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMax).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMaxExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMin).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMinExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeStep).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeStepExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Holiday>(entity =>
        {
            entity.HasKey(e => new { e.Year, e.Month, e.Day, e.From, e.To, e.Description })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0, 0 });

            entity
                .ToTable("mt5_holidays")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Year).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Month).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Day).HasColumnType("int(10) unsigned");
            entity.Property(e => e.From).HasColumnType("int(10) unsigned");
            entity.Property(e => e.To).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Mode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbols)
                .HasMaxLength(2000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5Manager>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_managers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Access)
                .HasMaxLength(2000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Groups)
                .HasMaxLength(2000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Mailbox)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.RequestLimitLogs).HasColumnType("int(10) unsigned");
            entity.Property(e => e.RequestLimitReports).HasColumnType("int(10) unsigned");
            entity.Property(e => e.RightAccDelete)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Acc_Delete");
            entity.Property(e => e.RightAccDetails)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Acc_Details");
            entity.Property(e => e.RightAccManager)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Acc_Manager");
            entity.Property(e => e.RightAccOnline)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Acc_Online");
            entity.Property(e => e.RightAccRead)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Acc_Read");
            entity.Property(e => e.RightAccountant)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Accountant");
            entity.Property(e => e.RightAdmin)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Admin");
            entity.Property(e => e.RightCfgAccess)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Access");
            entity.Property(e => e.RightCfgDatafeeds)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Datafeeds");
            entity.Property(e => e.RightCfgEcn)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_ECN");
            entity.Property(e => e.RightCfgGateways)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Gateways");
            entity.Property(e => e.RightCfgGroups)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Groups");
            entity.Property(e => e.RightCfgHolidays)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Holidays");
            entity.Property(e => e.RightCfgHstSync)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Hst_Sync");
            entity.Property(e => e.RightCfgManagers)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Managers");
            entity.Property(e => e.RightCfgPlugins)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Plugins");
            entity.Property(e => e.RightCfgReports)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Reports");
            entity.Property(e => e.RightCfgRequests)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Requests");
            entity.Property(e => e.RightCfgServers)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Servers");
            entity.Property(e => e.RightCfgSymbols)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Symbols");
            entity.Property(e => e.RightCfgTime)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Cfg_Time");
            entity.Property(e => e.RightCharts)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Charts");
            entity.Property(e => e.RightClientsAccess)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Clients_Access");
            entity.Property(e => e.RightClientsCreate)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Clients_Create");
            entity.Property(e => e.RightClientsDelete)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Clients_Delete");
            entity.Property(e => e.RightClientsEdit)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Clients_Edit");
            entity.Property(e => e.RightCommentsAccess)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Comments_Access");
            entity.Property(e => e.RightCommentsCreate)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Comments_Create");
            entity.Property(e => e.RightCommentsDelete)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Comments_Delete");
            entity.Property(e => e.RightConfirmActions)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Confirm_Actions");
            entity.Property(e => e.RightDocumentsAccess)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Documents_Access");
            entity.Property(e => e.RightDocumentsCreate)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Documents_Create");
            entity.Property(e => e.RightDocumentsDelete)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Documents_Delete");
            entity.Property(e => e.RightDocumentsEdit)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Documents_Edit");
            entity.Property(e => e.RightDocumentsFilesAdd)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Documents_Files_Add");
            entity.Property(e => e.RightDocumentsFilesDelete)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Documents_Files_Delete");
            entity.Property(e => e.RightEmail)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Email");
            entity.Property(e => e.RightExport)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Export");
            entity.Property(e => e.RightFintezaAccess)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Finteza_Access");
            entity.Property(e => e.RightFintezaCampaigns)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Finteza_Campaigns");
            entity.Property(e => e.RightFintezaReports)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Finteza_Reports");
            entity.Property(e => e.RightFintezaWebsites)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Finteza_Websites");
            entity.Property(e => e.RightGroupCommission)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Group_Commission");
            entity.Property(e => e.RightGroupMargin)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Group_Margin");
            entity.Property(e => e.RightManager)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Manager");
            entity.Property(e => e.RightMarket)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Market");
            entity.Property(e => e.RightNews)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_News");
            entity.Property(e => e.RightNotifications)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Notifications");
            entity.Property(e => e.RightQuotes)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Quotes");
            entity.Property(e => e.RightQuotesRaw)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Quotes_Raw");
            entity.Property(e => e.RightReports)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Reports");
            entity.Property(e => e.RightRiskManager)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Risk_Manager");
            entity.Property(e => e.RightSrvJournals)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Srv_Journals");
            entity.Property(e => e.RightSrvReports)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Srv_Reports");
            entity.Property(e => e.RightSymbolDetails)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Symbol_Details");
            entity.Property(e => e.RightTechsupport)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Techsupport");
            entity.Property(e => e.RightTradesDealer)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Trades_Dealer");
            entity.Property(e => e.RightTradesDelete)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Trades_Delete");
            entity.Property(e => e.RightTradesManager)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Trades_Manager");
            entity.Property(e => e.RightTradesRead)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Trades_Read");
            entity.Property(e => e.RightTradesSupervisor)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("Right_Trades_Supervisor");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5Network>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_network")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Adapter)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Adapters)
                .HasMaxLength(512)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Address)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.AddressIpv6)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("AddressIPv6");
            entity.Property(e => e.Addresses)
                .HasMaxLength(512)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Binds)
                .HasMaxLength(512)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Build).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BuildDate)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.FailoverMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FailoverTimeout).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PerfConnectsCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfConnectsMax).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfCpuCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfCpuMax).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfMemBlockCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfMemBlockMin).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfMemoryCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfMemoryMin).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfNetworkCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfNetworkMax).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfSocketsCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PerfSocketsMax).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Points)
                .HasMaxLength(512)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Port).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ServiceTime).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysBits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysBsodCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysBsodLast)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.SysConnection).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysCpuName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.SysCpuNumber).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.SysHddCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysHddFree).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysHddReadCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysHddReadSpeed).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysHddSize).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysHddWriteCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysHddWriteSpeed).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysLastBoot)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.SysMemoryCritical).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysMemoryFree).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysMemoryTotal).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysNetDriverDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.SysOsBuild).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysOsFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SysOsLastUpdate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.SysOsName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.SysOsVersion).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Version).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5NetworkAccessServer>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_network_access_servers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.AccessFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.AccessMask).HasColumnType("int(10) unsigned");
            entity.Property(e => e.AntifloodConnects).HasColumnType("int(10) unsigned");
            entity.Property(e => e.AntifloodEnable).HasColumnType("int(10) unsigned");
            entity.Property(e => e.AntifloodErrors).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BalancingConnections).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BalancingPriority).HasColumnType("int(10) unsigned");
            entity.Property(e => e.NewsMaxCount).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Priority).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Servers)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5NetworkAntiddo>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_network_antiddos")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.AccessMask).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Priority).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5NetworkBackupFolder>(entity =>
        {
            entity.HasKey(e => e.FolderId).HasName("PRIMARY");

            entity
                .ToTable("mt5_network_backup_folders")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Login, "IDX_mt5_backup_server_login");

            entity.Property(e => e.FolderId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Folder_ID");
            entity.Property(e => e.Filter)
                .HasMaxLength(260)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Flags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Folder)
                .HasMaxLength(260)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Masks)
                .HasMaxLength(260)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5NetworkBackupServer>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_network_backup_servers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.BackupFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.BackupLastArchive)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.BackupLastFull)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.BackupLastStartup)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.BackupLastSync)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.BackupLastSyncSql)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.BackupPath)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.BackupPeriod).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BackupTimeFull).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BackupTtl).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PairServer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.SqlFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.SqlFolder)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.SqlMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SqlPeriod).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SqlServer)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5NetworkHistoryServer>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_network_history_servers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.DatafeedTimeout).HasColumnType("int(10) unsigned");
            entity.Property(e => e.NewsMaxCount).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5NetworkTradeServer>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_network_trade_servers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.DealsRange)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.DealsRangeUsed)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.DemoMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DemoPeriod).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LoginsRange)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.LoginsRangeUsed)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.OrdersRange)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.OrdersRangeUsed)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.OvermonthLastTime)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.OvermonthMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.OvermonthPrevTime)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.OvernighPrevTime)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.OvernightDays).HasColumnType("int(10) unsigned");
            entity.Property(e => e.OvernightLastTime)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.OvernightMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.OvernightTime).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TotalDeals).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TotalOrders).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TotalOrdersHistory).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TotalPositions).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TotalUsers).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TotalUsersApi)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("TotalUsersAPI");
            entity.Property(e => e.TotalUsersReal).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5Order>(entity =>
        {
            entity.HasKey(e => e.Order).HasName("PRIMARY");

            entity
                .ToTable("mt5_orders")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_orders_Timestamp");

            entity.Property(e => e.Order)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ActivationFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationTime).HasColumnType("bigint(20)");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Dealer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DigitsCurrency).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpertId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertID");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ExternalID");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ModifyFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PositionById)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionByID");
            entity.Property(e => e.PositionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionID");
            entity.Property(e => e.PriceSl).HasColumnName("PriceSL");
            entity.Property(e => e.PriceTp).HasColumnName("PriceTP");
            entity.Property(e => e.Reason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.State).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.TimeDone)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeDoneMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.TimeExpiration)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeSetup)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeSetupMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TypeFill).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TypeTime).HasColumnType("int(10) unsigned");
            entity.Property(e => e.VolumeCurrent).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeCurrentExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeInitial).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeInitialExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Orders2023>(entity =>
        {
            entity.HasKey(e => e.Order).HasName("PRIMARY");

            entity
                .ToTable("mt5_orders_2023")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_orders_Timestamp");

            entity.Property(e => e.Order)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ActivationFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationTime).HasColumnType("bigint(20)");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Dealer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DigitsCurrency).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpertId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertID");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ExternalID");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ModifyFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PositionById)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionByID");
            entity.Property(e => e.PositionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("PositionID");
            entity.Property(e => e.PriceSl).HasColumnName("PriceSL");
            entity.Property(e => e.PriceTp).HasColumnName("PriceTP");
            entity.Property(e => e.Reason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.State).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.TimeDone)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeDoneMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.TimeExpiration)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeSetup)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeSetupMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TypeFill).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TypeTime).HasColumnType("int(10) unsigned");
            entity.Property(e => e.VolumeCurrent).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeCurrentExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeInitial).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeInitialExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Plugin>(entity =>
        {
            entity.HasKey(e => new { e.Name, e.Server })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_plugins")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Enable).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Flags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Module)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5PluginParam>(entity =>
        {
            entity.HasKey(e => e.ParamId).HasName("PRIMARY");

            entity
                .ToTable("mt5_plugin_params")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Plugin, "IDX_mt5_plugin_params_name");

            entity.HasIndex(e => e.Server, "IDX_mt5_plugin_params_server");

            entity.Property(e => e.ParamId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Param_ID");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Plugin)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Value)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PRIMARY");

            entity
                .ToTable("mt5_positions")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Position, "IDX_mt5_positions_Position");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_positions_Timestamp");

            entity.Property(e => e.PositionId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Position_ID");
            entity.Property(e => e.Action).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActivationTime).HasColumnType("bigint(20)");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Dealer).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.DigitsCurrency).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpertId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertID");
            entity.Property(e => e.ExpertPositionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ExpertPositionID");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ExternalID");
            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Position).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.PriceSl).HasColumnName("PriceSL");
            entity.Property(e => e.PriceTp).HasColumnName("PriceTP");
            entity.Property(e => e.Reason).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.TimeCreate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeCreateMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.TimeUpdate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeUpdateMsc)
                .HasMaxLength(6)
                .HasDefaultValueSql("'0000-00-00 00:00:00.000000'");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Volume).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5Price>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PRIMARY");

            entity
                .ToTable("mt5_prices")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Symbol, "IDX_mt5_prices_Symbol");

            entity.Property(e => e.PriceId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Price_ID");
            entity.Property(e => e.AskDir).HasColumnType("int(10) unsigned");
            entity.Property(e => e.BidDir).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LastDir).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Mt5Report>(entity =>
        {
            entity.HasKey(e => new { e.Name, e.Server })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_reports")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Enable).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Template)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5ReportParam>(entity =>
        {
            entity.HasKey(e => e.ParamId).HasName("PRIMARY");

            entity
                .ToTable("mt5_report_params")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Report, "IDX_mt5_report_params_name");

            entity.HasIndex(e => e.Server, "IDX_mt5_report_params_server");

            entity.Property(e => e.ParamId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Param_ID");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Report)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Server).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Value)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5Routing>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PRIMARY");

            entity
                .ToTable("mt5_routing")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Action).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActionType).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ActionValueInt).HasColumnType("bigint(20)");
            entity.Property(e => e.ActionValueString).HasMaxLength(128);
            entity.Property(e => e.ActionValueUint)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ActionValueUInt");
            entity.Property(e => e.Flags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Mode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Request).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5RoutingCond>(entity =>
        {
            entity.HasKey(e => e.ConditionId).HasName("PRIMARY");

            entity
                .ToTable("mt5_routing_conds")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.RoutingName, "IDX_mt5_routing_conds_name");

            entity.Property(e => e.ConditionId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Condition_ID");
            entity.Property(e => e.Condition).HasColumnType("int(10) unsigned");
            entity.Property(e => e.RoutingName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Rule).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ValueInt).HasColumnType("bigint(20)");
            entity.Property(e => e.ValueString).HasMaxLength(128);
            entity.Property(e => e.ValueUint)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ValueUInt");
            entity.Property(e => e.ValueUintExt)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ValueUIntExt");
        });

        modelBuilder.Entity<Mt5RoutingDealer>(entity =>
        {
            entity.HasKey(e => new { e.Login, e.RoutingName })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_routing_dealers")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Login).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.RoutingName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .UseCollation("utf8_bin");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<Mt5Symbol>(entity =>
        {
            entity.HasKey(e => e.SymbolId).HasName("PRIMARY");

            entity
                .ToTable("mt5_symbols")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Symbol, "IDX_mt5_symbols_Symbol");

            entity.Property(e => e.SymbolId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Symbol_ID");
            entity.Property(e => e.Basis)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CalcMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Category)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Cfi)
                .HasMaxLength(8)
                .HasDefaultValueSql("''")
                .HasColumnName("CFI");
            entity.Property(e => e.Color).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ColorBackground).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Country)
                .HasMaxLength(4)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CurrencyBase)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CurrencyBaseDigits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.CurrencyMargin)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CurrencyMarginDigits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.CurrencyProfit)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CurrencyProfitDigits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Description)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Digits).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Exchange)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ExecMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpirFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FillFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterDiscard).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterGap).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterGapTicks).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterHard).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterHardTicks).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterSoft).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterSoftTicks).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterSpreadMax).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FilterSpreadMin).HasColumnType("int(10) unsigned");
            entity.Property(e => e.FreezeLevel).HasColumnType("int(11)");
            entity.Property(e => e.Gtcmode)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("GTCMode");
            entity.Property(e => e.IecheckMode)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IECheckMode");
            entity.Property(e => e.IeslipLosing)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IESlipLosing");
            entity.Property(e => e.IeslipProfit)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IESlipProfit");
            entity.Property(e => e.Ietimeout)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("IETimeout");
            entity.Property(e => e.IevolumeMax)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("IEVolumeMax");
            entity.Property(e => e.IevolumeMaxExt)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("IEVolumeMaxExt");
            entity.Property(e => e.Industry).HasColumnType("int(10) unsigned");
            entity.Property(e => e.International)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Isin)
                .HasMaxLength(16)
                .HasDefaultValueSql("''")
                .HasColumnName("ISIN");
            entity.Property(e => e.MarginFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.OptionMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.OrderFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Page)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Path)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.QuotesTimeout).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Reflags)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("REFlags");
            entity.Property(e => e.Retimeout)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("RETimeout");
            entity.Property(e => e.Sector).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Source)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.SpliceTimeDays).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SpliceTimeType).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SpliceType).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Spread).HasColumnType("int(11)");
            entity.Property(e => e.SpreadBalance).HasColumnType("int(11)");
            entity.Property(e => e.SpreadDiff).HasColumnType("int(11)");
            entity.Property(e => e.SpreadDiffBalance).HasColumnType("int(11)");
            entity.Property(e => e.StopsLevel).HasColumnType("int(11)");
            entity.Property(e => e.SubscriptionsDelay).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SwapFlags).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SwapMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SwapYearDay).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Symbol)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.TickBookDepth).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TickChartMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.TickFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.TimeExpiration)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeStart)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.TradeFlags).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.TradeMode).HasColumnType("int(10) unsigned");
            entity.Property(e => e.VolumeLimit).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeLimitExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMax).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMaxExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMin).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeMinExt).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeStep).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.VolumeStepExt).HasColumnType("bigint(20) unsigned");
        });

        modelBuilder.Entity<Mt5SymbolsSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PRIMARY");

            entity
                .ToTable("mt5_symbols_sessions")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.SymbolId, "IDX_mt5_symbols_sessions_id");

            entity.Property(e => e.SessionId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Session_ID");
            entity.Property(e => e.Close).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Day).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Open).HasColumnType("int(10) unsigned");
            entity.Property(e => e.SymbolId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("Symbol_ID");
            entity.Property(e => e.Type).HasColumnType("int(10) unsigned");
        });

        modelBuilder.Entity<Mt5Time>(entity =>
        {
            entity.HasKey(e => e.TimeZone).HasName("PRIMARY");

            entity
                .ToTable("mt5_time")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.TimeZone)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.Daylight).HasColumnType("int(11)");
            entity.Property(e => e.DaylightState).HasColumnType("int(11)");
            entity.Property(e => e.TimeServer)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<Mt5TimeWeekday>(entity =>
        {
            entity.HasKey(e => new { e.TimeZone, e.Day })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("mt5_time_weekdays")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.TimeZone).HasColumnType("int(11)");
            entity.Property(e => e.Day).HasColumnType("int(10) unsigned");
            entity.Property(e => e._00)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("00");
            entity.Property(e => e._01)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("01");
            entity.Property(e => e._02)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("02");
            entity.Property(e => e._03)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("03");
            entity.Property(e => e._04)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("04");
            entity.Property(e => e._05)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("05");
            entity.Property(e => e._06)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("06");
            entity.Property(e => e._07)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("07");
            entity.Property(e => e._08)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("08");
            entity.Property(e => e._09)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("09");
            entity.Property(e => e._10)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("10");
            entity.Property(e => e._11)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("11");
            entity.Property(e => e._12)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("12");
            entity.Property(e => e._13)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("13");
            entity.Property(e => e._14)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("14");
            entity.Property(e => e._15)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("15");
            entity.Property(e => e._16)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("16");
            entity.Property(e => e._17)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("17");
            entity.Property(e => e._18)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("18");
            entity.Property(e => e._19)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("19");
            entity.Property(e => e._20)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("20");
            entity.Property(e => e._21)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("21");
            entity.Property(e => e._22)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("22");
            entity.Property(e => e._23)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("23");
        });

        modelBuilder.Entity<Mt5User>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("mt5_users")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Timestamp, "IDX_mt5_users_Timestamp");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.Account)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Address)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Agent).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.ApiData)
                .HasMaxLength(4000)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CertSerialNumber).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.City)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ClientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("ClientID");
            entity.Property(e => e.Color).HasColumnType("int(10) unsigned");
            entity.Property(e => e.Comment)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Company)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Country)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.FirstName)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Group)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Id)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("ID");
            entity.Property(e => e.Language).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LastAccess)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.LastIp)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("LastIP");
            entity.Property(e => e.LastName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.LastPassChange)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.LeadCampaign)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.LeadSource)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Leverage).HasColumnType("int(10) unsigned");
            entity.Property(e => e.LimitOrders).HasColumnType("int(10) unsigned");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(64)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Mqid)
                .HasMaxLength(16)
                .HasDefaultValueSql("''")
                .HasColumnName("MQID");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Phone)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PhonePassword)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Registration)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
            entity.Property(e => e.Rights).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.State)
                .HasMaxLength(32)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp).HasColumnType("bigint(20)");
            entity.Property(e => e.TimestampTrade).HasColumnType("bigint(20)");
            entity.Property(e => e.TradeAccounts)
                .HasMaxLength(128)
                .HasDefaultValueSql("''");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(16)
                .HasDefaultValueSql("''");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}