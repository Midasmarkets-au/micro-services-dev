using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.DTO;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.PaymentAsia;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task MigrateUser()
    {
        const long tid = 10004;
        using var scope = _serviceProvider.CreateTenantScope(tid);
        var tenantCtx = scope.ServiceProvider.GetTenantDbContext();
        var userSvc = scope.ServiceProvider.GetRequiredService<UserService>();
        await userSvc.MigrateUserAsync(333763, 10000);
    }

    private async Task TestAutoCreateAcc()
    {
        const long tid = 10000;
        using var scope = _serviceProvider.CreateTenantScope(tid);
        var svc = scope.ServiceProvider.GetRequiredService<AutoCreateAccountService>();
        await svc.TryAutoCreateTradeAccountFromPartyAsync(335775);
    }
}