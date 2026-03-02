using Bacera.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class SendEmailServiceTests : Startup
{
    private readonly ISendMailService _svc;

    public SendEmailServiceTests()
    {
        var sendMailSvc = ServiceProvider.GetRequiredService<IEmailSender>();
        var tenantSvc = ServiceProvider.GetRequiredService<ITenantService>();
        _svc = ServiceProvider.GetRequiredService<ISendMailService>();
        TenantDbContext.SeedEmailTemplateAsync().Wait();
    }

    [Fact]
    public async Task SendRegisterConfirmEmailTest()
    {
        var model = new ConfirmEmailViewModel("feng@bacera.com", "Yinsen", "https://demo.bacera.com?act=test");
        var result = await _svc.ConfirmEmailAsync(model, LanguageTypes.Chinese);
        result.Item1.ShouldBeTrue();

        result = await _svc.ConfirmEmailAsync(model);
        result.Item1.ShouldBeTrue();
    }
}