using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.PaymentAsia;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task TestCheckRebate()
    {
        using var scope = _serviceProvider.CreateTenantScope(10000);
        var rebateSvc = scope.ServiceProvider.GetRequiredService<RebateService>();
        var rebate = await rebateSvc.TradeRebateCheckForTenantAsync(1858226);
    }
}