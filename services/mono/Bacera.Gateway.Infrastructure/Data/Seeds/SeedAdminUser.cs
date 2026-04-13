using Bacera.Gateway;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bacera.Gateway.Infrastructure.Data.Seeds
{
    public class AdminUserConfig
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public long Uid { get; set; }
        public long PartyId { get; set; }
    }

    public static class SeedAdminUser
    {
        // Define all admins to create here
        private static List<AdminUserConfig> GetAdminUsersToSeed()
        {
            var admins = new List<AdminUserConfig>
            {
                // Previous Midas Markets admins (commented out)
                //new AdminUserConfig { Email = "gz-admin1@midasmkts.com", FirstName = "GZ", LastName = "Admin 1", Password = "R7u!yQ4xV@z", Uid = 9001, PartyId = 19001 },
                //new AdminUserConfig { Email = "gz-admin2@midasmkts.com", FirstName = "GZ", LastName = "Admin 2", Password = "P9k#mW2xL@n", Uid = 9002, PartyId = 19002 },
                //new AdminUserConfig { Email = "karen.chow@midasmkts.com", FirstName = "Karen", LastName = "Chow", Password = "K7r@nCh0w!X", Uid = 9003, PartyId = 19003 },
                //new AdminUserConfig { Email = "kay.wong@midasmkts.com", FirstName = "Kay", LastName = "Wong", Password = "K@yW0ng#9Zx", Uid = 9004, PartyId = 19004 },
                //new AdminUserConfig { Email = "karly.kwok@midasmkts.com", FirstName = "Karly", LastName = "Kwok", Password = "K@rlyKw0k!7", Uid = 9005, PartyId = 19005 },
                //new AdminUserConfig { Email = "katie.chan@midasmkts.com", FirstName = "Katie", LastName = "Chan", Password = "K@t1eCh@n#5", Uid = 9006, PartyId = 19006 },
                //new AdminUserConfig { Email = "cloris.cheng@midasmkts.com", FirstName = "Cloris", LastName = "Cheng", Password = "Cl0r!sChg@8", Uid = 9007, PartyId = 19007 },
                //new AdminUserConfig { Email = "lydia.yeung@midasmkts.com", FirstName = "Lydia", LastName = "Yeung", Password = "Lyd!@Y3ung#", Uid = 9008, PartyId = 19008 },
                //new AdminUserConfig { Email = "gary.wong@midasmkts.com", FirstName = "Gary", LastName = "Wong", Password = "G@ryW0ng!4X", Uid = 9009, PartyId = 19009 },
                //new AdminUserConfig { Email = "alan.cheung@midasmkts.com", FirstName = "Alan", LastName = "Cheung", Password = "Al@nCh3ung#", Uid = 9010, PartyId = 19010 },
                
                // Bacera admins (commented out - already seeded last time)
                //new AdminUserConfig { Email = "elvis.chan@bacera.com", FirstName = "Elvis", LastName = "Chan", Password = "Elv!s#Ch@n7", Uid = 9011, PartyId = 19011 },
                //new AdminUserConfig { Email = "alfred.lam@bacera.com", FirstName = "Alfred", LastName = "Lam", Password = "Alfr3d!L@m#", Uid = 9012, PartyId = 19012 },
                //new AdminUserConfig { Email = "elvis.chan@bacera.com", FirstName = "Elvis", LastName = "Chan", Password = "Elv!s2#Ch@n", Uid = 9013, PartyId = 19013 },
                //new AdminUserConfig { Email = "hung.lau@bacera.com", FirstName = "Hung", LastName = "Lau", Password = "Hung@L4u!9X", Uid = 9014, PartyId = 19014 },
                //new AdminUserConfig { Email = "jonny.chan@bacera.com", FirstName = "Jonny", LastName = "Chan", Password = "J0nny#Ch@n!", Uid = 9015, PartyId = 19015 },
                //new AdminUserConfig { Email = "kaz.tsang@bacera.com", FirstName = "Kaz", LastName = "Tsang", Password = "K@z!Ts4ng#X", Uid = 9016, PartyId = 19016 },
                //new AdminUserConfig { Email = "lui.lam@bacera.com", FirstName = "Lui", LastName = "Lam", Password = "Lu!@L4m#9Zx", Uid = 9017, PartyId = 19017 },
                //new AdminUserConfig { Email = "paul.pan@bacera.com", FirstName = "Paul", LastName = "Pan", Password = "P@ul!P4n#7X", Uid = 9018, PartyId = 19018 },
                
                // New Midas Markets admin
                //new AdminUserConfig { Email = "marketing@midasmkts.com", FirstName = "Marketing", LastName = "Admin", Password = "M@rk3t!ng#9X", Uid = 9019, PartyId = 19019 },
                
                // Viann Yeung admin (already seeded)
                //new AdminUserConfig { Email = "viann.yeung@midasmkts.com", FirstName = "Viann", LastName = "Yeung", Password = "V!@nnY3ung#9X", Uid = 9020, PartyId = 19020 },

                // GZ Admin 3
                new AdminUserConfig { Email = "gz-admin3@midasmkts.com", FirstName = "GZ", LastName = "Admin 3", Password = "Gz@dm!n3#8Xq", Uid = 9021, PartyId = 19021 }
            };
            return admins;
        }

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            try
            {
                // Get required services
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                var authDbContext = serviceProvider.GetRequiredService<AuthDbContext>();
                var tenantDbContext = serviceProvider.GetRequiredService<TenantDbContext>();
                var tenantId = 10000L; // Assuming tenant with ID 10000 exists

                // Define roles if they don't exist
                string[] roles = { 
                    UserRoleTypesString.Admin, 
                    UserRoleTypesString.TenantAdmin, 
                    UserRoleTypesString.Client, 
                    UserRoleTypesString.Guest,
                    "Manager", 
                    "User" 
                };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new ApplicationRole 
                        { 
                            Name = roleName,
                            NormalizedName = roleName.ToUpper()
                        };
                        await roleManager.CreateAsync(role);
                        Console.WriteLine($"✅ Created role: {roleName}");
                    }
                }

                Console.WriteLine("\n🔧 Starting admin users seeding...\n");

                // Get all admins to seed
                var adminsToSeed = GetAdminUsersToSeed();
                int successCount = 0;
                int skippedCount = 0;
                int failedCount = 0;

                // Create each admin user
                foreach (var adminConfig in adminsToSeed)
                {
                    var result = await CreateSingleAdminUser(userManager, tenantDbContext, adminConfig, tenantId);
                    if (result == "created") successCount++;
                    else if (result == "exists") skippedCount++;
                    else failedCount++;
                }

                Console.WriteLine("\n📊 Summary:");
                Console.WriteLine($"   ✅ Created: {successCount}");
                Console.WriteLine($"   ⚪ Already exists: {skippedCount}");
                Console.WriteLine($"   ❌ Failed: {failedCount}");
                Console.WriteLine($"   📝 Total processed: {adminsToSeed.Count}\n");

                // Create basic permissions
                await SeedPermissions(authDbContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding admin users: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static async Task<string> CreateSingleAdminUser(
            UserManager<User> userManager, 
            TenantDbContext tenantDbContext, 
            AdminUserConfig config, 
            long tenantId)
        {
            try
            {
                var adminUser = await userManager.FindByEmailAsync(config.Email);

                if (adminUser == null)
                {
                    // Create new user
                    adminUser = User.Create(config.Email);
                    adminUser.Tenant(tenantId);

                    // Set all required properties
                    adminUser.Uid = config.Uid;
                    adminUser.PartyId = config.PartyId;
                    adminUser.ReferrerPartyId = 1;
                    adminUser.NativeName = $"{config.FirstName} {config.LastName}";
                    adminUser.FirstName = config.FirstName;
                    adminUser.LastName = config.LastName;
                    adminUser.Language = "en";
                    adminUser.Avatar = "";
                    adminUser.TimeZone = "UTC";
                    adminUser.ReferCode = $"ADM{config.Uid}";
                    adminUser.CountryCode = "AU";
                    adminUser.Currency = "AUD";
                    adminUser.CCC = "+61";
                    adminUser.Birthday = new DateOnly(2000, 1, 1);
                    adminUser.Gender = 0;
                    adminUser.Status = (short)PartyStatusTypes.Active;
                    adminUser.Citizen = "US";
                    adminUser.Address = "Admin Address";
                    adminUser.IdType = 1;
                    adminUser.IdNumber = $"ID{config.Uid}";
                    adminUser.IdIssuer = "Admin Gov AU";
                    adminUser.IdIssuedOn = new DateOnly(2020, 1, 1);
                    adminUser.IdExpireOn = new DateOnly(2030, 1, 1);
                    adminUser.CreatedOn = DateTime.UtcNow;
                    adminUser.UpdatedOn = DateTime.UtcNow;
                    adminUser.RegisteredIp = "127.0.0.1";
                    adminUser.LastLoginIp = "";
                    adminUser.ReferPath = "";
                    adminUser.EmailConfirmed = true;

                    // Create the user with password
                    var result = await userManager.CreateAsync(adminUser, config.Password);

                    if (result.Succeeded)
                    {
                        Console.WriteLine($"✅ Created: {config.Email}");
                        
                        // Add to all required roles for full access
                        await userManager.AddToRoleAsync(adminUser, UserRoleTypesString.Admin);
                        await userManager.AddToRoleAsync(adminUser, UserRoleTypesString.TenantAdmin);
                        await userManager.AddToRoleAsync(adminUser, UserRoleTypesString.Client);
                        
                        // Add necessary claims if needed
                        await userManager.AddClaimAsync(adminUser, new Claim("Permission", "FullAccess"));
                        
                        // Create corresponding Party record in tenant database
                        await CreateAdminPartyRecord(tenantDbContext, adminUser);
                        
                        return "created";
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to create: {config.Email}");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"   - {error.Description}");
                        }
                        return "failed";
                    }
                }
                else
                {
                    Console.WriteLine($"⚪ Already exists: {config.Email}");
                    
                    // Ensure existing admin user has required roles
                    if (!await userManager.IsInRoleAsync(adminUser, UserRoleTypesString.Admin))
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRoleTypesString.Admin);
                    }
                    if (!await userManager.IsInRoleAsync(adminUser, UserRoleTypesString.TenantAdmin))
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRoleTypesString.TenantAdmin);
                    }
                    if (!await userManager.IsInRoleAsync(adminUser, UserRoleTypesString.Client))
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRoleTypesString.Client);
                    }
                    
                    // Ensure Party record exists in tenant database
                    await CreateAdminPartyRecord(tenantDbContext, adminUser);
                    
                    return "exists";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating {config.Email}: {ex.Message}");
                return "failed";
            }
        }

        private static async Task EnsureDefaultSiteExists(TenantDbContext tenantDbContext)
        {
            // Check if any sites exist, if not seed all sites
            if (!await tenantDbContext.Sites.AnyAsync())
            {
                // Use the existing site seeding logic to create all sites including Default (Id=0)
                var items = new List<Site>
                {
                    new() { Id = (int)SiteTypes.Default, Name = "DEFAULT" }, // Id = 0
                    new() { Id = (int)SiteTypes.BritishVirginIslands, Name = "BVI" }, // Id = 1
                    new() { Id = (int)SiteTypes.Australia, Name = "AU" }, // Id = 2
                    new() { Id = (int)SiteTypes.China, Name = "CN" }, // Id = 3
                    new() { Id = (int)SiteTypes.Taiwan, Name = "TW" }, // Id = 4
                    new() { Id = (int)SiteTypes.Vietnam, Name = "VN" }, // Id = 5
                    new() { Id = (int)SiteTypes.Japan, Name = "JP" }, // Id = 6
                    new() { Id = (int)SiteTypes.Mongolia, Name = "MN" }, // Id = 7
                    new() { Id = (int)SiteTypes.Malaysia, Name = "MY" }, // Id = 8
                };
                await tenantDbContext.Sites.AddRangeAsync(items);
                await tenantDbContext.SaveChangesAsync();
                Console.WriteLine("Created all sites including default site (Id=0) in tenant database");
            }
            else
            {
                // Check specifically for default site (Id=0)
                var defaultSite = await tenantDbContext.Sites.FirstOrDefaultAsync(s => s.Id == (int)SiteTypes.Default);
                if (defaultSite == null)
                {
                    // Create only the default site
                    var site = new Site
                    {
                        Id = (int)SiteTypes.Default, // This is 0
                        Name = "DEFAULT"
                    };
                    tenantDbContext.Sites.Add(site);
                    await tenantDbContext.SaveChangesAsync();
                    Console.WriteLine("Created default site (Id=0) in tenant database");
                }
            }
        }

        private static async Task CreateAdminPartyRecord(TenantDbContext tenantDbContext, User adminUser)
        {
            // Check if Party record with the admin user's PartyId already exists
            var existingParty = await tenantDbContext.Parties.FirstOrDefaultAsync(p => p.Id == adminUser.PartyId);
            
            if (existingParty == null)
            {
                // Create new Party record that matches the admin user
                var party = new Party
                {
                    Id = adminUser.PartyId, // 10001
                    Uid = adminUser.Uid,    // 1
                    SiteId = (int)SiteTypes.Default, // Use SiteTypes.Default (0) instead of hardcoded 1
                    Code = "ADMIN",
                    Name = adminUser.NativeName,
                    Email = adminUser.Email,
                    NativeName = adminUser.NativeName,
                    FirstName = adminUser.FirstName,
                    LastName = adminUser.LastName,
                    Language = adminUser.Language,
                    Avatar = adminUser.Avatar,
                    TimeZone = adminUser.TimeZone,
                    ReferCode = adminUser.ReferCode,
                    CountryCode = adminUser.CountryCode,
                    Currency = adminUser.Currency,
                    CCC = adminUser.CCC,
                    PhoneNumber = "", // Default empty
                    Birthday = adminUser.Birthday,
                    Gender = adminUser.Gender,
                    Status = adminUser.Status,
                    Citizen = adminUser.Citizen,
                    Address = adminUser.Address,
                    IdType = adminUser.IdType,
                    IdNumber = adminUser.IdNumber,
                    IdIssuer = adminUser.IdIssuer,
                    IdIssuedOn = adminUser.IdIssuedOn,
                    IdExpireOn = adminUser.IdExpireOn,
                    RegisteredIp = adminUser.RegisteredIp,
                    LastLoginIp = adminUser.LastLoginIp,
                    Note = "Administrator Party",
                    //ReferPath = adminUser.ReferPath,
                    SearchText = $"{adminUser.Email} {adminUser.NativeName} {adminUser.FirstName} {adminUser.LastName}",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    EmailConfirmed = adminUser.EmailConfirmed
                };

                tenantDbContext.Parties.Add(party);
                await tenantDbContext.SaveChangesAsync();
                Console.WriteLine($"Created Party record (Id={party.Id}) for admin user in tenant database with SiteId={(int)SiteTypes.Default}");
            }
            else
            {
                Console.WriteLine($"Party record (Id={existingParty.Id}) already exists in tenant database");
            }
        }

        private static async Task SeedPermissions(AuthDbContext context)
        {
            try
            {
                // Define basic permissions
                var permissions = new Permission[]
                {
                    new Permission { Key = "user", Category = "user", Action = "view", Method = "GET", Auth = true },
                    new Permission { Key = "user", Category = "user", Action = "edit", Method = "PUT", Auth = true },
                    new Permission { Key = "admin", Category = "admin", Action = "dashboard", Method = "GET", Auth = true }
                };

                // Add permissions one by one with individual error handling
                foreach (var permission in permissions)
                {
                    try
                    {
                        var exists = await context.Permissions.AnyAsync(p => 
                            p.Key == permission.Key && 
                            p.Category == permission.Category && 
                            p.Action == permission.Action &&
                            p.Method == permission.Method);

                        if (!exists)
                        {
                            context.Permissions.Add(permission);
                            await context.SaveChangesAsync(); // Save each permission individually
                            Console.WriteLine($"Added permission: {permission.Category}.{permission.Action}");
                        }
                        else
                        {
                            Console.WriteLine($"Permission already exists: {permission.Category}.{permission.Action}");
                        }
                    }
                    catch (Exception permEx)
                    {
                        Console.WriteLine($"Skipped permission {permission.Category}.{permission.Action}: {permEx.Message}");
                        // Reset context state
                        context.ChangeTracker.Clear();
                    }
                }

                Console.WriteLine("✅ Permissions seeding completed");
                Console.WriteLine("⚠️  Note: Role-permission assignments skipped to avoid database errors.");
                Console.WriteLine("   You can assign permissions to roles through your application UI or manually.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SeedPermissions: {ex.Message}");
                Console.WriteLine("⚠️  Continuing without permissions...");
            }
        }
    }
}
