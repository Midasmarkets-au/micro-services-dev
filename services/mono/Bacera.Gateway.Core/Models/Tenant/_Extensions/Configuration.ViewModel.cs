using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway;

partial class Configuration : IEntity
{
    public class TenantViewModel
    {
        public long Id { get; set; }

        public long RowId { get; set; }
        public string DataFormat { get; set; } = string.Empty;
        [JsonIgnore] public string ValueString { get; set; } = string.Empty;

        public object Value => ParseValue(ValueString, DataFormat);

        public string Category { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public TenantViewModel ToTenantViewModel() => new()
    {
        Id = Id,
        RowId = RowId,
        DataFormat = DataFormat,
        ValueString = Value,
        Category = Category,
        Key = Key,
        Description = Description,
        CreatedOn = CreatedOn,
        UpdatedOn = UpdatedOn,
        UpdatedBy = UpdatedBy,
        Name = Name
    };


    public class ClientViewModel
    {
        [JsonIgnore] public long Id { get; set; }

        [JsonIgnore] public long RowId { get; set; }
        public string DataFormat { get; set; } = string.Empty;
        [JsonIgnore] public string ValueString { get; set; } = string.Empty;

        public object Value => ParseValue(ValueString, DataFormat);
        public string Category { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ClientMeViewModel
    {
        [JsonIgnore] public string DataFormat { get; set; } = string.Empty;
        [JsonIgnore] public string ValueString { get; set; } = string.Empty;
        public object Value => ParseValue(ValueString, DataFormat);
        public string Key { get; set; } = string.Empty;
    }

    public bool Validate() => ValidateDataType() && ValidateValue();

    private bool ValidateDataType() => new HashSet<string>
        { "long", "int", "bool", "decimal", "double", "float", "DateTime", "json" }.Contains(DataFormat);

    private bool ValidateValue() => DataFormat switch
    {
        "long" => long.TryParse(Value, out _),
        "int" => int.TryParse(Value, out _),
        "bool" => bool.TryParse(Value, out _),
        "decimal" => decimal.TryParse(Value, out _),
        "double" => double.TryParse(Value, out _),
        "float" => float.TryParse(Value, out _),
        "DateTime" => DateTime.TryParse(Value, out _),
        "json" => ValidateJson(Value),
        _ => false
    };

    private static object ParseValue(string value, string format) => format switch
    {
        "long" => long.TryParse(value, out var longResult) ? longResult : 0,
        "int" => int.TryParse(value, out var intResult) ? intResult : 0,
        "bool" => bool.TryParse(value, out var boolResult) && boolResult,
        "decimal" => decimal.TryParse(value, out var decimalResult) ? decimalResult : 0,
        "double" => double.TryParse(value, out var doubleResult) ? doubleResult : 0,
        "float" => float.TryParse(value, out var floatResult) ? floatResult : 0,
        "DateTime" => DateTime.TryParse(value, out var dateTimeResult) ? dateTimeResult : DateTime.MinValue,
        "json" => ValidateJson(value) ? Utils.JsonDeserializeDynamic(value) : new { },
        _ => new { }
    };

    public sealed class ForAccountModel
    {
        [JsonIgnore] public string KeyString { get; set; } = string.Empty;
        [JsonIgnore] public string DataFormat { get; set; } = string.Empty;
        [JsonIgnore] public string ValueString { get; set; } = string.Empty;

        public string Key => Utils.PascalToCamelCase(KeyString);

        public object Value => ParseValue(ValueString, DataFormat);
    }
}

public static class ConfigurationViewModelExt
{
    public static IQueryable<Configuration.TenantViewModel> ToTenantViewModel(this IQueryable<Configuration> query)
        => query.Select(x => new Configuration.TenantViewModel
        {
            Id = x.Id,
            RowId = x.RowId,
            DataFormat = x.DataFormat,
            ValueString = x.Value,
            Category = x.Category,
            Key = x.Key,
            Description = x.Description,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            UpdatedBy = x.UpdatedBy,
            Name = x.Name
        });

    public static IQueryable<Configuration.ClientMeViewModel> ToClientMeViewModel(this IQueryable<Configuration> query)
        => query.Select(x => new Configuration.ClientMeViewModel
        {
            DataFormat = x.DataFormat,
            ValueString = x.Value,
            Key = x.Key,
        });
}