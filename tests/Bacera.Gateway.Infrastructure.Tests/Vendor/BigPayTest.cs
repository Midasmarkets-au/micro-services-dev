using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class BigPayTest : Startup
{
    private readonly IAccountingService _accountingService;

    public BigPayTest()
    {
        _accountingService = ServiceProvider.GetRequiredService<IAccountingService>();
    }
}