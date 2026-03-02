using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountCheck",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    AccountNumberContent = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountCheck_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountCheck__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountCheck_CreatedOn",
                schema: "trd",
                table: "_AccountCheck",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX__AccountCheck_OperatorPartyId",
                schema: "trd",
                table: "_AccountCheck",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountCheck_Status",
                schema: "trd",
                table: "_AccountCheck",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__AccountCheck_Type",
                schema: "trd",
                table: "_AccountCheck",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__AccountCheck_UpdatedOn",
                schema: "trd",
                table: "_AccountCheck",
                column: "UpdatedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountCheck",
                schema: "trd");
        }
    }
}
