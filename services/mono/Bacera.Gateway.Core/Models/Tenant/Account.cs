using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Account
{
    public long Id { get; set; }

    public long Uid { get; set; }

    public long PartyId { get; set; }

    public short Role { get; set; }

    public short Type { get; set; }

    public int ServiceId { get; set; }
    public int FundType { get; set; }

    public int CurrencyId { get; set; }
    public int Level { get; set; }

    public int HasLevelRule { get; set; }

    public long AccountNumber { get; set; }

    public long? AgentAccountId { get; set; }

    public long? ReferrerAccountId { get; set; }

    public long? SalesAccountId { get; set; }

    public long? BrokerAccountId { get; set; }
    public long? WalletId { get; set; }

    public bool HasTradeAccount { get; set; }

    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Group { get; set; } = null!;

    public string? ReferCode { get; set; }

    public string ReferPath { get; set; } = null!;
    public string SearchText { get; set; } = null!;
    public string Permission { get; set; } = null!;

    public int IsClosed { get; set; }
    public int SiteId { get; set; }

    public DateTime? ActiveOn { get; set; }
    public DateTime? SuspendedOn { get; set; }

    public virtual AccountPoint? AccountPoint { get; set; }

    public virtual ICollection<AccountPointTransaction> AccountPointTransactions { get; set; } =
        new List<AccountPointTransaction>();

    public virtual Account? AgentAccount { get; set; }

    public virtual Account? BrokerAccount { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;

    public virtual ICollection<Group> GroupsNavigation { get; set; } = new List<Group>();

    public virtual ICollection<Account> InverseAgentAccount { get; set; } = new List<Account>();

    public virtual ICollection<Account> InverseBrokerAccount { get; set; } = new List<Account>();

    public virtual ICollection<Account> InverseReferrerAccount { get; set; } = new List<Account>();

    public virtual ICollection<Account> InverseSalesAccount { get; set; } = new List<Account>();

    public virtual Party Party { get; set; } = null!;

    public virtual RebateAgentRule? RebateAgentRule { get; set; }

    public virtual RebateBrokerRule? RebateBrokerRule { get; set; }

    public virtual RebateClientRule? RebateClientRule { get; set; }

    public virtual ICollection<RebateDirectRule> RebateDirectRules { get; set; } = new List<RebateDirectRule>();

    public virtual ICollection<Rebate> Rebates { get; set; } = new List<Rebate>();

    public virtual Account? ReferrerAccount { get; set; }
    public virtual Wallet? Wallet { get; set; }

    public virtual Account? SalesAccount { get; set; }

    public virtual TradeAccount? TradeAccount { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<AccountTag> AccountTags { get; set; } = new List<AccountTag>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<AccountReport> AccountReports { get; set; } = new List<AccountReport>();
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<ReferralCode> ReferralCodes { get; set; } = new List<ReferralCode>();
    public virtual TradeAccountStatus? TradeAccountStatus { get; set; }

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    public virtual ICollection<TradeRebate> TradeRebates { get; set; } = new List<TradeRebate>();
    public virtual ICollection<AdjustRecord> AdjustRecords { get; set; } = new List<AdjustRecord>();

    public virtual ICollection<AccountLog> AccountLogs { get; set; } = new List<AccountLog>();

    public virtual ICollection<AccountAlias> AccountAliases { get; set; } = new List<AccountAlias>();

    // public virtual ICollection<EventAccount> EventAccounts { get; set; } = new List<EventAccount>();
    public virtual ICollection<EventShopClientPoint> ChildEventShopClientPoints { get; set; } =
        new List<EventShopClientPoint>();

    public virtual ICollection<EventShopClientPoint> ParentEventShopClientPoints { get; set; } =
        new List<EventShopClientPoint>();

    // public virtual SalesRebateSchema? SalesRebateSchema { get; set; }

    // public virtual ICollection<SalesRebate> SalesRebates { get; set; } = new List<SalesRebate>();
    public virtual ICollection<AccountStat> AccountStats { get; set; } = new List<AccountStat>();

    public virtual ICollection<EventShopPointTransaction> EventShopPointTransactions { get; set; } = new List<EventShopPointTransaction>();
    public virtual ICollection<SalesRebate> SalesRebateSalesAccounts { get; set; } = new List<SalesRebate>();

    public virtual ICollection<SalesRebateSchema> SalesRebateSchemaRebateAccounts { get; set; } = new List<SalesRebateSchema>();

    public virtual ICollection<SalesRebateSchema> SalesRebateSchemaSalesAccounts { get; set; } = new List<SalesRebateSchema>();

    public virtual ICollection<SalesRebate> SalesRebateTradeAccounts { get; set; } = new List<SalesRebate>();
    public virtual ICollection<AccountComment> AccountComments { get; set; } = new List<AccountComment>();
    public virtual ICollection<AccountPaymentMethodAccess> AccountPaymentMethodAccesses { get; set; } = new List<AccountPaymentMethodAccess>();

    public virtual ICollection<AccountExtraRelation> ParentAccountExtraRelations { get; set; } =
        new List<AccountExtraRelation>();

    public virtual ICollection<AccountExtraRelation> ChildAccountExtraRelations { get; set; } =
        new List<AccountExtraRelation>();
}