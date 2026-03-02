using Bacera.Gateway.Vendor.MetaTrader;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class MT5ServiceTests : Startup
{
    private readonly MT5Service _svc;
    private readonly CopyTradeOptions _options;
    private readonly string _defaultPassword = "Pass!1234";
    private readonly string _mt5Group = "demo\\demoUSD_STN";
    private readonly long _login = 15502;

    public MT5ServiceTests()
    {
        var options = ServiceProvider.GetRequiredService<IOptions<CopyTradeOptions>>();
        _options = options.Value;
        _svc = new MT5Service(_options.Endpoint, _options.Login, _options.Password);
    }

    [Fact]
    public async Task LoginTest()
    {
        // Act
        var result1 = await _svc.LoginAsync();
        var result2 = await _svc.LoginAsync();

        // Assert
        result1.ShouldBeTrue();
        result2.ShouldBeTrue();
    }

    [Fact]
    public async Task AddUserTest()
    {
        // Arrange
        var spec = CreateAccountRequest.Build(
            "jiehe+" + Guid.NewGuid().ToString()[..6],
            _mt5Group, 100, _defaultPassword,
            _defaultPassword
        );

        // Act
        var result = await _svc.CreateAccountAsync(spec);

        // Assert
        result.IsSuccessStatus().ShouldBeTrue();
    }
    //
    // [Fact]
    // public async Task AddRealUserTest()
    // {
    //     // Arrange
    //     var spec = CreateAccountRequest.Build("jiehe+0919", _mt5Group, 100, _defaultPassword, _defaultPassword);
    //
    //     // Act
    //     var result = await _svc.CreateAccountAsync(spec);
    //
    //     // Assert
    //     result.IsSuccessStatus().ShouldBeTrue();
    // }

    [Fact]
    public async Task GetUserInfoTest()
    {
        // Act
        var result = await _svc.GetAccountInfoAsync(_login);

        // Assert
        result.IsSuccessStatus().ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeBalanceTest()
    {
        // Arrange
        var spec = ChangeBalanceRequest.Build(_login, 15, "Unit Test");

        // Act
        var result = await _svc.ChangeBalanceAsync(spec);

        // Assert
        result.IsSuccessStatus().ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeCreditTest()
    {
        // Arrange
        var spec = ChangeCreditRequest.Build(_login, 250, "Unit Test");

        // Act
        var result = await _svc.ChangeCreditAsync(spec);

        // Assert
        result.IsSuccessStatus().ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeBalanceToRealUserTest()
    {
        // Arrange
        var spec = ChangeBalanceRequest.Build(_login, 800000, "Unit Test Real User");

        // Act
        var result = await _svc.ChangeBalanceAsync(spec);

        // Assert
        result.IsSuccessStatus().ShouldBeTrue();
    }

    [Fact]
    public async Task CheckPasswordTest()
    {
        var changePasswordSpec = ChangePasswordRequest.Build(_login, "prefx_" + _defaultPassword);
        var changePasswordResult = await _svc.ChangePasswordAsync(changePasswordSpec);
        changePasswordResult.IsSuccessStatus().ShouldBeTrue();

        var checkPasswordSpec = CheckPasswordRequest.Build(_login, "prefx_" + _defaultPassword);
        var checkPasswordResult = await _svc.CheckPasswordAsync(checkPasswordSpec);
        checkPasswordResult.IsSuccessStatus().ShouldBeTrue();

        var changePasswordSpec2 = ChangePasswordRequest.Build(_login, _defaultPassword);
        var changePasswordResult2 = await _svc.ChangePasswordAsync(changePasswordSpec2);
        changePasswordResult2.IsSuccessStatus().ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeLeverageTest()
    {
        var spec = ChangeLeverageRequest.Build(_login, 200);
        var result = await _svc.ChangeLeverageAsync(spec);
        result.IsSuccessStatus().ShouldBeTrue();
    }
}