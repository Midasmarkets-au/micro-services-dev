using System.Text.RegularExpressions;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Interfaces;
using Newtonsoft.Json;

// ReSharper disable MergeConditionalExpression

namespace Bacera.Gateway;

partial class Account
{
    public sealed class SQSModel
    {
        public long AccountNumber { get; set; }
        public long Ticket { get; set; }
        public long TenantId { get; set; }
        public string Symbol { get; set; } = "";
        public int Volume { get; set; }
        public DateTime CloseTime { get; set; }

        public static SQSModel Build(long tenantId, long accountNumber, long ticket)
            => new()
            {
                AccountNumber = accountNumber,
                Ticket = ticket,
                TenantId = tenantId
            };
    }

    public class ChildNetStatAmountResponseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public long Uid { get; set; }
        public string Group { get; set; } = string.Empty;
        public AccountRoleTypes Role { get; set; }
        public Dictionary<int, long> DepositAmounts { get; set; } = new();
        public Dictionary<int, long> WithdrawalAmounts { get; set; } = new();

        public Dictionary<int, long> RebateAmounts { get; set; } = new();
        public Dictionary<int, long> ProfitAmounts { get; set; } = new();

        public Dictionary<int, long> NetAmounts
            => DepositAmounts.Keys
                .Union(WithdrawalAmounts.Keys)
                .ToList()
                .ToDictionary(
                    currency => currency,
                    currency => DepositAmounts.GetValueOrDefault(currency, 0) -
                                WithdrawalAmounts.GetValueOrDefault(currency, 0));
    }

    public class ChildRebateStatAmountResponseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public long Uid { get; set; }
        public string Group { get; set; } = string.Empty;
        public AccountRoleTypes Role { get; set; }
        public Dictionary<int, long> SummedAmount { get; set; } = new();
        public Dictionary<string, SymbolDict> GroupedAmount { get; set; } = new();

        public class SymbolDict
        {
            public long Volume { get; set; }
            public double Profit { get; set; }
            public Dictionary<int, long> Amounts { get; set; } = new();
        }
    }


    public class ResponseModel
    {
        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public long Id { get; set; }

        public long Uid { get; set; }
        public AccountRoleTypes Role { get; set; }
        public AccountTypes Type { get; set; }
        public FundTypes FundType { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public long AgentAccountUid { get; set; }
        public long SalesAccountUid { get; set; }

        public short Status { get; set; }
        public string Name { get; set; } = null!;
        public string SelfReferCode { get; set; } = null!;
        public string Code { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public long SiteId { get; set; }
        public bool HasLevelRule { get; set; }

        public long GroupId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool HasTradeAccount { get; set; }
        public long AccountNumber { get; set; }
        public long ServiceId { get; set; }
        public TradeAccount.ClientResponseModel? TradeAccount { get; set; }
        public bool IsEmpty() => Uid == 0;
    }

    public sealed class ClientResponseModel : ResponseModel, ICanFulfillConfigurations
    {
        public long Point { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
        public List<Configuration> Configurations { get; set; } = new();
    }

    public sealed class SummaryResponseModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Uid { get; set; }
        public AccountRoleTypes Role { get; set; }
        public FundTypes FundType { get; set; }
        public AccountTypes Type { get; set; }
        public bool HasTradeAccount { get; set; }
        public long AccountNumber { get; set; } = 0;
        public AccountStatusTypes Status { get; set; }
        public bool IsEmpty() => Uid == 0;
    }

    public sealed class LogViewModel
    {
        public long Id { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public long AccountId { get; set; }
        public long AccountNumber { get; set; }
        public long AccountUid { get; set; }

       

        // split by pascal case
        public string Action { get; set; } = string.Empty;
        public string Before { get; set; } = string.Empty;
        public string After { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public sealed class ClientPageModel
    {
        public long AccountNumber { get; set; }
        public long Uid { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
        public List<Configuration.ForAccountModel> Configurations { get; set; } = new();
        public CurrencyTypes CurrencyId { get; set; }
        public FundTypes FundType { get; set; }
        public AccountRoleTypes Role { get; set; }
        public int ServiceId { get; set; }
        public AccountStatusTypes Status { get; set; }
        public AccountTypes Type { get; set; }
        public long Leverage { get; set; }
        public DateTime CreatedOn { get; set; }
        public long Balance => (long)(BalanceDouble * 100);
        public long Credit => (long)(CreditDouble * 100);
        public long Equity => (long)(EquityDouble * 100);

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public double BalanceDouble { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public double CreditDouble { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public double EquityDouble { get; set; }
    }

    public sealed class EquityBelowCreditModel
    {
        public long Id { get; set; }
        public long AccountNumber { get; set; }
        public long Uid { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string AgentGroup { get; set; } = string.Empty;
        public string SalesCode { get; set; } = string.Empty;
        public string SalesEmail { get; set; } = string.Empty;
        public string AgentEmail { get; set; } = string.Empty;
        public string MtGroup { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public decimal Equity { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public decimal Margin { get; set; }
        public string ServiceName => NameDict.GetValueOrDefault(ServiceId, "");
        [JsonIgnore] public Dictionary<int, string> NameDict { get; set; } = new();
    }
}

public static class AccountViewModelExtensions
{
    public static IQueryable<Account.ClientPageModel> ToClientPageModel(this IQueryable<Account> query,
        long partyUid = 0)
        => query.Select(x => new Account.ClientPageModel
        {
            Id = x.Id,
            Type = (AccountTypes)x.Type,
            Role = (AccountRoleTypes)x.Role,
            FundType = (FundTypes)x.FundType,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            Status = (AccountStatusTypes)x.Status,
            Alias = partyUid != 0 && x.AccountAliases.Any(y => y.Party.Uid == partyUid)
                ? x.AccountAliases.First(y => y.Party.Uid == partyUid).Alias
                : "",
            Permission = x.Permission,
            AccountNumber = x.AccountNumber,
            Uid = x.Uid,
            ServiceId = x.ServiceId,
            CreatedOn = x.CreatedOn,
            BalanceDouble = x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Balance,
            EquityDouble = x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Equity,
            CreditDouble = x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Credit,
            Leverage = x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Leverage,
        });

    public static IQueryable<Account.ClientResponseModel> ToClientResponseModels(this IQueryable<Account> query,
        long? partyId = null)
        => query.Select(x => new Account.ClientResponseModel
        {
            Id = x.Id,
            Uid = x.Uid,
            Type = (AccountTypes)x.Type,
            Role = (AccountRoleTypes)x.Role,
            FundType = (FundTypes)x.FundType,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            Permission = x.Permission,
            HasLevelRule = x.HasLevelRule == (int)HasLevelRuleTypes.HasLevelRule,
            SelfReferCode = x.ReferralCodes.Any() ? x.ReferralCodes.First().Code : "",
            Status = x.Status,
            Name = x.Name,
            Alias = x.AccountAliases.Any(y => y.PartyId == partyId)
                ? x.AccountAliases.First(y => y.PartyId == partyId).Alias
                : "",
            SiteId = x.SiteId,
            AccountNumber = x.AccountNumber,
            ServiceId = x.ServiceId,
            AgentAccountUid = x.AgentAccount != null ? x.AgentAccount.Uid : 0,
            SalesAccountUid = x.SalesAccount != null ? x.SalesAccount.Uid : 0,
            Code = (AccountRoleTypes)x.Role == AccountRoleTypes.Sales ? x.Code :
                x.SalesAccount != null ? x.SalesAccount.Code : "",
            Group = (AccountRoleTypes)x.Role == AccountRoleTypes.Agent ? x.Group :
                x.AgentAccount != null ? x.AgentAccount.Code : "",
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            HasTradeAccount = x.HasTradeAccount,
            Point = x.AccountPoint == null ? 0 : x.AccountPoint.Balance,
            TradeAccount = x.TradeAccount == null
                ? null
                : new TradeAccount.ClientResponseModel
                {
                    Uid = x.Uid,
                    ServiceId = x.TradeAccount.ServiceId,
                    AccountNumber = x.TradeAccount.AccountNumber,
                    CurrencyId = x.TradeAccount.CurrencyId,
                    LastSyncedOn = x.TradeAccount.LastSyncedOn,
                    Margin = x.TradeAccount.TradeAccountStatus == null
                        ? 0
                        : x.TradeAccount.TradeAccountStatus.Margin,

                    Equity = x.TradeAccount.TradeAccountStatus == null
                        ? 0
                        : x.TradeAccount.TradeAccountStatus.Equity,
                    Leverage = x.TradeAccount.TradeAccountStatus == null
                        ? 0
                        : x.TradeAccount.TradeAccountStatus.Leverage,
                    Balance = x.TradeAccount.TradeAccountStatus == null
                        ? 0
                        : x.TradeAccount.TradeAccountStatus.Balance,
                    Credit = x.TradeAccount.TradeAccountStatus == null
                        ? 0
                        : x.TradeAccount.TradeAccountStatus.Credit
                },
        });

    public static IQueryable<Account.LogViewModel> ToLogViewModel(this IQueryable<AccountLog> query)
        => query.Select(x => new Account.LogViewModel
        {
            Id = x.Id,
            OperatorName = x.OperatorParty.NativeName,
            Action = x.Action,
            Before = x.Before,
            After = x.After,
            CreatedOn = x.CreatedOn,
            AccountId = x.AccountId,
            AccountNumber = x.Account.AccountNumber,
            AccountUid = x.Account.Uid
        });

    public static IQueryable<Account.EquityBelowCreditModel> ToEquityBelowCreditModel(this IQueryable<Account> query, Dictionary<int, string> map)
        => query.Select(x => new Account.EquityBelowCreditModel
        {
            Id = x.Id,
            AccountNumber = x.AccountNumber,
            Uid = x.Uid,
            Email = x.Party.Email,
            NativeName = x.Party.NativeName,
            ServiceId = x.ServiceId,
            // Equity = (decimal)(x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Equity),
            // Credit = (decimal)(x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Credit),
            // Balance = (decimal)(x.TradeAccountStatus == null ? 0 : x.TradeAccountStatus.Balance),
            // MtGroup = x.TradeAccountStatus != null ? x.TradeAccountStatus.Group! : "",
            AgentGroup = x.Group,
            AgentEmail = x.AgentAccount != null ? x.AgentAccount.Party.Email : "",
            SalesEmail = x.SalesAccount != null ? x.SalesAccount.Party.Email : "",
            SalesCode = x.SalesAccount != null ? x.SalesAccount.Code : "",
            NameDict = map
        });

    public static IQueryable<Account.EquityBelowCreditModel> ToEquityBelowCreditModel(this IQueryable<Mt4User> query, Dictionary<int, string> map)
        => query.Select(x => new Account.EquityBelowCreditModel
        {
            AccountNumber = x.Login,
            Equity = (decimal)x.Equity,
            Credit = (decimal)x.Credit,
            Balance = (decimal)x.Balance,
            Margin = (decimal)x.Margin,
            MtGroup = x.Group,
        });

    public static IQueryable<Account.EquityBelowCreditModel> ToEquityBelowCreditModel(this IQueryable<Mt5Account> query, Dictionary<int, string> map)
        => query.Select(x => new Account.EquityBelowCreditModel
        {
            AccountNumber = (long)x.Login,
            Equity = (decimal)x.Equity,
            Credit = (decimal)x.Credit,
            Balance = (decimal)x.Balance,
            Margin = (decimal)x.Margin,
        });
}