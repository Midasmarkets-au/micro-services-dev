using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class UserBlackList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_UserBlackList",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    Email = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    IdNumber = table.Column<string>(type: "text", nullable: false),
                    OperatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_black_lists_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "_UserBlackList_Email_Index",
                schema: "core",
                table: "_UserBlackList",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "_UserBlackList_Name_Index",
                schema: "core",
                table: "_UserBlackList",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "_UserBlackList_OperatorName_Index",
                schema: "core",
                table: "_UserBlackList",
                column: "OperatorName");

            migrationBuilder.CreateIndex(
                name: "_UserBlackList_Phone_Index",
                schema: "core",
                table: "_UserBlackList",
                column: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_UserBlackList",
                schema: "core");
        }
    }
}
