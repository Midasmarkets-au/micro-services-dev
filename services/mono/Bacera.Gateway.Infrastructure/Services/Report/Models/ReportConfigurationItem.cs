namespace Bacera.Gateway.Services;

public class ReportConfigurationItem
{
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CurrencyTypes CurrencyId { get; set; }
    public List<long>? AccountNumbers { get; set; } = new();
    public List<string> IncludeSalesCode { get; set; } = new();
    public List<string> ExcludeSalesCode { get; set; } = new();
    public List<string> IncludeGroupStartWith { get; set; } = new();
    public List<string> ExcludeGroupStartWith { get; set; } = new();

    public List<string> IncludeTradeGroup { get; set; } = new();
    public List<string> ExcludeTradeGroup { get; set; } = new();
    public List<long> IncludeAccountNumber { get; set; } = new();
    public List<long> ExcludeAccountNumber { get; set; } = new();
    public AccountTypes? IncludeAccountType { get; set; } = null;
    public AccountTypes? ExcludeAccountType { get; set; } = null;
}