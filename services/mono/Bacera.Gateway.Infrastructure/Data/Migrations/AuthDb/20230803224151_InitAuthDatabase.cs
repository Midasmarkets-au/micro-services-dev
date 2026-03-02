using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class InitAuthDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "_Role",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Tag",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Tag_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_User",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uid = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ReferrerPartyId = table.Column<long>(type: "bigint", nullable: false),
                    NativeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Avatar = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReferCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CCC = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Citizen = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IdType = table.Column<int>(type: "integer", nullable: false),
                    IdNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdIssuer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdIssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    IdExpireOn = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisteredIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferPath = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_UserAudit",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_UserAudit_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_RoleClaim",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK__RoleClaim__Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_UserClaim",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK__UserClaim__User_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_UserLogin",
                schema: "auth",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK__UserLogin__User_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_UserRole",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK__UserRole__Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__UserRole__User_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_UserTag",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_UserTag_pk", x => new { x.UserId, x.TagId });
                    table.ForeignKey(
                        name: "_UserTag__Tag_Id_fk",
                        column: x => x.TagId,
                        principalSchema: "auth",
                        principalTable: "_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_UserTag__User_Id_fk",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_UserToken",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK__UserToken__User_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "auth",
                table: "_Role",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__RoleClaim_RoleId",
                schema: "auth",
                table: "_RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "_Tag_Name_ux",
                schema: "auth",
                table: "_Tag",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "auth",
                table: "_User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX__User_Uid",
                schema: "auth",
                table: "_User",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "auth",
                table: "_User",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_UserAudit_PartyId_index",
                schema: "auth",
                table: "_UserAudit",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_UserAudit_Type_RowId_index",
                schema: "auth",
                table: "_UserAudit",
                columns: new[] { "Type", "RowId" });

            migrationBuilder.CreateIndex(
                name: "IX__UserClaim_UserId",
                schema: "auth",
                table: "_UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX__UserLogin_UserId",
                schema: "auth",
                table: "_UserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX__UserRole_RoleId",
                schema: "auth",
                table: "_UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX__UserTag_TagId",
                schema: "auth",
                table: "_UserTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_RoleClaim",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_UserAudit",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_UserClaim",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_UserLogin",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_UserRole",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_UserTag",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_UserToken",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_Role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_Tag",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "_User",
                schema: "auth");
        }
    }
}
