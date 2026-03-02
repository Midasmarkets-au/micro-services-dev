using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class WithdrawalApproveTimeAndMessageRecordIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                schema: "acct",
                table: "_Withdrawal",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.CreateIndex(
                name: "_Withdrawal_ApprovedOn_index",
                schema: "acct",
                table: "_Withdrawal",
                column: "ApprovedOn");
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_CreatedOn",
                schema: "sto",
                table: "_MessageRecord",
                column: "CreatedOn");
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_Event",
                schema: "sto",
                table: "_MessageRecord",
                column: "Event");
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_Method",
                schema: "sto",
                table: "_MessageRecord",
                column: "Method");
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_Receiver",
                schema: "sto",
                table: "_MessageRecord",
                column: "Receiver");
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_ReceiverPartyId",
                schema: "sto",
                table: "_MessageRecord",
                column: "ReceiverPartyId");
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_ReceiverPartyId_Receiver_Method_Event_Created",
                schema: "sto",
                table: "_MessageRecord",
                columns: new[] { "ReceiverPartyId", "Event", "Method", "CreatedOn", "Receiver" });
            
            migrationBuilder.CreateIndex(
                name: "IX__MessageRecord_Status",
                schema: "sto",
                table: "_MessageRecord",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_Withdrawal_ApprovedOn_index",
                schema: "acct",
                table: "_Withdrawal");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_CreatedOn",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_Event",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_Method",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_Receiver",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_ReceiverPartyId",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_ReceiverPartyId_Receiver_Method_Event_Created",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropIndex(
                name: "IX__MessageRecord_Status",
                schema: "sto",
                table: "_MessageRecord");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                schema: "acct",
                table: "_Withdrawal");
        }
    }
}
