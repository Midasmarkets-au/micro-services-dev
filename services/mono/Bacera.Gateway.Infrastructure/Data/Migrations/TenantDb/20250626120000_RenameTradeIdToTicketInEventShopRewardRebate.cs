using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Infrastructure.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RenameTradeIdToTicketInEventShopRewardRebate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing index on TradeId
            migrationBuilder.DropIndex(
                name: "IX__EventShopRewardRebate_TradeId",
                schema: "event",
                table: "_EventShopRewardRebate");

            // Rename the TradeId column to Ticket
            migrationBuilder.RenameColumn(
                name: "TradeId",
                schema: "event",
                table: "_EventShopRewardRebate",
                newName: "Ticket");

            // Create the index on the renamed Ticket column
            migrationBuilder.CreateIndex(
                name: "IX__EventShopRewardRebate_TradeId",
                schema: "event",
                table: "_EventShopRewardRebate",
                column: "Ticket");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the index on Ticket
            migrationBuilder.DropIndex(
                name: "IX__EventShopRewardRebate_TradeId",
                schema: "event",
                table: "_EventShopRewardRebate");

            // Rename the Ticket column back to TradeId
            migrationBuilder.RenameColumn(
                name: "Ticket",
                schema: "event",
                table: "_EventShopRewardRebate",
                newName: "TradeId");

            // Recreate the index on TradeId
            migrationBuilder.CreateIndex(
                name: "IX__EventShopRewardRebate_TradeId",
                schema: "event",
                table: "_EventShopRewardRebate",
                column: "TradeId");
        }
    }
} 