using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Vendor.ExLinkCashier;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class PaymentMethodService(
    TenantDbContext ctx,
    ITenantGetter tenantGetter,
    IMyCache cache,
    ConfigService configSvc,
    AccountManageService accManageSvc,
    IHttpClientFactory clientFactory,
    ILogger<PaymentMethodService> logger)
{
    private readonly long _tenantId = tenantGetter.GetTenantId();

    public async Task<List<PaymentMethod>> GetMethodsAsync(bool fromDb = false)
    {
        var query = ctx.PaymentMethods
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .AsNoTracking();

        return fromDb
            ? await query.ToListAsync()
            : await cache.GetOrSetAsync(CacheKeys.GetPaymentMethodKey(_tenantId)
                , () => query.ToListAsync()
            , TimeSpan.FromDays(1));
    }

    public async Task<bool> MethodExistByIdAsync(long id)
    {
        var methods = await GetMethodsAsync();
        return methods.Any(x => x.Id == id);
    }

    public Task<List<long>> GetWalletAccessIdsAsync(long walletId, string? methodTypes = null,
        PaymentMethodStatusTypes? status = null,
        PaymentMethodAccessStatusTypes? accessStatus = null, string? group = null)
        => GetWalletAccessQuery(walletId, methodTypes, status, accessStatus, group)
            .Select(x => x.PaymentMethodId)
            .Distinct()
            .ToListAsync();

    public async Task<List<long>> GetAccountAccessIdsAsync(long accountId, string? methodTypes = null,
        PaymentMethodStatusTypes? status = null,
        PaymentMethodAccessStatusTypes? accessStatus = null, string? group = null)
    {
        var paymentMethods = await GetAccountAccessQuery(accountId, methodTypes, status, accessStatus, group)
            .Select(x => new { x.PaymentMethodId, x.PaymentMethod.Sort } )
            .OrderByDescending(x => x.Sort > 0)  // Sort > 0 items first
            .ThenBy(x => x.Sort > 0 ? x.Sort : int.MaxValue)  // Among Sort > 0, order by Sort ASC
            .ThenBy(x => x.PaymentMethodId)  // For Sort <= 0, order by PaymentMethodId
            .ToListAsync();

        return paymentMethods
            .Select(x => x.PaymentMethodId)
            .Distinct()
            .ToList();
    }

    public Task<bool> IsAccountAccessEnabledAsync(long accountId, long methodId)
        => GetAccountAccessQuery(accountId).AnyAsync(x =>
            x.PaymentMethodId == methodId && x.Status == (short)PaymentMethodAccessStatusTypes.Active);

    public Task<bool> IsWalletAccessEnabledAsync(long walletId, long methodId)
        => GetWalletAccessQuery(walletId).AnyAsync(x =>
            x.PaymentMethodId == methodId && x.Status == (short)PaymentMethodAccessStatusTypes.Active);

    public async Task<bool> IsAmountValidAsync(long methodId, long accountId, long amount)
    {
        var amountInDecimal = (amount / 100m).ToCentsFromScaled();
        var firstDeposit = await accManageSvc.AccountHasCompleteDepositByIdAsync(accountId) == false;
        var methods = await GetMethodsAsync();
        var method = methods.SingleOrDefault(x => x.Id == methodId);
        if (method == null) return false;
        
        if (firstDeposit) return amountInDecimal >= method.InitialValue && amountInDecimal <= method.MaxValue;
        return amountInDecimal >= method.MinValue && amountInDecimal <= method.MaxValue;
    }

    public async Task<bool> EnableAccountAccessByMethodIdAsync(long accountId, long methodId, long operatorPartyId = 1)
    {
        var methods = await GetMethodsAsync();
        if (methods.All(x => x.Id != methodId)) return false;

        var item = await ctx.AccountPaymentMethodAccesses.SingleOrDefaultAsync(x =>
            x.PaymentMethodId == methodId && x.AccountId == accountId);
        if (item == null)
        {
            item = new AccountPaymentMethodAccess
            {
                AccountId = accountId,
                PaymentMethodId = methodId,
                OperatedPartyId = operatorPartyId,
                Status = (short)PaymentMethodAccessStatusTypes.Active
            };
            ctx.AccountPaymentMethodAccesses.Add(item);
        }
        else
        {
            item.Status = (short)PaymentMethodAccessStatusTypes.Active;
            item.OperatedPartyId = operatorPartyId;
            ctx.AccountPaymentMethodAccesses.Update(item);
        }

        item.UpdatedOn = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableWalletAccessByMethodIdAsync(long walletId, long methodId, long operatorPartyId = 1)
    {
        var methods = await GetMethodsAsync();
        if (methods.All(x => x.Id != methodId)) return false;

        var item = await ctx.WalletPaymentMethodAccesses.SingleOrDefaultAsync(x =>
            x.PaymentMethodId == methodId && x.WalletId == walletId);
        if (item == null)
        {
            item = new WalletPaymentMethodAccess
            {
                WalletId = walletId,
                PaymentMethodId = methodId,
                OperatedPartyId = operatorPartyId,
                Status = (short)PaymentMethodAccessStatusTypes.Active
            };
            ctx.WalletPaymentMethodAccesses.Add(item);
        }
        else
        {
            item.Status = (short)PaymentMethodAccessStatusTypes.Active;
            item.OperatedPartyId = operatorPartyId;
            ctx.WalletPaymentMethodAccesses.Update(item);
        }

        item.UpdatedOn = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableAccountAccessByMethodIdAsync(long accountId, long methodId)
    {
        var methods = await GetMethodsAsync();
        if (methods.All(x => x.Id != methodId)) return false;
        var item = await ctx.AccountPaymentMethodAccesses.SingleOrDefaultAsync(x =>
            x.PaymentMethodId == methodId && x.AccountId == accountId);
        if (item == null) return false;
        item.Status = (short)PaymentMethodAccessStatusTypes.Inactive;
        item.UpdatedOn = DateTime.UtcNow;
        ctx.AccountPaymentMethodAccesses.Update(item);
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableWalletAccessByMethodIdAsync(long walletId, long methodId)
    {
        var methods = await GetMethodsAsync();
        if (methods.All(x => x.Id != methodId)) return false;
        var item = await ctx.WalletPaymentMethodAccesses.SingleOrDefaultAsync(x =>
            x.PaymentMethodId == methodId && x.WalletId == walletId);
        if (item == null) return false;
        item.Status = (short)PaymentMethodAccessStatusTypes.Inactive;
        item.UpdatedOn = DateTime.UtcNow;
        ctx.WalletPaymentMethodAccesses.Update(item);
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableAccountAccessByGroupAsync(long accountId, string group, long operatorPartyId = 1)
    {
        var existing = await GetAccountAccessQuery(accountId, group: group).ToListAsync();
        existing.ForEach(x =>
        {
            x.Status = (short)PaymentMethodAccessStatusTypes.Active;
            x.OperatedPartyId = operatorPartyId;
        });
        ctx.AccountPaymentMethodAccesses.UpdateRange(existing);
        await ctx.SaveChangesAsync();

        var methods = await GetMethodsAsync();
        var existingMethodIds = existing.Select(x => x.PaymentMethodId).ToList();
        var items = methods
            .Where(x => x.Group == group && !existingMethodIds.Contains(x.Id))
            .Select(x => new AccountPaymentMethodAccess
            {
                AccountId = accountId,
                PaymentMethodId = x.Id,
                Status = (short)PaymentMethodAccessStatusTypes.Active,
                OperatedPartyId = x.OperatorPartyId
            }).ToList();

        ctx.AccountPaymentMethodAccesses.AddRange(items);
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableAccountAccessByGroupAsync(long accountId, string group, long operatorPartyId = 1)
    {
        var existing = await GetAccountAccessQuery(accountId, group: group).ToListAsync();
        existing.ForEach(x =>
        {
            x.Status = (short)PaymentMethodAccessStatusTypes.Inactive;
            x.OperatedPartyId = operatorPartyId;
        });
        ctx.AccountPaymentMethodAccesses.UpdateRange(existing);
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableWalletAccessByGroupAsync(long walletId, string group)
    {
        var existing = await GetWalletAccessQuery(walletId, group: group).ToListAsync();
        existing.ForEach(x => x.Status = (short)PaymentMethodAccessStatusTypes.Active);

        var methods = await GetMethodsAsync();
        var items = methods
            .Where(x => x.Group == group && !existing.Select(y => y.PaymentMethodId).Contains(x.Id))
            .Select(x => new WalletPaymentMethodAccess
            {
                WalletId = walletId,
                PaymentMethodId = x.Id,
                Status = (short)PaymentMethodAccessStatusTypes.Active
            }).ToList();

        ctx.WalletPaymentMethodAccesses.AddRange(items);
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableWalletAccessByGroupAsync(long walletId, string group)
    {
        var existing = await GetWalletAccessQuery(walletId, group: group).ToListAsync();
        existing.ForEach(x => x.Status = (short)PaymentMethodAccessStatusTypes.Inactive);
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<PaymentMethod?> GetActiveDepositMethodRandomlyAsync(string group, long accountId)
    {
        var methods = await GetMethodsAsync();
        var available = await GetAccountAccessIdsAsync(accountId, PaymentMethodTypes.Deposit,
            PaymentMethodStatusTypes.Active,
            PaymentMethodAccessStatusTypes.Active, group);
        var item = methods
            .Where(x => available.Contains(x.Id))
            .ToList();

        var percentageSum = item.Sum(x => x.Percentage);
        var random = new Random().Next(1, percentageSum + 1);
        var sum = 0;
        foreach (var i in item)
        {
            sum += i.Percentage;
            if (random <= sum) return i;
        }

        return item.FirstOrDefault();
    }

    public async Task<long[]> GetRangeByGroupAsync(string group, long accountId)
    {
        var methods = await GetMethodsAsync();
        var available = await GetAccountAccessIdsAsync(accountId, PaymentMethodTypes.Deposit,
            PaymentMethodStatusTypes.Active,
            PaymentMethodAccessStatusTypes.Active, group);

        var item = methods
            .Where(x => available.Contains(x.Id))
            .GroupBy(x => x.Group)
            .Select(x => new
            {
                Ids = x.Select(y => y.Id).ToList(),
                MinInitialValue = x.Min(y => y.InitialValue),
                MaxInitialValue = x.Max(y => y.InitialValue),
                MinMinValue = x.Min(y => y.MinValue),
                MaxMaxValue = x.Any(y => y.IsHighDollarEnabled == 1)
                    ? x.Where(y => y.IsHighDollarEnabled == 1).Max(y => y.MaxValue)
                    : x.Max(y => y.MaxValue),
            })
            .FirstOrDefault();
        if (item == null) return [];
        var hasDeposit = await accManageSvc.AccountHasCompleteDepositByIdAsync(accountId);
        var range = new[] { hasDeposit ? item.MinMinValue * 100 : item.MinInitialValue * 100, item.MaxMaxValue * 100 };
        return range;
    }

    public async Task<decimal> GetSellingExchangeRateAsync(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1;
        
        // Handle USC conversions using the common helper
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(from, to);
        if (isUscConversion)
        {
            if (uscRate == -1m)
            {
                // Multi-step USC conversion needed
                return await CurrencyConversionHelper.CalculateUscConversionRateAsync(from, to, GetSellingExchangeRateAsync);
            }
            return uscRate;
        }
        
        var item = await ctx.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)from)
            .Where(x => x.ToCurrencyId == (int)to)
            .Select(x => new { x.SellingRate, x.AdjustRate })
            .SingleOrDefaultAsync();
        if (item == null) return -1;
        return Math.Ceiling(item.SellingRate * (1 + item.AdjustRate / 100) * 1000) / 1000;
    }

    /// <summary>
    /// Get exchange rate for ExLink Cashier - tries ExLink API first, then falls back to database
    /// This is specifically for ExLink multi-currency deposits
    /// </summary>
    public async Task<decimal> GetExLinkCashierExchangeRateAsync(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1;
        
        // Handle USC conversions first using the common helper (1 USC = 0.01 USD)
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(from, to);
        if (isUscConversion)
        {
            if (uscRate == -1m)
            {
                // Multi-step USC conversion needed (e.g., USC -> USD -> VND)
                return await CurrencyConversionHelper.CalculateUscConversionRateAsync(from, to, GetExLinkCashierExchangeRateAsync);
            }
            return uscRate;
        }

        // Try ExLink API first (for non-USC conversions)
        var exLinkRate = await TryGetExLinkExchangeRateAsync(from, to);
        if (exLinkRate > 0)
        {
            logger.LogInformation("PaymentMethodService.GetExLinkCashierExchangeRateAsync - Using ExLink API rate: {From} -> {To} = {Rate}", 
                from, to, exLinkRate);
            return exLinkRate;
        }

        // Fallback to database
        logger.LogInformation("PaymentMethodService.GetExLinkCashierExchangeRateAsync - ExLink API failed, falling back to database");
        return await GetSellingExchangeRateAsync(from, to);
    }

    /// <summary>
    /// Get exchange rate for ExLink Cashier withdrawals - tries ExLink API first, then falls back to database buying rate
    /// Uses the same ExLink API logic as deposits, but falls back to buying rate instead of selling rate
    /// </summary>
    public async Task<decimal> GetExLinkCashierBuyingExchangeRateAsync(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1;
        
        // Handle USC conversions first using the common helper (1 USC = 0.01 USD)
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(from, to);
        if (isUscConversion)
        {
            if (uscRate == -1m)
            {
                // Multi-step USC conversion needed (e.g., USC -> USD -> VND)
                return await CurrencyConversionHelper.CalculateUscConversionRateAsync(from, to, GetExLinkCashierBuyingExchangeRateAsync);
            }
            return uscRate;
        }

        // Try ExLink API first (for non-USC conversions) - uses same logic as deposits
        var exLinkRate = await TryGetExLinkExchangeRateAsync(from, to);
        if (exLinkRate > 0)
        {
            logger.LogInformation("PaymentMethodService.GetExLinkCashierBuyingExchangeRateAsync - Using ExLink API rate: {From} -> {To} = {Rate}", 
                from, to, exLinkRate);
            return exLinkRate;
        }

        // Fallback to database buying rate (for withdrawals)
        logger.LogInformation("PaymentMethodService.GetExLinkCashierBuyingExchangeRateAsync - ExLink API failed, falling back to database buying rate");
        return await GetBuyingExchangeRateAsync(from, to);
    }

    /// <summary>
    /// Try to get exchange rate from ExLink API
    /// ExLink API returns USDT-based rates (e.g., VND/USDT, THB/USDT)
    /// </summary>
    private async Task<decimal> TryGetExLinkExchangeRateAsync(CurrencyTypes from, CurrencyTypes to)
    {
        try
        {
            // Get ExLink configuration from any active ExLinkCashier payment method
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null && x.IsDeleted == false)
                .FirstOrDefaultAsync();

            if (exLinkMethod == null)
            {
                logger.LogDebug("PaymentMethodService.TryGetExLinkExchangeRateAsync - No active ExLink payment method found");
                return -1;
            }

            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            if (string.IsNullOrEmpty(options.Uid) || string.IsNullOrEmpty(options.SecretKey))
            {
                logger.LogWarning("PaymentMethodService.TryGetExLinkExchangeRateAsync - Invalid ExLink configuration");
                return -1;
            }

            var client = clientFactory.CreateClient();
            var rateResponse = await ExLinkCashier.QueryExchangeRateAsync(options.Uid, options.SecretKey, client, logger);

            if (rateResponse?.Data?.MarketPriceList == null || rateResponse.Data.MarketPriceList.Count == 0)
            {
                logger.LogDebug("PaymentMethodService.TryGetExLinkExchangeRateAsync - No exchange rates returned from ExLink API");
                return -1;
            }

            // ExLink returns rates like: VND/USDT, THB/USDT, etc. as a list
            // All rates are quoted against USDT (targetCoinId = 4)
            // Note: USC is handled at the GetExLinkCashierExchangeRateAsync level, so we won't see it here
            var fromCurrencyName = GetCurrencyNameForExLink(from);
            var toCurrencyName = GetCurrencyNameForExLink(to);

            if (string.IsNullOrEmpty(fromCurrencyName) || string.IsNullOrEmpty(toCurrencyName))
            {
                logger.LogDebug("PaymentMethodService.TryGetExLinkExchangeRateAsync - Currency not supported: {From} or {To}", 
                    from, to);
                return -1;
            }

            var marketPriceList = rateResponse.Data.MarketPriceList;
            
            // Handle USD → Local Currency (e.g., USD → VND)
            // Since USD ≈ USDT (1:1), we just need the target currency's rate
            if (from == CurrencyTypes.USD)
            {
                var toRate = marketPriceList.FirstOrDefault(x => 
                    x.SourceCoinName.Equals(toCurrencyName, StringComparison.OrdinalIgnoreCase));
                
                if (toRate == null || toRate.MarketOutPrice == 0)
                {
                    logger.LogDebug("PaymentMethodService.TryGetExLinkExchangeRateAsync - Missing rate for {To}", 
                        toCurrencyName);
                    return -1;
                }
                
                // 1 USD = 1 USDT = toRate.MarketOutPrice in target currency
                var rate = toRate.MarketOutPrice;
                logger.LogInformation("PaymentMethodService.TryGetExLinkExchangeRateAsync - USD -> {To}: rate = {Rate}",
                    toCurrencyName, rate);
                return rate;
            }
            
            // Handle Local Currency → USD (e.g., VND → USD)
            // We need to invert the local currency's rate
            if (to == CurrencyTypes.USD)
            {
                var fromRate = marketPriceList.FirstOrDefault(x => 
                    x.SourceCoinName.Equals(fromCurrencyName, StringComparison.OrdinalIgnoreCase));
                
                if (fromRate == null || fromRate.MarketInPrice == 0)
                {
                    logger.LogDebug("PaymentMethodService.TryGetExLinkExchangeRateAsync - Missing rate for {From}", 
                        fromCurrencyName);
                    return -1;
                }
                
                // 1 FromCurrency = (1 / fromRate.MarketInPrice) USDT ≈ (1 / fromRate.MarketInPrice) USD
                var rate = 1m / fromRate.MarketInPrice;
                logger.LogInformation("PaymentMethodService.TryGetExLinkExchangeRateAsync - {From} -> USD: rate = {Rate}",
                    fromCurrencyName, rate);
                return rate;
            }
            
            // Handle Local Currency → Local Currency (e.g., VND → THB)
            // Calculate cross rate via USDT
            var fromRateCross = marketPriceList.FirstOrDefault(x => 
                x.SourceCoinName.Equals(fromCurrencyName, StringComparison.OrdinalIgnoreCase));
            var toRateCross = marketPriceList.FirstOrDefault(x => 
                x.SourceCoinName.Equals(toCurrencyName, StringComparison.OrdinalIgnoreCase));
            
            if (fromRateCross == null || toRateCross == null)
            {
                logger.LogDebug("PaymentMethodService.TryGetExLinkExchangeRateAsync - Missing rates for {From} or {To}", 
                    fromCurrencyName, toCurrencyName);
                return -1;
            }

            // Calculate cross rate: FROM → USDT → TO
            // 1 FROM = (1 / fromRate.MarketInPrice) USDT = (1 / fromRate.MarketInPrice) * toRate.MarketOutPrice TO
            if (fromRateCross.MarketInPrice == 0 || toRateCross.MarketOutPrice == 0)
            {
                logger.LogWarning("PaymentMethodService.TryGetExLinkExchangeRateAsync - Invalid rate values: fromRate={FromRate}, toRate={ToRate}", 
                    fromRateCross.MarketInPrice, toRateCross.MarketOutPrice);
                return -1;
            }

            var crossRate = toRateCross.MarketOutPrice / fromRateCross.MarketInPrice;
            
            logger.LogInformation("PaymentMethodService.TryGetExLinkExchangeRateAsync - Cross rate: {From}({FromRate}) -> {To}({ToRate}) = {CrossRate}",
                fromCurrencyName, fromRateCross.MarketInPrice, toCurrencyName, toRateCross.MarketOutPrice, crossRate);

            return crossRate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PaymentMethodService.TryGetExLinkExchangeRateAsync - Error: {Message}", ex.Message);
            return -1;
        }
    }

    /// <summary>
    /// Map CurrencyTypes enum to ExLink API currency names
    /// Note: USC is handled separately via CurrencyConversionHelper (1 USC = 0.01 USD)
    /// </summary>
    private static string? GetCurrencyNameForExLink(CurrencyTypes currency)
    {
        return currency switch
        {
            CurrencyTypes.USD => "USD",   // USD is treated as USDT (1:1) for ExLink rates
            CurrencyTypes.VND => "VND",
            CurrencyTypes.THB => "THB",
            CurrencyTypes.IDR => "IDR",
            CurrencyTypes.INR => "INR",
            CurrencyTypes.MXN => "MXN",
            CurrencyTypes.KRW => "KRW",
            CurrencyTypes.JPY => "JPY",
            CurrencyTypes.BRL => "BRL",
            CurrencyTypes.PHP => "PHP",
            CurrencyTypes.MYR => "MYR",
            _ => null  // USC and other currencies not directly supported by ExLink
        };
    }

    public async Task<List<PaymentMethodDTO.CurrencyRate>> GetSellingExchangeRatesAsync(CurrencyTypes from,
        HashSet<CurrencyTypes> tos)
    {
        var sameCurrency = tos.FirstOrDefault(x => x == from);
        var items = await ctx.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)from)
            .Where(x => tos.Contains((CurrencyTypes)x.ToCurrencyId))
            .Select(x => new { x.ToCurrencyId, x.SellingRate, x.AdjustRate })
            .ToListAsync();
        var result = items.Select(x => new PaymentMethodDTO.CurrencyRate
        {
            CurrencyId = (CurrencyTypes)x.ToCurrencyId,
            Rate = Math.Ceiling(x.SellingRate * (1 + x.AdjustRate / 100) * 1000) / 1000
        }).ToList();
        
        if (sameCurrency != 0)
        {
            result.Add(new PaymentMethodDTO.CurrencyRate
            {
                CurrencyId = sameCurrency,
                Rate = 1
            });
        }

        return result;
    }

    public async Task<decimal> GetBuyingExchangeRateAsync(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1;
        
        // Handle USC conversions using the common helper
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(from, to);
        if (isUscConversion)
        {
            if (uscRate == -1m)
            {
                // Multi-step USC conversion needed
                return await CurrencyConversionHelper.CalculateUscConversionRateAsync(from, to, GetBuyingExchangeRateAsync);
            }
            return uscRate;
        }
        
        var item = await ctx.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)from)
            .Where(x => x.ToCurrencyId == (int)to)
            .Select(x => new { x.BuyingRate, x.AdjustRate })
            .SingleOrDefaultAsync();
        if (item == null) return -1;
        var exchangeRate = item.BuyingRate * (1 - item.AdjustRate / 100);
        return Math.Floor(exchangeRate * 1000) / 1000;
    }

    public async Task<List<PaymentMethodDTO.CurrencyRate>> GetBuyingExchangeRatesAsync(CurrencyTypes from,
        HashSet<CurrencyTypes> tos)
    {
        var items = await ctx.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)from)
            .Where(x => tos.Contains((CurrencyTypes)x.ToCurrencyId))
            .Select(x => new { x.ToCurrencyId, x.BuyingRate, x.AdjustRate })
            .ToListAsync();
        return items.Select(x => new PaymentMethodDTO.CurrencyRate
        {
            CurrencyId = (CurrencyTypes)x.ToCurrencyId,
            Rate = Math.Floor(x.BuyingRate * (1 - x.AdjustRate / 100) * 1000) / 1000
        }).ToList();
    }

    public Task<string> GetInstructionAsync(PaymentMethod method, string language)
        => GetInstructionByIdAsync(method.Id, language);

    public Task<string> GetPolicyAsync(PaymentMethod method, string language)
        => method.Group == "Union Pay"
            ? GetUnionPayPolicyAsync(language)
            : GetPolicyByIdAsync(method.Id, language);

    public async Task<Dictionary<string, string>> GetInstructionByIdAsync(long id)
    {
        var rawItem = await ctx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
            .Where(x => x.RowId == id)
            .Select(x => x.Data)
            .SingleOrDefaultAsync();
        if (rawItem == null) return new Dictionary<string, string>();
        var item = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, string>>(rawItem);
        return item;
    }

    // public async Task<Dictionary<string, string>> GetUnionPayInstructionAsync()
    // {
    //     var methods = await GetMethodsAsync();
    //     var ids = methods.Where(x => x.Group == "Union Pay").Select(x => x.Id).ToList();
    //     var rawItem = await ctx.Supplements
    //         .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
    //         .Where(x => ids.Contains(x.RowId))
    //         .OrderByDescending(x => x.Id)
    //         .Select(x => x.Data)
    //         .FirstOrDefaultAsync();
    //     if (rawItem == null) return new Dictionary<string, string>();
    //     var item = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, string>>(rawItem);
    //     return item;
    // }

    public async Task<(bool, string)> UpdateInstructionAsync(long id, Dictionary<string, string> dictionary)
    {
        var method = await GetMethodByIdAsync(id);
        if (method == null) return (false, "Method not found");

        var rawItem = await ctx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync();

        rawItem ??= Supplement.Build(SupplementTypes.PaymentServiceInstruction, id);
        rawItem.Data = Utils.JsonSerializeObject(dictionary);

        if (rawItem.Id == 0)
        {
            ctx.Supplements.Add(rawItem);
        }
        else
        {
            ctx.Supplements.Update(rawItem);
        }

        await ctx.SaveChangesAsync();
        return (true, "");
    }

    public async Task<Dictionary<string, string>> GetPolicyByIdAsync(long id)
    {
        var rawItem = await ctx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
            .Where(x => x.RowId == id)
            .Select(x => x.Data)
            .SingleOrDefaultAsync();
        if (rawItem == null) return new Dictionary<string, string>();
        var item = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, string>>(rawItem);
        return item;
    }

    public async Task<(bool, string)> UpdatePolicyAsync(long id, Dictionary<string, string> dictionary)
    {
        var method = await GetMethodByIdAsync(id);
        if (method == null) return (false, "Method not found");

        Supplement? rawItem;
        if (method.Group == "Union Pay")
        {
            var methods = await GetMethodsAsync();
            var ids = methods.Where(x => x.Group == "Union Pay").Select(x => x.Id).ToList();
            rawItem = await ctx.Supplements
                .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
                .Where(x => ids.Contains(x.RowId))
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }
        else
        {
            rawItem = await ctx.Supplements
                .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
                .Where(x => x.RowId == id)
                .SingleOrDefaultAsync();
        }

        rawItem ??= Supplement.Build(SupplementTypes.PaymentServicePolicy, id);
        rawItem.Data = Utils.JsonSerializeObject(dictionary);
        ctx.Supplements.Update(rawItem);
        await ctx.SaveChangesAsync();
        return (true, "");
    }

    public async Task<Dictionary<string, string>> GetUnionPayPolicyAsync()
    {
        var methods = await GetMethodsAsync();
        var ids = methods
            .Where(x => x is { Group: "Union Pay", MethodType: PaymentMethodTypes.Deposit })
            .Select(x => x.Id).ToList();
        var rawItem = await ctx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
            .Where(x => ids.Contains(x.RowId))
            .OrderByDescending(x => x.Id)
            .Select(x => x.Data)
            .FirstOrDefaultAsync();
        if (rawItem == null) return new Dictionary<string, string>();
        var item = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, string>>(rawItem);
        return item;
    }

    private async Task<string> GetInstructionByIdAsync(long id, string language)
    {
        var item = await GetInstructionByIdAsync(id);
        return item.GetValueOrDefault(language, item.GetValueOrDefault(LanguageTypes.English, ""));
    }

    private async Task<string> GetPolicyByIdAsync(long id, string language)
    {
        var item = await GetPolicyByIdAsync(id);
        return item.GetValueOrDefault(language, item.GetValueOrDefault(LanguageTypes.English, ""));
    }

    public async Task<string> GetUnionPayPolicyAsync(string language)
    {
        var item = await GetUnionPayPolicyAsync();
        return item.GetValueOrDefault(language, item.GetValueOrDefault(LanguageTypes.English, ""));
    }

    public async Task<PaymentMethod?> GetGroupMethodRandomlyAsync(long accountId, long amount, string group)
    {
        var amountInDecimal = amount / 100m;
        var hasDeposits = await ctx.Deposits
            .Where(x => x.TargetAccountId == accountId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCompleted)
            .AnyAsync();

        var siteId = await ctx.Accounts
            .Where(x => x.Id == accountId)
            .Select(x => x.SiteId)
            .SingleOrDefaultAsync();
        
        var highDollarValue = await configSvc.GetHighDollarValueAsync(siteId);

        var methods = await ctx.PaymentMethods
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.Group == group && x.MethodType == PaymentMethodTypes.Deposit)
            .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
            .Where(x => x.AccountPaymentMethodAccesses.Any(y =>
                y.AccountId == accountId && y.Status == (short)PaymentMethodAccessStatusTypes.Active))
            .Where(x => hasDeposits ? amountInDecimal >= x.MinValue : amountInDecimal >= x.InitialValue)
            .Select(x => new { x.Id, x.Percentage, CanHighDollar = x.IsHighDollarEnabled == 1 })
            .ToListAsync();

        if (methods.Count == 0) return null;
        
        var useHighDollar = amountInDecimal >= highDollarValue;
        var highDollarMethods = methods.Where(x => x.CanHighDollar).ToList();
        var normalMethods = methods.Where(x => !x.CanHighDollar).ToList();
        
        if (useHighDollar && highDollarMethods.Count > 0)
        {
            methods = highDollarMethods;
        }
        else if (!useHighDollar && normalMethods.Count > 0)
        {
            methods = normalMethods;
        }
        
        var percentageSum = methods.Sum(x => x.Percentage);
        var random = new Random().Next(0, percentageSum);
        long methodId = 0;
        foreach (var method in methods)
        {
            random -= method.Percentage;
            if (random > 0) continue;
            methodId = method.Id;
            break;
        }

        if (methodId == 0) return null;

        return await GetMethodByIdAsync(methodId);
    }

    public async Task ReloadCacheAsync()
    {
        await cache.KeyDeleteAsync(CacheKeys.GetPaymentMethodKey(_tenantId));
        await cache.KeyDeleteAsync(CacheKeys.GetPaymentMethodHashKey(_tenantId));
        await GetMethodsAsync();
    }

    public async Task<PaymentMethod?> GetMethodByIdAsync(long id)
    {
        var hkey = CacheKeys.GetPaymentMethodHashKey(_tenantId);
        var item = await cache.HGetAsync<PaymentMethod>(hkey, id.ToString());
        if (item != null) return item;

        item = await ctx.PaymentMethods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return null;

        await cache.HSetStringAsync(hkey, id.ToString(), item.ToJson(), TimeSpan.FromDays(1));
        return item;
    }

    public async Task<bool> UpdateMethodAsync(long id, Action<PaymentMethod> handler)
    {
        var lockKey = DistributedLockKeys.GetUpdatePaymentMethodKey(_tenantId);
        var lockValue = Guid.NewGuid().ToString();
        if (!cache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromSeconds(10)))
            return false;
        try
        {
            var items = await GetMethodsAsync();
            var item = items.SingleOrDefault(x => x.Id == id);
            if (item == null) return false;
            handler(item);
            ctx.PaymentMethods.Update(item);
            await Task.WhenAll(
                ctx.SaveChangesAsync(),
                cache.SetStringAsync(CacheKeys.GetPaymentMethodKey(_tenantId), JsonConvert.SerializeObject(items),
                    TimeSpan.FromDays(1)),
                cache.HSetStringAsync(CacheKeys.GetPaymentMethodHashKey(_tenantId), id.ToString(), item.ToJson(),
                    TimeSpan.FromDays(1)));
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"PaymentMethodService_UpdateMethod_Error: {e.Message}");
            return false;
        }
        finally
        {
            cache.ReleaseDistributedLock(lockKey, lockValue);
        }
    }

    public async Task<bool> DeleteMethodAsync(long id)
    {
        var lockKey = DistributedLockKeys.GetUpdatePaymentMethodKey(_tenantId);
        var lockValue = Guid.NewGuid().ToString();
        if (!cache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromSeconds(10)))
            return false;
        try
        {
            // var items = await GetMethodsAsync();
            // var item = items.SingleOrDefault(x => x.Id == id);
            var item = await ctx.PaymentMethods.SingleOrDefaultAsync(x => x.Id == id);
            if (item == null) return false;
            item.DeletedOn = DateTime.UtcNow;
            ctx.PaymentMethods.Update(item);
            await ctx.SaveChangesAsync();
            await ReloadCacheAsync();
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"PaymentMethodService_DeleteMethod_Error: {e.Message}");
            return false;
        }
        finally
        {
            cache.ReleaseDistributedLock(lockKey, lockValue);
        }
    }

    /// <summary>
    /// Soft delete a payment method by setting IsDeleted = true
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> SoftDeletePaymentMethodAsync(long id)
    {
        var lockKey = DistributedLockKeys.GetUpdatePaymentMethodKey(_tenantId);
        var lockValue = Guid.NewGuid().ToString();
        if (!cache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromSeconds(10)))
            return false;
        try
        {
            var item = await ctx.PaymentMethods.SingleOrDefaultAsync(x => x.Id == id);
            if (item == null) return false;
            if (item.IsDeleted) return true; // Already deleted

            item.IsDeleted = true;
            item.UpdatedOn = DateTime.UtcNow;
            ctx.PaymentMethods.Update(item);
            await ctx.SaveChangesAsync();
            await ReloadCacheAsync();
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"PaymentMethodService_SoftDeleteMethod_Error: {e.Message}");
            return false;
        }
        finally
        {
            cache.ReleaseDistributedLock(lockKey, lockValue);
        }
    }

    /// <summary>
    /// Restore a soft-deleted payment method by setting IsDeleted = false
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> RestorePaymentMethodAsync(long id)
    {
        var lockKey = DistributedLockKeys.GetUpdatePaymentMethodKey(_tenantId);
        var lockValue = Guid.NewGuid().ToString();
        if (!cache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromSeconds(10)))
            return false;
        try
        {
            var item = await ctx.PaymentMethods.SingleOrDefaultAsync(x => x.Id == id);
            if (item == null) return false;
            if (!item.IsDeleted) return true; // Already active

            item.IsDeleted = false;
            item.UpdatedOn = DateTime.UtcNow;
            ctx.PaymentMethods.Update(item);
            await ctx.SaveChangesAsync();
            await ReloadCacheAsync();
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"PaymentMethodService_RestoreMethod_Error: {e.Message}");
            return false;
        }
        finally
        {
            cache.ReleaseDistributedLock(lockKey, lockValue);
        }
    }
  

    public async Task<bool> CreateMethodAsync(PaymentMethod method)
    {
        var lockKey = DistributedLockKeys.GetUpdatePaymentMethodKey(_tenantId);
        var lockValue = Guid.NewGuid().ToString();
        if (!cache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromSeconds(10)))
            throw new Exception("Failed to get lock");
        try
        {
            var items = await GetMethodsAsync();
            method.Id = items.Max(x => x.Id) + 1;
            ctx.PaymentMethods.Add(method);
            await ctx.SaveChangesAsync();
            items.Add(method);

            await cache.SetStringAsync(CacheKeys.GetPaymentMethodKey(_tenantId), JsonConvert.SerializeObject(items),
                TimeSpan.FromDays(1));
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"PaymentMethodService_CreateMethod_Error: {e.Message}");
            return false;
        }
        finally
        {
            cache.ReleaseDistributedLock(lockKey, lockValue);
        }
    }

    public async Task<List<PaymentInfo.ClientPageModel>> QueryPaymentBankInfoForClientAsync(
        PaymentInfo.ClientCriteria criteria)
    {
        var items = await ctx.PaymentInfos.PagedFilterBy(criteria).ToClientPageModel().ToListAsync();
        return items;
    }

    public async Task<bool> CreatePaymentBankInfoAsync(long partyId, PaymentPlatformTypes platform, string name,
        string info)
    {
        var item = new PaymentInfo
        {
            PartyId = partyId,
            Name = name,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            PaymentPlatform = (int)platform,
            Info = info,
        };
        await ctx.PaymentInfos.AddAsync(item);
        return await ctx.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdatePaymentBankInfoAsync(long paymentInfoId, PaymentPlatformTypes platform, string name,
        string info)
    {
        var item = await ctx.PaymentInfos.SingleOrDefaultAsync(x => x.Id == paymentInfoId);
        if (item == null) return false;
        item.Name = name;
        item.PaymentPlatform = (int)platform;
        item.Info = info;
        item.UpdatedOn = DateTime.UtcNow;
        ctx.PaymentInfos.Update(item);
        return await ctx.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeletePaymentBankInfoAsync(long paymentInfoId)
    {
        var item = await ctx.PaymentInfos.SingleOrDefaultAsync(x => x.Id == paymentInfoId);
        if (item == null) return false;
        ctx.PaymentInfos.Remove(item);
        return await ctx.SaveChangesAsync() > 0;
    }

    public IQueryable<AccountPaymentMethodAccess> GetAccountAccessQuery(long accountId, string? methodTypes = null,
        PaymentMethodStatusTypes? status = null, PaymentMethodAccessStatusTypes? accessStatus = null,
        string? group = null)
        => ctx.AccountPaymentMethodAccesses
            .Where(x => x.PaymentMethod.DeletedOn == null)
            .Where(x => x.AccountId == accountId)
            .Where(x => methodTypes == null || x.PaymentMethod.MethodType == methodTypes)
            .Where(x => status == null || x.PaymentMethod.Status == (short)status)
            .Where(x => accessStatus == null || x.Status == (short)accessStatus)
            .Where(x => group == null || x.PaymentMethod.Group == group);

    public IQueryable<WalletPaymentMethodAccess> GetWalletAccessQuery(long walletId, string? methodTypes = null,
        PaymentMethodStatusTypes? status = null, PaymentMethodAccessStatusTypes? accessStatus = null,
        string? group = null)
        => ctx.WalletPaymentMethodAccesses
            .Where(x => x.PaymentMethod.DeletedOn == null)
            .Where(x => x.WalletId == walletId)
            .Where(x => methodTypes == null || x.PaymentMethod.MethodType == methodTypes)
            .Where(x => status == null || x.PaymentMethod.Status == (short)status)
            .Where(x => accessStatus == null || x.Status == (short)accessStatus)
            .Where(x => group == null || x.PaymentMethod.Group == group);

    /// <summary>
    /// Batch update payment method sort order
    /// </summary>
    public async Task<(bool success, string message)> BatchUpdateSortAsync(List<PaymentMethod.UpdateSortSpec> items)
    {
        if (items == null || items.Count == 0)
            return (false, "No items to update");

        var ids = items.Select(x => x.Id).ToList();
        var methods = await ctx.PaymentMethods
            .Where(x => ids.Contains(x.Id) && x.DeletedOn == null)
            .ToListAsync();

        if (methods.Count != items.Count)
            return (false, "Some payment methods not found");

        foreach (var item in items)
        {
            var method = methods.FirstOrDefault(x => x.Id == item.Id);
            if (method != null)
            {
                method.Sort = item.Sort;
                method.UpdatedOn = DateTime.UtcNow;
            }
        }

        ctx.PaymentMethods.UpdateRange(methods);
        var result = await ctx.SaveChangesAsync() > 0;
        
        if (result)
        {
            // Invalidate cache after updating sort order
            await ReloadCacheAsync();
        }

        return (result, result ? "Sort order updated successfully" : "Failed to update sort order");
    }
}