namespace Bacera.Gateway.Services;

public class ReportConfiguration
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> MailTo { get; set; } = new();
    public List<string> MailCc { get; set; } = new();
    public DateTime From { get; set; } = DateTime.MinValue;
    public DateTime To { get; set; } = DateTime.MaxValue;
    public TimeSpan TimezoneOffset { get; set; } = TimeSpan.Zero;
    public List<ReportConfigurationItem> Items { get; set; } = new();
}