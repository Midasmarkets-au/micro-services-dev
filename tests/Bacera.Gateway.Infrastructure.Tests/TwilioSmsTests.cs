using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Twilio;

using Microsoft.Extensions.Configuration;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Infrastructure)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class TwilioSmsTests
{
    private readonly ISmsVerification _svc;

    public TwilioSmsTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        var accountSid = configuration.GetValue<string>("Twilio:AccountSid") ?? string.Empty;
        var authToken = configuration.GetValue<string>("Twilio:AuthToken") ?? string.Empty;
        var serviceSid = configuration.GetValue<string>("Twilio:ServiceSid") ?? string.Empty;
        accountSid.ShouldNotBeNullOrEmpty();
        authToken.ShouldNotBeNullOrEmpty();
        serviceSid.ShouldNotBeNullOrEmpty();

        _svc = new TwilioSmsVerificationService(accountSid, authToken, serviceSid);
    }

    [Fact]
    public async Task Verification_Send_ReturnSuccess()
    {
        var result = await _svc.Verification("+1(626)818-7883", 0.ToString());
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task VerificationCheck_ReturnSuccess()
    {
        var result = await _svc.VerificationCheck("+1626-818-7883", "214153");
        result.Item1.ShouldBeTrue();
    }
}