using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountActiveAndSuspend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveOn",
                schema: "trd",
                table: "_Account",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedOn",
                schema: "trd",
                table: "_Account",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "_Account_ActiveOn_index",
                schema: "trd",
                table: "_Account",
                column: "ActiveOn");

            migrationBuilder.CreateIndex(
                name: "_Account_SuspendedOn_index",
                schema: "trd",
                table: "_Account",
                column: "SuspendedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_Account_ActiveOn_index",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropIndex(
                name: "_Account_SuspendedOn_index",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropColumn(
                name: "ActiveOn",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropColumn(
                name: "SuspendedOn",
                schema: "trd",
                table: "_Account");
        }
    }
}
