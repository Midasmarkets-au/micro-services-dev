using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway.Services.Common;

/// <summary>
/// Helper class for handling USC (US Cent) currency conversions
/// USC is handled programmatically with fixed rates: 1 USD = 100 USC
/// </summary>
public static class CurrencyConversionHelper
{
    /// <summary>
    /// Checks if USC currency conversion is needed and returns the conversion rate
    /// </summary>
    /// <param name="from">Source currency</param>
    /// <param name="to">Target currency</param>
    /// <returns>
    /// Tuple with:
    /// - isUscConversion: true if this is a USC conversion that should be handled programmatically
    /// - rate: the conversion rate (decimal for PaymentMethodService compatibility)
    /// </returns>
    public static (bool isUscConversion, decimal rate) GetUscConversionRate(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to)
            return (true, 1m);
        
        // Direct USC <-> USD conversions
        if (from == CurrencyTypes.USC && to == CurrencyTypes.USD)
            return (true, 0.01m); // 1 USC = 0.01 USD
        
        if (from == CurrencyTypes.USD && to == CurrencyTypes.USC)
            return (true, 100m); // 1 USD = 100 USC
        
        // USC involved but not direct USC <-> USD conversion
        if (from == CurrencyTypes.USC || to == CurrencyTypes.USC)
            return (true, -1m); // Indicates USC conversion needed but requires multi-step calculation
        
        // No USC involved
        return (false, 0m);
    }
    
    /// <summary>
    /// Calculates USC conversion rate for multi-step conversions (USC -> USD -> target or source -> USD -> USC)
    /// </summary>
    /// <param name="from">Source currency</param>
    /// <param name="to">Target currency</param>
    /// <param name="getExchangeRateFunc">Function to get exchange rate between non-USC currencies</param>
    /// <returns>The conversion rate, or -1 if conversion failed</returns>
    public static async Task<decimal> CalculateUscConversionRateAsync(
        CurrencyTypes from, 
        CurrencyTypes to, 
        Func<CurrencyTypes, CurrencyTypes, Task<decimal>> getExchangeRateFunc)
    {
        var (isUscConversion, directRate) = GetUscConversionRate(from, to);
        
        if (!isUscConversion)
            throw new InvalidOperationException("This method should only be called for USC conversions");
        
        // Return direct rate if it's a simple conversion
        if (directRate != -1m)
            return directRate;
        
        // Handle multi-step conversions
        if (from == CurrencyTypes.USC && to != CurrencyTypes.USD)
        {
            // Convert USC -> USD -> target currency
            var usdToTarget = await getExchangeRateFunc(CurrencyTypes.USD, to);
            return usdToTarget == -1 ? -1m : 0.01m * usdToTarget;
        }
        
        if (to == CurrencyTypes.USC && from != CurrencyTypes.USD)
        {
            // Convert source currency -> USD -> USC
            var fromToUsd = await getExchangeRateFunc(from, CurrencyTypes.USD);
            return fromToUsd == -1 ? -1m : fromToUsd * 100m;
        }
        
        return -1m; // Should not reach here
    }
    
    /// <summary>
    /// Overload for double return type (for RebateService compatibility)
    /// </summary>
    public static (bool isUscConversion, double rate) GetUscConversionRateDouble(CurrencyTypes from, CurrencyTypes to)
    {
        var (isUsc, decimalRate) = GetUscConversionRate(from, to);
        return (isUsc, (double)decimalRate);
    }
    
    /// <summary>
    /// Calculates USC conversion rate for multi-step conversions with double return type
    /// </summary>
    public static async Task<double> CalculateUscConversionRateDoubleAsync(
        CurrencyTypes from, 
        CurrencyTypes to, 
        Func<CurrencyTypes, CurrencyTypes, Task<double>> getExchangeRateFunc)
    {
        var (isUscConversion, directRate) = GetUscConversionRateDouble(from, to);
        
        if (!isUscConversion)
            throw new InvalidOperationException("This method should only be called for USC conversions");
        
        // Return direct rate if it's a simple conversion
        if (directRate != -1.0)
            return directRate;
        
        // Handle multi-step conversions
        if (from == CurrencyTypes.USC && to != CurrencyTypes.USD)
        {
            // Convert USC -> USD -> target currency
            var usdToTarget = await getExchangeRateFunc(CurrencyTypes.USD, to);
            return 0.01 * usdToTarget;
        }
        
        if (to == CurrencyTypes.USC && from != CurrencyTypes.USD)
        {
            // Convert source currency -> USD -> USC
            var fromToUsd = await getExchangeRateFunc(from, CurrencyTypes.USD);
            return fromToUsd * 100;
        }
        
        return -1.0; // Should not reach here
    }
    
    /// <summary>
    /// Converts amount in cents with precise USC handling
    /// Eliminates precision loss by using integer arithmetic
    /// </summary>
    /// <param name="amountInCents">Amount in cents (long)</param>
    /// <param name="from">Source currency</param>
    /// <param name="to">Target currency</param>
    /// <returns>Converted amount in cents</returns>
    public static long ConvertAmountInCentsPrecise(long amountInCents, CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return amountInCents;
        
        var (isUscConversion, rate) = GetUscConversionRate(from, to);
        
        if (!isUscConversion)
            throw new InvalidOperationException("Use this method only for USC conversions");
            
        if (rate == -1m)
            throw new InvalidOperationException("Multi-step conversions require external exchange rate");
        
        // Use precise calculation: amountInCents * rate (both are precise)
        // Since USC rate is always whole numbers (0.01 or 100), this maintains precision
        return (long)Math.Round(amountInCents * rate, MidpointRounding.AwayFromZero);
    }
    
    /// <summary>
    /// Converts amount in cents with precise USC handling including multi-step conversions
    /// </summary>
    public static async Task<long> ConvertAmountInCentsPreciseAsync(
        long amountInCents, 
        CurrencyTypes from, 
        CurrencyTypes to,
        Func<CurrencyTypes, CurrencyTypes, Task<decimal>> getExchangeRateFunc)
    {
        if (from == to) return amountInCents;

        var (isUscConversion, directRate) = GetUscConversionRate(from, to);

        if (!isUscConversion)
            throw new InvalidOperationException("Use this method only for USC conversions");

        // Simple conversion (USD <-> USC)
        if (directRate != -1m)
            return ConvertAmountInCentsPrecise(amountInCents, from, to);

        // Multi-step conversions with precision preservation
        if (from == CurrencyTypes.USC && to != CurrencyTypes.USD)
        {
            // USC -> USD -> target: amountInCents * 0.01 * usdToTarget
            var usdToTarget = await getExchangeRateFunc(CurrencyTypes.USD, to);
            if (usdToTarget == -1m)
                throw new InvalidOperationException($"Exchange rate not found for USD->{to}");

            var combinedRate = 0.01m * usdToTarget;
            return (long)Math.Round(amountInCents * combinedRate, MidpointRounding.AwayFromZero);
        }

        if (to == CurrencyTypes.USC && from != CurrencyTypes.USD)
        {
            // source -> USD -> USC: amountInCents * fromToUsd * 100
            var fromToUsd = await getExchangeRateFunc(from, CurrencyTypes.USD);
            if (fromToUsd == -1m)
                throw new InvalidOperationException($"Exchange rate not found for {from}->USD");

            var combinedRate = fromToUsd * 100m;
            return (long)Math.Round(amountInCents * combinedRate, MidpointRounding.AwayFromZero);
        }

        throw new InvalidOperationException("Invalid conversion path");
    }
}
