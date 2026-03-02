namespace Bacera.Gateway.Services;

public class SymbolReportConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> MailTo { get; set; } = new();
    public List<string> MailCc { get; set; } = new();
    public DateTime Date { get; set; } = DateTime.MinValue;
    public TimeSpan TimezoneOffset { get; set; } = TimeSpan.Zero;
    public List<SymbolReportConfigurationItem> Items { get; set; } = new();
}

public class SymbolReportConfigurationItem
{
    public string Name { get; set; } = null!;
    public int Size { get; set; }
    public string Code { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string Category { get; set; } = null!;
}