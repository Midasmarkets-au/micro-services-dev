using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class RebateBrokerRule
{
    public bool IsEmpty() => Id == 0;

    public class UpdateSpec
    {
        [Required] public Dictionary<int, long> Schema { get; set; } = null!;
        [Required] public List<int> AllowAccountTypes { get; set; } = null!;
        [Required] public List<int> AllowAccountRoles { get; set; } = null!;
    }

    public class CreateSpec : UpdateSpec
    {
        [Required] public long BrokerAccountId { get; set; }
    }

    public class ResponseModel
    {
        public long Id { get; set; }

        public long BrokerAccountUid { get; set; }
        public Account.ResponseModel BrokerAccount { get; set; } = new();

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [JsonIgnore] public string AllowAccountTypesJson { get; set; } = string.Empty;
        [JsonIgnore] public string AllowAccountRolesJson { get; set; } = string.Empty;
        [JsonIgnore] public string SchemaJson { get; set; } = string.Empty;

        public List<int> AllowAccountTypes
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrEmpty(AllowAccountTypesJson)
                               ? AllowAccountTypesJson
                               : "[]")
                           ?? new List<int>();
                }
                catch (Exception)
                {
                    return new List<int>();
                }
            }
        }

        public List<int> AllowAccountRoles
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrEmpty(AllowAccountRolesJson)
                               ? AllowAccountRolesJson
                               : "[]")
                           ?? new List<int>();
                }
                catch (Exception)
                {
                    return new List<int>();
                }
            }
        }

        public Dictionary<int, long> Schema
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<int, long>>(!string.IsNullOrEmpty(SchemaJson)
                               ? SchemaJson
                               : "{}")
                           ?? new Dictionary<int, long>();
                }
                catch (Exception)
                {
                    return new Dictionary<int, long>();
                }
            }
        }

        public bool IsEmpty() => Id == 0;
    }
}

public static class BrokerRebateRuleExt
{
    public static RebateBrokerRule.ResponseModel ToResponseModel(this RebateBrokerRule item)
        => new()
        {
            Id = item.Id,
            BrokerAccountUid = item.BrokerAccount?.Uid ?? 0,
            CreatedOn = item.CreatedOn,
            UpdatedOn = item.CreatedOn,
            AllowAccountRolesJson = item.AllowAccountRoles,
            AllowAccountTypesJson = item.AllowAccountTypes,
            SchemaJson = item.Schema
        };

    public static IQueryable<RebateBrokerRule.ResponseModel> ToResponseModels(this IQueryable<RebateBrokerRule> query)
        => query.Select(x => new RebateBrokerRule.ResponseModel
        {
            Id = x.Id,
            BrokerAccountUid = x.BrokerAccount.Uid,
            BrokerAccount = new Account.ResponseModel
            {
                Uid = x.BrokerAccount.Uid,
                Type = (AccountTypes)x.BrokerAccount.Type,
                Role = (AccountRoleTypes)x.BrokerAccount.Role,
                Name = x.BrokerAccount.Name,
                HasTradeAccount = x.BrokerAccount.HasTradeAccount,
            },
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            AllowAccountRolesJson = x.AllowAccountRoles,
            AllowAccountTypesJson = x.AllowAccountTypes,
            SchemaJson = x.Schema
        });
}