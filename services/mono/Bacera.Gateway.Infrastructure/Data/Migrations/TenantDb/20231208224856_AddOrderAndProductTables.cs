using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddOrderAndProductTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shop");

            migrationBuilder.CreateTable(
                name: "_Product",
                schema: "shop",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Point = table.Column<long>(type: "bigint", nullable: false),
                    Total = table.Column<long>(type: "bigint", nullable: false),
                    Supplement = table.Column<string>(type: "json", nullable: true, defaultValueSql: "'{}'::json"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Order",
                schema: "shop",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Recipient = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Supplement = table.Column<string>(type: "json", nullable: true, defaultValueSql: "'{}'::json"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "shop_order_party_id_foreign",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "shop_order_product_id_foreign",
                        column: x => x.ProductId,
                        principalSchema: "shop",
                        principalTable: "_Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "order_amount_index",
                schema: "shop",
                table: "_Order",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "order_number_index",
                schema: "shop",
                table: "_Order",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "order_party_id_index",
                schema: "shop",
                table: "_Order",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "order_product_id_index",
                schema: "shop",
                table: "_Order",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "order_status_index",
                schema: "shop",
                table: "_Order",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "_Product_OperatorPartyId_index",
                schema: "shop",
                table: "_Product",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "_Product_Point_index",
                schema: "shop",
                table: "_Product",
                column: "Point");

            migrationBuilder.CreateIndex(
                name: "_Product_Status_index",
                schema: "shop",
                table: "_Product",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "_Product_Total_index",
                schema: "shop",
                table: "_Product",
                column: "Total");

            migrationBuilder.CreateIndex(
                name: "_Product_Type_index",
                schema: "shop",
                table: "_Product",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Order",
                schema: "shop");

            migrationBuilder.DropTable(
                name: "_Product",
                schema: "shop");
        }
    }
}
