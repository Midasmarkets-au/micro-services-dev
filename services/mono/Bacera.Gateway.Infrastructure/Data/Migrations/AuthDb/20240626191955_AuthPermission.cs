using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AuthPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Permission",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValueSql: "''::character varying"),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''::character varying"),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "''::character varying"),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "''::character varying"),
                    Auth = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_permission_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_PermissionRoleAccess",
                schema: "auth",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PermissionRoleAccess_pk", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "_PermissionUserAccess__Permission_fk",
                        column: x => x.PermissionId,
                        principalSchema: "auth",
                        principalTable: "_Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_PermissionUserAccess__RoleId_fk",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_PermissionUserAccess",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PermissionUserAccess_pk", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "_PermissionUserAccess__Permission_fk",
                        column: x => x.PermissionId,
                        principalSchema: "auth",
                        principalTable: "_Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_PermissionUserAccess__UserId_fk",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "_Permission_Action_index",
                schema: "auth",
                table: "_Permission",
                column: "Action");
            
            migrationBuilder.CreateIndex(
                name: "_Permission_Category_index",
                schema: "auth",
                table: "_Permission",
                column: "Category");
            
            migrationBuilder.CreateIndex(
                name: "_Permission_Method_index",
                schema: "auth",
                table: "_Permission",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX__PermissionRoleAccess_PermissionId",
                schema: "auth",
                table: "_PermissionRoleAccess",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX__PermissionUserAccess_PermissionId",
                schema: "auth",
                table: "_PermissionUserAccess",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_PermissionRoleAccess",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_PermissionUserAccess",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_Permission",
                schema: "auth");
        }
    }
}
