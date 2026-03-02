//  // TODO: Move to MT4/MT5 database
// namespace Bacera.Gateway;
//
// using M = TradeTransaction;
//
// partial class TradeTransaction
// {
//     public sealed class ReportViewModel
//     {
//         public int Year { get; set; }
//         public int Month { get; set; }
//         public int Day { get; set; }
//         public int Hour { get; set; }
//         public int Period { get; set; }
//
//         public string DateTime => new DateTime(Year, Month, Day)
//             .AddHours(Hour)
//             .ToString("yyyy-MM-dd HH:00:00");
//
//         public int TotalTransaction { get; set; }
//
//         public long TotalValue { get; set; }
//     }
// }
//
// public static class TradeTransactionReportExt
// {
//     public static IQueryable<M.ReportViewModel> ToDailyResponseModel(this IQueryable<M> items,
//         double timeZoneOffset = 0)
//         => toResponseModel(items, timeZoneOffset, TimePeriod.Day);
//
//     public static IQueryable<M.ReportViewModel> ToHourlyResponseModel(this IQueryable<M> items,
//         double timeZoneOffset = 0)
//         => toResponseModel(items, timeZoneOffset, TimePeriod.Hour);
//
//     public static IQueryable<M.ReportViewModel> ToMonthlyResponseModel(this IQueryable<M> items,
//         double timeZoneOffset = 0)
//         => toResponseModel(items, timeZoneOffset, TimePeriod.Month);
//
//     public static IQueryable<M.ReportViewModel> ToYearlyResponseModel(this IQueryable<M> items,
//         double timeZoneOffset = 0)
//         => toResponseModel(items, timeZoneOffset, TimePeriod.Year);
//
//     private static IQueryable<M.ReportViewModel> toResponseModel(IQueryable<M> items, double timeZoneOffset,
//         TimePeriod period)
//     {
//         var minutesOffset = (int)TimeSpan.FromHours(timeZoneOffset).TotalMinutes;
//         return items
//             .GroupBy(x => new
//             {
//                 x.ModifiedAt.AddMinutes(minutesOffset).Year,
//                 Month = period >= TimePeriod.Month ? x.ModifiedAt.AddMinutes(minutesOffset).Month : 1,
//                 Day = period >= TimePeriod.Day ? x.ModifiedAt.AddMinutes(minutesOffset).Day : 1,
//                 Hour = period == TimePeriod.Hour ? x.ModifiedAt.AddMinutes(minutesOffset).Hour : 0
//             })
//             .Select(x => new M.ReportViewModel
//             {
//                 Year = x.Key.Year,
//                 Month = x.Key.Month,
//                 Day = x.Key.Day,
//                 Hour = x.Key.Hour,
//                 Period = (int)period,
//                 TotalValue = (long)x.Sum(y => y.OpenPrice * y.Volume),
//                 TotalTransaction = x.Count(),
//             });
//     }
//
//     private enum TimePeriod
//     {
//         Year,
//         Month,
//         Day,
//         Hour
//     }
// }

