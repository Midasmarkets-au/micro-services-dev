using System.Net.Mail;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Bacera.Gateway.Vendor.Amazon.Options;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Vendor.Amazon;

public class AwsEmailClientV2
{
    private readonly string _fromAddress;
    private readonly string _fromDisplayName;
    private readonly ILogger<AwsEmailClientV2> _logger;
    private readonly AmazonSimpleEmailServiceV2Client _clientV2;

    public AwsEmailClientV2(IOptions<AwsSesOptions> options, ILogger<AwsEmailClientV2>? logger = null)
    {
        var sesOptions = options.Value;
        _logger = logger ?? new NullLogger<AwsEmailClientV2>();
        _fromAddress = sesOptions.FromAddress;
        _fromDisplayName = sesOptions.FromDisplayName;
        _clientV2 = new AmazonSimpleEmailServiceV2Client(sesOptions.AccessKey, sesOptions.SecretKey,
            sesOptions.Region);
    }

    public async Task<bool> EmailInSuppressedDestinationAsync(string email)
    {
        var request = new GetSuppressedDestinationRequest
        {
            EmailAddress = email
        };
        try
        {
            var response = await _clientV2.GetSuppressedDestinationAsync(request);
            return response.SuppressedDestination != null;
        }
        catch (NotFoundException ex)
        {
            _logger.LogInformation("Email {email} is NOT in the suppression list", email);
        }
        catch (Exception ex)
        {
            BcrLog.Slack($"EmailInSuppressedDestinationAsync, An error occurred: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> PutEmailInSuppressedDestinationAsync(string email)
    {
        var request = new PutSuppressedDestinationRequest
        {
            EmailAddress = email,
            Reason = SuppressionListReason.COMPLAINT
        };
        try
        {
            var response = await _clientV2.PutSuppressedDestinationAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            BcrLog.Slack($"PutEmailInSuppressedDestinationAsync, An error occurred: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> DeleteEmailFromSuppressedDestinationAsync(string email)
    {
        var request = new DeleteSuppressedDestinationRequest
        {
            EmailAddress = email
        };
        try
        {
            var response = await _clientV2.DeleteSuppressedDestinationAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            BcrLog.Slack($"DeleteEmailFromSuppressedDestinationAsync, An error occurred: {ex.Message}");
        }

        return false;
    }
}