using System.Text.RegularExpressions;

namespace Bacera.Gateway.Services;

public interface IEmailViewModel : IHasEmail, IHaveBcc
{
    string TemplateTitle { get; }
    public bool IsValidReceiverEmail();
    public string GetDisplayTitle(string _);
}

public interface IRazorModel
{
}

public abstract partial class EmailViewModel : IEmailViewModel
{
    public abstract string TemplateTitle { get; }
    public string Email { get; set; } = string.Empty;
    public List<string>? BccEmails { get; set; }

    public virtual string GetDisplayTitle(string displayTitle) => displayTitle;

    public bool IsValidReceiverEmail() => IsValidReceiverEmail(Email);

    public static bool IsValidReceiverEmail(string email) => EmailRegex().IsMatch(email);

    [GeneratedRegex(@"^[a-zA-Z0-9+._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    public static partial Regex EmailRegex();
}