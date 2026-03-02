using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RebuildSymbol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Symbol__Currency_Id_fk",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropForeignKey(
                name: "_Symbol__SymbolCategory_Id_fk",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropTable(
                name: "_SymbolCategory",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_SymbolInfo",
                schema: "trd");

            migrationBuilder.DropIndex(
                name: "_Symbol_AliasCode_uindex",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropIndex(
                name: "IX__Symbol_CurrencyId",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "AliasCode",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "ContractSize",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "SwapsLong",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "SwapsShort",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                schema: "trd",
                table: "_Symbol",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "trd",
                table: "_Symbol",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "OperatorPartyId",
                schema: "trd",
                table: "_Symbol",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.CreateIndex(
                name: "IX__Symbol_Category",
                schema: "trd",
                table: "_Symbol",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX__Symbol_Code",
                schema: "trd",
                table: "_Symbol",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX__Symbol_OperatorPartyId",
                schema: "trd",
                table: "_Symbol",
                column: "OperatorPartyId");

            migrationBuilder.AddForeignKey(
                name: "_Symbol_OperatorPartyId_fkey",
                schema: "trd",
                table: "_Symbol",
                column: "OperatorPartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Symbol_OperatorPartyId_fkey",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropIndex(
                name: "IX__Symbol_Category",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropIndex(
                name: "IX__Symbol_Code",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropIndex(
                name: "IX__Symbol_OperatorPartyId",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.DropColumn(
                name: "OperatorPartyId",
                schema: "trd",
                table: "_Symbol");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "trd",
                table: "_Symbol",
                newName: "Sequence");

            migrationBuilder.AddColumn<string>(
                name: "AliasCode",
                schema: "trd",
                table: "_Symbol",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractSize",
                schema: "trd",
                table: "_Symbol",
                type: "integer",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "trd",
                table: "_Symbol",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "trd",
                table: "_Symbol",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SwapsLong",
                schema: "trd",
                table: "_Symbol",
                type: "numeric(16,2)",
                precision: 16,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SwapsShort",
                schema: "trd",
                table: "_Symbol",
                type: "numeric(16,2)",
                precision: 16,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                schema: "trd",
                table: "_Symbol",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateTable(
                name: "_SymbolCategory",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table => { table.PrimaryKey("_SymbolCategory_pk", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "_SymbolInfo",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_SymbolInfo_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_SymbolInfo__Symbol_Id_fk",
                        column: x => x.Id,
                        principalSchema: "trd",
                        principalTable: "_Symbol",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "_Symbol_AliasCode_uindex",
                schema: "trd",
                table: "_Symbol",
                column: "AliasCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Symbol_CurrencyId",
                schema: "trd",
                table: "_Symbol",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "_Symbol__Currency_Id_fk",
                schema: "trd",
                table: "_Symbol",
                column: "CurrencyId",
                principalSchema: "acct",
                principalTable: "_Currency",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "_Symbol__SymbolCategory_Id_fk",
                schema: "trd",
                table: "_Symbol",
                column: "CategoryId",
                principalSchema: "trd",
                principalTable: "_SymbolCategory",
                principalColumn: "Id");
        }
    }
}