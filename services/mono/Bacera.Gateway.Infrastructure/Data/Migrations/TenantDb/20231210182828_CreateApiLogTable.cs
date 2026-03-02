using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CreateApiLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_ApiLog",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: true),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    Action = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Parameters = table.Column<string>(type: "text", nullable: true),
                    RequestContent = table.Column<string>(type: "text", nullable: true),
                    ResponseContent = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
                    Ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true, defaultValueSql: "''::character varying"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                });
            migrationBuilder.CreateIndex(
                name: "_ApiLog_Method_index",
                schema: "core",
                table: "_ApiLog",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "_ApiLog_PartyId_index",
                schema: "core",
                table: "_ApiLog",
                column: "PartyId");
            
            migrationBuilder.CreateIndex(
                name: "_ApiLog_Ip_index",
                schema: "core",
                table: "_ApiLog",
                column: "Ip");

            migrationBuilder.CreateIndex(
                name: "_ApiLog_Action_index",
                schema: "core",
                table: "_ApiLog",
                column: "Action");
            
            migrationBuilder.CreateIndex(
                name: "_ApiLog_StatusCode_index",
                schema: "core",
                table: "_ApiLog",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
