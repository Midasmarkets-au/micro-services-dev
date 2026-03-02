using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CreatePermissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Permission",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "''::character varying"),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "''::character varying"),
                    Auth = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Permission_pk", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "_Permission_Action_index",
                schema: "core",
                table: "_Permission",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "_Permission_Method_index",
                schema: "core",
                table: "_Permission",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "_Permission_Category_index",
                schema: "core",
                table: "_Permission",
                column: "Category");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Permission",
                schema: "core");
        }
    }
}
