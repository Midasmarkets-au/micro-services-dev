using Bacera.Gateway.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

public class MeiliSearchTests : Startup
{
    private readonly MeiliSearchService _svc;

    public MeiliSearchTests()
    {
        var options = Options.Create(new SearchServiceOptions
        {
            Endpoint = Configuration.GetSection("SearchEngine").GetValue<string>("Endpoint") ?? string.Empty,
            MasterKey = Configuration.GetSection("SearchEngine").GetValue<string>("MasterKey") ?? string.Empty,
        });
        var authOption = new DbContextOptionsBuilder<AuthDbContext>();
        authOption.UseNpgsql((Configuration.GetConnectionString("TenantConnectionTemplate-au") ?? string.Empty)
            .Replace("{{DATABASE}}", "portal_tenant_0718_au"));
        var authCtx = new AuthDbContext(authOption.Options);

        var tenantOption = new DbContextOptionsBuilder<TenantDbContext>();
        tenantOption.UseNpgsql((Configuration.GetConnectionString("TenantConnectionTemplate-au") ?? string.Empty)
            .Replace("{{DATABASE}}", "portal_tenant_0718_au"));
        var ctx = new TenantDbContext(tenantOption.Options);

        var tenancySvc = new TenancyResolver();
        tenancySvc.SetTenant(new Tenant() { Id = 1, Name = "au" });

        _svc = new MeiliSearchService(options, tenancySvc, authCtx, ctx);
    }

    [Fact]
    public async Task CreateAccountIndexAsync()
    {
        await _svc.CreateAccountIndexAsync();

        var criteria = new Account.Criteria
        {
            Page = 0,
            Size = 200,
            SortField = "Id",
            SortFlag = true,
        };
        while (true)
        {
            var items = await _svc.QueryAccountAsync(criteria);
            if (!items.Any())
                break;
            criteria.Page++;

            await _svc.AddDocumentsAsync(items);
        }

        var result = await _svc.SearchAccountAsync<Account.SearchDocumentForTenant>("allen", 0);
        result.ShouldNotBeNull();
    }
}