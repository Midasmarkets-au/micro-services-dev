using Bacera.Gateway.ViewModels.Parent;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService
{
    public async Task<bool> AddAccountLogAsync(long id, string action, string before, string after, long operatorPartyId = 1)
    {
        try
        {
            tenantCtx.AccountLogs.Add(new AccountLog
            {
                AccountId = id,
                Action = action,
                Before = before,
                After = after,
                OperatorPartyId = operatorPartyId,
                CreatedOn = DateTime.UtcNow,
            });
            await tenantCtx.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}