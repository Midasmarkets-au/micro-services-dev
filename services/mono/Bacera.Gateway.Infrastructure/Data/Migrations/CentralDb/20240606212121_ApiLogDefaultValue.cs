using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class ApiLogDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "core",
                table: "_IpBlackList",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                schema: "core",
                table: "_ApiLog",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                schema: "core",
                table: "_ApiLog",
                type: "character varying",
                maxLength: 12,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                schema: "core",
                table: "_ApiLog",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionId",
                schema: "core",
                table: "_ApiLog",
                type: "character varying",
                maxLength: 256,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "core",
                table: "_ApiLog",
                type: "character varying",
                maxLength: 256,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "_UserBlackList_IdNumber_Index",
                schema: "core",
                table: "_UserBlackList",
                column: "IdNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_UserBlackList_IdNumber_Index",
                schema: "core",
                table: "_UserBlackList");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "core",
                table: "_IpBlackList",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                schema: "core",
                table: "_ApiLog",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                schema: "core",
                table: "_ApiLog",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying",
                oldMaxLength: 12,
                oldDefaultValueSql: "''::character varying");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                schema: "core",
                table: "_ApiLog",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionId",
                schema: "core",
                table: "_ApiLog",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying",
                oldMaxLength: 256,
                oldDefaultValueSql: "''::character varying");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "core",
                table: "_ApiLog",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying",
                oldMaxLength: 256,
                oldDefaultValueSql: "''::character varying");
        }
    }
}
