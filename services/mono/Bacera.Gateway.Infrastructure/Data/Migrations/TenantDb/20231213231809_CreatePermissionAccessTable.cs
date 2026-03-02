using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CreatePermissionAccessTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_PermissionAccess",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Model = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    ModelId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<long>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PermissionAccess_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_PermissionAccess__Permission_Id_fk",
                        column: x => x.PermissionId,
                        principalSchema: "core",
                        principalTable: "_Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "_PermissionAccess_Model_index",
                schema: "core",
                table: "_PermissionAccess",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "_PermissionAccess_ModelId_index",
                schema: "core",
                table: "_PermissionAccess",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "_PermissionAccess_PermissionId_index",
                schema: "core",
                table: "_PermissionAccess",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_PermissionAccess",
                schema: "core");
        }
    }
}
