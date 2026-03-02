using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddContextColInMedium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Context",
                schema: "sto",
                table: "_Medium",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX__Medium_Context",
                schema: "sto",
                table: "_Medium",
                column: "Context");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Medium_Context",
                schema: "sto",
                table: "_Medium");

            migrationBuilder.DropColumn(
                name: "Context",
                schema: "sto",
                table: "_Medium");
        }
    }
}