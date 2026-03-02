using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class SalesRebate
{
    public class BaseModel
    {
        public long TradeRebateId { get; set; }
        public long SalesAccountId { get; set; }
        public long SalesAccountUid { get; set; }
        public long Ticket { get; set; }
        public long Amount { get; set; }
        public short TradeAccountType { get; set; }
        public int TradeAccountFundType { get; set; }
        public int TradeAccountCurrencyId { get; set; }
        public SalesRebateStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? Note { get; set; }
        public long TradeAccountId { get; set; }
        public long TradeAccountUid { get; set; }
        public long TradeAccountNumber { get; set; }
        public string RebateType { get; set; } = null!;
        public long RebateBase { get; set; }
        public long? WalletAdjustId { get; set; }
        public long? WalletId { get; set; }
    }
    
    public class TenantPageModel : BaseModel
    {
        public long Id { get; set; }
    }
    
    public class UpdateSpec
    {
        public SalesRebateStatusTypes Status { get; set; }
    }
}

public static class SalesRebateExtensions
{
    public static IQueryable<SalesRebate.TenantPageModel> ToTenantPageModel(this IQueryable<SalesRebate> @event)
        => @event.Select(x => new SalesRebate.TenantPageModel
        {
            Id = x.Id,
            TradeRebateId = x.TradeRebateId,
            Ticket = x.TradeRebate.Ticket,
            SalesAccountUid = x.SalesAccount.Uid,
            Amount = x.Amount,
            TradeAccountType = x.TradeAccountType,
            TradeAccountFundType = x.TradeAccountFundType,
            TradeAccountCurrencyId = x.TradeAccountCurrencyId,
            Status = (SalesRebateStatusTypes)x.Status,
            Note = x.Note,
            TradeAccountId = x.TradeAccountId,
            TradeAccountUid = x.TradeAccount.Uid,
            TradeAccountNumber = x.TradeAccountNumber,
            RebateType = x.RebateType,
            RebateBase = x.RebateBase,
            WalletAdjustId = x.WalletAdjustId,
            WalletId = x.WalletAdjust != null ? x.WalletAdjust.WalletId : null,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });
}