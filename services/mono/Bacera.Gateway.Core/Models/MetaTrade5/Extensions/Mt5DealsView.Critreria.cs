using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway.Integration;

partial class Mt5Deals2025s : IMt5Deal
{
    public sealed class Criteria : Criteria<Mt5Deals2025s>
    {
        public Criteria()
        {
            SortField = nameof(Deal);
        }

        public long? Id { get; set; }
        public long? Login { get; set; }
        public List<long>? Logins { get; set; }

        public List<int>? Commands { get; set; }
        public int? Command { get; set; }
        public long? PositionId { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public DateTime? ClosedFrom { get; set; }
        public DateTime? ClosedTo { get; set; }

        public string? Symbol { get; set; }
        public bool? IsClosed { get; set; }

        //report 
        public double? TotalVolume { get; set; }
        public double? TotalProfit { get; set; }
        public double? TotalCommission { get; set; }
        public double? TotalSwap { get; set; }

        protected override void OnCollect(ICriteriaPool<Mt5Deals2025s> pool)
        {
            pool.Add(x => x.Deal == (ulong)Id!, Id.HasValue);
            pool.Add(x => x.PositionId == (ulong)PositionId!, PositionId.HasValue);
            pool.Add(x => x.Login == (ulong)Login!, Login.HasValue);
            pool.Add(x => x.Symbol == Symbol, !string.IsNullOrEmpty(Symbol));
            pool.Add(x => Logins!.Select(l => (ulong)l).Contains(x.Login), Logins != null);

            pool.Add(x => x.Action == (uint)Command!, Command.HasValue);
            pool.Add(x => Commands!.Select(c => (uint)c).Contains(x.Action), Commands != null && Commands.Any());

            pool.Add(x => x.TimeMsc >= From!.Value, From != null && From.IsTangible());
            pool.Add(x => x.TimeMsc < To!.Value, To != null && To.IsTangible());
            pool.Add(x => x.TimeMsc >= ClosedFrom!.Value && x.Entry == (uint)Mt5DealEntryTypes.Closed,
                ClosedFrom != null && ClosedFrom.IsTangible());
            pool.Add(x => x.TimeMsc < ClosedTo!.Value && x.Entry == (uint)Mt5DealEntryTypes.Closed,
                ClosedTo != null && ClosedTo.IsTangible());

            pool.Add(x => x.Entry == (uint)Mt5DealEntryTypes.Closed, IsClosed is true);
            pool.Add(x => x.Entry == (uint)Mt5DealEntryTypes.Open, IsClosed is false);
        }

        protected override IQueryable<Mt5Deals2025s> Pagination(IQueryable<Mt5Deals2025s> source)
        {
            try
            {
                if (Page < 1 && Size < 1) return source;
                Page = Page < 1 ? 1 : Page;
                Size = Size < 1 ? 20 : Size;

                if (ShouldUpdateSummary())
                {
                    var summary = source
                        .GroupBy(x => 1)
                        .Select(g => new
                        {
                            Total = g.Count(),
                            TotalVolume = g.Sum(x => (double)x.VolumeClosed),
                            TotalProfit = g.Sum(x => x.Profit),
                            TotalCommission = g.Sum(x => x.Commission),
                        })
                        .FirstOrDefault();

                    if (summary is not null)
                    {
                        Total = summary.Total;
                        TotalVolume = summary.TotalVolume / 10000.0;
                        TotalProfit = Math.Round(summary.TotalProfit, 2);
                        TotalCommission = Math.Round(summary.TotalCommission, 2);
                        PageCount = (int)Math.Ceiling(Total / (decimal)Size);
                        TotalSwap = 0;
                    }
                }

                return source.Skip((Page - 1) * Size).Take(Size);
            }
            catch (Exception)
            {
                return source;
            }
        }

        private bool ShouldUpdateSummary() => Total == 0
                                              || TotalVolume is null or 0
                                              || TotalProfit is null or 0
                                              || TotalCommission is null or 0;
    }
}

public static class Mt5DealCriteriaExt
{
    public static Mt5Deals2025s.Criteria ToMt5DealCriteria(this TradeViewModel.Criteria criteria) =>
        new()
        {
            Id = criteria.Ticket,
            Commands = criteria.Commands,
            Command = criteria.Command,
            Symbol = criteria.Symbol,
            Login = criteria.AccountNumber,
            Logins = criteria.AccountNumbers,
            From = criteria.From,
            To = criteria.To,
            ClosedFrom = criteria.ClosedFrom,
            ClosedTo = criteria.ClosedTo,
            Page = criteria.Page,
            Size = criteria.Size,
            SortFlag = criteria.SortFlag,
            IsClosed = criteria.IsClosed,

            Total = criteria.Total,
            TotalVolume = criteria.TotalVolume,
            TotalProfit = criteria.TotalProfit,
            TotalCommission = criteria.TotalCommission,
            TotalSwap = criteria.TotalSwap,
        };

    public static Mt5Deals2025s.Criteria ToMt5DealCriteria(this TradeViewModel.ReportCriteria criteria)
    {
        var offsetMinutes = (int)TimeSpan.FromHours(criteria.TimeZoneOffset).TotalMinutes;
        var fromDt = new DateTime(criteria.From.Year,
            criteria.From.Month,
            criteria.From.Day,
            criteria.From.Hour,
            0, 0, DateTimeKind.Utc).AddMinutes(offsetMinutes);
        var toDt = new DateTime(criteria.To.Year,
            criteria.To.Month,
            criteria.To.Day,
            criteria.To.Hour,
            0, 0, DateTimeKind.Utc).AddMinutes(offsetMinutes);
        return new Mt5Deals2025s.Criteria()
        {
            Login = criteria.AccountNumber,
            Logins = criteria.AccountNumbers,
            From = fromDt,
            To = toDt,
            SortField = criteria.SortField,
            SortFlag = criteria.SortFlag,
        };
    }

    public static TradeViewModel.Criteria MergeToViewModelCriteria(this Mt5Deals2025s.Criteria criteria,
        TradeViewModel.Criteria toCriteria, List<TradeViewModel>? deals = null)
    {
        deals ??= new List<TradeViewModel>();
        toCriteria.Page = criteria.Page;
        toCriteria.Size = criteria.Size;
        toCriteria.SortField = criteria.SortField;
        toCriteria.SortFlag = criteria.SortFlag;
        toCriteria.PageCount = criteria.PageCount;
        toCriteria.IsClosed = criteria.IsClosed;

        toCriteria.From = criteria.From;
        toCriteria.To = criteria.To;
        toCriteria.ClosedFrom = criteria.ClosedFrom;
        toCriteria.ClosedTo = criteria.ClosedTo;

        toCriteria.Total = criteria.Total;
        toCriteria.TotalSwap = criteria.TotalSwap;
        toCriteria.TotalVolume = criteria.TotalVolume;
        toCriteria.TotalProfit = criteria.TotalProfit;
        toCriteria.TotalCommission = criteria.TotalCommission;

        toCriteria.PageTotalSwap = Math.Round(deals.Sum(x => x.Swaps), 2);
        toCriteria.PageTotalVolume = Math.Round(deals.Sum(x => x.Volume), 2);
        toCriteria.PageTotalProfit = Math.Round(deals.Sum(x => x.Profit), 2);
        toCriteria.PageTotalCommission = Math.Round(deals.Sum(x => x.Commission), 2);
        return toCriteria;
    }
}