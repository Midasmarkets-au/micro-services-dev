using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddClientPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_EventShopClientPoint",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ParentAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ParentAccountRole = table.Column<short>(type: "smallint", nullable: false),
                    OpenAccount = table.Column<short>(type: "smallint", nullable: false),
                    Volume = table.Column<int>(type: "integer", nullable: false),
                    DepositAmount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_EventShopClientPoint_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_EventShopClientPoint__ClientAccount_Id_fk",
                        column: x => x.ChildAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_EventShopClientPoint__ParentAccount_Id_fk",
                        column: x => x.ParentAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__EventShopClientPoint_ClientAccountId",
                schema: "event",
                table: "_EventShopClientPoint",
                column: "ChildAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopClientPoint_ParentAccountId",
                schema: "event",
                table: "_EventShopClientPoint",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopClientPoint_ParentAccountRole",
                schema: "event",
                table: "_EventShopClientPoint",
                column: "ParentAccountRole");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_EventShopClientPoint",
                schema: "event");
        }
    }
}