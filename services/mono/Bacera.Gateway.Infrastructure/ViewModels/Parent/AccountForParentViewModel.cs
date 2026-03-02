using Bacera.Gateway.Core.Types;
using Bacera.Gateway.ViewModels.Tenant;

namespace Bacera.Gateway.ViewModels.Parent;

public class AccountForParentViewModel
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long Id { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long PartyId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int Level { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int ParentLevel { get; set; }

    public int RelativeLevel => Level - ParentLevel;

    public long Uid { get; set; }
    public long SalesUid { get; set; }
    public long AgentUid { get; set; }
    public long AccountNumber { get; set; }
    public int ServiceId { get; set; }
    public long WalletBalanceInCents { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public decimal Credit { get; set; }
    public string Alias { get; set; } = string.Empty;
    public bool HasLevelRule { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public AccountTypes Type { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? ActiveOn { get; set; }
    public FundTypes FundType { get; set; }
    public AccountRoleTypes Role { get; set; }
    public AccountStatusTypes Status { get; set; }
    public TradeAccountBasicViewModel TradeAccount { get; set; } = new();
    public ParentUserBasicModel User { get; set; } = ParentUserBasicModel.Empty();
    public static AccountForParentViewModel Empty() => new();
    public virtual ICollection<Tag> AccountTags { get; set; } = new List<Tag>();
}

public class AccountForSalesViewModel
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int Level { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int ParentLevel { get; set; }

    public int RelativeLevel => Level - ParentLevel;

    public long Uid { get; set; }
    public long AccountNumber { get; set; }
    public int ServiceId { get; set; }
    public long WalletBalanceInCents { get; set; }
    public decimal Credit { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool HasLevelRule { get; set; }
    public bool HasPips { get; set; }
    public bool HasCommission { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public AccountTypes Type { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? ActiveOn { get; set; }
    public FundTypes FundType { get; set; }
    public AccountRoleTypes Role { get; set; }
    public AccountStatusTypes Status { get; set; }
    public TradeAccountBasicViewModel TradeAccount { get; set; } = new();
    public ParentUserBasicModel User { get; set; } = ParentUserBasicModel.Empty();
}

public sealed class ParentLevelAccountViewModel
{
    public string NativeName { get; set; } = string.Empty;
    public AccountRoleTypes Role { get; set; }
    public int RelativeLevel { get; set; }
    public long Uid { get; set; }
    public AccountTypes Type { get; set; }
}

public static class AccountForParentViewModelExtension
{
    public static IQueryable<AccountForParentViewModel> ToParentViewModel(this IQueryable<Account> query, long? partyId = null)
        => query.Select(x => new AccountForParentViewModel
        {
            Id = x.Id,
            Uid = x.Uid,
            Code = x.Role == (short)AccountRoleTypes.Sales
                ? x.Code
                : x.SalesAccount != null
                    ? x.SalesAccount.Code
                    : string.Empty,
            Credit = (decimal)(x.TradeAccountStatus != null ? x.TradeAccountStatus.Credit : 0),
            Group = x.Group,
            Alias = x.AccountAliases.Any(y => y.PartyId == partyId)
                ? x.AccountAliases.First(y => y.PartyId == partyId).Alias
                : string.Empty,
            AccountNumber = x.AccountNumber,
            ServiceId = x.ServiceId,
            WalletBalanceInCents = x.Party.Wallets.Any(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType)
                ? x.Party.Wallets.First(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType).Balance
                : 0,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            PartyId = x.PartyId,
            CreatedOn = x.CreatedOn,
            ActiveOn = x.ActiveOn,
            UpdatedOn = x.UpdatedOn,
            Type = (AccountTypes)x.Type,
            Role = (AccountRoleTypes)x.Role,
            FundType = (FundTypes)x.FundType,
            Status = (AccountStatusTypes)x.Status,
            SalesUid = x.SalesAccount != null ? x.SalesAccount.Uid : 0,
            AgentUid = x.AgentAccount != null ? x.AgentAccount.Uid : 0,
            HasLevelRule = x.HasLevelRule == (int)HasLevelRuleTypes.HasLevelRule,
            // HasPips = x.AccountTags.Any(y => y.Name == AccountTagTypes.AddPips),
            // HasCommission = x.AccountTags.Any(y => y.Name == AccountTagTypes.AddCommission),
            Level = x.Level,

            TradeAccount = new TradeAccountBasicViewModel
            {
                AccountNumber = x.TradeAccount == null ? 0 : x.TradeAccount.AccountNumber,
                ServiceName = x.TradeAccount == null ? string.Empty : x.TradeAccount.Service.Name,
                CurrencyId = x.TradeAccount == null ? CurrencyTypes.Invalid : (CurrencyTypes)x.TradeAccount.CurrencyId,
                Balance = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Balance
                    : 0,
                Leverage = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Leverage
                    : 0,
                Equity = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Equity
                    : 0,
                Credit = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Credit
                    : 0,
            },
            AccountTags = x.Tags,
            User = x.Party.ToParentBasicViewModel()
        });

    public static IQueryable<AccountForSalesViewModel> ToSalesPageModel(this IQueryable<Account> query, long partyId, int queryLevel)
        => query.Select(x => new AccountForSalesViewModel
        {
            Uid = x.Uid,
            Code = x.Role == (short)AccountRoleTypes.Sales
                ? x.Code
                : x.SalesAccount != null
                    ? x.SalesAccount.Code
                    : string.Empty,
            Group = x.Group,
            Alias = x.AccountAliases.Any(y => y.PartyId == partyId)
                ? x.AccountAliases.First(y => y.PartyId == partyId).Alias
                : string.Empty,
            AccountNumber = x.AccountNumber,
            ServiceId = x.ServiceId,
            WalletBalanceInCents = x.Party.Wallets.Any(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType)
                ? x.Party.Wallets.First(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType).Balance
                : 0,
            Credit = (decimal)(x.TradeAccountStatus != null ? x.TradeAccountStatus.Credit : 0),
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            CreatedOn = x.CreatedOn,
            ActiveOn = x.ActiveOn,
            UpdatedOn = x.UpdatedOn,
            Type = (AccountTypes)x.Type,
            Role = (AccountRoleTypes)x.Role,
            FundType = (FundTypes)x.FundType,
            Status = (AccountStatusTypes)x.Status,
            HasLevelRule = x.HasLevelRule == (int)HasLevelRuleTypes.HasLevelRule,
            HasPips = x.Tags.Any(y => y.Name == AccountTagTypes.AddPips),
            HasCommission = x.Tags.Any(y => y.Name == AccountTagTypes.AddCommission),
            ParentLevel = queryLevel,
            Level = x.Level,
            NativeName = x.Party.NativeName,

            TradeAccount = new TradeAccountBasicViewModel
            {
                AccountNumber = x.TradeAccount == null ? 0 : x.TradeAccount.AccountNumber,
                ServiceName = x.TradeAccount == null ? string.Empty : x.TradeAccount.Service.Name,
                CurrencyId = x.TradeAccount == null ? CurrencyTypes.Invalid : (CurrencyTypes)x.TradeAccount.CurrencyId,
                Balance = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Balance
                    : 0,
                Leverage = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Leverage
                    : 0,
                Equity = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Equity
                    : 0,
                Credit = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                    ? x.TradeAccountStatus.Credit
                    : 0,
            },

            User = x.Party.ToParentBasicViewModel()
        });

    public static IQueryable<ParentLevelAccountViewModel> ToParentLevelViewModel(this IQueryable<Account> query, int baseLevel)
        => query.Select(x => new ParentLevelAccountViewModel
        {
            RelativeLevel = x.Level - baseLevel,
            Uid = x.Uid,
            Role = (AccountRoleTypes)x.Role,
            Type = (AccountTypes)x.Type,
            NativeName = x.Party.NativeName
        });
}