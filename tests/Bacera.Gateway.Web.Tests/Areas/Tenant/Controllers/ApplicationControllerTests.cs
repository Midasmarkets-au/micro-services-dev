using Bacera.Gateway.Web.Areas.Tenant.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bacera.Gateway.Web.Tests.Areas.Tenant.Controllers;

using M = Application;

public class ApplicationControllerTests : TenantBaseControllerTests
{
    private readonly Party _party;
    private readonly Party _operator;
    private readonly ApplicationController _controller;

    public ApplicationControllerTests()
    {
        _party = GetClient().Result;
        _operator = GetManager().Result;
        _controller =
            new ApplicationController(Mock.Of<IMediator>(), AuthDbContext, UserManager, TradingService, TenantDbContext,
                AccountingService, TokenService, ApplicationService);
        _controller.ControllerContext.HttpContext = FakeHttpContext(_operator.Id);
    }

    [Fact]
    public async Task QueryApplicationTest()
    {
        var json = await _controller.Index(null);
        json.ShouldBeOfType<OkObjectResult>();
        ((OkObjectResult)json).Value.ShouldBeOfType<Result<List<M.ResponseModel>, M.Criteria>>();
    }

    /// <summary>
    /// Agent application type should be TradeAccount
    /// SalesCode should be exists
    /// </summary>
    [Fact]
    public async Task ApproveForAgentTest()
    {
        var application = await CreateApplication();
        var spec = ApplicationSupplement.Build(
            AccountRoleTypes.Agent, FundTypes.Wire, CurrencyTypes.AUD, AccountTypes.Standard
        );

        // sales code not exists should be failed
        (await _controller.Approve(application.Id, spec)).ShouldBeOfType<BadRequestObjectResult>();

        // sales code exists should be ok
        var group = await GetSalesGroup();
        spec.SalesAccountId = group.OwnerAccountId;
        spec.AgentSelfGroup = Guid.NewGuid().ToString()[..12];
        var result = await _controller.Approve(application.Id, spec);

        ((OkObjectResult)result).Value.ShouldBeOfType<Account>();
        var account = ((OkObjectResult)result).Value as Account;
        account.ShouldNotBeNull();
        account.Type.ShouldBe((short)AccountTypes.Standard);
        account.Role.ShouldBe((short)AccountRoleTypes.Agent);
    }

    /// <summary>
    /// Sales application type should be TradeAccount
    /// SalesCode should be exists
    /// </summary>
    [Fact]
    public async Task ApproveForSalesTest()
    {
        var application = await CreateApplication();
        var spec = ApplicationSupplement.Build(AccountRoleTypes.Sales, FundTypes.Wire, CurrencyTypes.AUD,
            accountType: AccountTypes.Standard);
        // sales code not exists should be failed
        (await _controller.Approve(application.Id, spec)).ShouldBeOfType<BadRequestObjectResult>();

        spec.SalesSelfGroup = Guid.NewGuid().ToString()[..12];
        (await _controller.Approve(application.Id, spec)).ShouldBeOfType<BadRequestObjectResult>();

        // sales code exists should be ok
        spec.SalesSelfGroup = Guid.NewGuid().ToString()[..12];
        var result = await _controller.Approve(application.Id, spec);

        ((OkObjectResult)result).Value.ShouldBeOfType<Account>();
        var account = ((OkObjectResult)result).Value as Account;
        account.ShouldNotBeNull();
        account.Type.ShouldBe((short)AccountTypes.Standard);
        account.Role.ShouldBe((short)AccountRoleTypes.Sales);
    }

    [Fact]
    public async Task RejectTest()
    {
        var application = await CreateApplication();
        var result = await _controller.Reject(application.Id, M.RejectRequestModel.Build("TEST Reason"));
        result.ShouldBeOfType<NoContentResult>();
    }

    private async Task<Application> CreateApplication(ApplicationTypes applicationTypes = ApplicationTypes.TradeAccount)
    {
        var application = await
            ApplicationService.CreateApplication(_party.Id, ApplicationTypes.TradeAccount, new ApplicationSupplement());
        return application;
    }

    private async Task<Group> GetSalesGroup() => await TenantDbContext.Groups
        .Where(x => x.Type == (int)AccountGroupTypes.Sales)
        .FirstAsync();

    private async Task<string> GetAccountGroup() => await TenantDbContext.Accounts
        .Where(x => x.Role == (short)AccountRoleTypes.Sales)
        .Select(x => x.Group).FirstAsync();
}