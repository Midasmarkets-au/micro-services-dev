using Bacera.Gateway.ViewModels.Parent;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService
{
    public async Task<List<Account.ClientPageModel>> QueryAccountForClientAsync(Account.ClientCriteria criteria)
    {
        var items = await tenantCtx.Accounts
            .FilterBy(criteria)
            .ToClientPageModel()
            .Take(20)
            .ToListAsync();

        var configurations = await tenantCtx.Configurations
            .Where(x => x.Category == Configuration.CategoryTypes.Account)
            .Where(x => items.Select(y => y.Id).Contains(x.RowId))
            .Select(x => new { x.RowId, x.DataFormat, x.Key, x.Value })
            .ToListAsync();

        foreach (var item in items)
        {
            var config = configurations.FirstOrDefault(x => x.RowId == item.Id);
            if (config == null) continue;
            item.Configurations.Add(new Configuration.ForAccountModel
            {
                KeyString = config.Key,
                ValueString = config.Value,
                DataFormat = config.DataFormat
            });
        }

        return items;
    }

    public async Task<List<TradeDemoAccount.ClientPageModel>> QueryDemoAccountForClientAsync(TradeDemoAccount.ClientCriteria criteria)
    {
        var items = await tenantCtx.TradeDemoAccounts
            .OrderByDescending(x => x.ExpireOn)
            .FilterBy(criteria)
            .ToClientPageModel()
            .Take(20)
            .ToListAsync();

        return items;
    }

    public async Task<List<AccountForSalesViewModel>> QueryAccountForSalesAsync(long salesId, Account.SalesCriteria criteria)
    {
        var salesAccount = await tenantCtx.Accounts
            .Where(x => x.Id == salesId)
            .Select(x => new { x.PartyId, x.Level, x.Uid })
            .SingleOrDefaultAsync();
        if (salesAccount == null) return [];

        criteria.SalesUid = salesAccount.Uid;
        criteria.QueryLevel = salesAccount.Level;

        if (criteria.ParentAccountUid != null && criteria.ParentAccountUid != salesAccount.Uid)
        {
            var parentAccount = await tenantCtx.Accounts
                .Where(x => x.Uid == criteria.ParentAccountUid)
                .Select(x => new { x.Level, x.ReferPath })
                .SingleOrDefaultAsync();
            if (parentAccount == null) return [];

            if (criteria.MultiLevel == false)
            {
                criteria.ParentAccountLevel = parentAccount.Level;
            }

            var startIndex = parentAccount.ReferPath.IndexOf(salesAccount.Uid.ToString(), StringComparison.Ordinal) +
                             salesAccount.Uid.ToString().Length + 1;

            var path = parentAccount.ReferPath[startIndex..];
            var uids = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();

            var accountsInBetween = await tenantCtx.Accounts
                .Where(x => uids.Contains(x.Uid))
                .OrderBy(x => x.Level)
                .Select(x => new { x.Uid, RelativeLevel = x.Level - criteria.QueryLevel, x.Party.NativeName })
                .ToListAsync();

            criteria.LevelAccountsInBetween = accountsInBetween;
        }

        var items = await tenantCtx.Accounts
            .PagedFilterBy(criteria)
            .ToSalesPageModel(salesAccount.PartyId, salesAccount.Level)
            .ToListAsync();

        return items;
    }
}