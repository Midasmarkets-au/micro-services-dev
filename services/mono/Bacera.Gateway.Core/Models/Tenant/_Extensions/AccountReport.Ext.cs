using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

public partial class AccountReport
{
    public static AccountReport Build(long tenantId, long partyId, long accountId, int serviceId, long accountNumber, DateTime date,
        AccountReportTypes type) => new()
    {
        TenantId = tenantId,
        PartyId = partyId,
        AccountId = accountId,
        ServiceId = serviceId,
        AccountNumber = accountNumber,
        Date = date,
        DataFile = "",
        Status = (short)AccountReportStatusTypes.Initialed,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow,
        Type = (int)AccountReportTypes.DailyConfirmation,
    };

    public static DateTime TestFrom = DateTime.UtcNow.Date;

    public static DateTime TestTo => TestFrom
        .AddHours(Utils.IsCurrentDSTLosAngeles(TestFrom) ? 20 : 21)
        .AddMinutes(59)
        .AddSeconds(59);

    public DateTime FromDate => ToDate.AddDays(-1);

    public DateTime ToDate => Date.Date
        .AddHours(Utils.IsCurrentDSTLosAngeles(Date) ? 20 : 21)
        .AddMinutes(59)
        .AddSeconds(59);

    public DateTime ReportDate => ToDate;


    public long ReportTime => new DateTimeOffset(ReportDate).ToUnixTimeSeconds();

    public string GetFileDirInS3() => $"t_{TenantId}/user/{PartyId}/{AccountNumber}/daily/";

    // public string GetFileNameInS3() => $"daily_{ReportDate.Date:yyyy-MM-dd}.json";
    public string GetFileNameInS3() => $"{ReportDate.Date:yyyy-MM-dd}.json";

    public string GetFilePathInS3() => $"{GetFileDirInS3()}{GetFileNameInS3()}";
    
}

// public class CriteriaForPeriod
// {
//     public DateTime? From { get; set; } = DateTime.UtcNow.Date;
//     public DateTime? To { get; set; } = DateTime.UtcNow.Date.AddDays(1);
//     public DateTime Day => DateTime.SpecifyKind(To!.Value.Date, DateTimeKind.Utc);
//
//     public DateTime ReportDate => Day
//         .AddHours(Utils.IsCurrentDSTLosAngeles(To!.Value) ? 20 : 21)
//         .AddMinutes(59)
//         .AddSeconds(59);
//
//     public long ReportTime => new DateTimeOffset(ReportDate).ToUnixTimeSeconds();
//     public Deposit.Criteria DepositCriteria { get; set; } = null!;
//     public Withdrawal.Criteria WithdrawalCriteria { get; set; } = null!;
//     public TradeViewModel.Criteria OpenTradeCriteria { get; set; } = null!;
//     public TradeViewModel.Criteria ClosedTradeCriteria { get; set; } = null!;
//     public TradeViewModel.Criteria ClosedTradeCreditCriteria { get; set; } = null!;
//
//
//     public static CriteriaForPeriod Build(DateTime from, DateTime to)
//     {
//         var item = new CriteriaForPeriod
//         {
//             From = from,
//             To = to,
//             DepositCriteria = Deposit.Criteria.BuildForPeriod(from, to),
//             WithdrawalCriteria = Withdrawal.Criteria.BuildForPeriod(from, to),
//             OpenTradeCriteria = TradeViewModel.Criteria.BuildForOpenTrade(),
//             ClosedTradeCriteria = TradeViewModel.Criteria.BuildForClosedPeriod(from, to),
//             ClosedTradeCreditCriteria = TradeViewModel.Criteria.BuildForClosedCreditPeriod(from, to),
//         };
//         return item;
//     }
//
//     public CriteriaForPeriod ApplyAccountNumber(long accountNumber)
//     {
//         DepositCriteria.AccountNumber = accountNumber;
//         WithdrawalCriteria.AccountNumber = accountNumber;
//         OpenTradeCriteria.AccountNumber = accountNumber;
//         ClosedTradeCriteria.AccountNumber = accountNumber;
//         ClosedTradeCreditCriteria.AccountNumber = accountNumber;
//         return this;
//     }
// }
