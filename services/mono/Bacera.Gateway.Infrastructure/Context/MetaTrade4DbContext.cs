using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Integration;

public partial class MetaTrade4DbContext(DbContextOptions<MetaTrade4DbContext> options) : DbContext(options)
{
    public virtual DbSet<Mt4Config> Mt4Configs { get; set; }

    public virtual DbSet<Mt4Daily> Mt4Dailies { get; set; }

    public virtual DbSet<Mt4Price> Mt4Prices { get; set; }

    public virtual DbSet<Mt4Trade> Mt4Trades { get; set; }
    public virtual DbSet<Mt4OpenTrade> Mt4OpenTrades { get; set; }

    public virtual DbSet<Mt4User> Mt4Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Mt4Config>(entity =>
        {
            entity.HasKey(e => e.Config).HasName("PRIMARY");

            entity
                .ToTable("MT4_CONFIG")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Config)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("CONFIG");
            entity.Property(e => e.ValueInt)
                .HasColumnType("int(11)")
                .HasColumnName("VALUE_INT");
            entity.Property(e => e.ValueStr)
                .HasMaxLength(255)
                .IsFixedLength()
                .HasColumnName("VALUE_STR");
        });

        modelBuilder.Entity<Mt4Daily>(entity =>
        {
            entity.HasKey(e => new { e.Login, e.Time })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("MT4_DAILY")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Login, "INDEX_LOGIN");

            entity.HasIndex(e => e.Time, "INDEX_TIME");

            entity.Property(e => e.Login)
                .HasColumnType("int(11)")
                .HasColumnName("LOGIN");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("TIME");
            entity.Property(e => e.Balance).HasColumnName("BALANCE");
            entity.Property(e => e.BalancePrev).HasColumnName("BALANCE_PREV");
            entity.Property(e => e.Bank)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("BANK");
            entity.Property(e => e.Credit).HasColumnName("CREDIT");
            entity.Property(e => e.Deposit).HasColumnName("DEPOSIT");
            entity.Property(e => e.Equity).HasColumnName("EQUITY");
            entity.Property(e => e.Group)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("GROUP");
            entity.Property(e => e.Margin).HasColumnName("MARGIN");
            entity.Property(e => e.MarginFree).HasColumnName("MARGIN_FREE");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFY_TIME");
            entity.Property(e => e.Profit).HasColumnName("PROFIT");
            entity.Property(e => e.ProfitClosed).HasColumnName("PROFIT_CLOSED");
        });

        modelBuilder.Entity<Mt4Price>(entity =>
        {
            entity.HasKey(e => e.Symbol).HasName("PRIMARY");

            entity
                .ToTable("MT4_PRICES")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Symbol)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("SYMBOL");
            entity.Property(e => e.Ask).HasColumnName("ASK");
            entity.Property(e => e.Bid).HasColumnName("BID");
            entity.Property(e => e.Digits)
                .HasColumnType("int(11)")
                .HasColumnName("DIGITS");
            entity.Property(e => e.Direction)
                .HasColumnType("int(11)")
                .HasColumnName("DIRECTION");
            entity.Property(e => e.High).HasColumnName("HIGH");
            entity.Property(e => e.Low).HasColumnName("LOW");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFY_TIME");
            entity.Property(e => e.Spread)
                .HasColumnType("int(11)")
                .HasColumnName("SPREAD");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("TIME");
        });

        modelBuilder.Entity<Mt4Trade>(entity =>
        {
            entity.HasKey(e => e.Ticket).HasName("PRIMARY");

            entity
                .ToTable("MT4_TRADES")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.CloseTime, "INDEX_CLOSETIME");

            entity.HasIndex(e => e.Cmd, "INDEX_CMD");

            entity.HasIndex(e => new { e.CloseTime, e.Cmd }, "INDEX_CMD_CLOSETIME");

            entity.HasIndex(e => new { e.Cmd, e.OpenTime }, "INDEX_CMD_OPENTIME");

            entity.HasIndex(e => e.Login, "INDEX_LOGIN");

            entity.HasIndex(e => new { e.Login, e.Cmd, e.Profit }, "INDEX_LOGIN_CMD_PROFIT");

            entity.HasIndex(e => e.OpenTime, "INDEX_OPENTIME");

            entity.HasIndex(e => e.Timestamp, "INDEX_STAMP");

            entity.HasIndex(e => new { e.CloseTime, e.Cmd, e.Symbol }, "INDEX_SYMBOL_CMD_CLOSETIME");

            entity.HasIndex(e => new { e.CloseTime, e.Cmd, e.Login, e.Symbol }, "INDEX_SYMBOL_CMD_CLOSETIME_LOGIN");

            entity.Property(e => e.Ticket)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("TICKET");
            entity.Property(e => e.ClosePrice).HasColumnName("CLOSE_PRICE");
            entity.Property(e => e.CloseTime)
                .HasColumnType("datetime")
                .HasColumnName("CLOSE_TIME");
            entity.Property(e => e.Cmd)
                .HasColumnType("int(11)")
                .HasColumnName("CMD");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("COMMENT");
            entity.Property(e => e.Commission).HasColumnName("COMMISSION");
            entity.Property(e => e.CommissionAgent).HasColumnName("COMMISSION_AGENT");
            entity.Property(e => e.ConvRate1).HasColumnName("CONV_RATE1");
            entity.Property(e => e.ConvRate2).HasColumnName("CONV_RATE2");
            entity.Property(e => e.Digits)
                .HasColumnType("int(11)")
                .HasColumnName("DIGITS");
            entity.Property(e => e.Expiration)
                .HasColumnType("datetime")
                .HasColumnName("EXPIRATION");
            entity.Property(e => e.GwClosePrice)
                .HasColumnType("int(11)")
                .HasColumnName("GW_CLOSE_PRICE");
            entity.Property(e => e.GwOpenPrice)
                .HasColumnType("int(11)")
                .HasColumnName("GW_OPEN_PRICE");
            entity.Property(e => e.GwVolume)
                .HasColumnType("int(11)")
                .HasColumnName("GW_VOLUME");
            entity.Property(e => e.InternalId)
                .HasColumnType("int(11)")
                .HasColumnName("INTERNAL_ID");
            entity.Property(e => e.Login)
                .HasColumnType("int(11)")
                .HasColumnName("LOGIN");
            entity.Property(e => e.Magic)
                .HasColumnType("int(11)")
                .HasColumnName("MAGIC");
            entity.Property(e => e.MarginRate).HasColumnName("MARGIN_RATE");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFY_TIME");
            entity.Property(e => e.OpenPrice).HasColumnName("OPEN_PRICE");
            entity.Property(e => e.OpenTime)
                .HasColumnType("datetime")
                .HasColumnName("OPEN_TIME");
            entity.Property(e => e.Profit).HasColumnName("PROFIT");
            entity.Property(e => e.Reason)
                .HasColumnType("int(11)")
                .HasColumnName("REASON");
            entity.Property(e => e.Sl).HasColumnName("SL");
            entity.Property(e => e.Swaps).HasColumnName("SWAPS");
            entity.Property(e => e.Symbol)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("SYMBOL");
            entity.Property(e => e.Taxes).HasColumnName("TAXES");
            entity.Property(e => e.Timestamp)
                .HasColumnType("int(11)")
                .HasColumnName("TIMESTAMP");
            entity.Property(e => e.Tp).HasColumnName("TP");
            entity.Property(e => e.Volume)
                .HasColumnType("int(11)")
                .HasColumnName("VOLUME");
        });

        modelBuilder.Entity<Mt4OpenTrade>(entity =>
        {
            entity.HasKey(e => e.Ticket).HasName("PRIMARY");

            entity
                .ToTable("MT4_OPENTRADES")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.CloseTime, "INDEX_CLOSETIME");

            entity.HasIndex(e => e.Cmd, "INDEX_CMD");

            entity.HasIndex(e => new { e.CloseTime, e.Cmd }, "INDEX_CMD_CLOSETIME");

            entity.HasIndex(e => new { e.Cmd, e.OpenTime }, "INDEX_CMD_OPENTIME");

            entity.HasIndex(e => e.Login, "INDEX_LOGIN");

            entity.HasIndex(e => new { e.Login, e.Cmd, e.Profit }, "INDEX_LOGIN_CMD_PROFIT");

            entity.HasIndex(e => e.OpenTime, "INDEX_OPENTIME");

            entity.HasIndex(e => e.Timestamp, "INDEX_STAMP");

            entity.HasIndex(e => new { e.CloseTime, e.Cmd, e.Symbol }, "INDEX_SYMBOL_CMD_CLOSETIME");

            entity.HasIndex(e => new { e.CloseTime, e.Cmd, e.Login, e.Symbol }, "INDEX_SYMBOL_CMD_CLOSETIME_LOGIN");

            entity.Property(e => e.Ticket)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("TICKET");
            entity.Property(e => e.ClosePrice).HasColumnName("CLOSE_PRICE");
            entity.Property(e => e.CloseTime)
                .HasColumnType("datetime")
                .HasColumnName("CLOSE_TIME");
            entity.Property(e => e.Cmd)
                .HasColumnType("int(11)")
                .HasColumnName("CMD");
            entity.Property(e => e.Comment)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("COMMENT");
            entity.Property(e => e.Commission).HasColumnName("COMMISSION");
            entity.Property(e => e.CommissionAgent).HasColumnName("COMMISSION_AGENT");
            entity.Property(e => e.ConvRate1).HasColumnName("CONV_RATE1");
            entity.Property(e => e.ConvRate2).HasColumnName("CONV_RATE2");
            entity.Property(e => e.Digits)
                .HasColumnType("int(11)")
                .HasColumnName("DIGITS");
            entity.Property(e => e.Expiration)
                .HasColumnType("datetime")
                .HasColumnName("EXPIRATION");
            entity.Property(e => e.GwClosePrice)
                .HasColumnType("int(11)")
                .HasColumnName("GW_CLOSE_PRICE");
            entity.Property(e => e.GwOpenPrice)
                .HasColumnType("int(11)")
                .HasColumnName("GW_OPEN_PRICE");
            entity.Property(e => e.GwVolume)
                .HasColumnType("int(11)")
                .HasColumnName("GW_VOLUME");
            entity.Property(e => e.InternalId)
                .HasColumnType("int(11)")
                .HasColumnName("INTERNAL_ID");
            entity.Property(e => e.Login)
                .HasColumnType("int(11)")
                .HasColumnName("LOGIN");
            entity.Property(e => e.Magic)
                .HasColumnType("int(11)")
                .HasColumnName("MAGIC");
            entity.Property(e => e.MarginRate).HasColumnName("MARGIN_RATE");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFY_TIME");
            entity.Property(e => e.OpenPrice).HasColumnName("OPEN_PRICE");
            entity.Property(e => e.OpenTime)
                .HasColumnType("datetime")
                .HasColumnName("OPEN_TIME");
            entity.Property(e => e.Profit).HasColumnName("PROFIT");
            entity.Property(e => e.Reason)
                .HasColumnType("int(11)")
                .HasColumnName("REASON");
            entity.Property(e => e.Sl).HasColumnName("SL");
            entity.Property(e => e.Swaps).HasColumnName("SWAPS");
            entity.Property(e => e.Symbol)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("SYMBOL");
            entity.Property(e => e.Taxes).HasColumnName("TAXES");
            entity.Property(e => e.Timestamp)
                .HasColumnType("int(11)")
                .HasColumnName("TIMESTAMP");
            entity.Property(e => e.Tp).HasColumnName("TP");
            entity.Property(e => e.Volume)
                .HasColumnType("int(11)")
                .HasColumnName("VOLUME");
        });

        modelBuilder.Entity<Mt4User>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("PRIMARY");

            entity
                .ToTable("MT4_USERS")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.HasIndex(e => e.Enable, "INDEX_ENABLE");

            entity.HasIndex(e => e.EnableReadonly, "INDEX_READONLY");

            entity.HasIndex(e => e.Timestamp, "INDEX_STAMP");

            entity.Property(e => e.Login)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("LOGIN");
            entity.Property(e => e.Address)
                .HasMaxLength(128)
                .IsFixedLength()
                .HasColumnName("ADDRESS");
            entity.Property(e => e.AgentAccount)
                .HasColumnType("int(11)")
                .HasColumnName("AGENT_ACCOUNT");
            entity.Property(e => e.ApiData)
                .HasColumnType("blob")
                .HasColumnName("API_DATA");
            entity.Property(e => e.Balance).HasColumnName("BALANCE");
            entity.Property(e => e.City)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("CITY");
            entity.Property(e => e.Comment)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("COMMENT");
            entity.Property(e => e.Country)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("COUNTRY");
            entity.Property(e => e.Credit).HasColumnName("CREDIT");
            entity.Property(e => e.Currency)
                .HasMaxLength(16)
                .HasDefaultValueSql("''")
                .IsFixedLength()
                .HasColumnName("CURRENCY");
            entity.Property(e => e.Email)
                .HasMaxLength(48)
                .IsFixedLength()
                .HasColumnName("EMAIL");
            entity.Property(e => e.Enable)
                .HasColumnType("int(11)")
                .HasColumnName("ENABLE");
            entity.Property(e => e.EnableChangePass)
                .HasColumnType("int(11)")
                .HasColumnName("ENABLE_CHANGE_PASS");
            entity.Property(e => e.EnableOtp)
                .HasColumnType("int(11)")
                .HasColumnName("ENABLE_OTP");
            entity.Property(e => e.EnableReadonly)
                .HasColumnType("int(11)")
                .HasColumnName("ENABLE_READONLY");
            entity.Property(e => e.Equity).HasColumnName("EQUITY");
            entity.Property(e => e.Group)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("GROUP");
            entity.Property(e => e.Id)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.Interestrate).HasColumnName("INTERESTRATE");
            entity.Property(e => e.Lastdate)
                .HasColumnType("datetime")
                .HasColumnName("LASTDATE");
            entity.Property(e => e.LeadSource)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .IsFixedLength()
                .HasColumnName("LEAD_SOURCE");
            entity.Property(e => e.Leverage)
                .HasColumnType("int(11)")
                .HasColumnName("LEVERAGE");
            entity.Property(e => e.Margin).HasColumnName("MARGIN");
            entity.Property(e => e.MarginFree).HasColumnName("MARGIN_FREE");
            entity.Property(e => e.MarginLevel).HasColumnName("MARGIN_LEVEL");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFY_TIME");
            entity.Property(e => e.Mqid)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("MQID");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .IsFixedLength()
                .HasColumnName("NAME");
            entity.Property(e => e.PasswordPhone)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("PASSWORD_PHONE");
            entity.Property(e => e.Phone)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("PHONE");
            entity.Property(e => e.Prevbalance).HasColumnName("PREVBALANCE");
            entity.Property(e => e.Prevmonthbalance).HasColumnName("PREVMONTHBALANCE");
            entity.Property(e => e.Regdate)
                .HasColumnType("datetime")
                .HasColumnName("REGDATE");
            entity.Property(e => e.SendReports)
                .HasColumnType("int(11)")
                .HasColumnName("SEND_REPORTS");
            entity.Property(e => e.State)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("STATE");
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("STATUS");
            entity.Property(e => e.Taxes).HasColumnName("TAXES");
            entity.Property(e => e.Timestamp)
                .HasColumnType("int(11)")
                .HasColumnName("TIMESTAMP");
            entity.Property(e => e.UserColor)
                .HasColumnType("int(11)")
                .HasColumnName("USER_COLOR");
            entity.Property(e => e.Zipcode)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("ZIPCODE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}