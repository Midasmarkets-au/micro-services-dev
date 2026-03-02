namespace Bacera.Gateway;

public interface IEmailSender
{
    Task<Tuple<bool, string>> SendEmailAsync(string email, string subject, string message, List<string>? bcc = null);

    Task<Tuple<bool, string>> SendEmailAsync(List<string> emails, string subject, string message, List<string>? bcc = null);
    
    Task<Tuple<bool, string>> SendEmailAsync(string email, string subject, string message, string senderEmailAddress , string senderDisplayName , List<string>? bcc = null);

    Task<Tuple<bool, string>> SendEmailAsync(List<string> emails, string subject, string message, string senderEmailAddress , string senderDisplayName , List<string>? bcc = null, List<string>? cc = null);
}