using System.Text;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Rebate;

partial class Rebate : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public long? AccountId { get; set; }
        public long? AccountUid { get; set; }
        public long? ParentAccountUid { get; set; }
        public long? RepUid { get; set; }
        public long? SalesUid { get; set; }
        public long? AgentUid { get; set; }
        public long? TicketNumber { get; set; }
        public long? AccountNumber { get; set; }
        public string? Symbol { get; set; }
        public long? SourceAccountUid { get; set; }
        public long? TargetAccountUid { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public StateTypes? StateId { get; set; }
        public List<StateTypes>? StateIds { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public long? TotalAmount { get; set; }
        public long? TotalVolume { get; set; }

        public long? PageTotalAmount { get; set; }
        public long? PageTotalVolume { get; set; }
        public bool? ShowRebateTradeClosedLessThanOneMinute { get; set; }
        public string? Email { get; set; }

        public bool? IncludeClosedAccount { get; set; }

        /// <summary>
        /// 是否基于MT5关仓时间（ClosingTime）进行查询
        /// true: 使用 tr.ClosedOn 过滤（MT5关仓时间）
        /// false: 使用 StatedOn 过滤（Master状态时间）
        /// </summary>
        public bool? UseClosingTime { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.IdNavigation.StateId != (int)StateTypes.RebateTradeClosedLessThanOneMinute,
                ShowRebateTradeClosedLessThanOneMinute is not true);

            pool.Add(x => StateIds!.Contains((StateTypes)x.IdNavigation.StateId),
                StateIds != null);

            pool.Add(x => x.Account.Status == 0, IncludeClosedAccount is not true);
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.AccountId == AccountId, AccountId.IsTangible());
            pool.Add(x => x.IdNavigation.PostedOn < CreatedTo, CreatedTo.IsTangible());
            pool.Add(x => x.IdNavigation.PostedOn >= CreatedFrom, CreatedFrom.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn < To, To.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn >= From, From.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => x.Account.Uid == AccountUid, AccountUid.IsTangible());
            pool.Add(x => x.TradeRebate != null && x.TradeRebate.Symbol == Symbol,
                Symbol != null && Symbol.IsTangible());
            pool.Add(x => x.TradeRebate != null && x.TradeRebate.Account != null && x.TradeRebate.Account.AccountNumber == AccountNumber,
                AccountNumber.HasValue);
            pool.Add(
                x => x.TradeRebate != null && x.TradeRebate.Account != null &&
                     x.TradeRebate.Ticket == TicketNumber, TicketNumber.HasValue);
            pool.Add(
                x => x.TradeRebate != null && x.TradeRebate.Account != null &&
                     x.TradeRebate.Account.Uid == SourceAccountUid,
                SourceAccountUid.HasValue);
            pool.Add(x => x.Account.Uid == TargetAccountUid, TargetAccountUid.HasValue);
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId.HasValue);

            pool.Add(x => x.Account.SalesAccount != null && x.Account.SalesAccount.Uid == SalesUid,
                SalesUid.IsTangible());

            pool.Add(x => x.Account.AgentAccount != null && x.Account.AgentAccount.Uid == AgentUid,
                AgentUid.IsTangible());

            pool.Add(x => x.Account.ReferPath.StartsWith("." + RepUid!.Value),
                RepUid.IsTangible());
            pool.Add(x => x.Party.Email == Email!, Email != null);
        }

        protected override IQueryable<M> Pagination(IQueryable<M> source)
        {
            try
            {
                if (Page < 1 && Size < 1) return source;
                Page = Page < 1 ? 1 : Page;
                Size = Size < 1 ? 20 : Size;

                if (shouldUpdateSummary())
                {
                    var summary = source
                        .GroupBy(x => 1)
                        .Select(g => new
                        {
                            SumTotal = g.Count(),
                            SumTotalAmount = g.Sum(x => x.Amount),
                            SumTotalVolume = g
                                .Where(x => x.TradeRebate != null)
                                .GroupBy(x => x.TradeRebate!.Id)
                                .Select(x => x.First().TradeRebate!.Volume)
                                .Sum(),
                        })
                        .FirstOrDefault();
                    if (summary != null)
                    {
                        Total = summary.SumTotal;
                        TotalAmount = summary.SumTotalAmount;
                        TotalVolume = summary.SumTotalVolume;
                        PageCount = (int)Math.Ceiling(Total / (decimal)Size);
                    }
                }

                return source.Skip((Page - 1) * Size).Take(Size);
            }
            catch
            {
                return source;
            }
        }

        private bool shouldUpdateSummary() => Total == 0
                                              || TotalVolume is null or 0
                                              || TotalAmount is null or 0;

        // public string Hash()
        // {
        //     var sb = new StringBuilder();
        //     sb.Append(PartyId);
        //     sb.Append(AccountId);
        //     sb.Append(AccountUid);
        //     sb.Append(RepUid);
        //     sb.Append(SalesUid);
        //     sb.Append(AgentUid);
        //     sb.Append(TicketNumber);
        //     sb.Append(AccountNumber);
        //     sb.Append(Symbol);
        //     sb.Append(SourceAccountUid);
        //     sb.Append(TargetAccountUid);
        //     sb.Append(CurrencyId);
        //     sb.Append(StateId);
        //     sb.Append(Utils.RoundUpToHour(CreatedFrom));
        //     sb.Append(Utils.RoundUpToHour(CreatedTo));
        //     sb.Append(Utils.RoundUpToHour(From));
        //     sb.Append(Utils.RoundUpToHour(To));
        //     return Utils.Md5Hash(sb.ToString());
        // }
    }


    public sealed class ReportCriteria : EntityCriteria<Rebate>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public double TimeZoneOffset { get; set; }

        public ReportPeriodTypes PeriodType { get; set; } =
            ReportPeriodTypes.Hourly;

        public long? SalesUid { get; set; }
        public long? AgentUid { get; set; }
        public long? RepUid { get; set; }
        public long? AccountUid { get; set; }

        protected override void OnCollect(ICriteriaPool<Rebate> pool)
        {
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue);
            pool.Add(x => x.IdNavigation.StatedOn >= From, From.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn <= To, To.IsTangible());

            pool.Add(x => x.Account.AgentAccount != null && x.Account.AgentAccount.Uid == AgentUid,
                AgentUid.IsTangible());
            pool.Add(x => x.Account.SalesAccount != null && x.Account.SalesAccount.Uid == SalesUid,
                SalesUid.IsTangible());

            pool.Add(x => x.Account.Uid == AccountUid,
                AccountUid.IsTangible());

            pool.Add(x => x.Account.ReferPath.StartsWith("." + RepUid!.Value),
                RepUid.IsTangible());
        }

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

        // public string Hash()
        // {
        //     var sb = new StringBuilder();
        //     sb.Append(Utils.RoundUpToHour(From));
        //     sb.Append(Utils.RoundUpToHour(To));
        //     sb.Append(TimeSpan.FromHours(TimeZoneOffset).TotalMinutes);
        //     sb.Append(CurrencyId);
        //     sb.Append(PeriodType);
        //     sb.Append(SalesUid);
        //     sb.Append(AgentUid);
        //     sb.Append(RepUid);
        //     sb.Append(AccountUid);
        //     return Utils.Md5Hash(sb.ToString());
        // }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        [JsonIgnore] public long? WalletId { get; set; }

        [JsonIgnore] public long? PartyId { get; set; }
        public long? AccountNumber { get; set; }
        public long? Ticket { get; set; }
        public string? Symbol { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.TradeRebate != null && x.TradeRebate.AccountNumber == AccountNumber, AccountNumber != null);
            pool.Add(x => x.TradeRebate != null && x.TradeRebate.Ticket == Ticket, Ticket != null);
            pool.Add(x => x.TradeRebate != null && x.TradeRebate.Symbol == Symbol, Symbol != null);
            pool.Add(x => x.IdNavigation.WalletTransactions.Any(y => y.WalletId == WalletId), WalletId != null);
            pool.Add(x => x.IdNavigation.PostedOn >= From!.Value.ToUniversalTime(), From != null);
            pool.Add(x => x.IdNavigation.PostedOn <= To!.Value.ToUniversalTime(), To != null);
        }
    }
}