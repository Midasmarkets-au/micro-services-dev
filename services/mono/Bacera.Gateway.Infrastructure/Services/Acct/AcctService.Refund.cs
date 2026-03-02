using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public async Task<List<Refund.ClientPageModel>> QueryRefundForClientAsync(Refund.ClientCriteria criteria)
    {
        var items = await tenantCtx.Refunds
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return items;
    }
}