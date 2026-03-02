using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Web.Tests;

public class EmailTests : Startup
{
    private readonly ISendMailService _svc;
    private readonly TenantDbContext _ctx;


    public EmailTests()
    {
        _svc = AppServiceProvider.GetRequiredService<ISendMailService>();
        _ctx = AppServiceProvider.GetRequiredService<TenantDbContext>();
    }

    [Fact]
    public async Task RazorDebug()
    {
        var result = await _svc.DebugAsync(DebugEmailRequest.Build());
        result.ShouldNotBeNull();
    }
}