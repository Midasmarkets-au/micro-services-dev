namespace Bacera.Gateway.Services.Report.Models;

public class DailyEquityPerOfficeRecord
{
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
        "Office,New User,New Acc,Previous Equity,Current Equity,Margin In,Margin Out,Transfer,Credit,Adjust,Rebate,Net In/Out,Lots,P&L,Estimates Net PL";

    public string ToCsv()
    {
        return $"{Office},{NewUser},{NewAcc}," +
               $"{FormatValue(PreviousEquity)},{FormatValue(CurrentEquity)}," +
               $"{FormatValue(MarginIn)},{FormatValue(MarginOut)},{FormatValue(Transfer)}," +
               $"{FormatValue(Credit)},{FormatValue(Adjust)},{FormatValue(Rebate)}," +
               $"{FormatValue(NetInOut)},{FormatValue(Lots)},{FormatValue(PL)},{FormatValue(EstimatesNetPL)}";
    }

    private static string FormatValue(decimal value)
    {
        if (value != 0)
            return $"{value:F2}";
        return "0";
    }
}
