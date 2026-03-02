using Bacera.Gateway.Core.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.Event;

public partial class EventController
{
    [HttpGet("shop/item/category")]
    public IActionResult GetAllCategories()
    {
        return FromMobile()
            ? Ok(EventShopItemCategoryTypesExtensions.AvailableTypes)
            : Ok(EventShopItemCategoryTypesExtensions.All);
    }

    /// <summary>
    /// Get all shop item categories
    /// </summary>
    /// <returns></returns>
    [HttpGet("shop/item/category-name")]
    public async Task<IActionResult> GetAllShopItemCategories()
    {
        var partyId = GetPartyId();
        var lang = await GetLanguage();
        var items = await cfgSvc.GetAsync<Dictionary<string, EventShopItem.ShopItemCategoryData>>(nameof(Public), 0,
            ConfigKeys.EventShopItemCategoryKey);
        if (items == null || items.Count == 0)
            return Ok(new List<string>());

        // 判断当前请求来自哪个端 (1=Web, 2=App)
        var currentPlatform = FromMobile() ? 2 : 1;

        var results = items
            .Where(x => x.Value.Status == true)
            .Where(x => (x.Value.AvailableOn & currentPlatform) != 0) // 位运算过滤端类型
            .ToDictionary(
                x => x.Value.Value ?? 0,
                x => x.Value.Data.TryGetValue(lang, out var name) ? name : x.Value.Data.GetValueOrDefault(LanguageTypes.English) ?? ""
            );

        return Ok(results);
    }
    /// <summary>
    /// Shop Items Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/item")]
    [ProducesResponseType(typeof(Result<List<EventShopItem.ClientPageModel>, EventShopItem.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopItemIndex([FromQuery] EventShopItem.Criteria? criteria)
    {
        var partyId = GetPartyId();
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound("__USER_NOT_FOUND__");
        var roles = await userManager.GetRolesAsync(user);
        criteria ??= new EventShopItem.Criteria();
        criteria.Status = EventShopItemStatusTypes.Published;
        criteria.SortField = nameof(EventShopItem.Point);
        criteria.SortFlag = false;

        criteria.Status = EventShopItemStatusTypes.Published;
        criteria.SortField = nameof(EventShopItem.Point);
        criteria.SortFlag = false;

        // 从配置中读取启用的分类
        var categoryConfig = await cfgSvc.GetAsync<Dictionary<string, EventShopItem.ShopItemCategoryData>>(
            nameof(Public), 0, ConfigKeys.EventShopItemCategoryKey);

        if (categoryConfig != null && categoryConfig.Any())
        {
            // 获取所有启用的分类值
            var enabledCategoryValues = categoryConfig
                .Where(x => x.Value.Status == true && x.Value.Value.HasValue)
                .Select(x => (short)x.Value.Value!.Value)
                .ToList();

            // 只有在有启用的分类时才添加过滤
            if (enabledCategoryValues.Any())
            {
                criteria.Categories = enabledCategoryValues;
            }
        }

        var partySite = await tenantDbContext.Parties
            .Where(x => x.Id == partyId)
            .Select(x => x.SiteId)
            .SingleOrDefaultAsync();

        var items = await tenantDbContext.EventShopItems
            .FromSqlRaw(
                """
                select * from event."_EventShopItem" 
                         where exists (select 1 from jsonb_array_elements_text("AccessRoles"::jsonb) as elem where elem::text = any(array[{0}]))
                         and exists (select 1 from jsonb_array_elements_text("AccessSites"::jsonb) as site where site::int = {1})
                """
                , roles, partySite
            )
            .Where(x => x.Event.EventParties.Any(y => y.PartyId == partyId))
            .PagedFilterBy(criteria)
            .ToClientPageModel(user.Language)
            .ToListAsync();

        // 清空 Categories，避免前端在下次请求时带上这个参数
        criteria.Categories = null;

        return Ok(Result<List<EventShopItem.ClientPageModel>, EventShopItem.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Shop Item Detail
    /// </summary>
    /// <param name="shopItemHashId"></param>
    /// <returns></returns>
    [HttpGet("shop/item/{shopItemHashId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopItem.ClientPageModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopItemDetail(string shopItemHashId)
    {
        var lang = await GetLanguage();
        var shopItemId = EventShopItem.HashDecode(shopItemHashId);
        var item = await tenantDbContext.EventShopItems
            .Where(x => x.Id == shopItemId)
            .ToClientDetailModel(lang)
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Shop Point Transaction Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/point/transaction")]
    [ProducesResponseType(
        typeof(Result<List<EventShopPointTransaction.ClientPageModel>, EventShopPointTransaction.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopPointTransactionIndex([FromQuery] EventShopPointTransaction.Criteria? criteria)
    {
        criteria ??= new EventShopPointTransaction.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await tenantDbContext.EventShopPointTransactions
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();

        var tradeRebateIds = items.Where(x => x.SourceType == EventShopPointTransactionSourceTypes.Trade)
            .Select(x => x.SourceId)
            .ToList();

        var trades = await tenantDbContext.TradeRebates
            .Where(x => tradeRebateIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Ticket);

        foreach (var item in items.Where(x => x.SourceType == EventShopPointTransactionSourceTypes.Trade))
        {
            item.Ticket = trades.GetValueOrDefault(item.SourceId, 0);
        }

        return Ok(Result<List<EventShopPointTransaction.ClientPageModel>, EventShopPointTransaction.Criteria>.Of(items,
            criteria));
    }
}