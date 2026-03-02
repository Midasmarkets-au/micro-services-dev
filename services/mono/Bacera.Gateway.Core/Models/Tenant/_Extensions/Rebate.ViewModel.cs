namespace Bacera.Gateway;

using M = Rebate;

partial class Rebate
{
    public sealed class ReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Period { get; set; }

        public string DateTime => new DateTime(Year, Month, Day)
            .AddHours(Hour)
            .ToString("yyyy-MM-dd HH:00:00");

        public int TotalTransaction { get; set; }

        public long TotalValue { get; set; }
    }

    public sealed class ClientPageModel
    {
        public long Ticket { get; set; }
        public long AccountNumber { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
        public int StateId { get; set; }
        public long Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}

public static class RebateReportExt
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query) => query.Where(x => x.TradeRebate != null).Select(x =>
        new M.ClientPageModel
        {
            Ticket = x.TradeRebate!.Ticket,
            AccountNumber = x.TradeRebate!.AccountNumber,
            Symbol = x.TradeRebate!.Symbol,
            CurrencyId = x.TradeRebate!.CurrencyId,
            StateId = x.IdNavigation.StateId,
            Amount = x.Amount,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.StatedOn
        });
    public static IQueryable<M.ReportViewModel> ToDailyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => toResponseModel(items, timeZoneOffset, TimePeriod.Day);

    public static IQueryable<M.ReportViewModel> ToHourlyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => toResponseModel(items, timeZoneOffset, TimePeriod.Hour);

    public static IQueryable<M.ReportViewModel> ToMonthlyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => toResponseModel(items, timeZoneOffset, TimePeriod.Month);

    public static IQueryable<M.ReportViewModel> ToYearlyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => toResponseModel(items, timeZoneOffset, TimePeriod.Year);

    private static IQueryable<M.ReportViewModel> toResponseModel(IQueryable<M> items, double timeZoneOffset,
        TimePeriod period)
        => items
            .GroupBy(x => new
                {
                    x.IdNavigation.PostedOn.AddHours(timeZoneOffset).Year,
                    Month = period >= TimePeriod.Month ? x.IdNavigation.PostedOn.AddHours(timeZoneOffset).Month : 1,
                    Day = period >= TimePeriod.Day ? x.IdNavigation.PostedOn.AddHours(timeZoneOffset).Day : 1,
                    Hour = period == TimePeriod.Hour ? x.IdNavigation.PostedOn.AddHours(timeZoneOffset).Hour : 0
                }
            )
            .Select(x => new M.ReportViewModel
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                Day = x.Key.Day,
                Hour = x.Key.Hour,
                Period = (int)period,
                TotalValue = x.Sum(y => y.Amount),
                TotalTransaction = x.Count(),
            });

    private enum TimePeriod
    {
        Year,
        Month,
        Day,
        Hour
    }
}