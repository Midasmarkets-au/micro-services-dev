using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AuthCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AuthCode",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Event = table.Column<string>(type: "character varying", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    MethodValue = table.Column<string>(type: "character varying", maxLength: 32, nullable: false, defaultValueSql: "''::character varying"),
                    Method = table.Column<string>(type: "character varying", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ExpireOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AuthCode_pk", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "_AuthCode_Code_index",
                schema: "core",
                table: "_AuthCode",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "_AuthCode_Method_index",
                schema: "core",
                table: "_AuthCode",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "_AuthCode_Method_MethodValue_index",
                schema: "core",
                table: "_AuthCode",
                columns: new[] { "Method", "MethodValue" });

            migrationBuilder.CreateIndex(
                name: "_AuthCode_MethodValue_index",
                schema: "core",
                table: "_AuthCode",
                column: "MethodValue");

            migrationBuilder.CreateIndex(
                name: "_AuthCode_PartyId_index",
                schema: "core",
                table: "_AuthCode",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_AuthCode_Status_index",
                schema: "core",
                table: "_AuthCode",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AuthCode",
                schema: "core");
        }
    }
}
