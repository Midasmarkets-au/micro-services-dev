using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.Event;

public partial class EventController
{
    /// <summary>
    /// Get Shop Reward Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/reward")]
    [ProducesResponseType(typeof(Result<List<EventShopReward.ClientPageModel>, EventShopReward.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopRewardIndex([FromQuery] EventShopReward.Criteria? criteria)
    {
        criteria ??= new EventShopReward.Criteria();
        criteria.PartyId = GetPartyId();
        var lang = await GetLanguage();
        var items = await tenantDbContext.EventShopRewards
            .PagedFilterBy(criteria)
            .ToClientPageModel(lang)
            .ToListAsync();
        return Ok(Result<List<EventShopReward.ClientPageModel>, EventShopReward.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Shop Reward Detail
    /// </summary>
    /// <param name="rewardHashId"></param>
    /// <returns></returns>
    [HttpGet("shop/reward/{rewardHashId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopReward.ClientPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopReward(string rewardHashId)
    {
        var rewardId = EventShopReward.HashDecode(rewardHashId);
        var lang = await GetLanguage();
        var item = await tenantDbContext.EventShopRewards
            .ToClientDetailModel(lang)
            .SingleOrDefaultAsync(x => x.Id == rewardId);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Put Shop Reward to Active
    /// </summary>
    /// <param name="rewardHashId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardHashId}/active")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActiveRewardOrder(string rewardHashId)
    {
        var rewardId = EventShopReward.HashDecode(rewardHashId);
        var reward = await tenantDbContext.EventShopRewards
            .Where(x => x.Status == (short)EventShopRewardStatusTypes.Approved)
            .SingleOrDefaultAsync(x => x.Id == rewardId);

        if (reward == null) return NotFound();
        var rewardType = await tenantDbContext.EventShopRewards
            .Where(x => x.Id == rewardId)
            .Select(x => x.EventShopItem.Type)
            .SingleAsync();

        var activeReward = await tenantDbContext.EventShopRewards
            .Where(x => x.EventPartyId == reward.EventPartyId)
            .Where(x => x.EventShopItem.Type == rewardType)
            .Where(x => x.Status == (short)EventShopRewardStatusTypes.Active)
            .SingleOrDefaultAsync();
        if (activeReward != null)
        {
            if (activeReward.TotalPoint > reward.TotalPoint)
                return BadRequest(Result.Error("You have active reward with higher point."));
            activeReward.Status = (short)EventShopRewardStatusTypes.Inactive;
            activeReward.UpdatedOn = DateTime.UtcNow;
            activeReward.OperatorPartyId = GetPartyId();
            tenantDbContext.EventShopRewards.Update(activeReward);
        }

        var configJson = await tenantDbContext.EventShopItems
            .Where(x => x.Id == reward.EventShopItemId)
            .Select(x => x.Configuration)
            .SingleAsync();

        if (!EventShopItem.RewardConfiguration.TryParse(configJson, out var config))
            return BadRequest(Result.Error("Invalid reward configuration."));

        reward.EffectiveTo = DateTime.UtcNow.AddDays(config.ValidPeriodInDays);
        reward.Status = (short)EventShopRewardStatusTypes.Active;
        reward.UpdatedOn = DateTime.UtcNow;
        reward.OperatorPartyId = GetPartyId();
        tenantDbContext.EventShopRewards.Update(reward);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }


    /// <summary>
    /// Create Shop Reward
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("shop/reward")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateShopReward([FromBody] EventShopReward.CreateSpec spec)
    {
        var partyId = GetPartyId();
        var shopItemId = EventShopItem.HashDecode(spec.ShopItemHashId);

        var shopItem = await tenantDbContext.EventShopItems
            .Where(x => x.Id == shopItemId && x.Status == (int)EventShopItemStatusTypes.Published)
            .Select(x => new
            {
                x.Id, x.EventId, x.Point, x.Type, x.Configuration
            })
            .SingleOrDefaultAsync();
        if (shopItem == null) return NotFound("__SHOP_ITEM_NOT_FOUND_OR_NOT_PUBLISHED__");

        var eventParty = await tenantDbContext.EventParties
            .Where(x => x.PartyId == partyId && x.EventId == shopItem.EventId)
            .Where(x => x.Status == (int)EventPartyStatusTypes.Approved)
            .Select(x => new { x.Id, x.EventShopPoint.Point })
            .SingleOrDefaultAsync();
        if (eventParty == null) return NotFound("__EVENT_PARTY_NOT_FOUND_OR_NOT_APPROVED__");
        if (eventParty.Point < shopItem.Point) return BadRequest("__NOT_ENOUGH_POINT__");

        var itemJson = JsonConvert.SerializeObject(shopItem);
        var result = await eventService.ChangePointAsync(eventParty.Id, -shopItem.Point,
            EventShopPointTransactionSourceTypes.Purchase, itemJson);
        if (!result) return BadRequest("__FAILED_TO_CREATE_REWARD__");

        var reward = new EventShopReward
        {
            EventShopItemId = shopItemId,
            TotalPoint = shopItem.Point,
            EventPartyId = eventParty.Id,
            Status = (int)EventShopRewardStatusTypes.Pending,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            OperatorPartyId = GetPartyId()
        };
        tenantDbContext.EventShopRewards.Add(reward);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Get Shop Reward Rebate Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/reward/rebate")]
    [ProducesResponseType(typeof(Result<List<EventShopRewardRebate.ClientPageModel>, EventShopRewardRebate.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopRewardRebateIndex([FromQuery] EventShopRewardRebate.Criteria? criteria)
    {
        criteria ??= new EventShopRewardRebate.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await tenantDbContext.EventShopRewardRebates
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        await FulfillTradeInfoToRewardRebateModel(items);
        return Ok(Result<List<EventShopRewardRebate.ClientPageModel>, EventShopRewardRebate.Criteria>.Of(items,
            criteria));
    }

    /// <summary>
    /// Get Shop Reward Rebate Detail
    /// </summary>
    /// <param name="rewardRebateHashId"></param>
    /// <returns></returns>
    [HttpGet("shop/reward/rebate/{rewardRebateHashId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopRewardRebate.ClientPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopRewardRebate(string rewardRebateHashId)
    {
        var rewardId = EventShopRewardRebate.HashDecode(rewardRebateHashId);
        var item = await tenantDbContext.EventShopRewardRebates
            .ToClientDetailModel()
            .SingleOrDefaultAsync(x => x.Id == rewardId);
        if (item == null) return NotFound();
        await FulfillTradeInfoToRewardRebateModel(new List<EventShopRewardRebate.BaseModel> { item });
        return Ok(item);
    }

    private async Task FulfillTradeInfoToRewardRebateModel<T>(List<T> items)
        where T : EventShopRewardRebate.BaseModel
    {
        var tickets = items.Select(x => x.Ticket).Distinct().ToList();
        var trades = await tenantDbContext.TradeRebates
            .Where(x => tickets.Contains(x.Ticket))
            .Select(x => new { x.Volume, x.Symbol, x.Ticket, x.OpenedOn, x.ClosedOn, x.TradeServiceId })
            .ToListAsync();
        foreach (var item in items)
        {
            var trade = trades.SingleOrDefault(x => x.Ticket == item.Ticket);
            if (trade == null) continue;
            item.Volume = trade.Volume;
            item.Symbol = trade.Symbol;
            item.OpenAt = trade.OpenedOn;
            item.CloseAt = trade.ClosedOn;
            item.ServiceType = await GetServiceTypeNameByIdAsync(trade.TradeServiceId);
        }
    }

    private async Task<string> GetServiceTypeNameByIdAsync(int serviceId)
    {
        var cacheKey = CacheKeys.GetServiceIdToNameHashKey();
        var name = await myCache.HGetStringAsync(cacheKey, serviceId.ToString());
        if (!string.IsNullOrWhiteSpace(name)) return name;

        var services = await centerDbContext.CentralTradeServices
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();
        foreach (var service in services)
        {
            await myCache.HSetStringAsync(cacheKey, service.Id.ToString(), service.Name, TimeSpan.FromDays(1));
        }

        return services.Single(x => x.Id == serviceId).Name;
    }
}