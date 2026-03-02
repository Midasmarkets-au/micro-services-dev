using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SalesTypeAndEventShopItemSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesType",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AccessSites",
                schema: "event",
                table: "_EventShopItem",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.CreateIndex(
                name: "_SalesRebateSchema_SalesType_index",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "SalesType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_SalesRebateSchema_SalesType_index",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.DropColumn(
                name: "SalesType",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.DropColumn(
                name: "AccessSites",
                schema: "event",
                table: "_EventShopItem");
        }
    }
}