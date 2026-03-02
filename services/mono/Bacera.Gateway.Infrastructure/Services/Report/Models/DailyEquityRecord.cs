using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;

namespace Bacera.Gateway.Services.Report.Models;

public class DailyEquityRecord : ICanExportToCsv
{
    // Interface implementations
    public long PartyId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    // Report fields
    public string Currency { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public long NewUser { get; set; }
    public long NewAcc { get; set; }
    public decimal PreviousEquity { get; set; }
    public decimal CurrentEquity { get; set; }
    public decimal MarginIn { get; set; }
    public decimal MarginOut { get; set; }
    public decimal Transfer { get; set; }
    public decimal Credit { get; set; }
    public decimal Adjust { get; set; }
    public decimal Rebate { get; set; }
    public decimal NetInOut { get; set; }
    public decimal Lots { get; set; }
    public decimal PL { get; set; }
    public decimal EstimatesNetPL { get; set; }

    public static string Header() =>
        "Currency,Office,New User,New Acc,Previous Equity,Current Equity,Margin In,Margin Out,Transfer,Credit,Adjust,Rebate,Net In/Out,Lots,P&L,Estimates Net PL";

    public string ToCsv()
    {
        return $"{Currency},{Office},{NewUser},{NewAcc}," +
               $"{PreviousEquity:F2},{CurrentEquity:F2}," +
               $"{MarginIn:F2},{MarginOut:F2},{Transfer:F2}," +
               $"{Credit:F2},{Adjust:F2},{Rebate:F2}," +
               $"{NetInOut:F2},{Lots:F2},{PL:F2},{EstimatesNetPL:F2}";
    }

    /// <summary>
    /// Enhanced CSV export with grouping support:
    /// - Currency column only shows on first row of each group
    /// - "Total" rows show in Currency column instead of Office
    /// - Blank rows show completely empty (no zeros)
    /// </summary>
    public string ToCsvGrouped(ref string? lastCurrency)
    {
        // For blank separator rows
        if (string.IsNullOrEmpty(Currency) && string.IsNullOrEmpty(Office))
        {
            return ",,,,,,,,,,,,,,";
        }

        // For Total rows - show in Currency column
        if (Office == "Total")
        {
            lastCurrency = null; // Reset for next group
            return $"Total,," + 
                   $"{NewUser},{NewAcc}," +
                   $"{FormatValue(PreviousEquity)},{FormatValue(CurrentEquity)}," +
                   $"{FormatValue(MarginIn)},{FormatValue(MarginOut)},{FormatValue(Transfer)}," +
                   $"{FormatValue(Credit)},{FormatValue(Adjust)},{FormatValue(Rebate)}," +
                   $"{FormatValue(NetInOut)},{FormatValue(Lots)},{FormatValue(PL)},{FormatValue(EstimatesNetPL)}";
        }

        // For regular data rows - only show Currency on first row
        string currencyDisplay = "";
        if (lastCurrency != Currency)
        {
            currencyDisplay = Currency;
            lastCurrency = Currency;
        }

        return $"{currencyDisplay},{Office}," +
               $"{NewUser},{NewAcc}," +
               $"{FormatValue(PreviousEquity)},{FormatValue(CurrentEquity)}," +
               $"{FormatValue(MarginIn)},{FormatValue(MarginOut)},{FormatValue(Transfer)}," +
               $"{FormatValue(Credit)},{FormatValue(Adjust)},{FormatValue(Rebate)}," +
               $"{FormatValue(NetInOut)},{FormatValue(Lots)},{FormatValue(PL)},{FormatValue(EstimatesNetPL)}";
    }

    /// <summary>
    /// Format decimal values - show empty string for zero values in blank rows
    /// USC: Always 4 decimal places
    /// Wallet: 2 decimal places (Total calculated with full precision, then displayed as F2)
    /// Others: 2 decimal places
    /// </summary>
    private string FormatValue(decimal value)
    {
        // For non-zero values, format with appropriate decimal places
        if (value != 0)
        {
            // USC always uses 4 decimal places
            if (Currency == "USC")
                return $"{value:F4}";
            // Wallet and all others use 2 decimal places for display
            // Note: Wallet Total is calculated with full precision internally, then displayed as F2
            else
                return $"{value:F2}";
        }
        
        // For zero values, return "0" 
        return "0";
    }
}

// Intermediate result from PostgreSQL query
public class DailyEquityPostgresResult
{
    public string Office { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string? AccountList { get; set; }
    public int ServiceId { get; set; }
    public long NewAccUser { get; set; }
    public decimal PrevEquity { get; set; }
    public decimal CurrEquity { get; set; }
    public decimal MarginIn { get; set; }
    public decimal MarginOut { get; set; }
    public decimal TransferIn { get; set; }
    public decimal TransferOut { get; set; }
    public decimal Credit { get; set; }
    public decimal Adjust { get; set; }
    public decimal Rebate { get; set; }
}

// Result from MySQL query
public class DailyEquityMysqlResult
{
    public string Office { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal PreviousEquity { get; set; }
    public decimal CurrentEquity { get; set; }
    public decimal Lots { get; set; }
    public decimal PL { get; set; }
}

