using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.Event;

public partial class EventController
{
    /// <summary>
    /// Get Shop Order Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/order")]
    [ProducesResponseType(typeof(Result<List<EventShopOrder.TenantPageModel>, EventShopOrder.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopOrderIndex([FromQuery] EventShopOrder.Criteria? criteria)
    {
        criteria ??= new EventShopOrder.Criteria();
        var lang = await GetLanguage();
        var items = await tenantDbContext.EventShopOrders
            .PagedFilterBy(criteria)
            .ToTenantPageModel(lang)
            .ToListAsync();
        return Ok(Result<List<EventShopOrder.TenantPageModel>, EventShopOrder.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Shop Order Detail
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpGet("shop/order/{orderId:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopOrder.TenantDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopOrder(long orderId)
    {
        var lang = await GetLanguage();
        var item = await tenantDbContext.EventShopOrders
            .ToTenantDetailModel(lang)
            .SingleOrDefaultAsync(x => x.Id == orderId);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Update Shop Order Shipping Info
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EventShopOrder.TenantDetailModel), StatusCodes.Status200OK)]
    [HttpPut("shop/order/{orderId:long}/update-shipping")]
    public async Task<IActionResult> UpdateShopOrderShipping(long orderId,
        [FromBody] EventShopOrder.UpdateShippingSpec spec)
    {
        var item = await tenantDbContext.EventShopOrders.FindAsync(orderId);
        if (item == null) return NotFound();
        if (item.Status is not (short)EventShopOrderStatusTypes.Processing
            and not (short)EventShopOrderStatusTypes.Shipped)
            return BadRequest(Result.Error("Only Processing or Shipped order can update shipping"));

        var shipping = EventShopOrder.ShippingModel.FromJson(item.Shipping);
        shipping.TrackingNumber = spec.TrackingNumber;
        item.Shipping = JsonConvert.SerializeObject(shipping);
        item.UpdatedOn = DateTime.UtcNow;
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Put Shop Order to "Pending"
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderId:long}/pending")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> PendingShopOrder(long orderId)
        => ChangeShopOrderStatus(orderId, EventShopOrderStatusTypes.Pending);

    /// <summary>
    /// Put Shop Order to "Processing"
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderId:long}/process")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> ProcessingShopOrder(long orderId)
        => ChangeShopOrderStatus(orderId, EventShopOrderStatusTypes.Processing);

    /// <summary>
    /// Put Shop Order to "Shipped"
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderId:long}/ship")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> ShippedShopOrder(long orderId)
        => ChangeShopOrderStatus(orderId, EventShopOrderStatusTypes.Shipped);

    /// <summary>
    /// Put Shop Order to "Succeed"
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderId:long}/succeed")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> SucceedShopOrder(long orderId)
        => ChangeShopOrderStatus(orderId, EventShopOrderStatusTypes.Succeed);

    /// <summary>
    /// Put Shop Order to "Cancelled" 
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderId:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelShopOrder(long orderId)
    {
        var order = await tenantDbContext.EventShopOrders.FindAsync(orderId);
        if (order == null) return NotFound();
        if (order.Status is not (short)EventShopOrderStatusTypes.Pending
            and not (short)EventShopOrderStatusTypes.Processing)
            return BadRequest(Result.Error("Only Pending or Processing order can cancel"));

        order.Status = (short)EventShopOrderStatusTypes.Cancelled;
        order.UpdatedOn = DateTime.UtcNow;
        order.OperatorPartyId = GetPartyId();
        tenantDbContext.EventShopOrders.Update(order);
        await tenantDbContext.SaveChangesAsync();

        var srcContent = JsonConvert.SerializeObject(new
        {
            Message = "OrderCancelled",
            EventShopOrderId = order.Id,
            order.TotalPoint
        });

        await eventService.ChangePointAsync(order.EventPartyId, order.TotalPoint.ToCentsFromScaled(), EventShopPointTransactionSourceTypes.Refund, srcContent);
        return Ok();
    }

    private async Task<IActionResult> ChangeShopOrderStatus(long orderId, EventShopOrderStatusTypes status)
    {
        var order = await tenantDbContext.EventShopOrders.FindAsync(orderId);
        if (order == null) return NotFound();
        order.Status = (short)status;
        order.UpdatedOn = DateTime.UtcNow;
        order.OperatorPartyId = GetPartyId();
        tenantDbContext.EventShopOrders.Update(order);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }
}