using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddedAdjust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AdjustBatch",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false),
                    File = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AdjustBatch_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AdjustBatch__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_AdjustRecord",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdjustBatchId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Ticket = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AdjustRecord_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AdjustBatch__AdjustRecord_Id_fk",
                        column: x => x.AdjustBatchId,
                        principalSchema: "trd",
                        principalTable: "_AdjustBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AdjustRecord__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdjustBatch_OperatorPartyId",
                schema: "trd",
                table: "_AdjustBatch",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustBatch_ServiceId",
                schema: "trd",
                table: "_AdjustBatch",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustBatch_Status",
                schema: "trd",
                table: "_AdjustBatch",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustBatch_Type",
                schema: "trd",
                table: "_AdjustBatch",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustRecord_AccountId",
                schema: "trd",
                table: "_AdjustRecord",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustRecord_AccountNumber",
                schema: "trd",
                table: "_AdjustRecord",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustRecord_AdjustBatchId",
                schema: "trd",
                table: "_AdjustRecord",
                column: "AdjustBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustRecord_Status",
                schema: "trd",
                table: "_AdjustRecord",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustRecord_Ticket",
                schema: "trd",
                table: "_AdjustRecord",
                column: "Ticket");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustRecord_Type",
                schema: "trd",
                table: "_AdjustRecord",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AdjustRecord",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_AdjustBatch",
                schema: "trd");
        }
    }
}