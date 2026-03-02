using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

public partial class AccountingService
{
    /// <summary>
    /// Status:
    /// 1: pending
    /// 2: canceled
    /// 3: processing
    /// 4: completed
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="currencyId"></param>
    /// <param name="type"></param>
    /// <param name="stage"></param>
    /// <param name="queryPage"></param>
    /// <param name="queryPageSize"></param>
    /// <returns></returns>
    public async Task<List<TransferView>> TransferViews(long partyId, CurrencyTypes currencyId,
        MatterTypes? type = default,
        int? stage = default,
        int? queryPage = default,
        int? queryPageSize = default)
    {
        var page = queryPage ?? 1;
        var pageSize = queryPageSize ?? 20;
        pageSize = pageSize > 50 ? 50 : pageSize;
        var query = _tenantDbContext.TransferViews
            .Where(x => x.CurrencyId == (int)currencyId)
            .Where(x => x.PartyId == partyId);

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == (int)type);
        }

        if (stage.HasValue)
        {
            query = stage switch
            {
                1 => query.Where(x => StageTypes.Padding.Contains(x.StateId ?? -1)),
                2 => query.Where(x => StageTypes.Canceled.Contains(x.StateId ?? -1)),
                3 => query.Where(x => StageTypes.Processing.Contains(x.StateId ?? -1)),
                4 => query.Where(x => StageTypes.Completed.Contains(x.StateId ?? -1)),
                _ => query
            };
        }

        return await query.OrderByDescending(x => x.PostedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Result<List<TransferView>, TransferView.Criteria>> TransferViewQueryAsync(
        TransferView.Criteria criteria)
    {
        var items = await _tenantDbContext.TransferViews
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Result<List<TransferView>, TransferView.Criteria>.Of(items, criteria);
    }
}