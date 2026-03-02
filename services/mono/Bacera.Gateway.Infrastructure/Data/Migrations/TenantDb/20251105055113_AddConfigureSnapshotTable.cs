using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddConfigureSnapshotTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_ConfigurationSnapshot",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    ConfigName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SnapshotVersion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ConfigurationSnapshot_pk", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSnapshot_ConfigName_RowId",
                schema: "core",
                table: "_ConfigurationSnapshot",
                columns: new[] { "ConfigName", "RowId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSnapshot_CreatedOn",
                schema: "core",
                table: "_ConfigurationSnapshot",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSnapshot_UserId_SnapshotId",
                schema: "core",
                table: "_ConfigurationSnapshot",
                columns: new[] { "PartyId", "SnapshotId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_ConfigurationSnapshot",
                schema: "core");
        }
    }
}
