using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class AccountViewModel : ICanFulfillConfigurations
{
    public long Id { get; set; }
    public long Uid { get; set; }
    public int Level { get; set; }
    public bool HasClosedAccount { get; set; }
    public AccountTypes Type { get; set; }
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public AccountRoleTypes Role { get; set; }
    public bool IsInUserBlackList { get; set; } = false;
    public bool IsInIpBlackList { get; set; } = false;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string ReferPath { get; set; } = string.Empty;
    public long WalletId { get; set; }
    public AccountStatusTypes Status { get; set; }
    public SiteTypes SiteId { get; set; }

    public bool? HasComment { get; set; }
    public bool HasTradeAccount { get; set; }
    public bool HasRebateRule { get; set; }
    public bool HasLevelRule { get; set; }
    public long AccountNumber { get; set; }
    public RebateDistributionTypes? ClientRebateDistributionType { get; set; }
    public int ServiceId { get; set; }
    public List<string> AccountTags { get; set; } = new();
    public List<Configuration> Configurations { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? ActiveOn { get; set; }
    public DateTime? SuspendedOn { get; set; }

    public Supplement.AccountWizard Wizard { get; set; } = new();
    public TradeAccountBasicViewModel TradeAccount { get; set; } = new();
    public AccountBasicViewModel SalesAccount { get; set; } = AccountBasicViewModel.Empty();
    public AccountBasicViewModel AgentAccount { get; set; } = AccountBasicViewModel.Empty();
    public long PartyId { get; set; }

    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();

    // public bool IsEmpty => Id == 0;
    public IEnumerable<long> GetPartyIds() => new List<long> { PartyId, SalesAccount.PartyId, AgentAccount.PartyId };
}

public static class AccountViewModelExtension
{
    public static IQueryable<AccountViewModel> ToTenantViewModel(this IQueryable<Account> query, long? partyId = null,
        bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            // .Include(x => x.AgentAccount)
            // .ThenInclude(x => x!.Party.PartyComments)
            // .Include(x => x.AgentAccount)
            // .ThenInclude(x => x!.Party.Tags)
            // .Include(x => x.SalesAccount)
            // .ThenInclude(x => x!.Party.PartyComments)
            // .Include(x => x.SalesAccount)
            // .ThenInclude(x => x!.Party.Tags)
            .Select(x => new AccountViewModel
            {
                Id = x.Id,
                Uid = x.Uid,
                Level = x.Level,
                PartyId = x.PartyId,
                Type = (AccountTypes)x.Type,
                FundType = (FundTypes)x.FundType,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                HasLevelRule = x.HasLevelRule == (int)HasLevelRuleTypes.HasLevelRule,
                HasComment = x.AccountComments.Any(),
                HasClosedAccount = x.Party.Tags.Any(t => t.Name == "HasClosedAccount"),
                Role = (AccountRoleTypes)x.Role,
                Status = (AccountStatusTypes)x.Status,
                SiteId = (SiteTypes)x.SiteId,
                AccountTags = x.Tags.Select(t => t.Name).ToList(),
                Name = x.Name,
                ReferPath = x.ReferPath,
                AccountNumber = x.AccountNumber,
                ServiceId = x.ServiceId,
                ClientRebateDistributionType = x.RebateClientRule != null ? (RebateDistributionTypes)x.RebateClientRule.DistributionType : null,
                WalletId = x.Party.Wallets.Any(w => w.FundType == x.FundType && w.CurrencyId == x.CurrencyId)
                    ? x.Party.Wallets.First(w => w.FundType == x.FundType && w.CurrencyId == x.CurrencyId).Id
                    : 0,
                Code = x.Role == (short)AccountRoleTypes.Sales
                    ? x.Code
                    : x.SalesAccount != null
                        ? x.SalesAccount.Code
                        : "",
                Group = x.Group,
                Alias = x.AccountAliases.Any(y => y.PartyId == partyId)
                    ? x.AccountAliases.First(y => y.PartyId == partyId).Alias
                    : "",
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                SuspendedOn = x.SuspendedOn,
                ActiveOn = x.ActiveOn,
                HasRebateRule = x.RebateClientRule != null
                                && (
                                    (
                                        x.RebateClientRule.DistributionType == (int)RebateDistributionTypes.Direct
                                        && x.TradeAccount != null
                                        && x.TradeAccount.RebateDirectRules.Any()
                                    )
                                    || (
                                        x.RebateClientRule.DistributionType == (int)RebateDistributionTypes.Allocation
                                        && x.RebateClientRule.RebateDirectSchemaId != null
                                        && x.AgentAccount != null
                                        && x.AgentAccount.RebateAgentRule != null
                                    )
                                ),
                HasTradeAccount = x.HasTradeAccount,
                SalesAccount = new AccountBasicViewModel
                {
                    Code = x.Role != (int)AccountRoleTypes.Sales && x.SalesAccount != null ? x.SalesAccount.Code : "",
                    Id = x.SalesAccountId != null ? x.SalesAccountId!.Value : 0,
                    Uid = x.Role != (int)AccountRoleTypes.Sales && x.SalesAccount != null ? x.SalesAccount.Uid : 0,
                    Group = x.Role != (int)AccountRoleTypes.Sales && x.SalesAccount != null ? x.SalesAccount.Group : "",
                    Name = x.Role != (int)AccountRoleTypes.Sales && x.SalesAccount != null ? x.SalesAccount.Party.NativeName : "",
                    User = x.SalesAccount == null
                        ? new TenantUserBasicModel()
                        : x.SalesAccount.Party.ToTenantBasicViewModel(hideEmail)
                    // User = x.SalesAccount == null
                    //     ? new TenantUserBasicModel()
                    //     : new TenantUserBasicModel
                    //     {
                    //         Id = 0,
                    //         PartyId = x.SalesAccount.PartyId,
                    //         Email = x.SalesAccount.Party.Email,
                    //         Avatar = x.SalesAccount.Party.Avatar,
                    //         FirstName = x.SalesAccount.Party.FirstName,
                    //         LastName = x.SalesAccount.Party.LastName,
                    //         NativeName = x.SalesAccount.Party.NativeName,
                    //         PartyTags = x.SalesAccount.Party.Tags.Select(t => t.Name).ToList(),
                    //     },
                },
                AgentAccount = new AccountBasicViewModel
                {
                    Code = x.AgentAccount != null ? x.AgentAccount.Code : "",
                    Id = x.AgentAccountId != null ? x.AgentAccountId!.Value : 0,
                    Uid = x.AgentAccount != null ? x.AgentAccount.Uid : 0,
                    Group = x.AgentAccount != null ? x.AgentAccount.Group : "",
                    Name = x.AgentAccount != null ? x.AgentAccount.Party.NativeName : "",
                    User = x.AgentAccount == null
                        ? new TenantUserBasicModel()
                        : x.AgentAccount.Party.ToTenantBasicViewModel(hideEmail)
                    // User = x.AgentAccount == null
                    //     ? new TenantUserBasicModel()
                    //     : new TenantUserBasicModel
                    //     {
                    //         Id = 0,
                    //         PartyId = x.AgentAccount.PartyId,
                    //         Email = x.AgentAccount.Party.Email,
                    //         Avatar = x.AgentAccount.Party.Avatar,
                    //         FirstName = x.AgentAccount.Party.FirstName,
                    //         LastName = x.AgentAccount.Party.LastName,
                    //         NativeName = x.AgentAccount.Party.NativeName,
                    //         PartyTags = x.AgentAccount.Party.Tags.Select(t => t.Name).ToList(),
                    //     },
                },
                TradeAccount = new TradeAccountBasicViewModel
                {
                    AccountNumber = x.AccountNumber,
                    CurrencyId = (CurrencyTypes)x.CurrencyId,
                    ServiceName = x.TradeAccount == null ? string.Empty : x.TradeAccount.Service.Name,
                    Balance = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                        ? x.TradeAccountStatus.Balance
                        : 0,
                    Leverage = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                        ? x.TradeAccountStatus.Leverage
                        : 0,
                    Credit = x.TradeAccountStatus != null && x.TradeAccountStatus != null
                        ? x.TradeAccountStatus.Credit
                        : 0,
                },
                User = x.Party.ToTenantBasicViewModel(hideEmail)
                // User = new TenantUserBasicModel
                // {
                //     Id = 0,
                //     PartyId = x.PartyId,
                //     Email = x.Party.Email,
                //     Avatar = x.Party.Avatar,
                //     FirstName = x.Party.FirstName,
                //     LastName = x.Party.LastName,
                //     NativeName = x.Party.NativeName,
                //     PartyTags = x.Party.Tags.Select(t => t.Name).ToList(),
                // },
            });
}

// public static IQueryable<AccountViewModel> ToTenantViewModel(this IQueryable<Account> query)
//         => query.Select(x => new AccountViewModel
//         {
//             Id = x.Id,
//             Uid = x.Uid,
//             PartyId = x.PartyId,
//             Type = (AccountTypes)x.Type,
//             FundType = (FundTypes)x.FundType,
//             CurrencyId = (CurrencyTypes)x.CurrencyId,
//             HasLevelRule = x.HasLevelRule == (int)HasLevelRuleTypes.HasLevelRule,
//             Role = (AccountRoleTypes)x.Role,
//             Status = (AccountStatusTypes)x.Status,
//             SiteId = (SiteTypes)x.SiteId,
//             AccountTags = x.AccountTags.Select(t => t.Name).ToList(),
//             Name = x.Name,
//             ReferPath = x.ReferPath,
//             AccountNumber = x.AccountNumber,
//             ServiceId = x.ServiceId,
//             WalletId = x.Party.Wallets.Any(w => w.FundType == x.FundType && w.CurrencyId == x.CurrencyId)
//                 ? x.Party.Wallets.First(w => w.FundType == x.FundType && w.CurrencyId == x.CurrencyId).Id
//                 : 0,
//             Code = x.Role == (short)AccountRoleTypes.Sales
//                 ? x.GroupsNavigation.Any(g => g.Type == (short)AccountGroupTypes.Sales)
//                     ? x.GroupsNavigation.First(g => g.Type == (short)AccountGroupTypes.Sales).Name
//                     : string.Empty
//                 : x.Groups.Any(g => g.Type == (short)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (short)AccountGroupTypes.Sales).Name
//                     : string.Empty,
//             Group = x.Role == (short)AccountRoleTypes.Agent
//                 ? x.GroupsNavigation.Any(g => g.Type == (short)AccountGroupTypes.Agent)
//                     ? x.GroupsNavigation.First(g => g.Type == (short)AccountGroupTypes.Agent).Name
//                     : string.Empty
//                 : x.Groups.Any(g => g.Type == (short)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (short)AccountGroupTypes.Agent).Name
//                     : x.Group,
//             AgentGroupId = x.Role == (short)AccountRoleTypes.Agent
//                 ? x.GroupsNavigation.Any(g => g.Type == (short)AccountGroupTypes.Agent)
//                     ? x.GroupsNavigation.First(g => g.Type == (short)AccountGroupTypes.Agent).Id
//                     : 0
//                 : x.Groups.Any(g => g.Type == (short)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (short)AccountGroupTypes.Agent).Id
//                     : 0,
//             SalesGroupId = x.Role == (short)AccountRoleTypes.Sales
//                 ? x.GroupsNavigation.Any(g => g.Type == (short)AccountGroupTypes.Sales)
//                     ? x.GroupsNavigation.First(g => g.Type == (short)AccountGroupTypes.Sales).Id
//                     : 0
//                 : x.Groups.Any(g => g.Type == (short)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (short)AccountGroupTypes.Sales).Id
//                     : 0,
//             CreatedOn = x.CreatedOn,
//             UpdatedOn = x.UpdatedOn,
//             HasRebateRule = x.RebateClientRule != null
//                             && (
//                                 (
//                                     x.RebateClientRule.DistributionType == (int)RebateDistributionTypes.Direct
//                                     && x.TradeAccount != null
//                                     && x.TradeAccount.RebateDirectRules.Any()
//                                 )
//                                 || (
//                                     x.RebateClientRule.DistributionType == (int)RebateDistributionTypes.Allocation
//                                     && x.RebateClientRule.RebateDirectSchemaId != null
//                                     && x.Groups.Any(ag =>
//                                         ag.Type == (int)AccountGroupTypes.Agent
//                                         && ag.OwnerAccount.RebateAgentRule != null)
//                                 )
//                             ),
//             HasTradeAccount = x.HasTradeAccount,
//             SalesAccount = new AccountBasicViewModel
//             {
//                 Code = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Sales).OwnerAccount.Code
//                     : "",
//                 Id = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Sales).OwnerAccountId
//                     : 0,
//                 Uid = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Sales).OwnerAccount.Uid
//                     : 0,
//                 Group = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Sales).Name
//                     : "",
//                 Name = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Sales)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Sales).OwnerAccount.Name
//                     : "",
//             },
//             AgentAccount = new AccountBasicViewModel
//             {
//                 Id = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Agent).OwnerAccountId
//                     : 0,
//                 Uid = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Agent).OwnerAccount.Uid
//                     : 0,
//                 PartyId = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Agent).OwnerAccount.PartyId
//                     : 0,
//                 Group = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Agent).Name
//                     : "",
//                 Name = x.Groups.Any(g => g.Type == (int)AccountGroupTypes.Agent)
//                     ? x.Groups.First(g => g.Type == (int)AccountGroupTypes.Agent).OwnerAccount.Name
//                     : "",
//             },
//             TradeAccount = new TradeAccountBasicViewModel
//             {
//                 AccountNumber = x.TradeAccount == null ? 0 : x.TradeAccount.AccountNumber,
//                 CurrencyId = x.TradeAccount == null ? CurrencyTypes.Invalid : (CurrencyTypes)x.TradeAccount.CurrencyId,
//                 ServiceName = x.TradeAccount == null ? string.Empty : x.TradeAccount.Service.Name,
//                 Balance = x.TradeAccount != null && x.TradeAccount.TradeAccountStatus != null
//                     ? x.TradeAccount.TradeAccountStatus.Balance
//                     : 0,
//                 Leverage = x.TradeAccount != null && x.TradeAccount.TradeAccountStatus != null
//                     ? x.TradeAccount.TradeAccountStatus.Leverage
//                     : 0,
//             }
//         });