namespace Bacera.Gateway.Integration;

partial class Mt4Trade
{
    public static readonly DateTime DefaultDateTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public sealed class Criteria : Criteria<Mt4Trade>
    {
        public Criteria()
        {
            SortField = nameof(CloseTime);
        }

        public long? Id { get; set; }

        public long? Login { get; set; }
        public List<long>? Logins { get; set; }
        public List<int>? Commands { get; set; }
        public int? Command { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public DateTime? OpenedFrom { get; set; }
        public bool? IsClosed { get; set; }

        public DateTime? OpenedTo { get; set; }

        public string? Symbol { get; set; }

        //report 
        public double? TotalVolume { get; set; }
        public double? TotalProfit { get; set; }
        public double? TotalCommission { get; set; }
        public double? TotalSwap { get; set; }

        protected override void OnCollect(ICriteriaPool<Mt4Trade> pool)
        {
            pool.Add(x => x.Ticket == (int)Id!, Id.HasValue);
            pool.Add(x => x.Symbol == Symbol, !string.IsNullOrEmpty(Symbol));
            pool.Add(x => x.Login == (int)Login!, Login.HasValue);
            pool.Add(x => Logins!.Select(l => (int)l).Contains(x.Login), Logins != null);
            pool.Add(x => x.Cmd == Command, Command.HasValue);
            pool.Add(x => Commands!.Contains(x.Cmd), Commands != null && Commands.Any());
            pool.Add(x => x.CloseTime >= From!.Value, From != null && From.IsTangible());
            pool.Add(x => x.CloseTime < To!.Value, To != null && To.IsTangible());
            pool.Add(x => x.OpenTime >= OpenedFrom!.Value, OpenedFrom != null && OpenedFrom.IsTangible());
            pool.Add(x => x.OpenTime < OpenedTo!.Value, OpenedTo != null && OpenedTo.IsTangible());
            pool.Add(x => x.CloseTime > DefaultDateTime, IsClosed is true);
            pool.Add(x => x.CloseTime <= DefaultDateTime, IsClosed is false);
        }

        protected override IQueryable<Mt4Trade> Pagination(IQueryable<Mt4Trade> source)
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
                            TotalVolume = g.Sum(x => x.Volume),
                            TotalProfit = g.Sum(x => x.Profit),
                            TotalCommission = g.Sum(x => x.Commission),
                            TotalSwap = g.Sum(x => x.Swaps),
                        })
                        .FirstOrDefault();

                    if (summary is not null)
                    {
                        Total = summary.Total;
                        TotalVolume = summary.TotalVolume / 100.0;
                        TotalProfit = Math.Round(summary.TotalProfit, 2);
                        TotalCommission = Math.Round(summary.TotalCommission, 2);
                        TotalSwap = Math.Round(summary.TotalSwap, 2);
                        PageCount = (int)Math.Ceiling(Total / (decimal)Size);
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
                                              || TotalSwap is null or 0
                                              || TotalVolume is null or 0
                                              || TotalProfit is null or 0
                                              || TotalCommission is null or 0;
    }
}

public static class MtrTradeCriteriaExt
{
    public static Mt4Trade.Criteria ToMt4Criteria(this TradeViewModel.Criteria criteria)
    {
        return new Mt4Trade.Criteria
        {
            Id = criteria.Ticket,
            Commands = criteria.Commands,
            Command = criteria.Command,
            Symbol = criteria.Symbol,
            Login = criteria.AccountNumber,
            Logins = criteria.AccountNumbers,
            From = criteria.From,
            To = criteria.To,
            OpenedFrom = criteria.ClosedFrom,
            OpenedTo = criteria.ClosedTo,
            IsClosed = criteria.IsClosed,
            Page = criteria.Page,
            Size = criteria.Size,
            SortField = criteria.SortField,
            SortFlag = criteria.SortFlag,

            Total = criteria.Total,
            TotalSwap = criteria.TotalSwap,
            TotalVolume = criteria.TotalVolume,
            TotalProfit = criteria.TotalProfit,
            TotalCommission = criteria.TotalCommission,
        };
    }

    public static Mt4Trade.Criteria ToMt4Criteria(this TradeViewModel.ReportCriteria criteria)
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
        return new Mt4Trade.Criteria
        {
            Login = criteria.AccountNumber,
            Logins = criteria.AccountNumbers,
            From = fromDt,
            To = toDt,
            SortField = criteria.SortField,
            SortFlag = criteria.SortFlag,
        };
    }

    public static TradeViewModel.Criteria MergeToViewModelCriteria(this Mt4Trade.Criteria criteria,
        TradeViewModel.Criteria toCriteria, List<TradeViewModel>? trades = null)
    {
        trades ??= new List<TradeViewModel>();
        toCriteria.Page = criteria.Page;
        toCriteria.Size = criteria.Size;
        toCriteria.SortField = criteria.SortField;
        toCriteria.SortFlag = criteria.SortFlag;
        toCriteria.TotalProfit = criteria.TotalProfit;
        toCriteria.PageCount = criteria.PageCount;
        toCriteria.IsClosed = criteria.IsClosed;
        toCriteria.HasMore = criteria.HasMore;

        toCriteria.From = criteria.From;
        toCriteria.To = criteria.To;
        toCriteria.ClosedFrom = criteria.OpenedFrom;
        toCriteria.ClosedTo = criteria.OpenedTo;

        toCriteria.Total = criteria.Total;
        toCriteria.TotalSwap = criteria.TotalSwap;
        toCriteria.TotalVolume = criteria.TotalVolume;
        toCriteria.TotalProfit = criteria.TotalProfit;
        toCriteria.TotalCommission = criteria.TotalCommission;

        toCriteria.PageTotalSwap = Math.Round(trades.Sum(x => x.Swaps), 2);
        toCriteria.PageTotalVolume = Math.Round(trades.Sum(x => x.Volume), 2);
        toCriteria.PageTotalProfit = Math.Round(trades.Sum(x => x.Profit), 2);
        toCriteria.PageTotalCommission = Math.Round(trades.Sum(x => x.Commission), 2);
        return toCriteria;
    }
}