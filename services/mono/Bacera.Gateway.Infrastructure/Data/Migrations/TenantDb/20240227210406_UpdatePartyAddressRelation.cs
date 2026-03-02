using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class UpdatePartyAddressRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Address_PartyId",
                schema: "core",
                table: "_Address");

            migrationBuilder.RenameIndex(
                name: "IX__Address_PartyId1",
                schema: "core",
                table: "_Address",
                newName: "IX__Address_PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX__Address_PartyId",
                schema: "core",
                table: "_Address",
                newName: "IX__Address_PartyId1");

            migrationBuilder.CreateIndex(
                name: "IX__Address_PartyId",
                schema: "core",
                table: "_Address",
                column: "PartyId",
                unique: true);
        }
    }
}
