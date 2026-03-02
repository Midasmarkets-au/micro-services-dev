using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class PaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_PaymentMethod",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    MethodType = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false, defaultValueSql: "'deposit'::character varying"),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'-1'::integer"),
                    Percentage = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "100"),
                    InitialValue = table.Column<long>(type: "bigint", nullable: false),
                    MinValue = table.Column<long>(type: "bigint", nullable: false),
                    MaxValue = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    CommentCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValueSql: "''::character varying"),
                    IsHighDollarEnabled = table.Column<short>(type: "smallint", nullable: false),
                    IsAutoDepositEnabled = table.Column<short>(type: "smallint", nullable: false),
                    Group = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValueSql: "''::character varying"),
                    Logo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Note = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentMethod_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_PaymentMethodAccess",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    Model = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false, defaultValueSql: "'account'::character varying"),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentMethodAccess_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "_PaymentMethodAccess_Model_RowId_index",
                schema: "acct",
                table: "_PaymentMethodAccess",
                columns: new[] { "Model", "RowId" });

            migrationBuilder.CreateIndex(
                name: "_PaymentMethodAccess_PartyId_index",
                schema: "acct",
                table: "_PaymentMethodAccess",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_PaymentMethodAccess_PartyId_Model_RowId_index",
                schema: "acct",
                table: "_PaymentMethodAccess",
                columns: new[] { "PartyId", "Model", "RowId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_PaymentMethod",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_PaymentMethodAccess",
                schema: "acct");
        }
    }
}
