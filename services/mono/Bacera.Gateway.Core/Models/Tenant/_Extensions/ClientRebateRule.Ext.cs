using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class RebateClientRule
{
    // public List<RebateAllowedAccountTypes> AllowedAccountTypes
    // {
    //     get
    //     {
    //         try
    //         {
    //             var result = JsonConvert.DeserializeObject<List<RebateAllowedAccountTypes>>(Schema);
    //             return result ?? new List<RebateAllowedAccountTypes>();
    //         }
    //         catch
    //         {
    //             return new List<RebateAllowedAccountTypes>();
    //         }
    //     }
    // }

    public bool IsEmpty() => Id == 0;

    public class ResponseModel
    {
        public long Id { get; set; }
        public long ClientAccountId { get; set; }
        public RebateDistributionTypes DistributionType { get; set; }
        public Account ClientAccount { get; set; } = new();

        public List<RebateAllowedAccountTypes> Schema
        {
            get
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<List<RebateAllowedAccountTypes>>(SchemaJson);
                    return result ?? new List<RebateAllowedAccountTypes>();
                }
                catch
                {
                    return new List<RebateAllowedAccountTypes>();
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string SchemaJson { get; set; } = string.Empty;

        public long? RebateDirectSchemaId { get; set; }
        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public bool IsEmpty() => Id == 0;
    }

    public class UpdateSpec
    {
        [Required] public RebateDistributionTypes DistributionType { get; set; }
        public long? RebateDirectSchemaId { get; set; }
        public bool ShouldUpdateRebateDirectSchemaId() => RebateDirectSchemaId != null && RebateDirectSchemaId != 0;
    }

    public class RebateAllowedAccountTypes
    {
        public string OptionName { get; set; } = string.Empty;
        public AccountTypes AccountType { get; set; }
        public decimal? Pips { get; set; }
        public decimal? Commission { get; set; }
    }

    public static List<RebateLevelSchema>? ListFromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonConvert.DeserializeObject<List<RebateLevelSchema>>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public static class ClientRebateRuleExtension
{
    public static IQueryable<RebateClientRule.ResponseModel> ToResponseModel(this IQueryable<RebateClientRule> query)
        => query.Select(x => new RebateClientRule.ResponseModel
        {
            Id = x.Id,
            ClientAccountId = x.ClientAccountId,
            ClientAccount = x.ClientAccount,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            RebateDirectSchemaId = x.RebateDirectSchemaId,
            DistributionType = (RebateDistributionTypes)x.DistributionType,
            SchemaJson = x.Schema
        });

    public static RebateClientRule.ResponseModel ToResponseModel(this RebateClientRule x)
        => new()
        {
            Id = x.Id,
            ClientAccountId = x.ClientAccountId,
            ClientAccount = x.ClientAccount,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            RebateDirectSchemaId = x.RebateDirectSchemaId,
            DistributionType = (RebateDistributionTypes)x.DistributionType
        };
}