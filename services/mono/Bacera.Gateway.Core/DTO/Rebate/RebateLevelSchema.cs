using Newtonsoft.Json;

namespace Bacera.Gateway;

public class RebateLevelSchema
{
    public string? OptionName { get; set; }
    public AccountTypes AccountType { get; set; }
    public decimal? Pips { get; set; }
    public decimal? Commission { get; set; }
    public decimal Percentage { get; set; }
    public List<RebateLevelSchemaItem> Items { get; set; } = new();
    public List<int> AllowPips { get; set; } = new();
    public List<int> AllowCommissions { get; set; } = new();
    public string ToJson() => Utils.JsonSerializeObject(this);

    // public bool IsValidPips(decimal? pips) => pips is null || AllowPips.Any(x => x == pips);
    //
    // public bool IsValidCommission(decimal? commission) =>
    //     commission is null || AllowCommissions.Any(x => x == commission);
    //
    // public bool IsValidLevelSchemaItems(IEnumerable<RebateLevelSchemaItem>? items) =>
    //     items?.All(x => RebateSymbol.Categories.Any(y => y.Key == x.CategoryId)) ?? false;
    //
    // public bool IsValidFromRemaining(RebateLevelSchema remaining) =>
    //     remaining.Items.All(x => Items.Any(y => y.CategoryId == x.CategoryId && y.Rate >= x.Rate));

    public static RebateLevelSchema? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonConvert.DeserializeObject<RebateLevelSchema>(json);
        }
        catch (Exception)
        {
            return null;
        }
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

    public static List<RebateLevelSchema> BuildDefaultAllowedAccountTypesForRebateLevelSetting(
        IEnumerable<RebateLevelSchema> schema)
    {
        var allowedAccountsInLevelSetting = schema.ToList();
        foreach (var product in allowedAccountsInLevelSetting.SelectMany(allowedAccount => allowedAccount.Items))
        {
            product.Rate = 0;
        }

        return allowedAccountsInLevelSetting;
    }


    public bool IsEmpty() => !Items.Any();

    public RebateLevelSchema? Clone() => FromJson(ToJson());
}

public sealed class RebatePercentageLevelSchema
{
    public string OptionName { get; set; } = string.Empty;
    public AccountTypes AccountType { get; set; }
    public Dictionary<string, List<decimal>> PercentageSetting { get; set; } = new();

    public decimal GetPips() => OptionName switch
    {
        "SEA Standard" => 5,
        "SEA Standard 10" => 10,
        "SEA Standard 15" => 15,
        _ => 0
    };
}