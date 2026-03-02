using System.Reflection;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupDbContext(this WebApplicationBuilder me)
    {
        var centralOptions = GetCentralDatabaseOptions();
        me.Services.AddSingleton(_ => Options.Create(centralOptions));

        var tenantOptions = GetTenantDatabaseOptions();
        me.Services.AddSingleton(_ => Options.Create(tenantOptions));

        var websiteOptions = GetWebsiteDatabaseOptions();
        me.Services.AddSingleton(_ => Options.Create(websiteOptions));

        var mybcrOptions = GetMybcrDatabaseOptions();
        me.Services.AddSingleton(_ => Options.Create(mybcrOptions));

        me.Services.AddScoped(provider =>
        {
            var getter = provider.GetRequiredService<ITenantGetter>();
            var pool = provider.GetRequiredService<MyDbContextPool>();
            var tenantId = getter.GetTenantId();
            var connectionString = pool.GetConnectionStringByTenantId(tenantId);
            return new TenantDbConnection(new NpgsqlConnection(connectionString));
        });

        me.Services.AddScoped(provider =>
        {
            var centralDbOptions = provider.GetRequiredService<IOptions<CentralDatabaseOptions>>().Value;
            var connection = new NpgsqlConnection(centralDbOptions.ConnectionString);
            return new CentralDbConnection(connection);
        });

        me.Services.AddScoped(provider =>
        {
            var centralDbOptions = provider.GetRequiredService<IOptions<CentralDatabaseOptions>>().Value;
            var connection = new NpgsqlConnection(centralDbOptions.ConnectionString);
            return new AuthDbConnection(connection);
        });

        me.Services.AddDbContext<CentralDbContext>(options => options.UseNpgsql(centralOptions.ConnectionString));

        me.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(centralOptions.ConnectionString));

        me.Services.AddDbContext<WebsiteDbContext>(options =>
            options.UseMySql(websiteOptions.ConnectionString, ServerVersion.Parse("8.0.35-mysql")));

        me.Services.AddDbContext<MybcrDbContext>(options =>
            options.UseMySql(mybcrOptions.ConnectionString, ServerVersion.Parse("8.0.35-mysql")));

        me.Services.AddDbContext<TenantDbContext>((services, options) =>
        {
            var getter = services.GetRequiredService<ITenantGetter>();
            var pool = services.GetRequiredService<MyDbContextPool>();
            var tenantId = getter.GetTenantId();
            var connectionString = pool.GetConnectionStringByTenantId(tenantId);
            // options.UseNpgsql(connectionString).UseLazyLoadingProxies();
            // connectionString = $"{connectionString};Include Error Details=True";
            // connectionString += ";Include Error Details=True";
            options.UseNpgsql(connectionString);
        });

        // TODO Step 2: OpenIddict tables are registered via AuthDbContext (no separate DbContext needed)

        me.Services.AddSingleton<MyDbContextPool>();
    }
}