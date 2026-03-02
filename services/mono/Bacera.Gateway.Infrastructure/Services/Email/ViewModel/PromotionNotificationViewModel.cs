namespace Bacera.Gateway.Services;

public class PromotionNotificationViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.PromotionNotification;
    public long AccountNumber { get; set; }
    public DateOnly Date { get; set; }
    public override string GetDisplayTitle(string templateTitle) => $"{templateTitle} ({AccountNumber})"; 
}