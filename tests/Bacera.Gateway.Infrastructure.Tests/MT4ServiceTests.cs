using Bacera.Gateway.Vendor.MetaTrader;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class MT4ServiceTests : Startup
{
    private readonly MT4Service _svc;

    private const int Port = 4578;
    private const string Host = "13.57.233.166";

    public MT4ServiceTests()
    {
        _svc = new MT4Service(Options.Create(ApiOptions.Create(Host, Port)));
    }

    [Fact]
    public async Task GetAccountInfoTest()
    {
        // Arrange
        // var accountNumber = await tenantDbContext.TradeAccounts
        //     .OrderByDescending(x => x.Id)
        //     .Select(x => x.AccountNumber)
        //     .FirstAsync();
        var accountNumber = 1362485066;
        // Act
        var result = await _svc.GetAccountInfoAsync(accountNumber);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.Data.ShouldNotBeNull();
        result.Data.Login.ShouldBeGreaterThan(0);
    }

    private async Task<long> CreateAccountTest()
    {
        // Arrange
        var request = CreateAccountRequest.Build("Portal Tester 2", "demoUSD_STN", 200);
        request.Password = "Pas456";
        request.EnableChangePassword = "1";
        request.Enable = 1;
        request.PasswordPhone = "none";
        request.Email = "1732175@qq.com";
        request.Comment = "Portal Testing";
        request.Phone = "none";

        // Act
        var result = await _svc.CreateAccountAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.Data.ShouldNotBeNull();
        result.Data.Login.ShouldBeGreaterThan(0);
        return result.Data.Login;
    }

    [Fact]
    public async Task ChangeBalanceTest()
    {
        // Arrange
        // var accountNumber = await createAccountTest();
        var accountNumber = 63241623;
        var request = ChangeBalanceRequest.Build(accountNumber, 100000.05m, "Portal Testing");

        // Act
        var result = await _svc.ChangeBalanceAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.Data.ShouldNotBeNull();
        result.Data.Login.ShouldBe(accountNumber);
        result.Data.NewBalance.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ChangePasswordTest()
    {
        // Arrange
        var accountNumber = await CreateAccountTest();
        var request = ChangePasswordRequest.Build(accountNumber, "Pass!1234", true);

        // Act
        var result = await _svc.ChangePasswordAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.Data.ShouldNotBeNull();
        result.Data.Login.ShouldBe(accountNumber);
    }

    [Fact]
    public async Task ChangeLeverageTest()
    {
        // Arrange
        //var accountNumber = await createAccountTest();
        var accountNumber = 63241623;
        var request = ChangeLeverageRequest.Build(accountNumber, 100);

        // Act
        var result = await _svc.ChangeLeverageAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.Data.ShouldNotBeNull();
        result.Data.Login.ShouldBe(accountNumber);
    }

    [Fact]
    public void RequestToQueryStringTest()
    {
        // Arrange
        var model = CreateAccountRequest.Build("test", "group", 200);

        // Act
        var query = model.ToQueryString();

        // Assert
        query.ShouldNotBeNull();
    }

    [Fact]
    public void ResponseStringToResponseTest()
    {
        // Arrange
        var model = ApiResult<GetAccountInfoResult>.Build();

        // Act
        model.FromQueryString("");

        // Assert
        model.Status.ShouldBe(ApiResultStatus.SystemError);
    }
}