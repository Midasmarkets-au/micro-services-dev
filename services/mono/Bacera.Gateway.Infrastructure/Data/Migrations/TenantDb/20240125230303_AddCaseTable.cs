using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddCaseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_CaseCategory",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Role = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_CaseCategory_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_CaseCategory__Parent_Id_fk",
                        column: x => x.ParentId,
                        principalSchema: "sto",
                        principalTable: "_CaseCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_Case",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplyId = table.Column<long>(type: "bigint", nullable: true),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    CaseId = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text"),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    AdminPartyId = table.Column<long>(type: "bigint", nullable: true),
                    Data = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'::json"),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Files = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Case_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Case__Admin_Party_Id_fk",
                        column: x => x.AdminPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Case__Category_Id_fk",
                        column: x => x.CategoryId,
                        principalSchema: "sto",
                        principalTable: "_CaseCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Case__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Case__Reply_Id_fk",
                        column: x => x.ReplyId,
                        principalSchema: "sto",
                        principalTable: "_Case",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__Case_AdminPartyId",
                schema: "sto",
                table: "_Case",
                column: "AdminPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Case_CaseId",
                schema: "sto",
                table: "_Case",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX__Case_CategoryId",
                schema: "sto",
                table: "_Case",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX__Case_IsAdmin",
                schema: "sto",
                table: "_Case",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX__Case_PartyId",
                schema: "sto",
                table: "_Case",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Case_ReplyId",
                schema: "sto",
                table: "_Case",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX__Case_Status",
                schema: "sto",
                table: "_Case",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__CaseCategory_Name",
                schema: "sto",
                table: "_CaseCategory",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX__CaseCategory_ParentId",
                schema: "sto",
                table: "_CaseCategory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX__CaseCategory_Role",
                schema: "sto",
                table: "_CaseCategory",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX__CaseCategory_Status",
                schema: "sto",
                table: "_CaseCategory",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Case",
                schema: "sto");

            migrationBuilder.DropTable(
                name: "_CaseCategory",
                schema: "sto");
        }
    }
}
