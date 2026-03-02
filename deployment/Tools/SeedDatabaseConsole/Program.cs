using Bacera.Gateway;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Infrastructure.Tools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting database seeding process...");

                // Default connection string — auth schema in portal_central database
                string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=qazwsx1@3;Database=portal_central;";

                if (args.Length > 0)
                    connectionString = args[0];

                Console.WriteLine($"Using connection string: {connectionString}");

                var services = new ServiceCollection();
                services.AddLogging();

                services.AddDbContext<AuthDbContext>(options =>
                    options.UseNpgsql(connectionString));

                var tenantConnectionString = connectionString.Replace("portal_central", "portal_tenant_bvi");
                services.AddDbContext<TenantDbContext>(options =>
                    options.UseNpgsql(tenantConnectionString));

                services.AddIdentity<User, ApplicationRole>()
                    .AddEntityFrameworkStores<AuthDbContext>()
                    .AddDefaultTokenProviders();

                var serviceProvider = services.BuildServiceProvider();

                // Seed admin user (Identity tables)
                await SeedAdminUser.SeedAdminUserAsync(serviceProvider);

                Console.WriteLine("");
                Console.WriteLine("🎉 Database seeding completed successfully!");
                Console.WriteLine("");
                Console.WriteLine("✅ Admin user seeded/verified");
                Console.WriteLine("");
                Console.WriteLine("ℹ️  OpenIddict clients (api, mobile) and scopes are seeded");
                Console.WriteLine("   automatically by OpenIddictSeedService on first app startup.");
                Console.WriteLine("   No manual SQL seed required.");
                Console.WriteLine("");
                Console.WriteLine("🔐 You can now authenticate with:");
                Console.WriteLine("   Email: admin@example.com");
                Console.WriteLine("   Password: Admin@123456");
                Console.WriteLine("   Endpoint: POST /connect/token");
                Console.WriteLine("   grant_type=password  client_id=api");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
