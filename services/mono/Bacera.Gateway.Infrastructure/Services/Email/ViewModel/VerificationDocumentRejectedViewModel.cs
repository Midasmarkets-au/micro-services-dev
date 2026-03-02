namespace Bacera.Gateway.Services;

public class VerificationDocumentRejectedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.VerificationDocumentRejected;
    public string NativeName { get; set; } = string.Empty;
    public string Documents { get; set; } = string.Empty;
}