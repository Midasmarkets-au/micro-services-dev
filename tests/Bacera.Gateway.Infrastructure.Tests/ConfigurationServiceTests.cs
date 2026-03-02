using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class ConfigurationServiceTests : Startup
{
    private readonly ConfigurationService _svc;

    public ConfigurationServiceTests()
    {
        _svc = ServiceProvider.GetRequiredService<ConfigurationService>();
        _svc.ShouldNotBeNull();
    }

    // [Fact]
    // public async Task CreateLogSettingTest()
    // {
    //     const string url = "https://docs.duendesoftware.com/identityserver/v6";
    //     var cfg = await _svc.UpdateAsync(TenantConfigurations.LogoSetting.Create(url));
    //     cfg.ShouldNotBeNull();
    //
    //     var validate = await _svc.GetAsync<TenantConfigurations.Logo>(ConfigurationTypes.Logo);
    //     validate.ShouldNotBeNull();
    //     validate.Url.ShouldBe(url);
    // }

    //[Fact]
    //public async Task CreateMt4SettingTest()
    //{
    //    var cfg = await _svc.UpdateAsync(TenantConfigurations.Mt4Setting.Create("localhost", 443));
    //    cfg.ShouldNotBeNull();

    //}
}