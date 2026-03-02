using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class RebateAgentRule
{
    public bool IsEmpty() => Id == 0;

    public List<RebateLevelSchema> GetSchema() => RebateLevelSchema.ListFromJson(Schema) ?? [];

    public (RebateLevelSchemaItem?, decimal) GetSchemaItemAndPercentage(AccountTypes accountType, long symbolCategoryId)
    {
        var schema = GetSchema();
        var accountTypeSchema = schema.FirstOrDefault(x => x.AccountType == accountType);
        if (accountTypeSchema == null) return (null, 0);
      
        var schemaItem = accountTypeSchema.Items.FirstOrDefault(x => x.CategoryId == symbolCategoryId);
        return schemaItem == null ? (null, 0) : (schemaItem, accountTypeSchema.Percentage);

        // var symbolCategoryId = RebateSymbol.ClientAll
        //     .Where(x => x.Code == symbolCode)
        //     .Select(x => x.CategoryId)
        //     .FirstOrDefault();
    }

    public RebateLevelSetting GetLevelSetting()
    {
        RebateLevelSetting? setting;
        try
        {
            setting = JsonConvert.DeserializeObject<RebateLevelSetting>(LevelSetting) ?? new RebateLevelSetting();
        }
        catch
        {
            return new RebateLevelSetting();
        }

        return setting;
    }

    public class ResponseModel
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }

        public long AgentAccountUid { get; set; }
        public long AgentAccountId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        [JsonIgnore] public string SchemaJson { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string LevelSettingJson { get; set; } = "{}";

        public RebateLevelSetting? CalculatedLevelSetting { get; set; }

        public List<RebateLevelSchema> Schema
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<RebateLevelSchema>>(SchemaJson)
                           ?? new List<RebateLevelSchema>();
                }
                catch
                {
                    return new List<RebateLevelSchema>();
                }
            }
        }

        public RebateLevelSetting LevelSetting
        {
            get
            {
                if (CalculatedLevelSetting != null)
                    return CalculatedLevelSetting;
                try
                {
                    return JsonConvert.DeserializeObject<RebateLevelSetting>(LevelSettingJson)
                           ?? new RebateLevelSetting();
                }
                catch
                {
                    return new RebateLevelSetting();
                }
            }
        }

        public bool IsEmpty() => Id == 0;
        public bool IsRoot;
    }

    public class RebateLevelSetting
    {
        public RebateDistributionTypes DistributionType { get; set; } = RebateDistributionTypes.Allocation;
        public Dictionary<string, List<decimal>> PercentageSetting { get; set; } = new();
        public List<RebateLevelSchema> AllowedAccounts { get; set; } = [];
        public string? Language { get; set; } = string.Empty;
        public bool IsEmpty() => AllowedAccounts.Count == 0;

        public static RebateLevelSetting Build(List<RebateLevelSchema> allowedAccounts, string? language = LanguageTypes.English)
            => new()
            {
                AllowedAccounts = allowedAccounts,
                Language = language,
            };
    }

    // public static string GetLevelPercentageSymbolCategory(string trimmedSymbol)
    // {
    //     var category = RebateSymbol.ClientAll
    //         .Where(x => x.Code == trimmedSymbol)
    //         .Select(x => x.Category)
    //         .FirstOrDefault() ?? "OTHER";
    //
    //     category = category.ToUpper();
    //     if (category.Contains("FOREX")) return "FOREX";
    //     return category == "GOLD" ? "GOLD" : "OTHER";
    // }

    public class CreateSpec
    {
        [Required] public long AgentAccountId { get; set; }
        [Required] public List<RebateLevelSchema> Schema { get; set; } = new();
        [Required] public RebateLevelSetting LevelSetting { get; set; } = new();
    }

    public class UpdateSpec
    {
        [Required] public List<RebateLevelSchema> Schema { get; set; } = new();
    }

    public class DefaultLevelSetting
    {
        public string? OptionName { get; set; } = string.Empty;
        public Dictionary<int, double> Category { get; set; } = new();
        public List<int> AllowPipOptions { get; set; } = new();
        public Dictionary<int, PipSetting> AllowPipSetting { get; set; } = new();
        public List<int> AllowCommissionOptions { get; set; } = new();
        public Dictionary<int, CommissionSetting> AllowCommissionSetting { get; set; } = new();
    }

    public class PipSetting
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public Dictionary<int, double> Items { get; set; } = new();
    }

    public class CommissionSetting
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public Dictionary<int, double> Items { get; set; } = new();
    }
}

public static class AgentRebateRuleExtension
{
    public static IQueryable<RebateAgentRule.ResponseModel> ToResponseModel(this IQueryable<RebateAgentRule> query)
        => query.Select(x => new RebateAgentRule.ResponseModel
        {
            Id = x.Id,
            ParentId = x.ParentId,
            AgentAccountUid = x.AgentAccount.Uid,
            AgentAccountId = x.AgentAccountId,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            SchemaJson = x.Schema,
            LevelSettingJson = x.LevelSetting,
            IsRoot = x.AgentAccount.AgentAccountId == x.AgentAccount.Id || x.AgentAccount.AgentAccountId == null,
        });

    public static RebateAgentRule.ResponseModel ToResponseModel(this RebateAgentRule model)
        => new()
        {
            Id = model.Id,
            ParentId = model.ParentId,
            AgentAccountUid = model.AgentAccount?.Uid ?? 0,
            AgentAccountId = model.AgentAccountId,
            CreatedOn = model.CreatedOn,
            UpdatedOn = model.UpdatedOn,
            SchemaJson = model.Schema,
            LevelSettingJson = model.LevelSetting,
        };
}