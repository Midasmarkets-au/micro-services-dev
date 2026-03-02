using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class CentralConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_CentralConfig",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    DataFormat = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_CentralConfig_pk", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX__CentralConfig_Category",
                schema: "core",
                table: "_CentralConfig",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX__CentralConfig_DataFormat",
                schema: "core",
                table: "_CentralConfig",
                column: "DataFormat");

            migrationBuilder.CreateIndex(
                name: "IX__CentralConfig_Key",
                schema: "core",
                table: "_CentralConfig",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX__CentralConfig_RowId",
                schema: "core",
                table: "_CentralConfig",
                column: "RowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_CentralConfig",
                schema: "core");
        }
    }
}
