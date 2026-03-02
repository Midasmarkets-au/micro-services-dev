using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class CentralDbInit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<long>(
            //     name: "Uid",
            //     schema: "trd",
            //     table: "_CentralAccount",
            //     type: "bigint",
            //     nullable: false,
            //     defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "_CentralReferralCode",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false,
                        defaultValueSql: "''::character varying"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_CentralReferralCode_PrimeKey", x => x.Id);
                    table.ForeignKey(
                        name: "_CentralReferralCode_TenantId_Foreign",
                        column: x => x.TenantId,
                        principalSchema: "core",
                        principalTable: "_Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "_CentralReferralCode_AccountId_Index",
                schema: "core",
                table: "_CentralReferralCode",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "_CentralReferralCode_Code_Index",
                schema: "core",
                table: "_CentralReferralCode",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "_CentralReferralCode_Name_Index",
                schema: "core",
                table: "_CentralReferralCode",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "_CentralReferralCode_PartyId_Index",
                schema: "core",
                table: "_CentralReferralCode",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_CentralReferralCode_TenantId_Index",
                schema: "core",
                table: "_CentralReferralCode",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}