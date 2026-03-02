using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

partial class AccountingService
{
    public async Task<ExchangeRate?> GetExchangeRateAsync(CurrencyTypes fromCurrencyId, CurrencyTypes toCurrencyId)
        => await _tenantDbContext.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)fromCurrencyId)
            .Where(x => x.ToCurrencyId == (int)toCurrencyId)
            .SingleOrDefaultAsync();
}