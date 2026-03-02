using System.Net.Mail;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Bacera.Gateway.Vendor.Amazon.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Vendor.Amazon;

public class AwsEmailSender : IEmailSender
{
    private readonly string _fromAddress;
    private readonly string _fromDisplayName;
    private readonly ILogger<AwsEmailSender> _logger;
    private readonly AmazonSimpleEmailServiceClient _client;

    public AwsEmailSender(IOptions<AwsSesOptions> options, ILogger<AwsEmailSender>? logger = null)
    {
        var sesOptions = options.Value;
        _logger = logger ?? new NullLogger<AwsEmailSender>();
        _fromAddress = sesOptions.FromAddress;
        _fromDisplayName = sesOptions.FromDisplayName;
        _client = new AmazonSimpleEmailServiceClient(sesOptions.AccessKey, sesOptions.SecretKey,
            sesOptions.Region);
    }

    public async Task<Tuple<bool, string>> SendEmailAsync(string email, string subject, string message,
        List<string>? bcc = null) => await SendEmailAsync(new List<string> { email }, subject, message, _fromAddress,
        _fromDisplayName, bcc);

    public async Task<Tuple<bool, string>> SendEmailAsync(List<string> emails, string subject, string message,
        List<string>? bcc = null)
        => await SendEmailAsync(emails, subject, message, _fromAddress, _fromDisplayName, bcc);

    public async Task<Tuple<bool, string>> SendEmailAsync(string email, string subject, string message,
        string senderEmailAddress, string senderDisplayName, List<string>? bcc = null)
        => await SendEmailAsync(new List<string> { email }, subject, message, senderEmailAddress, senderDisplayName,
            bcc);

    public async Task<Tuple<bool, string>> SendEmailAsync(List<string> emails, string subject, string message,
        string senderEmailAddress, string senderDisplayName, List<string>? bcc = null, List<string>? cc = null)
    {
        var request = new SendEmailRequest
        {
            Source = new MailAddress(senderEmailAddress, senderDisplayName).ToString(),

            Destination = new Destination { ToAddresses = emails },
            Message = new global::Amazon.SimpleEmail.Model.Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = message
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = ""
                    }
                }
            }
        };

        if (bcc != null && bcc.Any())
            request.Destination.BccAddresses = bcc;

        if (cc != null && cc.Any())
            request.Destination.CcAddresses = cc;

        try
        {
            var response = await _client.SendEmailAsync(request);
            _logger.LogInformation("Email {Subject} sent to {Email} Message ID = {MessageId}", subject, emails,
                response.MessageId);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK
                ? Tuple.Create(true, response.MessageId)
                : Tuple.Create(false, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Email {Subject} send to {Email} failed with exception: {Message}", subject, emails,
                ex.Message);
            //return Tuple.Create(false, ex.Message);
            throw;
        }
    }
}