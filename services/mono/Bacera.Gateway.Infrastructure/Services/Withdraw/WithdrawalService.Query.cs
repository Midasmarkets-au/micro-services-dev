using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Withdraw;

public partial class WithdrawalService
{
    public Task<List<Withdrawal.ClientPageModel>> QueryForClientAsync(Withdrawal.ClientCriteria criteria)
        => tenantCtx.Withdrawals.PagedFilterBy(criteria).ToClientPageModel().ToListAsync();
}