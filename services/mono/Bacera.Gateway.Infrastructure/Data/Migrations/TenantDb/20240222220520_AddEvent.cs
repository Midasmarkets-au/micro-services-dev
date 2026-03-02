using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "event");

            migrationBuilder.CreateTable(
                name: "_Address",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CCC = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Address_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Address__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_Event",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplyStartOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplyEndOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessRoles = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table => { table.PrimaryKey("_Event_pkey", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "_EventLanguage",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false,
                        defaultValueSql: "'en-us'::character varying"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Images = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Term = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventLanguage_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventLanguage__Event_Id_fk",
                        column: x => x.EventId,
                        principalSchema: "event",
                        principalTable: "_Event",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventParty",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Settings = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventParty_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventParty__Event_Id_fk",
                        column: x => x.EventId,
                        principalSchema: "event",
                        principalTable: "_Event",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventParty__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventParty__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopItem",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Category = table.Column<short>(type: "smallint", nullable: false),
                    AccessRoles = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Configuration = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json"),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopItem_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopItem__Event_Id_fk",
                        column: x => x.EventId,
                        principalSchema: "event",
                        principalTable: "_Event",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopPoint",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventPartyId = table.Column<long>(type: "bigint", nullable: false),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    TotalPoint = table.Column<long>(type: "bigint", nullable: false),
                    FrozenPoint = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopPoint_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopPoint__EventParty_Id_fk",
                        column: x => x.EventPartyId,
                        principalSchema: "event",
                        principalTable: "_EventParty",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopPointTransaction",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventPartyId = table.Column<long>(type: "bigint", nullable: false),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<short>(type: "smallint", nullable: false),
                    SourceContent = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopPointTransaction_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopPointTransaction__EventParty_Id_fk",
                        column: x => x.EventPartyId,
                        principalSchema: "event",
                        principalTable: "_EventParty",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopItemLanguage",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventShopItemId = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false,
                        defaultValueSql: "'en-us'::character varying"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopItemLanguage_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopItemLanguage__EventShopItem_Id_fk",
                        column: x => x.EventShopItemId,
                        principalSchema: "event",
                        principalTable: "_EventShopItem",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopOrder",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventPartyId = table.Column<long>(type: "bigint", nullable: false),
                    EventShopItemId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    TotalPoint = table.Column<long>(type: "bigint", nullable: false),
                    AddressId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopOrder_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopOrder__Address_Id_fk",
                        column: x => x.AddressId,
                        principalSchema: "core",
                        principalTable: "_Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventShopOrder__EventParty_Id_fk",
                        column: x => x.EventPartyId,
                        principalSchema: "event",
                        principalTable: "_EventParty",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventShopOrder__EventShopItem_Id_fk",
                        column: x => x.EventShopItemId,
                        principalSchema: "event",
                        principalTable: "_EventShopItem",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventShopOrder__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopReward",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventPartyId = table.Column<long>(type: "bigint", nullable: false),
                    EventShopItemId = table.Column<long>(type: "bigint", nullable: false),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopReward_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopReward__EventParty_Id_fk",
                        column: x => x.EventPartyId,
                        principalSchema: "event",
                        principalTable: "_EventParty",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventShopReward__EventShopItem_Id_fk",
                        column: x => x.EventShopItemId,
                        principalSchema: "event",
                        principalTable: "_EventShopItem",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventShopReward__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_EventShopRewardRebate",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventShopRewardId = table.Column<long>(type: "bigint", nullable: false),
                    TradeId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopRewardRebate_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopRewardRebate__EventShopReward_Id_fk",
                        column: x => x.EventShopRewardId,
                        principalSchema: "event",
                        principalTable: "_EventShopReward",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__Address_CCC",
                schema: "core",
                table: "_Address",
                column: "CCC");

            migrationBuilder.CreateIndex(
                name: "IX__Address_Country",
                schema: "core",
                table: "_Address",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX__Address_CreatedOn",
                schema: "core",
                table: "_Address",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX__Address_DeletedOn",
                schema: "core",
                table: "_Address",
                column: "DeletedOn");

            migrationBuilder.CreateIndex(
                name: "IX__Address_Name",
                schema: "core",
                table: "_Address",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX__Address_PartyId",
                schema: "core",
                table: "_Address",
                column: "PartyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Address_PartyId1",
                schema: "core",
                table: "_Address",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Address_Phone",
                schema: "core",
                table: "_Address",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX__Address_UpdatedOn",
                schema: "core",
                table: "_Address",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: " IX__Event_ApplyStartOn",
                schema: "event",
                table: "_Event",
                column: "ApplyStartOn");

            migrationBuilder.CreateIndex(
                name: "IX__Event_ApplyEndOn",
                schema: "event",
                table: "_Event",
                column: "ApplyEndOn");

            migrationBuilder.CreateIndex(
                name: "IX__Event_CreatedOn",
                schema: "event",
                table: "_Event",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX__Event_EndOn",
                schema: "event",
                table: "_Event",
                column: "EndOn");

            migrationBuilder.CreateIndex(
                name: "IX__Event_Key",
                schema: "event",
                table: "_Event",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX__Event_StartOn",
                schema: "event",
                table: "_Event",
                column: "StartOn");

            migrationBuilder.CreateIndex(
                name: "IX__Event_Status",
                schema: "event",
                table: "_Event",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventLanguage_Description",
                schema: "event",
                table: "_EventLanguage",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX__EventLanguage_EventId",
                schema: "event",
                table: "_EventLanguage",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX__EventLanguage_Language",
                schema: "event",
                table: "_EventLanguage",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX__EventLanguage_Title",
                schema: "event",
                table: "_EventLanguage",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX__EventParty_EventId",
                schema: "event",
                table: "_EventParty",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX__EventParty_OperatorPartyId",
                schema: "event",
                table: "_EventParty",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventParty_PartyId",
                schema: "event",
                table: "_EventParty",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventParty_Status",
                schema: "event",
                table: "_EventParty",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItem_Category",
                schema: "event",
                table: "_EventShopItem",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItem_EventId",
                schema: "event",
                table: "_EventShopItem",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItem_Status",
                schema: "event",
                table: "_EventShopItem",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItem_Type",
                schema: "event",
                table: "_EventShopItem",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItemLanguage_Description",
                schema: "event",
                table: "_EventShopItemLanguage",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItemLanguage_EventShopItemId",
                schema: "event",
                table: "_EventShopItemLanguage",
                column: "EventShopItemId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItemLanguage_Language",
                schema: "event",
                table: "_EventShopItemLanguage",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopItemLanguage_Title",
                schema: "event",
                table: "_EventShopItemLanguage",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: " IX__EventShopOrder_AddressId",
                schema: "event",
                table: "_EventShopOrder",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopOrder_EventPartyId",
                schema: "event",
                table: "_EventShopOrder",
                column: "EventPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopOrder_EventShopItemId",
                schema: "event",
                table: "_EventShopOrder",
                column: "EventShopItemId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopOrder_OperatorPartyId",
                schema: "event",
                table: "_EventShopOrder",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopOrder_Status",
                schema: "event",
                table: "_EventShopOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPoint_EventPartyId",
                schema: "event",
                table: "_EventShopPoint",
                column: "EventPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPointTransaction_EventPartyId",
                schema: "event",
                table: "_EventShopPointTransaction",
                column: "EventPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPointTransaction_SourceType",
                schema: "event",
                table: "_EventShopPointTransaction",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPointTransaction_Status",
                schema: "event",
                table: "_EventShopPointTransaction",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopReward_EventPartyId",
                schema: "event",
                table: "_EventShopReward",
                column: "EventPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopReward_EventShopItemId",
                schema: "event",
                table: "_EventShopReward",
                column: "EventShopItemId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopReward_OperatorPartyId",
                schema: "event",
                table: "_EventShopReward",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopReward_Status",
                schema: "event",
                table: "_EventShopReward",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopRewardRebate_EventShopRewardId",
                schema: "event",
                table: "_EventShopRewardRebate",
                column: "EventShopRewardId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopRewardRebate_Status",
                schema: "event",
                table: "_EventShopRewardRebate",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopRewardRebate_TradeId",
                schema: "event",
                table: "_EventShopRewardRebate",
                column: "TradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_EventLanguage",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventShopItemLanguage",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventShopOrder",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventShopPoint",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventShopPointTransaction",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventShopRewardRebate",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_Address",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_EventShopReward",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventParty",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_EventShopItem",
                schema: "event");

            migrationBuilder.DropTable(
                name: "_Event",
                schema: "event");
        }
    }
}