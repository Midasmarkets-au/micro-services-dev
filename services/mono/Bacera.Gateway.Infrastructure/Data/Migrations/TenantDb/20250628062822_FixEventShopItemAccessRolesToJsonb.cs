using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class FixEventShopItemAccessRolesToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert AccessRoles column from json to jsonb
            migrationBuilder.Sql(@"
                ALTER TABLE ""event"".""_EventShopItem"" 
                ALTER COLUMN ""AccessRoles"" TYPE jsonb 
                USING ""AccessRoles""::jsonb;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert AccessRoles column back from jsonb to json
            migrationBuilder.Sql(@"
                ALTER TABLE ""event"".""_EventShopItem"" 
                ALTER COLUMN ""AccessRoles"" TYPE json 
                USING ""AccessRoles""::json;
            ");
        }
    }
}
