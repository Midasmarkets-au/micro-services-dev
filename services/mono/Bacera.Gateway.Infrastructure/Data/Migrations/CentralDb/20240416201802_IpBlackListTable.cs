using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class IpBlackListTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_IpBlackList",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false,
                        defaultValueSql: "''::character varying"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    OperatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false,
                        defaultValueSql: "''::character varying"),
                    Note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false,
                        defaultValueSql: "''::character varying")
                },
                constraints: table => { table.PrimaryKey("ip_black_lists_pkey", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "_IpBlackList_Ip_Index",
                schema: "core",
                table: "_IpBlackList",
                column: "Ip",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_IpBlackList_OperatorName_Index",
                schema: "core",
                table: "_IpBlackList",
                column: "OperatorName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_IpBlackList",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "VolumeOriginal",
                schema: "trd",
                table: "_MetaTrade");
        }
    }
}