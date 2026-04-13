namespace Bacera.Gateway;

public partial class DailyEquitySnapshot
{
    public long Id { get; set; }

    public DateTime ReportDate { get; set; }

    public EquityReportVersion ReportVersion { get; set; }

    public string Office { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

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

    public string? AdditionalInfo { get; set; }

    public DateTime CreatedOn { get; set; }
}
