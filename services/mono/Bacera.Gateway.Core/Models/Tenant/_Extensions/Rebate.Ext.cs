using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Rebate;

partial class Rebate : IHasMatter
{
    public sealed class PkModel
    {
        public long Id { get; set; }
    }
    public M ResetMatterId()
    {
        IdNavigation = Matter.Build().Rebate().SetState(StateTypes.RebateOnHold);
        Id = 0;
        return this;
    }

    public M SetAmount(long amount)
    {
        Amount = amount;
        return this;
    }
    
    public bool IsEmpty() => Id == 0;

    public Rebate()
    {
        Information = string.Empty;
    }

    public static M Build(long partyId, long accountId, long tradeRebateId, long amount, CurrencyTypes currency,
        int onHoldDays = 0, string? information = "")
        => new()
        {
            Amount = amount,
            PartyId = partyId,
            AccountId = accountId,
            TradeRebateId = tradeRebateId,
            CurrencyId = (int)currency,
            IdNavigation = Matter.Build().Rebate(),
            HoldUntilOn = DateTime.UtcNow.AddDays(onHoldDays),
            Information = information ?? ""
        };

    public sealed class ClientResponseModel
    {
        public bool IsEmpty() => Id == 0;
        public long Id { get; set; }
        public long Amount { get; set; }
        public int CurrencyId { get; set; }
        public DateTime? HoldUntilOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public long AccountUid { get; set; }
        public string? Information { get; set; } = string.Empty;
        public dynamic? Info => JsonConvert.SerializeObject(Information ?? "{}");
        public int StateId { get; set; }
        public long? TradeRebateId { get; set; }
        public TradeRebate.SummaryResponseModel? Trade { get; set; }
    }

    public sealed class DailyReport
    {
        public string Date { get; set; } = string.Empty;

        public int CurrencyId { get; set; }

        public long Amount { get; set; }

        public int Count { get; set; }

        public int StateId { get; set; }

        // reminder: just for Completed rebates only for now
        public static List<DailyReport> Of(IEnumerable<M> rebates) =>
            rebates
                //.GroupBy(x => new { x.Matter.StateId, x.Matter.StatedOn, x.CurrencyId })
                .GroupBy(x => new { x.IdNavigation.StatedOn, x.CurrencyId })
                .Select(x => new DailyReport
                {
                    Date = x.Key.StatedOn.ToString("yyyy-MM-dd"),
                    CurrencyId = x.Key.CurrencyId,
                    Amount = x.Sum(y => y.Amount),
                    Count = x.Count(),
                    StateId = 0,
                })
                .OrderBy(x => x.Date)
                .ThenBy(x => x.CurrencyId)
                .ThenBy(x => x.StateId)
                .ToList();
    }
}

public static class RebateExtensions
{
    public static IEnumerable<RebateNew> ToRebateNew(this List<M> rebates)
        => rebates.Where(x => x.TradeRebateId != null).Select(x => new RebateNew
        {
            Id = x.Id,
            AccountId = x.AccountId,
            Amount = x.Amount,
            CurrencyId = x.CurrencyId,
            FundType = x.FundType,
            Information = x.Information,
            PostedOn = x.IdNavigation.PostedOn,
            StateId = x.IdNavigation.StateId,
            StatedOn = x.IdNavigation.StatedOn,
            TradeRebateId = x.TradeRebateId!.Value
        });

    public static IQueryable<M.ClientResponseModel> ToClientResponseModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientResponseModel
        {
            Id = x.Id,
            TradeRebateId = x.TradeRebateId,
            Amount = x.Amount,
            AccountUid = x.Account.Uid,
            CurrencyId = x.CurrencyId,
            HoldUntilOn = x.HoldUntilOn,
            CreatedOn = x.IdNavigation.PostedOn,
            Information = x.Information,
            StateId = x.IdNavigation.StateId,
            Trade = x.TradeRebate == null
                ? new TradeRebate.SummaryResponseModel()
                : new TradeRebate.SummaryResponseModel
                {
                    Id = x.TradeRebate.Id,
                    Symbol = x.TradeRebate.Symbol,
                    AccountNumber = x.TradeRebate.AccountNumber,
                    Ticket = x.TradeRebate.Ticket,
                    Volume = x.TradeRebate.Volume,
                    AccountName = x.TradeRebate.Account != null
                        ? x.TradeRebate.Account.Party.NativeName
                        : "",
                    AccountUid = x.TradeRebate.Account != null
                        ? x.TradeRebate.Account.Uid
                        : 0,
                    CloseAt = x.TradeRebate.ClosedOn,
                    CurrencyId = x.TradeRebate.Currency.Id
                }
        });
}