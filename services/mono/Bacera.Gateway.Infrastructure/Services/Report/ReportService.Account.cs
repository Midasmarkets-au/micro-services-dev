using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    public async Task<List<UserAccountTransaction>> AccountRecentReportAsync(long agentUid,
        int count = 5)
    {
        var key = $"report-account-recent-t:{GetTenantId}-agent:{agentUid}";
        var cached = await myCache.GetAsync<List<UserAccountTransaction>>(key);
        if (cached != null)
            return cached;

        var query =
            from client in tenantDbContext.Accounts
            where client.AgentAccount != null && client.AgentAccount.Uid == agentUid
            orderby client.CreatedOn descending
            select new UserAccountTransaction
            {
                MatterId = 0,
                Amount = 0,
                AccountId = client.Id,
                PartyId = client.PartyId,
                CurrencyId = CurrencyTypes.Invalid,
                Account = new Account.SummaryResponseModel
                {
                    Uid = client.Uid,
                    Role = (AccountRoleTypes)client.Role,
                    Type = (AccountTypes)client.Type,
                    HasTradeAccount = client.HasTradeAccount,
                    Status = (AccountStatusTypes)client.Status
                },
            };
        var result = await query.Take(count).ToListAsync();
        await authDbContext.AppendUserInfo(result);
        await myCache.SetAsync(key, result, TimeSpan.FromMinutes(15));
        return result;
    }

    public async Task<int> AccountTodayCreation(long agentUid)
    {
        var key = $"report-account-today-count-t:{GetTenantId}-agent:{agentUid}";
        var cached = await myCache.GetStringAsync(key);
        if (cached != null)
            return int.Parse(cached);

        var result = await (
            from client in tenantDbContext.Accounts
            where client.AgentAccount != null && client.AgentAccount.Uid == agentUid
            where client.CreatedOn >= DateTime.UtcNow.AddDays(-1)
            select client.Id
        ).CountAsync();
        await myCache.SetStringAsync(key, result.ToString(), TimeSpan.FromMinutes(15));
        return result;
    }
}