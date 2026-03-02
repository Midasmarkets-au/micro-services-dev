using Bacera.Gateway.Auth;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    public async Task<List<UserAccountTransaction>> DepositRecentReportAsync(long agentUid, int count = 5)
    {
        var referPath = await tenantDbContext.Accounts
            .Where(x => x.Uid == agentUid)
            .Select(x => x.ReferPath)
            .FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(referPath))
            return new List<UserAccountTransaction>();

        var deposits = await tenantDbContext.Deposits
            .Where(x => x.TargetAccount != null)
            .Where(x => x.TargetAccount!.ReferPath.StartsWith(referPath))
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCompleted)
            .OrderByDescending(x => x.Id)
            .Take(count)
            .Select(x => new UserAccountTransaction
            {
                MatterId = x.Id,
                MatterType = x.IdNavigation.Type,
                PartyId = x.PartyId,
                AccountId = x.TargetAccount!.Id,
                Amount = x.Amount,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                Account = new Account.SummaryResponseModel
                {
                    Uid = x.TargetAccount!.Uid,
                    Role = (AccountRoleTypes)x.TargetAccount!.Role,
                    Type = (AccountTypes)x.TargetAccount!.Type,
                    HasTradeAccount = true,
                }
            })
            .ToListAsync();
        await FulfillUsersAsync(deposits);
        return deposits;
    }

    public async Task<List<KeyValuePair<int, long>>> DepositTodayValueAsync(long agentUid)
    {
        var referPath = await tenantDbContext.Accounts
            .Where(x => x.Uid == agentUid)
            .Select(x => x.ReferPath)
            .FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(referPath))
            return new List<KeyValuePair<int, long>>();

        var result = await tenantDbContext.Deposits
            .Where(x => x.TargetAccount != null)
            .Where(x => x.TargetAccount!.ReferPath.StartsWith(referPath))
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCompleted)
            .Where(x => x.IdNavigation.StatedOn >= DateTime.UtcNow.AddDays(-1))
            .GroupBy(x => x.CurrencyId)
            .Select(x => new KeyValuePair<int, long>(x.Key, x.Sum(y => y.Amount)))
            .ToListAsync();
        return result;
    }

    private async Task FulfillUsersAsync<T>(
        List<T> items) where T : class, IUserInfoAppendable, new()
    {
        var users = await authDbContext.Users
            .Where(x => items.Select(a => a.PartyId).Contains(x.PartyId) && x.TenantId == tenantGetter.GetTenantId())
            .ToUserInfo()
            .ToListAsync();

        foreach (var item in items)
        {
            var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
            if (user != null) item.User = user;
        }
    }

    public class UserAccountTransaction : IUserInfoAppendable
    {
        public long MatterId { get; set; }
        public int MatterType { get; set; }
        [JsonIgnore] public long PartyId { get; set; }
        public long AccountId { get; set; }
        public long Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public Account.SummaryResponseModel Account { get; set; } = null!;
        public virtual UserInfo User { get; set; } = null!;
    }
}