using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.EventHandlers.Event;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.Event;

public partial class EventController
{
    /// <summary>
    /// Get Shop Order Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/order")]
    [ProducesResponseType(typeof(Result<List<EventShopOrder.ClientPageModel>, EventShopOrder.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopOrderIndex([FromQuery] EventShopOrder.Criteria? criteria)
    {
        criteria ??= new EventShopOrder.Criteria();
        criteria.PartyId = GetPartyId();
        var lang = await GetLanguage();
        var items = await tenantDbContext.EventShopOrders
            .PagedFilterBy(criteria)
            .ToClientPageModel(lang)
            .ToListAsync();
        return Ok(Result<List<EventShopOrder.ClientPageModel>, EventShopOrder.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Shop Order Detail
    /// </summary>
    /// <param name="orderHashId"></param>
    /// <returns></returns>
    [HttpGet("shop/order/{orderHashId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopOrder.ClientPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopOrder(string orderHashId)
    {
        var orderId = EventShopOrder.HashDecode(orderHashId);
        var lang = await GetLanguage();
        var item = await tenantDbContext.EventShopOrders
            .ToClientDetailModel(lang)
            .SingleOrDefaultAsync(x => x.Id == orderId);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create Shop Order, Address is from user's addresses
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("shop/order")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateShopOrder([FromBody] EventShopOrder.CreateSpec spec)
    {
        var partyId = GetPartyId();
        var shopItemId = EventShopItem.HashDecode(spec.ShopItemHashId);
        var addressId = Address.HashDecode(spec.AddressHashId);

        var address = await tenantDbContext.Addresses
            .Where(x => x.Id == addressId && x.PartyId == partyId)
            .ToCopyModel()
            .SingleOrDefaultAsync();
        if (address == null) return NotFound("__ADDRESS_NOT_FOUND__");

        var shopItem = await tenantDbContext.EventShopItems
            .Where(x => x.Id == shopItemId && x.Status == (int)EventShopItemStatusTypes.Published)
            .Select(x => new { x.Id, x.Point, x.EventId })
            .SingleOrDefaultAsync();
        if (shopItem == null) return NotFound("__SHOP_ITEM_NOT_FOUND_OR_NOT_PUBLISHED__");

        var eventParty = await tenantDbContext.EventParties
            .Where(x => x.PartyId == partyId && x.EventId == shopItem.EventId)
            .Where(x => x.Status == (int)EventPartyStatusTypes.Approved)
            .Where(x => x.Party.Addresses.Any(y => y.Id == addressId))
            .Select(x => new { x.Id, x.EventShopPoint.Point })
            .SingleOrDefaultAsync();
        if (eventParty == null) return NotFound("__EVENT_PARTY_NOT_FOUND_OR_NOT_APPROVED_OR_ADDRESS_NOT_EXISTED__");

        var totalPoint = shopItem.Point * spec.Quantity;
        if (eventParty.Point < totalPoint)
            return BadRequest("__POINT_NOT_ENOUGH__");

        var itemJson = JsonConvert.SerializeObject(shopItem);
        var result = await eventService.ChangePointAsync(eventParty.Id, -totalPoint.ToCentsFromScaled(), // ChangePointAsync already increased 10000
            EventShopPointTransactionSourceTypes.Purchase, itemJson);
        if (!result)
            return BadRequest("__FAILED_TO_CREATE_ORDER__");

        var order = new EventShopOrder
        {
            EventPartyId = eventParty.Id,
            EventShopItemId = shopItemId,
            Quantity = spec.Quantity,
            TotalPoint = totalPoint,
            AddressId = addressId,
            Status = (int)EventShopOrderStatusTypes.Pending,
            Comment = spec.Comment,
            Shipping = EventShopOrder.ShippingModel.Build().SetAddress(address).ToString(),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        tenantDbContext.EventShopOrders.Add(order);
        await tenantDbContext.SaveChangesAsync();
        await mediator.Publish(new UserEventShopOrderPlacedEvent(order.Id));
        return Ok();
    }

    /// <summary>
    /// Put a Shop Order to "Succeed"
    /// </summary>
    /// <param name="orderHashId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderHashId}/succeed")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopOrder.ClientPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> SucceedShopOrder(string orderHashId)
    {
        var orderId = EventShopOrder.HashDecode(orderHashId);
        var item = await tenantDbContext.EventShopOrders.FindAsync(orderId);
        if (item == null) return NotFound();
        if (item.Status != (int)EventShopOrderStatusTypes.Shipped)
            return BadRequest(Result.Error("Only Shipped order can be succeed"));

        item.Status = (int)EventShopOrderStatusTypes.Succeed;
        item.UpdatedOn = DateTime.UtcNow;
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }


    /// <summary>
    /// Put a Shop Order to "Succeed"
    /// </summary>
    /// <param name="orderHashId"></param>
    /// <param name="addressHashId"></param>
    /// <returns></returns>
    [HttpPut("shop/order/{orderHashId}/address/{addressHashId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopOrder.ClientPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeShopOrderAddress(string orderHashId, string addressHashId)
    {
        var orderId = EventShopOrder.HashDecode(orderHashId);
        var addressId = Address.HashDecode(addressHashId);
        var partyId = GetPartyId();
        var item = await tenantDbContext.EventShopOrders.FindAsync(orderId);
        if (item == null) return NotFound();
        if (item.Status != (int)EventShopOrderStatusTypes.Pending)
            return BadRequest(Result.Error("Order can not switch address at this status"));

        var address = await tenantDbContext.Addresses
            .Where(x => x.Id == addressId && x.PartyId == partyId)
            .ToCopyModel()
            .SingleOrDefaultAsync();

        if (address == null) return NotFound(Result.Error("Address not found"));

        item.AddressId = addressId;
        item.Shipping = EventShopOrder.ShippingModel.FromJson(item.Shipping).SetAddress(address).ToString();
        item.UpdatedOn = DateTime.UtcNow;
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }
}