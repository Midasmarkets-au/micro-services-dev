// using Bacera.Gateway.Web.Areas.Client.Controllers;
//
// using Microsoft.AspNetCore.Mvc;
//
// using Shouldly;
//
// using M = Bacera.Gateway.Wallet;
//
// namespace Bacera.Gateway.Web.Tests.Areas.Client.Controllers;
//
// public class WalletControllerTestTests : ClientBaseControllerTest
// {
//     private readonly WalletController _ctl;
//
//     public WalletControllerTestTests()
//     {
//         _ctl = new WalletController(AccountingService);
//         _ctl.ControllerContext.HttpContext = FakeHttpContext(100);
//     }
//
//     [Fact]
//     public async Task Query_ReturnData()
//     {
//         var result = await GetAllWallet();
//         result.Count.ShouldBeGreaterThan(0);
//         result[0].Id.ShouldBeGreaterThan(0);
//     }
//
//     [Fact]
//     public async Task Get_ReturnOk()
//     {
//         var list = await GetAllWallet();
//         var response = await _ctl.Get(list[0].Id);
//         response.ShouldBeOfType<OkObjectResult>();
//     }
//
//     [Fact]
//     public async Task Transaction_ReturnOK()
//     {
//         var list = await GetAllWallet();
//         var response = await _ctl.Transaction(list[0].Id, null);
//         response.ShouldBeOfType<OkObjectResult>();
//     }
//
//     private async Task<List<M.ResponseModel>> GetAllWallet()
//     {
//         var response = await _ctl.Index(null);
//         response.ShouldBeOfType<OkObjectResult>();
//         var result = ((OkObjectResult)response).Value.ShouldBeOfType<Result<List<M.ResponseModel>, M.Criteria>>();
//         result.ShouldNotBeNull();
//         result.Data.ShouldNotBeNull();
//         result.Data.Any().ShouldBeTrue();
//         return result.Data;
//     }
// }

