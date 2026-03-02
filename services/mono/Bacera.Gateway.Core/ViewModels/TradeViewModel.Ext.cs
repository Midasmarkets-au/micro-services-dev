namespace Bacera.Gateway;

using M = TradeViewModel;

partial class TradeViewModel
{
    public bool ClosedLessThanOneMinute() => CloseAt.HasValue && CloseAt.Value - OpenAt < TimeSpan.FromMinutes(1);

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

    public TradeRebate ToTradeRebate()
        => new()
        {
            TradeServiceId = ServiceId,
            Ticket = Ticket,
            AccountNumber = AccountNumber,
            CurrencyId = (int)CurrencyTypes.Invalid,
            Volume = (int)(Volume * 100),
            Status = (int)TradeRebateStatusTypes.Created,
            OpenedOn = DateTime.SpecifyKind(OpenAt ?? DateTime.MinValue, DateTimeKind.Utc),
            ClosedOn = DateTime.SpecifyKind(CloseAt ?? DateTime.MinValue, DateTimeKind.Utc),
            RuleType = 0,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            Symbol = Symbol,
            TimeStamp = TimeStamp,
            Action = Cmd,
            DealId = Ticket,
            Commission = Commission,
            Swaps = Swaps,
            OpenPrice = OpenPrice ?? 0,
            ClosePrice = ClosePrice ?? 0,
            Profit = Profit
        };
}

public static class TradeViewModelReportExt
{
    public static IQueryable<M.ReportViewModel> ToDailyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => ToResponseModel(items, timeZoneOffset, TimePeriod.Day);

    public static IQueryable<M.ReportViewModel> ToHourlyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => ToResponseModel(items, timeZoneOffset, TimePeriod.Hour);

    public static IQueryable<M.ReportViewModel> ToMonthlyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => ToResponseModel(items, timeZoneOffset, TimePeriod.Month);

    public static IQueryable<M.ReportViewModel> ToYearlyResponseModel(this IQueryable<M> items,
        double timeZoneOffset = 0)
        => ToResponseModel(items, timeZoneOffset, TimePeriod.Year);

    private static IQueryable<M.ReportViewModel> ToResponseModel(IQueryable<M> items, double timeZoneOffset,
        TimePeriod period)
    {
        var minutesOffset = (int)TimeSpan.FromHours(timeZoneOffset).TotalMinutes;
        return items
            .GroupBy(x => new
            {
                x.UpdatedOn.AddMinutes(minutesOffset).Year,
                Month = period >= TimePeriod.Month ? x.UpdatedOn.AddMinutes(minutesOffset).Month : 1,
                Day = period >= TimePeriod.Day ? x.UpdatedOn.AddMinutes(minutesOffset).Day : 1,
                Hour = period == TimePeriod.Hour ? x.UpdatedOn.AddMinutes(minutesOffset).Hour : 0
            })
            .Select(x => new M.ReportViewModel
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                Day = x.Key.Day,
                Hour = x.Key.Hour,
                Period = (int)period,
                TotalValue = (long)x.Sum(y => y.Price * y.Volume),
                TotalTransaction = x.Count(),
            });
    }

    private enum TimePeriod
    {
        Year,
        Month,
        Day,
        Hour
    }
}