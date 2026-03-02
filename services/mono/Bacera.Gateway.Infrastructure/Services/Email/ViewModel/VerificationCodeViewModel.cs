namespace Bacera.Gateway.Services;

public class VerificationCodeViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.VerificationCode;
    public string VerificationCode { get; set; } = null!;
    public int ExpireMinutes { get; set; } = 10;
}