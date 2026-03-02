using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class ReferralCode
{
    public object? DisplaySummary => JsonConvert.DeserializeObject<object>(Summary);

    public CentralReferralCode ToCentralReferralCode(long tenantId)
        => new()
        {
            Code = Code,
            Name = Name,
            TenantId = tenantId,
            PartyId = PartyId,
            AccountId = AccountId,
            CreatedOn = CreatedOn,
            UpdatedOn = UpdatedOn,
        };


    public ClientCreateSpec? TryGetClientCreateSpec()
    {
        try
        {
            return JsonConvert.DeserializeObject<ClientCreateSpec>(Summary);
        }
        catch
        {
            return null;
        }
    }

    public AgentCreateSpec? TryGetAgentCreateSpec()
    {
        try
        {
            var result = JsonConvert.DeserializeObject<AgentCreateSpec>(Summary);
            return result?.Schema == null ? null : result;
        }
        catch
        {
            return null;
        }
    }

    public ReferralCode()
    {
        Code = string.Empty;
        Summary = "{}";
    }

    public sealed class CreateSpec
    {
        public string? Name { get; set; }
        public long PartyId { get; set; }
        public long AccountId { get; set; }
        public ReferralServiceTypes ServiceType { get; set; }
        public string? Summary { get; set; }
    }

    public sealed class UpdateSpec
    {
        public string Name { get; set; } = string.Empty;
        public short Status { get; set; }
    }

    public sealed class UpdateDefaultPaymentMethodSpec
    {
        public List<long> PaymentMethodIds { get; set; } = new();
        public string Type { get; set; } // "Withdrawal" or "Deposit"
    }

    public class WithDefaultPaymentMethodResponseModel
    {
        // All ReferralCode properties
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = null!;
        public long PartyId { get; set; }
        public long AccountId { get; set; }
        public ReferralServiceTypes ServiceType { get; set; }
        public ReferralCodeStatusTypes Status { get; set; }
        public int IsDefault { get; set; }
        public string Summary { get; set; } = "{}";
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int IsAutoCreatePaymentMethod { get; set; }
        
        // Navigation properties
        public Party? Party { get; set; }
        public Account? Account { get; set; }
        public List<Referral> Referrals { get; set; } = new();
        
        // Computed property
        public object? DisplaySummary { get; set; }
        
        // Additional configuration fields
        public List<long> DefaultAutoCreatePaymentMethod { get; set; } = new();
        public List<long> DefaultAutoCreateWithdrawalPaymentMethod { get; set; } = new();
    }

    public sealed class AgentCreateSpec
    {
        [Required] public string Name { get; set; } = string.Empty;
        public List<RebateLevelSchema> Schema { get; set; } = [];
        public RebatePercentageLevelSchema PercentageSchema { get; set; } = new();
        public RebateDistributionTypes DistributionType { get; set; } = RebateDistributionTypes.Allocation;
        public string? Language { get; set; } = string.Empty;
        public SiteTypes? SiteId { get; set; } = SiteTypes.Default;
        public int IsAutoCreatePaymentMethod { get; set; }
    }

    public sealed class ClientCreateSpec
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        [Required] public List<RebateClientRule.RebateAllowedAccountTypes> AllowAccountTypes { get; set; } = null!;

        public RebatePercentageLevelSchema PercentageSchema { get; set; } = new();
        public RebateDistributionTypes DistributionType { get; set; } = RebateDistributionTypes.Allocation;
        public string? Language { get; set; } = string.Empty;
        public SiteTypes? SiteId { get; set; } = SiteTypes.Default;
        public int IsAutoCreatePaymentMethod { get; set; }
}

    public class ResponseModel
    {
        public string Name { get; set; } = string.Empty;
        public int IsDefault { get; set; }
        public string Code { get; set; } = null!;
        public ReferralServiceTypes? ServiceType { get; set; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SupplementJson { get; set; } = "{}";

        public dynamic? Supplement => JsonConvert.DeserializeObject<dynamic>(SupplementJson);

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SummaryJson { get; set; } = "{}";

        public dynamic? Summary => JsonConvert.DeserializeObject<dynamic>(SummaryJson);

        // public dynamic? Summary
        // {
        //     get
        //     {
        //         if (ServiceType == ReferralServiceTypes.Client) return JsonConvert.DeserializeObject<ClientResponseModel>(SummaryJson);
        //         var summary = Utils.JsonDeserializeObjectWithDefault<AgentCreateSpec>(SummaryJson);
        //         if (summary.DistributionType != RebateDistributionTypes.LevelPercentage) return summary;
        //         summary.PercentageSchema.PercentageSetting = summary.PercentageSchema.PercentageSetting.Select(x => Math.Round(x * 100, 0)).ToList();
        //         return summary;
        //     }
        // }
        public int ReferredCount { get; set; }

        public int IsAutoCreatePaymentMethod { get; set; }
    }

    public class ClientResponseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = null!;
        public ReferralServiceTypes? ServiceType { get; set; }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SupplementJson { get; set; } = "{}";

        public dynamic? Supplement => JsonConvert.DeserializeObject<dynamic>(SupplementJson);

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SummaryJson { get; set; } = "{}";

        public object? Summary => JsonConvert.DeserializeObject<object>(SummaryJson);
    }

    public class TenantWithAccountInfoResponseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = null!;
        public int ReferredCount { get; set; }
        public ReferralServiceTypes? ServiceType { get; set; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SupplementJson { get; set; } = "{}";

        public dynamic? Supplement => JsonConvert.DeserializeObject<dynamic>(SupplementJson);

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SummaryJson { get; set; } = "{}";

        public long AccountId { get; set; } = 0;
        public long SalesAccountId { get; set; } = 0;
        public string SalesName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string SalesAccountCode { get; set; } = string.Empty;
        public string AccountGroupName { get; set; } = string.Empty;
    }

    public static ReferralCode Build(ReferralServiceTypes type, long accountId, long partyId, string code,
        string name = "")
        => new()
        {
            PartyId = partyId,
            AccountId = accountId,
            ServiceType = (int)type,
            Code = code,
            Name = string.IsNullOrEmpty(name) ? code : name,
        };

    public static string GetHashSalt() => "__HASH_CODE_SALT__";
}

public static class ReferralCodeExtension
{
    public static IQueryable<ReferralCode.ResponseModel> ToResponse(this IQueryable<ReferralCode> query) =>
        query.Select(x => new ReferralCode.ResponseModel
        {
            //Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            IsDefault = x.IsDefault,
            SummaryJson = x.Summary,
            ServiceType = (ReferralServiceTypes)x.ServiceType,
            ReferredCount = x.Referrals.Count,
            IsAutoCreatePaymentMethod = x.IsAutoCreatePaymentMethod
        });

    public static IQueryable<ReferralCode.ResponseModel> ToClientResponse(this IQueryable<ReferralCode> query) =>
        query.Select(x => new ReferralCode.ResponseModel
        {
            //Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            IsDefault = x.IsDefault,
            SummaryJson = x.Summary,
            ServiceType = (ReferralServiceTypes)x.ServiceType,
            IsAutoCreatePaymentMethod = x.IsAutoCreatePaymentMethod,
        });

    public static ReferralCode.ResponseModel ToClientResponse(this ReferralCode model) =>
        new()
        {
            //Id = x.Id,
            Code = model.Code,
            Name = model.Name,
            SummaryJson = model.Summary,
            ServiceType = (ReferralServiceTypes)model.ServiceType,
            IsAutoCreatePaymentMethod = model.IsAutoCreatePaymentMethod
        };

    // public static IQueryable<ReferralCode.TenantWithAccountInfoResponseModel> ToTenantWithAccountInfoResponse(
    //     this IQueryable<ReferralCode> query) =>
    //     query
    //         .Select(x => new
    //         {
    //             ReferralCode = x,
    //             Account = x.Party.Accounts.First(y => y.Id == x.AccountId), // where the refer code is from
    //         })
    //         .Select(x => new ReferralCode.TenantWithAccountInfoResponseModel
    //         {
    //             Code = x.ReferralCode.Code,
    //             Name = x.ReferralCode.Name,
    //
    //             AccountId = x.Account.Role == (short)AccountRoleTypes.Agent ? x.Account.Id : 0,
    //             AccountName = x.Account.Role == (short)AccountRoleTypes.Agent ? x.Account.Name : "",
    //             AccountGroupName = x.Account.Role == (short)AccountRoleTypes.Agent
    //                 ? x.Account.GroupsNavigation.Any(g =>
    //                     g.Type == (short)AccountGroupTypes.Agent && g.OwnerAccountId == x.Account.Id)
    //                     ? x.Account.GroupsNavigation.First(g =>
    //                         g.Type == (short)AccountGroupTypes.Agent && g.OwnerAccountId == x.Account.Id).Name
    //                     : ""
    //                 : "",
    //
    //             SalesAccountId = x.Account.Role == (short)AccountRoleTypes.Sales
    //                 ? x.Account.Id
    //                 : x.Account.Groups.Any(g => g.Type == (short)AccountGroupTypes.Sales)
    //                     ? x.Account.Groups.First(g => g.Type == (short)AccountGroupTypes.Sales).OwnerAccount
    //                         .Id
    //                     : 0,
    //             SalesName = x.Account.Role == (short)AccountRoleTypes.Sales
    //                 ? x.Account.Name
    //                 : x.Account.Groups.Any(g => g.Type == (short)AccountGroupTypes.Sales)
    //                     ? x.Account.Groups.First(g => g.Type == (short)AccountGroupTypes.Sales).OwnerAccount
    //                         .Name
    //                     : "",
    //             SalesAccountCode = x.Account.Role == (short)AccountRoleTypes.Sales
    //                 ? x.Account.Code
    //                 : x.Account.Groups.Any(g => g.Type == (short)AccountGroupTypes.Sales)
    //                     ? x.Account.Groups.First(g => g.Type == (short)AccountGroupTypes.Sales).OwnerAccount
    //                         .Code
    //                     : "",
    //
    //             SummaryJson = x.ReferralCode.Summary,
    //             ReferredCount = x.ReferralCode.Referrals.Count,
    //             ServiceType = (ReferralServiceTypes)x.ReferralCode.ServiceType,
    //         });

    public static IQueryable<ReferralCode.TenantWithAccountInfoResponseModel> ToTenantResponseModel(
        this IQueryable<ReferralCode> query) =>
        query.Select(x => new ReferralCode.TenantWithAccountInfoResponseModel
        {
            Code = x.Code,
            Name = x.Name,
            SummaryJson = x.Summary,
            ReferredCount = x.Referrals.Count,
            ServiceType = (ReferralServiceTypes)x.ServiceType,

            AccountId = x.Account.Role == (short)AccountRoleTypes.Agent
                ? x.AccountId
                : x.Account.AgentAccount != null
                    ? x.Account.AgentAccount.Id
                    : 0,
            AccountName = x.Account.Role == (short)AccountRoleTypes.Agent
                ? x.Account.Name
                : x.Account.AgentAccount != null
                    ? x.Account.AgentAccount.Name
                    : "",
            AccountGroupName = x.Account.Role == (short)AccountRoleTypes.Agent
                ? x.Account.Group
                : x.Account.AgentAccount != null
                    ? x.Account.AgentAccount.Group
                    : "",

            SalesAccountId = x.Account.Role == (short)AccountRoleTypes.Sales
                ? x.AccountId
                : x.Account.SalesAccountId != null
                    ? x.Account.SalesAccountId!.Value
                    : 0,
            SalesName = x.Account.Role == (short)AccountRoleTypes.Sales
                ? x.Account.Name
                : x.Account.SalesAccount != null
                    ? x.Account.SalesAccount.Name
                    : "",
            SalesAccountCode = x.Account.Role == (short)AccountRoleTypes.Sales
                ? x.Account.Code
                : x.Account.SalesAccount != null
                    ? x.Account.SalesAccount.Code
                    : ""
        });
}