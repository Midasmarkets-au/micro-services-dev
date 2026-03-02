using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                schema: "cms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    site = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    link = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    languages = table.Column<string>(type: "json", nullable: true),
                    operator_info = table.Column<string>(type: "json", nullable: true),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "historical_documents",
                schema: "cms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<long>(type: "bigint", nullable: false),
                    link = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    language = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    site = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    operator_info = table.Column<string>(type: "json", nullable: true),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historical_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_historical_documents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "cms",
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_historical_documents_document_id",
                schema: "cms",
                table: "historical_documents",
                column: "document_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historical_documents",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "cms");
        }
    }
}
