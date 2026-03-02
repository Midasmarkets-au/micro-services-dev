//using System.Security.Claims;

//using Bacera.Gateway.Web.Areas.Tenant.Controllers;

//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging.Abstractions;

//namespace Bacera.Gateway.Web.Tests.Areas.Tenant.Controllers;

//public class RebateRuleControllerTests : TenantBaseControllerTests
//{
//    private readonly RebateRuleController _controller;

//    public RebateRuleControllerTests()
//    {
//        var host = new HostString("demo.localhost");
//        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new(UserClaimTypes.PartyId, "100") }));
//        _controller = new RebateRuleController(tradingService, NullLogger<RebateRuleController>.Instance);
//        _controller.ControllerContext.HttpContext = new DefaultHttpContext { Request = { Host = host }, User = user };
//    }

//    [Fact]
//    public async Task<List<M.ResponseModel>> QueryTest()
//    {
//        var result = await _controller.Index(new M.Criteria { Size = 5 });
//        result.ShouldBeOfType<OkObjectResult>();
//        var okResult = result as OkObjectResult;
//        okResult.ShouldNotBeNull();
//        okResult.Value.ShouldBeOfType<Result<List<M.ResponseModel>, M.Criteria>>();
//        var response = okResult.Value as Result<List<M.ResponseModel>, M.Criteria>;
//        response.ShouldNotBeNull();
//        response.Data.Any().ShouldBeTrue();
//        return response.Data;
//    }

//    [Fact]
//    public async Task<M.ResponseModel> GetTest()
//    {
//        var items = await QueryTest();
//        items.ShouldNotBeNull();
//        var result = await _controller.Get(items.First().Id);
//        result.ShouldBeOfType<OkObjectResult>();
//        var okResult = result as OkObjectResult;
//        okResult.ShouldNotBeNull();
//        okResult.Value.ShouldBeOfType<M.ResponseModel>();
//        var response = okResult.Value as M.ResponseModel;
//        response.ShouldNotBeNull();
//        return response;
//    }

//    [Fact]
//    public async Task GetNotFoundTest()
//    {
//        var fakeId = new Random().Next(1, 10);
//        var result = await _controller.Get(fakeId);
//        result.ShouldBeOfType<NotFoundResult>();
//    }

//    [Fact]
//    public async Task<M.ResponseModel> UpdateTest()
//    {
//        var idModel = await GetTest();
//        var item = await tradingService.RebateRuleGetAsync(idModel.Id);
//        item.IsEmpty().ShouldBeFalse();
//        var updatedOn = item.UpdatedOn;
//        var spec = new M.UpdateSpec
//        {
//            Type = RebateRuleTypes.Direct
//        };

//        var model = await _controller.Update(item.Id, spec);
//        model.ShouldBeOfType<NoContentResult>();

//        item = await tradingService.RebateRuleGetAsync(idModel.Id);
//        item.UpdatedOn.ShouldBeGreaterThan(updatedOn);
//        return item;
//    }

//    [Fact]
//    public async Task CreateFailedTest()
//    {
//        var spec = new M.CreateSpec
//        {
//            AccountId = 0,
//            Type = RebateRuleTypes.Direct,
//            BaseSchemas = new List<RebateBaseSchemaItem>
//                { new(10, 0.1m, 0.1m, 0) }
//        };

//        var model = await _controller.Create(spec);
//        model.ShouldBeOfType<BadRequestObjectResult>();
//    }

//    private async Task<long> getSourceAccountIdNotInRebateRules()
//    {
//        var idsQuery = from a in tenantDbContext.Accounts
//                       join r in tenantDbContext.RebateRules on a.Id equals r.AccountId into emptyR
//                       from e in emptyR.DefaultIfEmpty()
//                       where e == null
//                       select a.Id;

//        return await idsQuery.FirstOrDefaultAsync();
//    }

//    private async Task<long> getTargetAccountIdNotInRebateRules()
//    {
//        var idsQuery = from a in tenantDbContext.Accounts
//                       join r in tenantDbContext.RebateRules on a.Id equals r.AccountId into emptyR
//                       from e in emptyR.DefaultIfEmpty()
//                       where e == null
//                       select a.Id;

//        return await idsQuery.FirstOrDefaultAsync();
//    }

//    [Fact]
//    public async Task CreateSuccessTest_CheckSourceAndTargetIds()
//    {
//        // Arrange
//        var sourceId = await getSourceAccountIdNotInRebateRules();
//        var targetId = await getTargetAccountIdNotInRebateRules();

//        // Assert
//        sourceId.ShouldBeGreaterThan(0);
//        targetId.ShouldBeGreaterThan(0);
//    }


//    [Fact]
//    public async Task SymbolIsExitsTest()
//    {
//        var item = await tradingService.IsSymbolExists(10);
//        item.ShouldBeTrue();
//    }

//    [Fact]
//    public async Task<M> CreateSuccessTest_CheckActionResultTypeAndValue()
//    {
//        // Arrange
//        var targetId = await getTargetAccountIdNotInRebateRules();
//        var spec = new M.CreateSpec
//        {
//            AccountId = targetId,
//            Type = RebateRuleTypes.Direct,
//            BaseSchemas = new List<RebateBaseSchemaItem>
//            {
//                new(10, 0.1m, 0.1m, 0),
//                new(11, 0.1m, 0, 0.1m),
//            }
//        };

//        // Act
//        var result = await _controller.Create(spec);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var model = Assert.IsType<M>(okResult.Value);
//        model.ShouldNotBeNull();
//        model.IsEmpty().ShouldBeFalse();
//        return model;
//    }
//}