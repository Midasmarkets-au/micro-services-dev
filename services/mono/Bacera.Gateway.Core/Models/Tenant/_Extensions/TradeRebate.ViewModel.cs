namespace Bacera.Gateway;

using M = TradeRebate;

partial class TradeRebate
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }

        public long? AccountId { get; set; }

        public int TradeServiceId { get; set; }

        public long Ticket { get; set; }

        public long AccountNumber { get; set; }

        public int CurrencyId { get; set; }

        public int Volume { get; set; }

        public int Status { get; set; }

        public int RuleType { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public DateTime ClosedOn { get; set; }

        public DateTime OpenedOn { get; set; }

        public long TimeStamp { get; set; }

        public int Action { get; set; }
        public long DealId { get; set; }

        public string Symbol { get; set; } = null!;
        public string ReferPath { get; set; } = null!;

        public double Commission { get; set; }
        public double Swaps { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public double Profit { get; set; }

        public long TargetWalletId { get; set; }
    }
}

public static class TradeRebateViewModelExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q) => q.Select(x => new M.TenantPageModel
    {
        Id = x.Id,
        AccountId = x.AccountId,
        TradeServiceId = x.TradeServiceId,
        Ticket = x.Ticket,
        AccountNumber = x.AccountNumber,
        CurrencyId = x.CurrencyId,
        Volume = x.Volume,
        Status = x.Status,
        RuleType = x.RuleType,
        CreatedOn = x.CreatedOn,
        UpdatedOn = x.UpdatedOn,
        ClosedOn = x.ClosedOn,
        OpenedOn = x.OpenedOn,
        TimeStamp = x.TimeStamp,
        Action = x.Action,
        DealId = x.DealId,
        Symbol = x.Symbol,
        ReferPath = x.ReferPath,
        Commission = x.Commission,
        Swaps = x.Swaps,
        OpenPrice = x.OpenPrice,
        ClosePrice = x.ClosePrice,
        Profit = x.Profit,
    });
}