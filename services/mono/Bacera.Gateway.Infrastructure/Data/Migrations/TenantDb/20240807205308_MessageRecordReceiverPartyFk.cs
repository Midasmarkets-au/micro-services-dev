using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class MessageRecordReceiverPartyFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "_MessageRecord__ReceiverParty_Id_fk",
                schema: "sto",
                table: "_MessageRecord",
                column: "ReceiverPartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_MessageRecord__ReceiverParty_Id_fk",
                schema: "sto",
                table: "_MessageRecord");
        }
    }
}
