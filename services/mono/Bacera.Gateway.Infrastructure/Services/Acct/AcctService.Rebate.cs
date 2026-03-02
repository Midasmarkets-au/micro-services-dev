using Bacera.Gateway.MyException;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public Task<(bool, string)> RebateCompleteAsync(long rebateId, long operatorPartyId = 1, string note = "") => ProcessMatterAsync(rebateId,
        async () =>
        {
            var item = await tenantCtx.Rebates
                .Include(x => x.IdNavigation)
                .SingleOrDefaultAsync(x => x.Id == rebateId);
            if (item == null) throw new ProcessMatterException("Rebate not found");

            var receiverAccount = await tenantCtx.Accounts
                .Where(x => x.Id == item.AccountId)
                .Include(x => x.Tags.Where(t => t.Name == "PauseReleaseRebate"))
                .SingleOrDefaultAsync();
            if (receiverAccount == null || receiverAccount.IsClosed != 0 || receiverAccount.Status != (int)AccountStatusTypes.Activate)
            {
                TransitRaw(item, StateTypes.RebateCanceled, operatorPartyId);
                return;
            }


            if (receiverAccount.Tags.Count != 0)
            {
                TransitRaw(item, StateTypes.RebateCanceled, operatorPartyId,
                    $"{note}, Receiver account release paused");
                return;
            }

            var hasReleased = await tenantCtx.WalletTransactions
                .Where(x => x.MatterId == rebateId)
                .Where(x => x.Matter.Rebate != null && x.Matter.Rebate!.Amount == item.Amount)
                .AnyAsync();

            if (hasReleased)
            {
                TransitRaw(item, StateTypes.RebateCompleted, operatorPartyId, note);
                return;
            }


            var walletId = receiverAccount.WalletId;
            if (walletId == null)
            {
                walletId = await tenantCtx.Wallets
                .Where(x => x.PartyId == receiverAccount.PartyId)
                .Where(x => x.CurrencyId == receiverAccount.CurrencyId && x.FundType == receiverAccount.FundType)
                .Select(x => x.Id)
                .SingleOrDefaultAsync();
                
                if (walletId == null)
                {
                    throw new ProcessMatterException($"No wallet found for account {item.AccountId}");
                }
                
                // Update account in separate operation to avoid tracking conflicts
                await tenantCtx.Database.ExecuteSqlRawAsync(
                    "UPDATE acct.\"_Account\" SET \"WalletId\" = {0} WHERE \"Id\" = {1}",
                    walletId.Value, receiverAccount.Id);
            }

            await WalletChangeBalanceRawAsync(walletId.Value, rebateId, item.Amount);
            TransitRaw(item, StateTypes.RebateCompleted, operatorPartyId, note);
        });

    public async Task<List<Rebate.ClientPageModel>> QueryRebateForClientAsync(Rebate.ClientCriteria criteria)
    {
        var items = await tenantCtx.Rebates
            .Include(x => x.TradeRebate)
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return items;
    }
}