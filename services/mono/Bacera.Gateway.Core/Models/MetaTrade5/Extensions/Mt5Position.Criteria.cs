namespace Bacera.Gateway.Integration;

partial class Mt5Position
{
    public sealed class Criteria : Criteria<Mt5Position>
    {
        public Criteria()
        {
            SortField = nameof(PositionId);
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

        //report 
        public double? TotalVolume { get; set; }
        public double? TotalProfit { get; set; }
        public double? TotalCommission { get; set; }
        public double? TotalSwap { get; set; }

        protected override void OnCollect(ICriteriaPool<Mt5Position> pool)
        {
            pool.Add(x => x.PositionId == (ulong)Id!, Id.HasValue);
            pool.Add(x => x.PositionId == (ulong)PositionId!, PositionId.HasValue);
            pool.Add(x => x.Login == (ulong)Login!, Login.HasValue);
            pool.Add(x => x.Symbol == Symbol, !string.IsNullOrEmpty(Symbol));
            pool.Add(x => Logins!.Select(l => (ulong)l).Contains(x.Login), Logins != null);

            pool.Add(x => x.Action == (uint)Command!, Command.HasValue);
            pool.Add(x => Commands!.Select(c => (uint)c).Contains(x.Action), Commands != null && Commands.Any());

            pool.Add(x => x.TimeCreateMsc >= From!.Value, From != null && From.IsTangible());
            pool.Add(x => x.TimeCreateMsc < To!.Value, To != null && To.IsTangible());
            pool.Add(x => x.TimeUpdateMsc >= ClosedFrom!.Value, ClosedFrom != null && ClosedFrom.IsTangible());
            pool.Add(x => x.TimeUpdateMsc < ClosedTo!.Value,
                ClosedTo != null && ClosedTo.IsTangible());
        }

        protected override IQueryable<Mt5Position> Pagination(IQueryable<Mt5Position> source)
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
                            TotalVolume = g.Sum(x => (long)x.Volume),
                            TotalProfit = g.Sum(x => x.Profit),
                            TotalSwap = g.Sum(x => x.Storage),
                        })
                        .FirstOrDefault();

                    if (summary is not null)
                    {
                        Total = summary.Total;
                        TotalVolume = summary.TotalVolume / 10000.0;
                        TotalProfit = Math.Round(summary.TotalProfit, 2);
                        PageCount = (int)Math.Ceiling(Total / (decimal)Size);
                        TotalSwap = Math.Round(summary.TotalSwap, 2);
                        TotalCommission = 0;
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
                                              || TotalProfit is null or 0;
    }
}

public static class Mt5PositionCriteriaExt
{
    public static Mt5Position.Criteria ToMt5PositionCriteria(this TradeViewModel.Criteria criteria)
    {
        return new Mt5Position.Criteria
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
            SortField = criteria.SortField,
            SortFlag = criteria.SortFlag,
            Total = criteria.Total,
            TotalVolume = criteria.TotalVolume,
            TotalProfit = criteria.TotalProfit,
        };
    }

    public static Mt5Position.Criteria ToMt5PositionCriteria(this TradeViewModel.ReportCriteria criteria)
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
        return new Mt5Position.Criteria
        {
            Login = criteria.AccountNumber,
            Logins = criteria.AccountNumbers,
            From = fromDt,
            To = toDt,
            SortField = criteria.SortField,
            SortFlag = criteria.SortFlag,
        };
    }

    public static TradeViewModel.Criteria MergeToViewModelCriteria(this Mt5Position.Criteria criteria,
        TradeViewModel.Criteria toCriteria, List<TradeViewModel>? positions = null)
    {
        positions ??= new List<TradeViewModel>();
        toCriteria.Page = criteria.Page;
        toCriteria.Size = criteria.Size;
        toCriteria.SortField = criteria.SortField;
        toCriteria.SortFlag = criteria.SortFlag;

        toCriteria.From = criteria.From;
        toCriteria.To = criteria.To;
        toCriteria.ClosedFrom = criteria.ClosedFrom;
        toCriteria.ClosedTo = criteria.ClosedTo;

        toCriteria.Total = criteria.Total;
        toCriteria.TotalVolume = criteria.TotalVolume;
        toCriteria.TotalProfit = criteria.TotalProfit;
        toCriteria.TotalSwap = criteria.TotalSwap;
        toCriteria.TotalCommission = criteria.TotalCommission;

        toCriteria.PageTotalVolume = Math.Round(positions.Sum(x => x.Volume), 2);
        toCriteria.PageTotalProfit = Math.Round(positions.Sum(x => x.Profit), 2);
        toCriteria.PageTotalSwap = Math.Round(positions.Sum(x => x.Swaps), 2);
        toCriteria.PageTotalCommission = Math.Round(positions.Sum(x => x.Commission), 2);
        return toCriteria;
    }
}