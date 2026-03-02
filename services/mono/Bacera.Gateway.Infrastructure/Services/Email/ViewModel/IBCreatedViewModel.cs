namespace Bacera.Gateway.Services.Email.ViewModel;

public class IBCreatedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.IBCreated;
    public string UserName { get; set; } = string.Empty;
    public long AccountUid { get; set; }
    public string IbCode { get; set; } = string.Empty;
}