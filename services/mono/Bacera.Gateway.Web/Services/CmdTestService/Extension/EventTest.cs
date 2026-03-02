using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor.PaymentAsia;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task TestEventProcessOpenClientAccount()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
        await eventService.ProcessOpenAccountSourceAsync(2, 52473, true);
    }

    private async Task TestEventProcessDepositAccount()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
        await eventService.ProcessDepositSourceAsync(2, 52473, true);
    }

    private async Task TestEventProcessTrade()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
        await eventService.ProcessTradeSourceAsync(1, 736292);
    }
}