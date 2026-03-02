using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorPaymentMethodTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "acct",
                table: "_PaymentMethod",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_DeletedOn",
                schema: "acct",
                table: "_PaymentMethod",
                column: "DeletedOn");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_Group",
                schema: "acct",
                table: "_PaymentMethod",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_Group_MethodType_Status",
                schema: "acct",
                table: "_PaymentMethod",
                columns: new[] { "Group", "MethodType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_InitialValue",
                schema: "acct",
                table: "_PaymentMethod",
                column: "InitialValue");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_MaxValue",
                schema: "acct",
                table: "_PaymentMethod",
                column: "MaxValue");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_MethodType",
                schema: "acct",
                table: "_PaymentMethod",
                column: "MethodType");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_MinValue",
                schema: "acct",
                table: "_PaymentMethod",
                column: "MinValue");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_Status",
                schema: "acct",
                table: "_PaymentMethod",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_DeletedOn",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_Group",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_Group_MethodType_Status",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_InitialValue",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_MaxValue",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_MethodType",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_MinValue",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_Status",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "acct",
                table: "_PaymentMethod");
        }
    }
}
