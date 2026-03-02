using Bacera.Gateway;
using Bacera.Gateway.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Cleaner;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true);
        var configuration = builder.Build();
        var tenantDbConnectionString = configuration.GetConnectionString("DefaultConnection");
        var mt5DbConnectionString = configuration.GetConnectionString("Mt5Connection");

        if (string.IsNullOrEmpty(tenantDbConnectionString) || string.IsNullOrEmpty(mt5DbConnectionString))
        {
            Console.WriteLine("Connection string is empty");
            return;
        }

        var tenantDbContext = new TenantDbContext(
            new DbContextOptionsBuilder<TenantDbContext>()
                .UseNpgsql(tenantDbConnectionString)
                .Options
        );

        var svc = new BuildGroupFromPath(tenantDbContext);
        var (result, msg) = await svc.Run();
        Console.WriteLine(!result ? "Error: " : "Done!: " + msg);
    }
}

// Group To Path
// var valid = await tenantDbContext.Accounts.AnyAsync();
// if (!valid) return;
//
// var svc = new GroupToPath(tenantDbContext);
// await svc.BuildAgentGroupHierarchyPathFromRoot();

// Rebuild ticket no for mt5 trade(!!! Not accurate)
// var mt5DbContext = new MetaTrade5DbContext(
//     new DbContextOptionsBuilder<MetaTrade5DbContext>()
//         .UseMySql(mt5DbConnectionString, ServerVersion.Parse("5.7.38-mysql"))
//         .Options
// );
//
// var svc = new TicketRegenerate(tenantDbContext, mt5DbContext);
// var (result, msg) = await svc.Run();
//
// Console.WriteLine(!result ? "Error: " : "Done!: " + msg);