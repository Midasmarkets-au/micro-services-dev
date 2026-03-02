namespace Bacera.Gateway;

partial class TradeViewModel : IEntity
{
    public sealed class Criteria : EntityCriteria<TradeViewModel>
    {
        public Criteria()
        {
            SortField = nameof(TradeViewModel.Id);
        }

        public static Criteria BuildForOpenTrade() => new()
        {
            IsClosed = false,
            Commands = new List<int> { 1, 0 },
        };

        public static Criteria BuildForClosedPeriod(DateTime? from, DateTime? to) => new()
        {
            To = to,
            From = from,
            IsClosed = true,
            Commands = new List<int> { 1, 0 },
        };

        public static Criteria BuildForClosedCreditPeriod(DateTime? from, DateTime? to) => new()
        {
            ClosedTo = to,
            ClosedFrom = from,
            Commands = new List<int> { 7 },
        };


        public int? ServiceId { get; set; }
        public PlatformTypes? Platform { get; set; }

        public long? AccountNumber { get; set; }
        public List<long>? AccountNumbers { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public DateTime? ClosedFrom { get; set; }
        public DateTime? ClosedTo { get; set; }
        public string? Target { get; set; }
        public long? PositionId { get; set; }
        public long? Ticket { get; set; }
        public string? Symbol { get; set; }
        public int? Command { get; set; }
        public List<int>? Commands { get; set; }
        public bool? IsClosed { get; set; }

        public long? Uid { get; set; }
        public long? PartyId { get; set; }
        public long? AccountId { get; set; }
        public long? SalesUid { get; set; }
        public long? AgentUid { get; set; }
        public long? AgentId { get; set; }
        public long? SalesId { get; set; }
        public long? RepUid { get; set; }
        public long? GroupId { get; set; }
        public long? ParentAccountUid { get; set; }

        //report 
        public double? TotalVolume { get; set; }
        public double? TotalProfit { get; set; }
        public double? TotalCommission { get; set; }
        public double? TotalSwap { get; set; }

        public double? PageTotalVolume { get; set; }
        public double? PageTotalProfit { get; set; }
        public double? PageTotalCommission { get; set; }
        public double? PageTotalSwap { get; set; }

        public bool HasAccountCriteria()
            => Uid != null
               || ParentAccountUid != null
               || PartyId != null
               || AccountId != null
               || SalesUid != null
               || AgentUid != null
               || AgentId != null
               || SalesId != null
               || RepUid != null
               || GroupId != null;

        protected override void OnCollect(ICriteriaPool<TradeViewModel> pool)
        {
            pool.Add(x => x.UpdatedOn >= From, From.HasValue);
            pool.Add(x => x.UpdatedOn <= To, To.HasValue);
            pool.Add(x => x.CloseAt >= ClosedFrom, ClosedFrom.HasValue);
            pool.Add(x => x.CloseAt <= ClosedTo, ClosedTo.HasValue);
            pool.Add(x => x.Cmd == Command, Command.HasValue);
            pool.Add(x => x.Ticket == Ticket, Ticket.HasValue);
            pool.Add(x => x.Symbol == Symbol, !string.IsNullOrEmpty(Symbol));
            pool.Add(x => Commands!.Contains(x.Cmd), Commands != null && Commands.Any());
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.HasValue);
            pool.Add(x => AccountNumbers!.Contains(x.AccountNumber), AccountNumbers != null && AccountNumbers.Any());
            pool.Add(x => x.CloseAt != null, IsClosed.HasValue && IsClosed.Value);
        }
    }

    public sealed class ReportCriteria : Criteria<TradeViewModel>
    {
        public int ServiceId { get; set; }
        public PlatformTypes? Platform { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public double TimeZoneOffset { get; set; }
        public bool? IsClosed { get; set; }

        public ReportPeriodTypes PeriodType { get; set; } =
            ReportPeriodTypes.Hourly;

        public long? AccountNumber { get; set; }
        public List<long>? AccountNumbers { get; set; }
        public long? ParentAccountUid { get; set; }

        public long? SalesUid { get; set; }
        public long? AgentUid { get; set; }
        public long? RepUid { get; set; }

        protected override void OnCollect(ICriteriaPool<TradeViewModel> pool)
        {
            var offsetMinutes = (int)TimeSpan.FromHours(TimeZoneOffset).TotalMinutes;
            var fromDt = new DateTime(From.Year,
                From.Month,
                From.Day,
                From.Hour,
                0, 0, DateTimeKind.Utc).AddMinutes(offsetMinutes);
            var toDt = new DateTime(To.Year,
                To.Month,
                To.Day,
                To.Hour,
                0, 0, DateTimeKind.Utc).AddMinutes(offsetMinutes);
            pool.Add(x => x.UpdatedOn >= fromDt);
            pool.Add(x => x.UpdatedOn <= toDt);
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.HasValue);
            pool.Add(x => AccountNumbers!.Contains(x.AccountNumber), AccountNumbers != null && AccountNumbers.Any());
        }

        protected override IQueryable<TradeViewModel> Pagination(IQueryable<TradeViewModel> source) => source;

        public static ReportCriteria Build(DateTime from, DateTime to, ReportPeriodTypes periodType,
            double timeZoneOffset = 0d, CurrencyTypes? currencyId = null)
            => new()
            {
                From = from,
                To = to,
                CurrencyId = currencyId,
                PeriodType = periodType,
                TimeZoneOffset = timeZoneOffset,
                Size = 500,
            };

        public bool HasAccountCriteria()
            => SalesUid != null
               || AgentUid != null
               || RepUid != null;
    }
}

public static class TradeCriteriaExtensions
{
    public static Account.Criteria ToAccountCriteria(this TradeViewModel.Criteria me)
        => new()
        {
            Id = me.AccountId ?? 0,
            Uid = me.Uid,
            PartyId = me.PartyId,
            SalesUid = me.SalesUid,
            AgentUid = me.AgentUid,
            AgentId = me.AgentId,
            SalesId = me.SalesId,
            GroupId = me.GroupId,
        };

    public static Account.Criteria ToAccountCriteria(this TradeViewModel.ReportCriteria me)
        => new()
        {
            SalesUid = me.SalesUid,
            AgentUid = me.AgentUid,
        };
}