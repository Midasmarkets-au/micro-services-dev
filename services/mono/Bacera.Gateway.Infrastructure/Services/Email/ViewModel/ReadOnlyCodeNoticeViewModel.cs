namespace Bacera.Gateway.Services;

public class ReadOnlyCodeNoticeViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ReadOnlyCodeNotice;
    public string NativeName { get; set; } = string.Empty;
    public string ReadOnlyCode { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
}