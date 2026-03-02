using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class InitTenantDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trd");

            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "acct");

            migrationBuilder.EnsureSchema(
                name: "cms");

            migrationBuilder.EnsureSchema(
                name: "sto");

            migrationBuilder.CreateTable(
                name: "_Action",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("actions_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Audit",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Audit_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Charge",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character(12)", fixedLength: true, maxLength: 12, nullable: false),
                    APCode = table.Column<string>(type: "character(10)", fixedLength: true, maxLength: 10, nullable: false),
                    ARCode = table.Column<string>(type: "character(10)", fixedLength: true, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("charges_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Comment",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Comment_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_ContactRequest",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    IsArchived = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    PhoneNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Ip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    Content = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ContactRequest_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Currency",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entity = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currencies_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_FundType",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("wallet_types_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Lead",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    PhoneNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Supplement = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Lead_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_MatterType",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("matter_types_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Medium",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pid = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Guid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Medium_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Medium__Medium_Id_fk",
                        column: x => x.Pid,
                        principalSchema: "sto",
                        principalTable: "_Medium",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_NotificationEvent",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectType = table.Column<int>(type: "integer", nullable: false),
                    MethodType = table.Column<int>(type: "integer", nullable: false),
                    ChannelType = table.Column<int>(type: "integer", nullable: false),
                    IsActivated = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    ModuleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_NotificationEvent_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Role",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Role_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Site",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Site_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_State",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("states_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Supplement",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplements_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_SymbolCategory",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_SymbolCategory_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Topic",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AdditionalInformation = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Topic_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_TradeService",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'0'::smallint"),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1"),
                    IsAllowAccountCreation = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    Configuration = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("trade_services_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_ExchangeRate",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    ToCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    BuyingRate = table.Column<decimal>(type: "numeric(16,8)", precision: 16, scale: 8, nullable: false),
                    SellingRate = table.Column<decimal>(type: "numeric(16,8)", precision: 16, scale: 8, nullable: false),
                    AdjustRate = table.Column<decimal>(type: "numeric(16,8)", precision: 16, scale: 8, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ExchangeRate_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_ExchangeRate__Currency_Id_fk",
                        column: x => x.FromCurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_ExchangeRate__Currency_Id_fk2",
                        column: x => x.ToCurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_PaymentService",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'-1'::integer"),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsActivated = table.Column<short>(type: "smallint", nullable: false),
                    CanDeposit = table.Column<short>(type: "smallint", nullable: false),
                    CanWithdraw = table.Column<short>(type: "smallint", nullable: false),
                    InitialValue = table.Column<long>(type: "bigint", nullable: false),
                    MinValue = table.Column<long>(type: "bigint", nullable: false),
                    MaxValue = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, defaultValueSql: "''::character varying"),
                    CategoryName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValueSql: "''::character varying"),
                    Configuration = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'{}'::text"),
                    IsHighDollarEnabled = table.Column<short>(type: "smallint", nullable: false),
                    CommentCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentService_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_PaymentService__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Matter",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pid = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PostedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StateId = table.Column<int>(type: "integer", nullable: false),
                    StatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("matters_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "core_matters_pid_foreign",
                        column: x => x.Pid,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_matters_type_foreign",
                        column: x => x.Type,
                        principalSchema: "core",
                        principalTable: "_MatterType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Configuration",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Configuration_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Configuration__Site_Id_fk",
                        column: x => x.SiteId,
                        principalSchema: "core",
                        principalTable: "_Site",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Party",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pid = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("parties_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_Party__Site_Id_fk",
                        column: x => x.SiteId,
                        principalSchema: "core",
                        principalTable: "_Site",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_parties_pid_foreign",
                        column: x => x.Pid,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Transition",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ActionId = table.Column<int>(type: "integer", nullable: false),
                    OnStateId = table.Column<int>(type: "integer", nullable: false),
                    ToStateId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("transitions_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_Transition__Role_Id_fk",
                        column: x => x.RoleId,
                        principalSchema: "core",
                        principalTable: "_Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_transitions_action_id_foreign",
                        column: x => x.ActionId,
                        principalSchema: "core",
                        principalTable: "_Action",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_transitions_on_state_id_foreign",
                        column: x => x.OnStateId,
                        principalSchema: "core",
                        principalTable: "_State",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_transitions_to_state_id_foreign",
                        column: x => x.ToStateId,
                        principalSchema: "core",
                        principalTable: "_State",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Symbol",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    ContractSize = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1"),
                    SwapsLong = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    SwapsShort = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AliasCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Symbol_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Symbol__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Symbol__SymbolCategory_Id_fk",
                        column: x => x.CategoryId,
                        principalSchema: "trd",
                        principalTable: "_SymbolCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_TopicContent",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Author = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_TopicContent_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_TopicContent__Topic_Id_fk",
                        column: x => x.TopicId,
                        principalSchema: "cms",
                        principalTable: "_Topic",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_PaymentServiceFundType",
                schema: "acct",
                columns: table => new
                {
                    FundTypeId = table.Column<int>(type: "integer", nullable: false),
                    PaymentServiceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentServiceFundType_pk", x => new { x.FundTypeId, x.PaymentServiceId });
                    table.ForeignKey(
                        name: "_PaymentServiceFundType__FundType_Id_fk",
                        column: x => x.FundTypeId,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_PaymentServiceFundType__PaymentService_Id_fk",
                        column: x => x.PaymentServiceId,
                        principalSchema: "acct",
                        principalTable: "_PaymentService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_Account",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uid = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'0'::smallint"),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    HasLevelRule = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'0'::smallint"),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'-1'::integer"),
                    AgentAccountId = table.Column<long>(type: "bigint", nullable: true),
                    ReferrerAccountId = table.Column<long>(type: "bigint", nullable: true),
                    SalesAccountId = table.Column<long>(type: "bigint", nullable: true),
                    BrokerAccountId = table.Column<long>(type: "bigint", nullable: true),
                    HasTradeAccount = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    Group = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ReferPath = table.Column<string>(type: "text", nullable: false),
                    IsClosed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("accounts_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_Account__Account_Id_fk",
                        column: x => x.BrokerAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Account__Agent_Id_fk",
                        column: x => x.AgentAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Account__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Account__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_accounts_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_accounts_refer_id_foreign",
                        column: x => x.ReferrerAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_accounts_sales_id_foreign",
                        column: x => x.SalesAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Activity",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatterId = table.Column<long>(type: "bigint", nullable: false),
                    ActionId = table.Column<int>(type: "integer", nullable: false),
                    OnStateId = table.Column<int>(type: "integer", nullable: false),
                    ToStateId = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PerformedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Data = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("activities_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "core_activities_matter_id_foreign",
                        column: x => x.MatterId,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_activities_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Application",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    ReferenceId = table.Column<long>(type: "bigint", nullable: false),
                    ApprovedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<long>(type: "bigint", nullable: true),
                    RejectedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<long>(type: "bigint", nullable: true),
                    RejectedReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CompletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<long>(type: "bigint", nullable: true),
                    Supplement = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_applications_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "trd_account_applications_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_CommunicateHistory",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Content = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_CommunicateHistory_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_CommunicateHistory__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_CommunicateHistory__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Invoice",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LedgerSide = table.Column<short>(type: "smallint", nullable: false),
                    Recipient = table.Column<long>(type: "bigint", nullable: false),
                    Sender = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    DueBalance = table.Column<long>(type: "bigint", nullable: false),
                    InvoicedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("invoices_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "acct_invoices_currency_id_foreign",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_invoices_recipient_foreign",
                        column: x => x.Recipient,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_invoices_sender_foreign",
                        column: x => x.Sender,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Ledger",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    MatterId = table.Column<long>(type: "bigint", nullable: false),
                    ChargeId = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    ChargeAmount = table.Column<long>(type: "bigint", nullable: false),
                    LedgerSide = table.Column<short>(type: "smallint", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: true),
                    TalliedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ledgers_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "acct_ledgers_charge_id_foreign",
                        column: x => x.ChargeId,
                        principalSchema: "acct",
                        principalTable: "_Charge",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_ledgers_currency_id_foreign",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_ledgers_matter_id_foreign",
                        column: x => x.MatterId,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_ledgers_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Message",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    SenderType = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceType = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ReadOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Content = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Message_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Message__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_NotificationSubscription",
                schema: "cms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    NotificationEventId = table.Column<long>(type: "bigint", nullable: false),
                    IsActivated = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_NotificationSubscription_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_NotificationSubscription__NotificationEvent_Id_fk",
                        column: x => x.NotificationEventId,
                        principalSchema: "cms",
                        principalTable: "_NotificationEvent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_NotificationSubscription__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_PartyRole",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PartyRole_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_PartyRole__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_PartyRole__Role_Id_fk",
                        column: x => x.RoleId,
                        principalSchema: "core",
                        principalTable: "_Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Payment",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pid = table.Column<long>(type: "bigint", nullable: true),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    LedgerSide = table.Column<short>(type: "smallint", nullable: false),
                    PaymentServiceId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("payments_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_Payment__PaymentService_Id_fk",
                        column: x => x.PaymentServiceId,
                        principalSchema: "acct",
                        principalTable: "_PaymentService",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_payments_currency_id_foreign",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_payments_pid_foreign",
                        column: x => x.Pid,
                        principalSchema: "acct",
                        principalTable: "_Payment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_payments_recipient_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_PaymentInfo",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentPlatform = table.Column<int>(type: "integer", nullable: false),
                    PaymentServiceId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Info = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'{}'::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentInfo_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_PaymentInfo__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_PaymentInfo__PaymentService_Id_fk",
                        column: x => x.PaymentServiceId,
                        principalSchema: "acct",
                        principalTable: "_PaymentService",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_PaymentServiceAccess",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentServiceId = table.Column<long>(type: "bigint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'-1'::integer"),
                    CanDeposit = table.Column<short>(type: "smallint", nullable: false),
                    CanWithdraw = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentServiceAccess_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_PaymentServiceAccess__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_PaymentServiceAccess__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_PaymentServiceAccess__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_PaymentServiceAccess__PaymentService_Id_fk",
                        column: x => x.PaymentServiceId,
                        principalSchema: "acct",
                        principalTable: "_PaymentService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_RebateBaseSchema",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_RebateBaseSchema_Id_uindex", x => x.Id);
                    table.ForeignKey(
                        name: "_RebateBaseSchema__Party_Id_fk",
                        column: x => x.CreatedBy,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateDirectSchema",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ConfirmedBy = table.Column<long>(type: "bigint", nullable: true),
                    ConfirmedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_RebateDirectSchema_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_RebateDirectSchema__Party_Id_fk",
                        column: x => x.ConfirmedBy,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateDirectSchema__Party_Id_fk2",
                        column: x => x.CreatedBy,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateSchemaBundle",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_RebateSchemaBundle_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_RebateSchemaBundle__Party_Id_fk",
                        column: x => x.CreatedBy,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_ReferralCode",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceType = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'0'::smallint"),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    Summary = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json")
                },
                constraints: table =>
                {
                    table.PrimaryKey("referral_codes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "core_referral_codes_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_ReportRequest",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    GeneratedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpireOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    FileName = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text"),
                    Query = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'{}'::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ReportRequest_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_ReportRequest__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_TradeDemoAccount",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    ExpireOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Leverage = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<double>(type: "double precision", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    PhoneNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying"),
                    CountryCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValueSql: "''::character varying"),
                    ReferralCode = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("trade_demo_accounts_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "trd_trade_demo_accounts_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_trade_demo_accounts_service_id_foreign",
                        column: x => x.ServiceId,
                        principalSchema: "trd",
                        principalTable: "_TradeService",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Transaction",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    SourceAccountId = table.Column<long>(type: "bigint", nullable: false),
                    SourceAccountType = table.Column<int>(type: "integer", nullable: false),
                    TargetAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TargetAccountType = table.Column<int>(type: "integer", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ReferenceNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Transaction_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Transaction__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Transaction__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Transaction__Matter_Id_fk",
                        column: x => x.Id,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Transaction__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Verification",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Note = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Verification_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Verification__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Wallet",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<long>(type: "bigint", nullable: false),
                    TalliedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Sequence = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "999"),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("wallets_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "acct_wallets_currency_id_foreign",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_wallets_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_wallets_type_foreign",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_SymbolInfo",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_SymbolInfo_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_SymbolInfo__Symbol_Id_fk",
                        column: x => x.Id,
                        principalSchema: "trd",
                        principalTable: "_Symbol",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_AccountPoint",
                schema: "trd",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Balance = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountPoint_pk", x => x.AccountId);
                    table.ForeignKey(
                        name: "_AccountPoint__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_AccountPointTransaction",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    PreviousBalance = table.Column<long>(type: "bigint", nullable: false),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountPointTransaction_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountPointTransaction__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Group",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Group_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Group__Account_Id_fk",
                        column: x => x.OwnerAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_LeadOwnerAccount",
                schema: "trd",
                columns: table => new
                {
                    LeadId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_LeadOwnerAccount_pk", x => new { x.LeadId, x.AccountId });
                    table.ForeignKey(
                        name: "_LeadOwnerAccount__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_LeadOwnerAccount__Lead_Id_fk",
                        column: x => x.LeadId,
                        principalSchema: "trd",
                        principalTable: "_Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_RebateAgentRule",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    AgentAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Schema = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    LevelSetting = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AgentRebateRule_Id_uindex", x => x.Id);
                    table.ForeignKey(
                        name: "_AgentRebateRule__Account_Id_fk",
                        column: x => x.AgentAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateAgentRule__RebateAgentRule_Id_fk",
                        column: x => x.ParentId,
                        principalSchema: "trd",
                        principalTable: "_RebateAgentRule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateBrokerRule",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrokerAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    AllowAccountRoles = table.Column<string>(type: "json", nullable: false),
                    AllowAccountTypes = table.Column<string>(type: "json", nullable: false),
                    Schema = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_BrokerRebateRule_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_BrokerRebateRule__Account_Id_fk",
                        column: x => x.BrokerAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateBaseSchemaItem",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RebateBaseSchemaId = table.Column<long>(type: "bigint", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    Pips = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    Commission = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    SymbolCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_RebateBaseSchemaItem_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_RebateBaseSchemaItem__RebateBaseSchema_Id_fk",
                        column: x => x.RebateBaseSchemaId,
                        principalSchema: "trd",
                        principalTable: "_RebateBaseSchema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_TradeAccount",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    LastSyncedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    RebateBaseSchemaId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("trade_accounts_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_TradeAccount__RebateRuleTemplate_Id_fk",
                        column: x => x.RebateBaseSchemaId,
                        principalSchema: "trd",
                        principalTable: "_RebateBaseSchema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_trade_accounts_currency_id_foreign",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_trade_accounts_id_foreign",
                        column: x => x.Id,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_trade_accounts_service_id_foreign",
                        column: x => x.ServiceId,
                        principalSchema: "trd",
                        principalTable: "_TradeService",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateClientRule",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DistributionType = table.Column<short>(type: "smallint", nullable: false),
                    RebateDirectSchemaId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ClientRebateRule_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_ClientRebateRule__Account_Id_fk",
                        column: x => x.ClientAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateClientRule__RebateDirectSchema_Id_fk",
                        column: x => x.RebateDirectSchemaId,
                        principalSchema: "trd",
                        principalTable: "_RebateDirectSchema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateDirectSchemaItem",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RebateDirectSchemaId = table.Column<long>(type: "bigint", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    Pips = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    Commission = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    SymbolCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_RebateDirectSchemaItem_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_RebateDirectSchemaItem__RebateDirectSchema_Id_fk",
                        column: x => x.RebateDirectSchemaId,
                        principalSchema: "trd",
                        principalTable: "_RebateDirectSchema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_Referral",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    ReferralCodeId = table.Column<long>(type: "bigint", nullable: false),
                    ReferrerPartyId = table.Column<long>(type: "bigint", nullable: false),
                    ReferredPartyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Module = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("referrals_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "core_referrals_referral_code_id_foreign",
                        column: x => x.ReferralCodeId,
                        principalSchema: "core",
                        principalTable: "_ReferralCode",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_referrals_referred_party_id_foreign",
                        column: x => x.ReferredPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "core_referrals_referrer_party_id_foreign",
                        column: x => x.ReferrerPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_VerificationItem",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VerificationId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_VerificationItem_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_VerificationItem__Verification_Id_fk",
                        column: x => x.VerificationId,
                        principalSchema: "core",
                        principalTable: "_Verification",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_WalletTransaction",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: true),
                    MatterId = table.Column<long>(type: "bigint", nullable: false),
                    PrevBalance = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("wallet_transactions_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_WalletTransaction__Matter_Id_fk",
                        column: x => x.MatterId,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_wallet_transactions_invoice_id_foreign",
                        column: x => x.InvoiceId,
                        principalSchema: "acct",
                        principalTable: "_Invoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_wallet_transactions_wallet_id_foreign",
                        column: x => x.WalletId,
                        principalSchema: "acct",
                        principalTable: "_Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_AccountGroup",
                schema: "trd",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountGroup_pk", x => new { x.AccountId, x.GroupId });
                    table.ForeignKey(
                        name: "_AccountGroup__Accounts_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AccountGroup__Group_Id_fk",
                        column: x => x.GroupId,
                        principalSchema: "trd",
                        principalTable: "_Group",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_CopyTrade",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    RuleNumber = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    SourceAccountId = table.Column<long>(type: "bigint", nullable: false),
                    SourceAccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    TargetAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TargetAccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_CopyTrade_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_CopyTrade__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_CopyTrade__TradeAccount_Id_Source_fk",
                        column: x => x.SourceAccountId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_CopyTrade__TradeAccount_Id_Target_fk",
                        column: x => x.TargetAccountId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Deposit",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    TargetTradeAccountId = table.Column<long>(type: "bigint", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Deposit_pk", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Deposit__Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "acct",
                        principalTable: "_Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_Deposit__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Deposit__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Deposit__Matters_Id_fk",
                        column: x => x.Id,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Deposit__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Deposit__TradeAccount_Id_fk",
                        column: x => x.TargetTradeAccountId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_RebateDirectRule",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceTradeAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TargetAccountId = table.Column<long>(type: "bigint", nullable: false),
                    RebateDirectSchemaId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    ConfirmedBy = table.Column<long>(type: "bigint", nullable: true),
                    ConfirmedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_RebateDirectRule_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_RebateDirectRule__Account_Id_fk",
                        column: x => x.TargetAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateDirectRule__Party_Id_fk",
                        column: x => x.CreatedBy,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateDirectRule__Party_Id_fk2",
                        column: x => x.ConfirmedBy,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateDirectRule__RebateDirectSchema_Id_fk",
                        column: x => x.RebateDirectSchemaId,
                        principalSchema: "trd",
                        principalTable: "_RebateDirectSchema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_RebateDirectRule__TradeAccount_Id_fk",
                        column: x => x.SourceTradeAccountId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_TradeAccountStatus",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Leverage = table.Column<int>(type: "integer", nullable: false),
                    AgentAccount = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    PrevMonthBalance = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    PrevBalance = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    Credit = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    InterestRate = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    Taxes = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    Equity = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    Margin = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    MarginLevel = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    MarginFree = table.Column<double>(type: "double precision", nullable: false, defaultValueSql: "'0'::double precision"),
                    LastLoginOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ReadOnlyCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("trade_account_status_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "trd_trade_account_status_id_foreign",
                        column: x => x.Id,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_TradeRebate",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TradeAccountId = table.Column<long>(type: "bigint", nullable: true),
                    TradeServiceId = table.Column<int>(type: "integer", nullable: false),
                    Ticket = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Volume = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ClosedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OpenedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    TimeStamp = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_TradeRebate_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_TradeRebate__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_TradeRebate__TradeAccount_Id_fk",
                        column: x => x.TradeAccountId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_TradeRebate__TradeService_Id_fk",
                        column: x => x.TradeServiceId,
                        principalSchema: "trd",
                        principalTable: "_TradeService",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Withdrawal",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    SourceTradeAccountId = table.Column<long>(type: "bigint", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(16,8)", precision: 16, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Withdrawal_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Withdrawal__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Withdrawal__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Withdrawal__Matters_Id_fk",
                        column: x => x.Id,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Withdrawal__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Withdrawal__Payment_Id_fk",
                        column: x => x.PaymentId,
                        principalSchema: "acct",
                        principalTable: "_Payment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Withdrawal__TradeAccount_Id_fk",
                        column: x => x.SourceTradeAccountId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Rebate",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    TradeRebateId = table.Column<long>(type: "bigint", nullable: true),
                    HoldUntilOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Information = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("rebates_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_Rebate__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Rebate__TradeRebate_Id_fk",
                        column: x => x.TradeRebateId,
                        principalSchema: "trd",
                        principalTable: "_TradeRebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_rebates_account_id_foreign",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_rebates_currency_id_foreign",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_rebates_matter_id_foreign",
                        column: x => x.Id,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "trd_rebates_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "_Account_Code_index",
                schema: "trd",
                table: "_Account",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "_Account_IsClosed_index",
                schema: "trd",
                table: "_Account",
                column: "IsClosed");

            migrationBuilder.CreateIndex(
                name: "_Account_Uid_ux",
                schema: "trd",
                table: "_Account",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Account_Aid",
                schema: "trd",
                table: "_Account",
                column: "AgentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__Account_BrokerAccountId",
                schema: "trd",
                table: "_Account",
                column: "BrokerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__Account_CurrencyId",
                schema: "trd",
                table: "_Account",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Account_FundType",
                schema: "trd",
                table: "_Account",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_refer_id",
                schema: "trd",
                table: "_Account",
                column: "ReferrerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_sales_id",
                schema: "trd",
                table: "_Account",
                column: "SalesAccountId");

            migrationBuilder.CreateIndex(
                name: "trd_accounts_group_index",
                schema: "trd",
                table: "_Account",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "trd_accounts_party_id_index",
                schema: "trd",
                table: "_Account",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountGroup_GroupId",
                schema: "trd",
                table: "_AccountGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountPointTransaction_AccountId",
                schema: "trd",
                table: "_AccountPointTransaction",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "core_activities_matter_id_index",
                schema: "core",
                table: "_Activity",
                column: "MatterId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_action_id",
                schema: "core",
                table: "_Activity",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_on_state_id",
                schema: "core",
                table: "_Activity",
                column: "OnStateId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_party_id",
                schema: "core",
                table: "_Activity",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_to_state_id",
                schema: "core",
                table: "_Activity",
                column: "ToStateId");

            migrationBuilder.CreateIndex(
                name: "trd_account_applications_party_id_index",
                schema: "trd",
                table: "_Application",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_Audit_PartyId_index",
                schema: "core",
                table: "_Audit",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_Audit_Type_RowId_index",
                schema: "core",
                table: "_Audit",
                columns: new[] { "Type", "RowId" });

            migrationBuilder.CreateIndex(
                name: "acct_charges_ap_code_unique",
                schema: "acct",
                table: "_Charge",
                column: "APCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_charges_ar_code_unique",
                schema: "acct",
                table: "_Charge",
                column: "ARCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_charges_code_unique",
                schema: "acct",
                table: "_Charge",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_charges_name_unique",
                schema: "acct",
                table: "_Charge",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_Comment_Type_RowId_index",
                schema: "core",
                table: "_Comment",
                columns: new[] { "Type", "RowId" });

            migrationBuilder.CreateIndex(
                name: "_CommunicateHistory_CreatedOn_index",
                schema: "cms",
                table: "_CommunicateHistory",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "_CommunicateHistory_Type_index",
                schema: "cms",
                table: "_CommunicateHistory",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__CommunicateHistory_OperatorPartyId",
                schema: "cms",
                table: "_CommunicateHistory",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__CommunicateHistory_PartyId",
                schema: "cms",
                table: "_CommunicateHistory",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_Configuration_Type_SiteId_uindex",
                schema: "core",
                table: "_Configuration",
                columns: new[] { "Type", "SiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Configuration_SiteId",
                schema: "core",
                table: "_Configuration",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "_CopyTrade_PartyId_index",
                schema: "trd",
                table: "_CopyTrade",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__CopyTrade_SourceAccountId",
                schema: "trd",
                table: "_CopyTrade",
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__CopyTrade_TargetAccountId",
                schema: "trd",
                table: "_CopyTrade",
                column: "TargetAccountId");

            migrationBuilder.CreateIndex(
                name: "acct_currencies_code_unique",
                schema: "acct",
                table: "_Currency",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_Deposit_Type_index",
                schema: "acct",
                table: "_Deposit",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__Deposit_CurrencyId",
                schema: "acct",
                table: "_Deposit",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Deposit_FundType",
                schema: "acct",
                table: "_Deposit",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX__Deposit_PartyId",
                schema: "acct",
                table: "_Deposit",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Deposit_PaymentId",
                schema: "acct",
                table: "_Deposit",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX__Deposit_TargetTradeAccountId",
                schema: "acct",
                table: "_Deposit",
                column: "TargetTradeAccountId");

            migrationBuilder.CreateIndex(
                name: "_ExchangeRate_From_To_ux",
                schema: "trd",
                table: "_ExchangeRate",
                columns: new[] { "FromCurrencyId", "ToCurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_ExchangeRate_FromCurrencyId_index",
                schema: "trd",
                table: "_ExchangeRate",
                column: "FromCurrencyId");

            migrationBuilder.CreateIndex(
                name: "_ExchangeRate_FromCurrencyId_ToCurrencyId_uindex",
                schema: "trd",
                table: "_ExchangeRate",
                columns: new[] { "FromCurrencyId", "ToCurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_ExchangeRate_Name_uindex",
                schema: "trd",
                table: "_ExchangeRate",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_ExchangeRate_ToCurrencyId_index",
                schema: "trd",
                table: "_ExchangeRate",
                column: "ToCurrencyId");

            migrationBuilder.CreateIndex(
                name: "_Group_AccountId_Type_uindex",
                schema: "trd",
                table: "_Group",
                columns: new[] { "OwnerAccountId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_Group_Name_uindex",
                schema: "trd",
                table: "_Group",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_invoices_invoiced_at_index",
                schema: "acct",
                table: "_Invoice",
                column: "InvoicedOn");

            migrationBuilder.CreateIndex(
                name: "acct_invoices_number_index",
                schema: "acct",
                table: "_Invoice",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "acct_invoices_recipient_index",
                schema: "acct",
                table: "_Invoice",
                column: "Recipient");

            migrationBuilder.CreateIndex(
                name: "acct_invoices_sender_index",
                schema: "acct",
                table: "_Invoice",
                column: "Sender");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_currency_id",
                schema: "acct",
                table: "_Invoice",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "_Lead_CreatedOn_index",
                schema: "trd",
                table: "_Lead",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "_Lead_Email_index",
                schema: "trd",
                table: "_Lead",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "_Lead_PhoneNumber_index",
                schema: "trd",
                table: "_Lead",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX__LeadOwnerAccount_AccountId",
                schema: "trd",
                table: "_LeadOwnerAccount",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_charge_amount_index",
                schema: "acct",
                table: "_Ledger",
                column: "ChargeAmount");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_charge_id_index",
                schema: "acct",
                table: "_Ledger",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_currency_id_index",
                schema: "acct",
                table: "_Ledger",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_invoice_id_index",
                schema: "acct",
                table: "_Ledger",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_ledger_side_index",
                schema: "acct",
                table: "_Ledger",
                column: "LedgerSide");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_matter_id_index",
                schema: "acct",
                table: "_Ledger",
                column: "MatterId");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_party_id_index",
                schema: "acct",
                table: "_Ledger",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "acct_ledgers_tallied_at_index",
                schema: "acct",
                table: "_Ledger",
                column: "TalliedOn");

            migrationBuilder.CreateIndex(
                name: "core_matters_pid_index",
                schema: "core",
                table: "_Matter",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "core_matters_posted_at_index",
                schema: "core",
                table: "_Matter",
                column: "PostedOn");

            migrationBuilder.CreateIndex(
                name: "core_matters_state_id_index",
                schema: "core",
                table: "_Matter",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "core_matters_stated_at_index",
                schema: "core",
                table: "_Matter",
                column: "StatedOn");

            migrationBuilder.CreateIndex(
                name: "core_matters_type_index",
                schema: "core",
                table: "_Matter",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__Medium_Guid",
                schema: "sto",
                table: "_Medium",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Medium_Pid",
                schema: "sto",
                table: "_Medium",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX__Message_PartyId",
                schema: "cms",
                table: "_Message",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_NotificationEvent_Code_uindex",
                schema: "cms",
                table: "_NotificationEvent",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_NotificationEvent_Name_uindex",
                schema: "cms",
                table: "_NotificationEvent",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_NotificationEvent_SubjectType_ChannelType_MethodType_uindex",
                schema: "cms",
                table: "_NotificationEvent",
                columns: new[] { "SubjectType", "ChannelType", "MethodType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_NotificationSubscription_PartyId_NotificationEventId_uindex",
                schema: "cms",
                table: "_NotificationSubscription",
                columns: new[] { "PartyId", "NotificationEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__NotificationSubscription_NotificationEventId",
                schema: "cms",
                table: "_NotificationSubscription",
                column: "NotificationEventId");

            migrationBuilder.CreateIndex(
                name: "core_parties_code_index",
                schema: "core",
                table: "_Party",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "core_parties_pid_index",
                schema: "core",
                table: "_Party",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX__Party_SiteId",
                schema: "core",
                table: "_Party",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX__PartyRole_PartyId",
                schema: "core",
                table: "_PartyRole",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__PartyRole_RoleId",
                schema: "core",
                table: "_PartyRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "_Payment_Number_uindex",
                schema: "acct",
                table: "_Payment",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_Payment_ReferenceNumber_index",
                schema: "acct",
                table: "_Payment",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "acct_payments_number_index",
                schema: "acct",
                table: "_Payment",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "acct_payments_pid_index",
                schema: "acct",
                table: "_Payment",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "acct_payments_recipient_index",
                schema: "acct",
                table: "_Payment",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_currency_id",
                schema: "acct",
                table: "_Payment",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_payment_method",
                schema: "acct",
                table: "_Payment",
                column: "PaymentServiceId");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentInfo_PartyId",
                schema: "acct",
                table: "_PaymentInfo",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentInfo_PaymentServiceId",
                schema: "acct",
                table: "_PaymentInfo",
                column: "PaymentServiceId");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentService_CurrencyId",
                schema: "acct",
                table: "_PaymentService",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "_PaymentServiceAccess_PartyId_FundType_CurrencyId_PaymentServic",
                schema: "acct",
                table: "_PaymentServiceAccess",
                columns: new[] { "PartyId", "FundType", "CurrencyId", "PaymentServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_PaymentServiceAccess_PartyId_index",
                schema: "acct",
                table: "_PaymentServiceAccess",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentServiceAccess_CurrencyId",
                schema: "acct",
                table: "_PaymentServiceAccess",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentServiceAccess_FundType",
                schema: "acct",
                table: "_PaymentServiceAccess",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentServiceAccess_PaymentServiceId",
                schema: "acct",
                table: "_PaymentServiceAccess",
                column: "PaymentServiceId");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentServiceFundType_PaymentServiceId",
                schema: "acct",
                table: "_PaymentServiceFundType",
                column: "PaymentServiceId");

            migrationBuilder.CreateIndex(
                name: "_Rebate_HoldUntilOn_index",
                schema: "trd",
                table: "_Rebate",
                column: "HoldUntilOn");

            migrationBuilder.CreateIndex(
                name: "_Rebate_TradeRebateId_index",
                schema: "trd",
                table: "_Rebate",
                column: "TradeRebateId");

            migrationBuilder.CreateIndex(
                name: "IX__Rebate_FundType",
                schema: "trd",
                table: "_Rebate",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "trd_rebates_account_id_index",
                schema: "trd",
                table: "_Rebate",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "trd_rebates_currency_id_index",
                schema: "trd",
                table: "_Rebate",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "trd_rebates_party_id_index",
                schema: "trd",
                table: "_Rebate",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_AgentRebateRule_AgentAccountId_uindex",
                schema: "trd",
                table: "_RebateAgentRule",
                column: "AgentAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__RebateAgentRule_ParentId",
                schema: "trd",
                table: "_RebateAgentRule",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "_RebateBaseSchema_CreatedBy_index",
                schema: "trd",
                table: "_RebateBaseSchema",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "_RebateBaseSchemaItem_RebateRuleId_index",
                schema: "trd",
                table: "_RebateBaseSchemaItem",
                column: "RebateBaseSchemaId");

            migrationBuilder.CreateIndex(
                name: "_RebateBaseSchemaItem_SymbolCode_index",
                schema: "trd",
                table: "_RebateBaseSchemaItem",
                column: "SymbolCode");

            migrationBuilder.CreateIndex(
                name: "_BrokerRebateRule_pk2",
                schema: "trd",
                table: "_RebateBrokerRule",
                column: "BrokerAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_ClientRebateRule_pk2",
                schema: "trd",
                table: "_RebateClientRule",
                column: "ClientAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__RebateClientRule_RebateDirectSchemaId",
                schema: "trd",
                table: "_RebateClientRule",
                column: "RebateDirectSchemaId");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectRule_ConfirmedBy_index",
                schema: "trd",
                table: "_RebateDirectRule",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectRule_CreatedBy_index",
                schema: "trd",
                table: "_RebateDirectRule",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectRule_SourceTradeAccountId_index",
                schema: "trd",
                table: "_RebateDirectRule",
                column: "SourceTradeAccountId");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectRule_TargetAccountId_SourceTradeAccountId_uindex",
                schema: "trd",
                table: "_RebateDirectRule",
                columns: new[] { "TargetAccountId", "SourceTradeAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__RebateDirectRule_RebateDirectSchemaId",
                schema: "trd",
                table: "_RebateDirectRule",
                column: "RebateDirectSchemaId");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectSchema_ConfirmedBy_index",
                schema: "trd",
                table: "_RebateDirectSchema",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectSchema_CreatedBy_index",
                schema: "trd",
                table: "_RebateDirectSchema",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectSchema_Name_uindex",
                schema: "trd",
                table: "_RebateDirectSchema",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_RebateDirectSchemaItem_RebateRuleId_index",
                schema: "trd",
                table: "_RebateDirectSchemaItem",
                column: "RebateDirectSchemaId");

            migrationBuilder.CreateIndex(
                name: "_RebateDirectSchemaItem_SymbolCode_index",
                schema: "trd",
                table: "_RebateDirectSchemaItem",
                column: "SymbolCode");

            migrationBuilder.CreateIndex(
                name: "_RebateSchemaBundle_CreatedBy_index",
                schema: "trd",
                table: "_RebateSchemaBundle",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "_RebateSchemaBundle_Name_Type_uindex",
                schema: "trd",
                table: "_RebateSchemaBundle",
                columns: new[] { "Name", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "core_referrals_referral_code_id_index",
                schema: "core",
                table: "_Referral",
                column: "ReferralCodeId");

            migrationBuilder.CreateIndex(
                name: "core_referrals_referred_party_id_index",
                schema: "core",
                table: "_Referral",
                column: "ReferredPartyId");

            migrationBuilder.CreateIndex(
                name: "core_referrals_referrer_party_id_index",
                schema: "core",
                table: "_Referral",
                column: "ReferrerPartyId");

            migrationBuilder.CreateIndex(
                name: "core_referral_codes_code_unique",
                schema: "core",
                table: "_ReferralCode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "core_referral_codes_party_id_index",
                schema: "core",
                table: "_ReferralCode",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "core_referral_codes_service_type_index",
                schema: "core",
                table: "_ReferralCode",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "_ReportRequest_CreatedOn_index",
                schema: "sto",
                table: "_ReportRequest",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "_ReportRequest_Type_index",
                schema: "sto",
                table: "_ReportRequest",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__ReportRequest_PartyId",
                schema: "sto",
                table: "_ReportRequest",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_Site_Name_uindex",
                schema: "core",
                table: "_Site",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "core_supplements_row_id_type_unique",
                schema: "core",
                table: "_Supplement",
                columns: new[] { "RowId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_Symbol_AliasCode_uindex",
                schema: "trd",
                table: "_Symbol",
                column: "AliasCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Symbol_CategoryId",
                schema: "trd",
                table: "_Symbol",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX__Symbol_CurrencyId",
                schema: "trd",
                table: "_Symbol",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "_Topic_EffectiveFrom_EffectiveTo_index",
                schema: "cms",
                table: "_Topic",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "_Topic_Type_index",
                schema: "cms",
                table: "_Topic",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "_TopicContent_TopicId_Language_UX",
                schema: "cms",
                table: "_TopicContent",
                columns: new[] { "Language", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__TopicContent_TopicId",
                schema: "cms",
                table: "_TopicContent",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX__TradeAccount_RebateBaseSchemaId",
                schema: "trd",
                table: "_TradeAccount",
                column: "RebateBaseSchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_trade_accounts_currency_id",
                schema: "trd",
                table: "_TradeAccount",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "trd_trade_accounts_service_id_account_number_unique",
                schema: "trd",
                table: "_TradeAccount",
                columns: new[] { "ServiceId", "AccountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trade_demo_accounts_party_id",
                schema: "trd",
                table: "_TradeDemoAccount",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "trd_trade_demo_accounts_service_id_account_number_unique",
                schema: "trd",
                table: "_TradeDemoAccount",
                columns: new[] { "ServiceId", "AccountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_AccountNumber_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_Action_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_Status_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_TimeStamp_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_TradeAccountId_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "TradeAccountId");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_TradeServiceId_Ticket_uindex",
                schema: "trd",
                table: "_TradeRebate",
                columns: new[] { "TradeServiceId", "Ticket" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__TradeRebate_CurrencyId",
                schema: "trd",
                table: "_TradeRebate",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Transaction_CurrencyId",
                schema: "acct",
                table: "_Transaction",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Transaction_FundType",
                schema: "acct",
                table: "_Transaction",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX__Transaction_PartyId",
                schema: "acct",
                table: "_Transaction",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_Transition_pk",
                schema: "core",
                table: "_Transition",
                columns: new[] { "RoleId", "OnStateId", "ToStateId", "ActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transitions_action_id",
                schema: "core",
                table: "_Transition",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_transitions_on_state_id",
                schema: "core",
                table: "_Transition",
                column: "OnStateId");

            migrationBuilder.CreateIndex(
                name: "IX_transitions_to_state_id",
                schema: "core",
                table: "_Transition",
                column: "ToStateId");

            migrationBuilder.CreateIndex(
                name: "_Verification_PartyId_Type_pk",
                schema: "core",
                table: "_Verification",
                columns: new[] { "PartyId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_VerificationItem_Category_index",
                schema: "core",
                table: "_VerificationItem",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "_VerificationItem_Vid_Category_pk",
                schema: "core",
                table: "_VerificationItem",
                columns: new[] { "VerificationId", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_Wallet_Number_uindex",
                schema: "acct",
                table: "_Wallet",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_wallets_party_id_index",
                schema: "acct",
                table: "_Wallet",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "acct_wallets_party_id_type_currency_id_unique",
                schema: "acct",
                table: "_Wallet",
                columns: new[] { "PartyId", "FundType", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallets_currency_id",
                schema: "acct",
                table: "_Wallet",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_wallets_type",
                schema: "acct",
                table: "_Wallet",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "_WalletTransaction_MatterId_index",
                schema: "acct",
                table: "_WalletTransaction",
                column: "MatterId");

            migrationBuilder.CreateIndex(
                name: "_WalletTransaction_WalletId_MatterId_uindex",
                schema: "acct",
                table: "_WalletTransaction",
                columns: new[] { "WalletId", "MatterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_wallet_transactions_wallet_id_index",
                schema: "acct",
                table: "_WalletTransaction",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_invoice_id",
                schema: "acct",
                table: "_WalletTransaction",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "_Withdrawal_CurrencyId_index",
                schema: "acct",
                table: "_Withdrawal",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "_Withdrawal_FundType_index",
                schema: "acct",
                table: "_Withdrawal",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX__Withdrawal_PartyId",
                schema: "acct",
                table: "_Withdrawal",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Withdrawal_PaymentId",
                schema: "acct",
                table: "_Withdrawal",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX__Withdrawal_SourceTradeAccountId",
                schema: "acct",
                table: "_Withdrawal",
                column: "SourceTradeAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountGroup",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_AccountPoint",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_AccountPointTransaction",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Activity",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Application",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Audit",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Comment",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_CommunicateHistory",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_Configuration",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_ContactRequest",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_CopyTrade",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Deposit",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_ExchangeRate",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_LeadOwnerAccount",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Ledger",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Medium",
                schema: "sto");

            migrationBuilder.DropTable(
                name: "_Message",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_NotificationSubscription",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_PartyRole",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_PaymentInfo",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_PaymentServiceAccess",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_PaymentServiceFundType",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Rebate",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateAgentRule",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateBaseSchemaItem",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateBrokerRule",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateClientRule",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateDirectRule",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateDirectSchemaItem",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateSchemaBundle",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Referral",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_ReportRequest",
                schema: "sto");

            migrationBuilder.DropTable(
                name: "_Supplement",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_SymbolInfo",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_TopicContent",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_TradeAccountStatus",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_TradeDemoAccount",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Transaction",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Transition",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_VerificationItem",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_WalletTransaction",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Withdrawal",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Group",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Lead",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Charge",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_NotificationEvent",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_TradeRebate",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_RebateDirectSchema",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_ReferralCode",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Symbol",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Topic",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "_Role",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Action",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_State",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Verification",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Invoice",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Wallet",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Matter",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Payment",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_TradeAccount",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_SymbolCategory",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_MatterType",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_PaymentService",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_RebateBaseSchema",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Account",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_TradeService",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Currency",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_FundType",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Party",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Site",
                schema: "core");
        }
    }
}
