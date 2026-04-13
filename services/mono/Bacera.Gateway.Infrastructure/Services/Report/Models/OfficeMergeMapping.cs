namespace Bacera.Gateway.Services.Report.Models;

public class OfficeMergeMapping
{
    public Dictionary<string, string> PrefixMappings { get; set; } = new();
    public List<string> StaticOffices { get; set; } = new();
    public string CurrencyPrefix { get; set; } = "";
    public List<string> OfficeOrder { get; set; } = new();
}
