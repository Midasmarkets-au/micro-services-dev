using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Response;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

[Tags("IB/Shop")]
public class ShopController : AgentBaseController
{
    private readonly ShopService _shopSvc;

    public ShopController(ShopService shopSvc)
    {
        _shopSvc = shopSvc;
    }

    /// <summary>
    /// Product pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("product")]
    public async Task<IActionResult> ProductIndex([FromQuery] Product.Criteria? criteria)
    {
        criteria ??= new Product.Criteria();
        return Ok(await _shopSvc.ProductQueryAsync(criteria));
    }

    /// <summary>
    /// Order pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("order")]
    public async Task<IActionResult> OrderIndex([FromQuery] Order.Criteria? criteria)
    {
        criteria ??= new Order.Criteria();
        criteria.PartyId = GetPartyId();
        return Ok(await _shopSvc.OrderQueryAsync(criteria));
    }
}