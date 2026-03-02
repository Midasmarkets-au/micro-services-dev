using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Tag",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => { table.PrimaryKey("_Tag_pk", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "_PartyTag",
                schema: "core",
                columns: table => new
                {
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PartyTag_pk", x => new { x.PartyId, x.TagId });
                    table.ForeignKey(
                        name: "_PartyTag__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_PartyTag__Tag_Id_fk",
                        column: x => x.TagId,
                        principalSchema: "core",
                        principalTable: "_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX__PartyTag_TagId",
                schema: "core",
                table: "_PartyTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "_Tag_Name_index",
                schema: "core",
                table: "_Tag",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "_Tag_Type_index",
                schema: "core",
                table: "_Tag",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_PartyTag",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_Tag",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "core_parties_uid_index",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Uid",
                schema: "core",
                table: "_Party");
        }
    }
}