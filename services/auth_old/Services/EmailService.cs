using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Net.Mail;

namespace Bacera.Gateway.Auth.Services;

/// <summary>
/// Minimal AWS SES email sender for 2FA codes.
/// Uses the same env vars as mono (AWS_SES_*).
/// </summary>
public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    private readonly string _accessKey = config["AWS_SES_ACCESS_KEY"] ?? "";
    private readonly string _secretKey = config["AWS_SES_ACCESS_SECRET"] ?? "";
    private readonly string _region = config["AWS_SES_REGION"] ?? "ap-southeast-2";
    private readonly string _fromAddress = config["AWS_SES_FROM_ADDRESS"] ?? "";
    private readonly string _fromName = config["AWS_SES_FROM_NAME"] ?? "Notification";

    public async Task SendTwoFactorCodeAsync(string toEmail, string code)
    {
        if (string.IsNullOrEmpty(_accessKey) || string.IsNullOrEmpty(_fromAddress))
        {
            logger.LogWarning("AWS SES not configured — skipping 2FA email to {Email}", toEmail);
            return;
        }

        var subject = "Your verification code";
        var body = $"""
            <p>Your verification code is: <strong>{code}</strong></p>
            <p>This code will expire in 10 minutes.</p>
            <p>If you did not request this code, please ignore this email.</p>
            """;

        try
        {
            var region = RegionEndpoint.GetBySystemName(_region);
            var client = new AmazonSimpleEmailServiceClient(_accessKey, _secretKey, region);

            var request = new SendEmailRequest
            {
                Source = new MailAddress(_fromAddress, _fromName).ToString(),
                Destination = new Destination { ToAddresses = [toEmail] },
                Message = new global::Amazon.SimpleEmail.Model.Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content { Charset = "UTF-8", Data = body },
                        Text = new Content { Charset = "UTF-8", Data = $"Your verification code is: {code}" }
                    }
                }
            };

            await client.SendEmailAsync(request);
            logger.LogInformation("2FA email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send 2FA email to {Email}", toEmail);
        }
    }
}
