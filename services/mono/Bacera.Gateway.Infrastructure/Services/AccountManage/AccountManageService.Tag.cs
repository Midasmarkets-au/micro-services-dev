using Bacera.Gateway.ViewModels.Parent;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService
{
    public async Task<bool> AddAccountTagsAsync(long id, params string[] tagName)
    {
        var account = await QueryById(id).SingleOrDefaultAsync();
        if (account == null) return false;

        foreach (var name in tagName)
        {
            await tagSvc.AddAccountTagAsync(id, name);
        }

        return true;
    }

    public async Task<bool> RemoveAccountTagsAsync(long id, params string[] tagName)
    {
        var account = await QueryById(id).SingleOrDefaultAsync();
        if (account == null) return false;

        foreach (var name in tagName)
        {
            await tagSvc.RemoveAccountTagAsync(id, name);
        }

        return true;
    }

    public async Task SetPipAndCommissionTagAsync(long id)
    {
        var clientRule = await tenantCtx.RebateClientRules
            .Where(x => x.ClientAccountId == id)
            .OrderBy(x => x.Id)
            .Select(x => new { x.DistributionType, x.RebateDirectSchemaId })
            .FirstOrDefaultAsync();

        if (clientRule == null) return;

        var schemaIds = new List<long>();
        if (clientRule.DistributionType == (short)RebateDistributionTypes.Allocation && clientRule.RebateDirectSchemaId != null)
        {
            schemaIds.Add(clientRule.RebateDirectSchemaId.Value);
        }
        else if (clientRule.DistributionType == (short)RebateDistributionTypes.Direct)
        {
            schemaIds = await tenantCtx.RebateDirectRules
                .Where(x => x.SourceTradeAccountId == id)
                .Select(x => x.RebateDirectSchemaId)
                .ToListAsync();
        }
        else
        {
            return;
        }

        if (schemaIds.Count == 0) return;

        var hasPips = await tenantCtx.RebateDirectSchemaItems
            .Where(x => schemaIds.Contains(x.RebateDirectSchemaId))
            .AnyAsync(x => x.Pips > 0);

        var hasCommission = await tenantCtx.RebateDirectSchemaItems
            .Where(x => schemaIds.Contains(x.RebateDirectSchemaId))
            .AnyAsync(x => x.Commission > 0);

        var account = await tenantCtx.Accounts.Where(x => x.Id == id)
            .Include(x => x.Tags)
            .SingleAsync();

        var hasPipsTag = account.Tags.SingleOrDefault(x => x.Name == AccountTagTypes.AddPips);
        var hasCommissionTag = account.Tags.SingleOrDefault(x => x.Name == AccountTagTypes.AddCommission);

        if (!hasPips && hasPipsTag != null)
        {
            account.Tags.Remove(hasPipsTag);
        }

        if (!hasCommission && hasCommissionTag != null)
        {
            account.Tags.Remove(hasCommissionTag);
        }

        var tags = new List<string>();
        if (hasPips && hasPipsTag == null)
        {
            tags.Add(AccountTagTypes.AddPips);
        }

        if (hasCommission && hasCommissionTag == null)
        {
            tags.Add(AccountTagTypes.AddCommission);
        }

        if (tags.Count > 0)
        {
            await AddAccountTagsAsync(id, tags.ToArray());
        }

        await UpdateAccountSearchText(id);
    }
}