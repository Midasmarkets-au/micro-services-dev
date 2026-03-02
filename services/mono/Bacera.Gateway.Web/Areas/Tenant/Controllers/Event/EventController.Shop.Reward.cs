using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.Event;

public partial class EventController
{
    /// <summary>
    /// Get Shop Reward Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/reward")]
    [ProducesResponseType(typeof(Result<List<EventShopReward.TenantPageModel>, EventShopReward.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopRewardIndex([FromQuery] EventShopReward.Criteria? criteria)
    {
        criteria ??= new EventShopReward.Criteria();
        var lang = await GetLanguage();
        var items = await tenantDbContext.EventShopRewards
            .PagedFilterBy(criteria)
            .ToTenantPageModel(lang)
            .ToListAsync();
        return Ok(Result<List<EventShopReward.TenantPageModel>, EventShopReward.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Shop Reward Detail
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpGet("shop/reward/{rewardId:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopReward.TenantDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopReward(long rewardId)
    {
        var lang = await GetLanguage();
        var item = await tenantDbContext.EventShopRewards
            .ToTenantDetailModel(lang)
            .SingleOrDefaultAsync(x => x.Id == rewardId);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Put Shop Reward to Pending
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/pending")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> PendingRewardOrder(long rewardId)
        => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Pending);

    /// <summary>
    /// Put Shop Reward to Processing
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/process")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> ProcessRewardOrder(long rewardId)
        => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Processing);

    /// <summary>
    /// Put Shop Reward to Approved
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/approve")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApproveRewardOrder(long rewardId)
    {
        return await ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Approved);
    }

    // /// <summary>
    // /// Put Shop Reward to Succeeded
    // /// </summary>
    // /// <param name="rewardId"></param>
    // /// <returns></returns>
    // [HttpPut("shop/reward/{rewardId:long}/succeed")]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // public Task<IActionResult> SucceedRewardOrder(long rewardId)
    //     => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Succeeded);

    /// <summary>
    /// Put Shop Reward to Cancelled
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> CancelRewardOrder(long rewardId)
        => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Cancelled);

    /// <summary>
    /// Put Shop Reward to Expired
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/expire")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> ExpiredRewardOrder(long rewardId)
        => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Expired);

    /// <summary>
    /// Put Shop Reward to Active
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/active")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> ActiveRewardOrder(long rewardId)
        => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Active);

    /// <summary>
    /// Put Shop Reward to Inactive
    /// </summary>
    /// <param name="rewardId"></param>
    /// <returns></returns>
    [HttpPut("shop/reward/{rewardId:long}/inactive")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> InactiveRewardOrder(long rewardId)
        => ChangeShopRewardStatus(rewardId, EventShopRewardStatusTypes.Inactive);

    private async Task<IActionResult> ChangeShopRewardStatus(long rewardId, EventShopRewardStatusTypes status)
    {
        var reward = await tenantDbContext.EventShopRewards.FindAsync(rewardId);
        if (reward == null) return NotFound();

        reward.Status = (short)status;
        reward.UpdatedOn = DateTime.UtcNow;
        reward.OperatorPartyId = GetPartyId();
        tenantDbContext.EventShopRewards.Update(reward);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }


    /// <summary>
    /// Get Shop Reward Rebate Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/reward/rebate")]
    [ProducesResponseType(typeof(Result<List<EventShopRewardRebate.TenantPageModel>, EventShopRewardRebate.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopRewardRebateIndex([FromQuery] EventShopRewardRebate.Criteria? criteria)
    {
        criteria ??= new EventShopRewardRebate.Criteria();
        var items = await tenantDbContext.EventShopRewardRebates
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();
        await FulfillTradeInfoToRewardRebateModel(items);
        return Ok(Result<List<EventShopRewardRebate.TenantPageModel>, EventShopRewardRebate.Criteria>.Of(items,
            criteria));
    }

    /// <summary>
    /// Get Shop Reward Rebate Detail
    /// </summary>
    /// <param name="rewardRebateHashId"></param>
    /// <returns></returns>
    [HttpGet("shop/reward/rebate/{rewardRebateHashId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopRewardRebate.TenantPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopRewardRebate(string rewardRebateHashId)
    {
        var rewardId = EventShopRewardRebate.HashDecode(rewardRebateHashId);
        var item = await tenantDbContext.EventShopRewardRebates
            .ToTenantDetailModel()
            .SingleOrDefaultAsync(x => x.Id == rewardId);
        if (item == null) return NotFound();
        await FulfillTradeInfoToRewardRebateModel(new List<EventShopRewardRebate.BaseModel> { item });
        return Ok(item);
    }


    private async Task FulfillTradeInfoToRewardRebateModel<T>(List<T> items)
        where T : EventShopRewardRebate.BaseModel
    {
        var tickets = items.Select(x => x.Ticket).Distinct().ToList();
        var trades = await centralDbContext.MetaTrades
            .Where(x => tickets.Contains(x.Ticket))
            .Select(x => new { x.Volume, x.Symbol, x.Ticket, x.OpenAt, x.CloseAt, x.ServiceId })
            .ToListAsync();
        foreach (var item in items)
        {
            var trade = trades.SingleOrDefault(x => x.Ticket == item.Ticket);
            if (trade == null) continue;
            item.Volume = trade.Volume;
            item.Symbol = trade.Symbol;
            item.OpenAt = trade.OpenAt;
            item.CloseAt = trade.CloseAt;
            item.ServiceType = await GetServiceTypeNameByIdAsync(trade.ServiceId);
        }
    }

    private async Task<string> GetServiceTypeNameByIdAsync(int serviceId)
    {
        var cacheKey = CacheKeys.GetServiceIdToNameHashKey();
        var name = await myCache.HGetStringAsync(cacheKey, serviceId.ToString());
        if (!string.IsNullOrWhiteSpace(name)) return name;

        var services = await centralDbContext.CentralTradeServices
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();
        foreach (var service in services)
        {
            await myCache.HSetStringAsync(cacheKey, service.Id.ToString(), service.Name);
        }

        return services.Single(x => x.Id == serviceId).Name;
    }
}