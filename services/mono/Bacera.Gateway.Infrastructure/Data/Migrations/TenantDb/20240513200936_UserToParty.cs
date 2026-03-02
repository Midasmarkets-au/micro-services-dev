using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class UserToParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NativeName",
                schema: "core",
                table: "_Party",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "core",
                table: "_Party",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "core",
                table: "_Party",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                schema: "core",
                table: "_Party",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Birthday",
                schema: "core",
                table: "_Party",
                type: "date",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "CCC",
                schema: "core",
                table: "_Party",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "Citizen",
                schema: "core",
                table: "_Party",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "core",
                table: "_Party",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "core",
                table: "_Party",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "core",
                table: "_Party",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                schema: "core",
                table: "_Party",
                type: "integer",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AddColumn<DateOnly>(
                name: "IdExpireOn",
                schema: "core",
                table: "_Party",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "IdIssuedOn",
                schema: "core",
                table: "_Party",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "IdIssuer",
                schema: "core",
                table: "_Party",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                schema: "core",
                table: "_Party",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdType",
                schema: "core",
                table: "_Party",
                type: "integer",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "core",
                table: "_Party",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "core",
                table: "_Party",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "ReferCode",
                schema: "core",
                table: "_Party",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "core",
                table: "_Party",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                schema: "core",
                table: "_Party",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValueSql: "''::character varying");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Avatar",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Birthday",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "CCC",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Citizen",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "IdExpireOn",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "IdIssuedOn",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "IdIssuer",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "IdType",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "ReferCode",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                schema: "core",
                table: "_Party");

            migrationBuilder.AlterColumn<string>(
                name: "NativeName",
                schema: "core",
                table: "_Party",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldDefaultValueSql: "''::character varying");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "core",
                table: "_Party",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldDefaultValueSql: "''::character varying");
        }
    }
}
