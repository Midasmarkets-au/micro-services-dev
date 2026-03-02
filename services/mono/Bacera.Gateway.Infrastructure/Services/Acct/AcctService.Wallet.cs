using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public async Task<WalletDailySnapshot?> GenerateWalletDailySnapshotAsync(long walletId, DateTime date, bool replace = false)
    {
        var toTime = date.Date.AddHours(Utils.IsCurrentDSTLosAngeles(date) ? 21 : 22);
        var latestWalletTransaction = await tenantCtx.WalletTransactions
            .Where(x => x.WalletId == walletId && x.CreatedOn < toTime)
            .OrderByDescending(x => x.CreatedOn)
            .ThenByDescending(x => x.Id)
            .Select(x => new { x.PrevBalance, x.Amount })
            .FirstOrDefaultAsync();
        if (latestWalletTransaction == null) return null;

        if (replace)
        {
            var existing = await tenantCtx.WalletDailySnapshots
                .Where(x => x.WalletId == walletId && x.SnapshotDate == toTime)
                .FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Balance = latestWalletTransaction.PrevBalance + latestWalletTransaction.Amount;
                await tenantCtx.SaveChangesAsync();
                return existing;
            }
        }

        var item = new WalletDailySnapshot
        {
            WalletId = walletId,
            Balance = latestWalletTransaction.PrevBalance + latestWalletTransaction.Amount,
            SnapshotDate = toTime,
        };

        tenantCtx.WalletDailySnapshots.Add(item);
        await tenantCtx.SaveChangesAsync();
        return item;
    }
}