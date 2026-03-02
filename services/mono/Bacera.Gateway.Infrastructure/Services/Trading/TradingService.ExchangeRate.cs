using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

/**
 * Rebate related methods
 */
partial class TradingService
{
    public async Task<Result<List<ExchangeRate.ResponseModel>, ExchangeRate.Criteria>> ExchangeRateQueryAsync(
        ExchangeRate.Criteria criteria)
    {
        var item = await dbContext.ExchangeRates.PagedFilterBy(criteria).ToClientResponseModels().ToListAsync();
        return Result<List<ExchangeRate.ResponseModel>, ExchangeRate.Criteria>.Of(item, criteria);
    }

    public async Task<Result<List<ExchangeRate.BasicViewModel>, ExchangeRate.Criteria>> ExchangeRateForClientQueryAsync(
        ExchangeRate.Criteria criteria)
    {
        var item = await dbContext.ExchangeRates.PagedFilterBy(criteria).ToBasicViewModel().ToListAsync();
        return Result<List<ExchangeRate.BasicViewModel>, ExchangeRate.Criteria>.Of(item, criteria);
    }

    public async Task<ExchangeRate.ResponseModel> ExchangeRateGetResponseModelAsync(long id) =>
        await dbContext.ExchangeRates.Where(x => x.Id == id)
            .ToClientResponseModels()
            .FirstOrDefaultAsync()
        ?? new ExchangeRate.ResponseModel();

    public async Task<ExchangeRate.ResponseModel> ExchangeRateCreateAsync(ExchangeRate.CreateSpec spec)
    {
        var item = ExchangeRate.Build(spec.FromCurrencyId, spec.ToCurrencyId, spec.BuyingRate,
            spec.SellingRate, spec.AdjustRate, spec.Name);
        await dbContext.ExchangeRates.AddAsync(item);
        await dbContext.SaveChangesAsync();
        return await ExchangeRateGetResponseModelAsync(item.Id);
    }

    public async Task<ExchangeRate.ResponseModel> ExchangeRateUpdateAsync(ExchangeRate.UpdateSpec spec,
        long partyId = 0)
    {
        var item = await dbContext.ExchangeRates.SingleOrDefaultAsync(x => x.Id == spec.Id);
        if (item == null)
            return new ExchangeRate.ResponseModel();

        item.BuyingRate = spec.BuyingRate;
        item.SellingRate = spec.SellingRate;
        item.AdjustRate = spec.AdjustRate;
        item.UpdatedOn = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(spec.Name))
            item.Name = spec.Name;

        dbContext.ExchangeRates.Update(item);
        await dbContext.SaveChangesWithAuditAsync(partyId);
        return await ExchangeRateGetResponseModelAsync(item.Id);
    }
}