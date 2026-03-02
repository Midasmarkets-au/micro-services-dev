using Bacera.Gateway.Web.Areas.Client.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Bacera.Gateway.Web.Tests.Areas.Client.Controllers;

using M = Application;

public class ApplicationControllerTests : ClientBaseControllerTest
{
    private readonly ApplicationController _controller;

    public ApplicationControllerTests()
    {
        /*
         *  public ApplicationController(
           IMediator mediator
           , IUserService userService
           , UserManager<User> userManager
           , TradingService tradingService
           , TenantDbContext tenantDbContext
           , IApplicationTokenService tokenSvc
           , ApplicationService applicationService
           )
           {
           _mediator = mediator;
           _tokenSvc = tokenSvc;
           _userSvc = userService;
           _userManager = userManager;
           _tradingSvc = tradingService;
           _tenantDbContext = tenantDbContext;
           _applicationService = applicationService;
           }
         */
        // _controller =
        //     new ApplicationController(applicationService, tradingService, new Mock<IUserService>().Object,
        //         new Mock<IMediator>().Object, new Mock<TenantDbContext>().Object);
        _controller.ControllerContext.HttpContext = FakeHttpContext(100);
    }

    [Fact]
    public async Task QueryApplicationTest()
    {
        var json = await _controller.Index(null);
        json.ShouldBeOfType<ActionResult<Result<List<M.ResponseModel>, M.Criteria>>>();
    }

    [Fact]
    public async Task CreateTradeApplicationTest()
    {
        var json = await _controller
            .Create(ApplicationSupplement.Build(AccountRoleTypes.Client, FundTypes.Wire));
        json.ShouldBeOfType<ActionResult<M>>();
    }
}