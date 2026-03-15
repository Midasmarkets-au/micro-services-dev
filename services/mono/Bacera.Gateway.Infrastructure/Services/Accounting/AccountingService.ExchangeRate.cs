using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

partial class AccountingService
{
    public async Task<ExchangeRate?> GetExchangeRateAsync(CurrencyTypes fromCurrencyId, CurrencyTypes toCurrencyId)
        => await _tenantDbContext.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)fromCurrencyId)
            .Where(x => x.ToCurrencyId == (int)toCurrencyId)
            .SingleOrDefaultAsync();

    /// <summary>
    /// Pre-fetches exchange rates for a set of currencies to a common target currency in a single DB query.
    /// Returns a dictionary keyed by the source CurrencyTypes for O(1) lookup in loops.
    /// </summary>
    private async Task<Dictionary<CurrencyTypes, ExchangeRate>> GetExchangeRatesForCurrenciesAsync(
        IEnumerable<CurrencyTypes> fromCurrencies, CurrencyTypes toCurrency)
    {
        var fromIds = fromCurrencies
            .Where(c => c != toCurrency)
            .Select(c => (int)c)
            .Distinct()
            .ToList();

        if (fromIds.Count == 0)
            return new Dictionary<CurrencyTypes, ExchangeRate>();

        var rates = await _tenantDbContext.ExchangeRates
            .Where(x => fromIds.Contains(x.FromCurrencyId) && x.ToCurrencyId == (int)toCurrency)
            .ToListAsync();

        return rates.ToDictionary(r => (CurrencyTypes)r.FromCurrencyId);
    }
}