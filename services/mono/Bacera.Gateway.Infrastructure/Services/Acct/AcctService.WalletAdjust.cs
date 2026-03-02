using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public async Task<(bool, string)> CreateAndCompleteWalletAdjustAsync(long walletId, long operatorPartyId = 1, string note = "")
    {
        var walletAdjust = WalletAdjust.Build(walletId, 100, note);
        tenantCtx.WalletAdjusts.Add(walletAdjust);
        await tenantCtx.SaveChangesAsync();

        await ProcessMatterAsync(walletAdjust.Id, async () =>
        {
            TransitRaw(walletAdjust, StateTypes.WalletAdjustCompleted, operatorPartyId, note);
            await WalletChangeBalanceRawAsync(walletId, walletAdjust.Id, walletAdjust.Amount.ToScaledFromCents());
        });

        return (true, "");
    }

    public async Task<List<WalletAdjust.ClientPageModel>> QueryWalletAdjustForClientAsync(WalletAdjust.ClientCriteria criteria)
    {
        var items = await tenantCtx.WalletAdjusts
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return items;
    }
}