using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class DropPaymentServiceFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = '_Payment__PaymentService_Id_fk' 
                        AND table_schema = 'acct'
                        AND table_name = '_Payment'
                    ) THEN
                        ALTER TABLE acct.""_Payment"" DROP CONSTRAINT ""_Payment__PaymentService_Id_fk"";
                    END IF;
                END $$;
            ");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "acct",
                table: "_Crypto",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'acct'
                        AND tablename = '_Crypto'
                        AND indexname = 'IX__Crypto_IsDeleted'
                    ) THEN
                        CREATE INDEX ""IX__Crypto_IsDeleted"" ON acct.""_Crypto"" (""IsDeleted"");
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Crypto_IsDeleted",
                schema: "acct",
                table: "_Crypto");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "acct",
                table: "_Crypto",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "_Payment__PaymentService_Id_fk",
                schema: "acct",
                table: "_Payment",
                column: "PaymentServiceId",
                principalSchema: "acct",
                principalTable: "_PaymentService",
                principalColumn: "Id");
        }
    }
}
