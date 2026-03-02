using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorTagSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountWithTag",
                schema: "trd",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountWithTag_pk", x => new { x.AccountId, x.TagId });
                    table.ForeignKey(
                        name: "_AccountWithTag__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_AccountWithTag__Tag_Id_fk",
                        column: x => x.TagId,
                        principalSchema: "core",
                        principalTable: "_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_PartyWithTag",
                schema: "core",
                columns: table => new
                {
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PartyWithTag_pk", x => new { x.PartyId, x.TagId });
                    table.ForeignKey(
                        name: "_PartyWithTag__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_PartyWithTag__Tag_Id_fk",
                        column: x => x.TagId,
                        principalSchema: "core",
                        principalTable: "_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountWithTag_TagId",
                schema: "trd",
                table: "_AccountWithTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX__PartyWithTag_TagId",
                schema: "core",
                table: "_PartyWithTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountWithTag",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_PartyWithTag",
                schema: "core");
        }
    }
}