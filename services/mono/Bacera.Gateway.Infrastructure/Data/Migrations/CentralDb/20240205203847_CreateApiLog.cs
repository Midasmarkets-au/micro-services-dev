using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class CreateApiLog : Migration
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
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Parameters = table.Column<string>(type: "text", nullable: true),
                    RequestContent = table.Column<string>(type: "text", nullable: true),
                    ResponseContent = table.Column<string>(type: "text", nullable: true),
                    Ip = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("apiLog_pkey", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "_ApiLog_Action_index",
                schema: "core",
                table: "_ApiLog",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "_ApiLog_Ip_index",
                schema: "core",
                table: "_ApiLog",
                column: "Ip");

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
                name: "_ApiLog_StatusCode_index",
                schema: "core",
                table: "_ApiLog",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_ApiLog",
                schema: "core");
        }
    }
}