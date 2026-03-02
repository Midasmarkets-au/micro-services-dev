using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.Event;

public partial class EventController
{
    /// <summary>
    /// Shop Point Transaction Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/point/transaction")]
    [ProducesResponseType(
        typeof(Result<List<EventShopPointTransaction.TenantPageModel>, EventShopPointTransaction.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopPointTransactionIndex([FromQuery] EventShopPointTransaction.Criteria? criteria)
    {
        criteria ??= new EventShopPointTransaction.Criteria();
        var items = await tenantDbContext.EventShopPointTransactions
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
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

        return Ok(Result<List<EventShopPointTransaction.TenantPageModel>, EventShopPointTransaction.Criteria>.Of(items,
            criteria));
    }

    /// <summary>
    /// Manual Adjust Point
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("shop/point/adjust")]
    public async Task<IActionResult> ManualAdjustPoint([FromBody] EventShopPointTransaction.ManualAdjustSpec spec)
    {
        var exist = await tenantDbContext.EventParties.AnyAsync(x => x.Id == spec.EventPartyId);
        if (!exist) return NotFound("__EVENT_PARTY_NOT_FOUND__");
        await eventService.ChangePointAsync(spec.EventPartyId, spec.Point, EventShopPointTransactionSourceTypes.Adjust, spec.ToTransactionSource());
        return Ok();
    }

    /// <summary>
    /// Get all shop item categories
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/item/category")]
    public async Task<IActionResult> GetShopItemCategory([FromQuery] EventShopItem.ShopItemCategoryCriteria? criteria)
    {
        var item = await cfgSvc.GetAsync<Dictionary<string, EventShopItem.ShopItemCategoryData>>(nameof(Public), 0,
           ConfigKeys.EventShopItemCategoryKey);

        if (item == null) return NotFound("__SHOP_ITEM_CATEGORY_NOT_FOUND__");

        var sortedItem = item.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        if (criteria != null && criteria.Status != null)
        {
            sortedItem = sortedItem.Where(x => x.Value.Status == criteria.Status.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        return Ok(sortedItem);
    }

    /// <summary>
    /// Update Shop Item Category
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("shop/item/category")]
    public async Task<IActionResult> UpdateShopItemCategory([FromBody] EventShopItem.AddShopItemCategorySpec spec)
    {
        var item = await cfgSvc.GetAsync<Dictionary<string, EventShopItem.ShopItemCategoryData>>(nameof(Public), 0,
               ConfigKeys.EventShopItemCategoryKey);

        if (item == null) return NotFound("__SHOP_ITEM_CATEGORY_NOT_FOUND__");

        if (!item.ContainsKey(spec.Key))
        {
            var maxValue = item.Values.Where(v => v.Value.HasValue).Max(v => v.Value.Value);
            var nextValue = maxValue + 1;
            item[spec.Key] = new EventShopItem.ShopItemCategoryData
            {
                Status = false,
                Value = nextValue,
                Data = new Dictionary<string, string>(),
                AvailableOn = spec.AvailableOn ?? 3  // 默认两端都显示
            };
        }
        else
        {
            // 如果分类已存在，更新 AvailableOn（如果提供）
            if (spec.AvailableOn.HasValue)
            {
                item[spec.Key].AvailableOn = spec.AvailableOn.Value;
            }
        }

        foreach (var (lang, content) in spec.Languages)
        {
            if (LanguageTypes.All.All(x => x != lang))
                continue;
            item[spec.Key].Data[lang] = content;
        }

        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.EventShopItemCategoryKey, item);
        return Ok(item[spec.Key]);
    }

    /// <summary>
    /// Update Shop Item Category Status
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpPut("shop/item/category/{key}/status")]
    public async Task<IActionResult> UpdateShopItemCategoryStatus(string key)
    {
        var item = await cfgSvc.GetAsync<Dictionary<string, EventShopItem.ShopItemCategoryData>>(nameof(Public), 0,
              ConfigKeys.EventShopItemCategoryKey);

        if (item == null) return NotFound("__SHOP_ITEM_CATEGORY_NOT_FOUND__");
        if (!item.ContainsKey(key))
            return NotFound("__SHOP_ITEM_CATEGORY_NOT_FOUND__");

        item[key].Status = !item[key].Status;

        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.EventShopItemCategoryKey, item);
        return Ok(item[key]);
    }


    /// <summary>
    /// Shop Items Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("shop/item")]
    [ProducesResponseType(typeof(Result<List<EventShopItem.TenantPageModel>, EventShopItem.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ShopItemIndex([FromQuery] EventShopItem.Criteria? criteria)
    {
        criteria ??= new EventShopItem.Criteria();
        //criteria.IncludeAll = true;
        var lang = await GetLanguage();
        var sites = criteria.AccessSite == null ? SiteType.GetAll() : [(int)criteria.AccessSite.Value];

        var items = await tenantDbContext.EventShopItems
            .FromSqlInterpolated(
                $"""
                 select * from event."_EventShopItem" where exists (select 1 from jsonb_array_elements_text("AccessSites") as site where site::int = any(array[{sites}]))
                 """
            )
            .PagedFilterBy(criteria)
            .ToTenantPageModel(lang)
            .ToListAsync();
        return Ok(Result<List<EventShopItem.TenantPageModel>, EventShopItem.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Shop Item Detail
    /// </summary>
    /// <param name="shopItemId"></param>
    /// <returns></returns>
    [HttpGet("shop/item/{shopItemId:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventShopItem.TenantDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopItemDetail(long shopItemId)
    {
        var item = await tenantDbContext.EventShopItems
            .Where(x => x.Id == shopItemId)
            .ToTenantDetailModel()
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create Shop Item with the specified event id and language
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("shop/item")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateShopItem([FromBody] EventShopItem.CreateWithLanguageSpec spec)
    {
        var (result, message) = spec.Validate();
        if (!result) return BadRequest(message);

        var item = await tenantDbContext.Events.FindAsync(spec.EventId);
        if (item == null) return NotFound();

        var (success, entity) = spec.ToEntity();
        if (!success || entity == null) return BadRequest("__INVALID_CONFIGURATION__");

        item.EventShopItems.Add(entity);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Update Shop item
    /// </summary>
    /// <param name="shopItemId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("shop/item/{shopItemId:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShopItem(long shopItemId, [FromBody] EventShopItem.UpdateSpec spec)
    {
        var item = await tenantDbContext.EventShopItems
            .SingleOrDefaultAsync(x => x.Id == shopItemId);

        if (item == null) return NotFound();
        item.Point = spec.Point.ToScaledFromCents();
        item.Type = (short)spec.Type;
        item.Category = (short)spec.Category;
        item.AccessRoles = JsonConvert.SerializeObject(spec.AccessRoles);
        item.AccessSites = JsonConvert.SerializeObject(spec.AccessSites);
        item.Configuration = JsonConvert.SerializeObject(spec.Configuration);
        item.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.EventShopItems.Update(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Draft Shop item
    /// </summary>
    /// <param name="shopItemId"></param>
    /// <returns></returns>
    [HttpPut("shop/item/{shopItemId:long}/draft")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> DraftShopItem(long shopItemId)
        => ChangeShopItemStatus(shopItemId, EventShopItemStatusTypes.Draft);

    /// <summary>
    /// Publish Shop item
    /// </summary>
    /// <param name="shopItemId"></param>
    /// <returns></returns>
    [HttpPut("shop/item/{shopItemId:long}/publish")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> PublishShopItem(long shopItemId)
        => ChangeShopItemStatus(shopItemId, EventShopItemStatusTypes.Published);


    /// <summary>
    /// Close Shop item
    /// </summary>
    /// <param name="shopItemId"></param>
    /// <returns></returns>
    [HttpPut("shop/item/{shopItemId:long}/close")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> CloseShopItem(long shopItemId)
        => ChangeShopItemStatus(shopItemId, EventShopItemStatusTypes.Closed);

    private async Task<IActionResult> ChangeShopItemStatus(long shopItemId, EventShopItemStatusTypes status)
    {
        var item = await tenantDbContext.EventShopItems.FindAsync(shopItemId);
        if (item == null) return NotFound();
        item.Status = (short)status;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.EventShopItems.Update(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }


    /// <summary>
    /// Update Shop Item with the specified id and language
    /// </summary>
    /// <param name="shopItemId"></param>
    /// <param name="language"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("shop/item/{shopItemId:long}/{language}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShopItemContentByLanguage(long shopItemId, string language,
        [FromBody] EventShopItem.UpdateLanguageSpec spec)
    {
        if (LanguageTypes.All.All(x => x != language))
            return BadRequest($"__LANGUAGE_SHOULD_BE_ONE_OF_THESE__{string.Join(",", LanguageTypes.All)}");
        var shopItemExists = await tenantDbContext.EventShopItems.AnyAsync(x => x.Id == shopItemId);
        if (!shopItemExists) return NotFound("__SHOP_ITEM_NOT_FOUND__");

        var shopLanguage = await tenantDbContext.EventShopItems
                               .SelectMany(x => x.EventShopItemLanguages)
                               .FirstOrDefaultAsync(x => x.EventShopItemId == shopItemId && x.Language == language)
                           ?? new EventShopItemLanguage();

        shopLanguage.Name = spec.Name;
        shopLanguage.Title = spec.Title;
        shopLanguage.Description = spec.Description;
        shopLanguage.Images = JsonConvert.SerializeObject(spec.Images);

        if (shopLanguage.Id == 0)
        {
            shopLanguage.EventShopItemId = shopItemId;
            shopLanguage.Language = language;
            tenantDbContext.EventShopItemLanguages.Add(shopLanguage);
        }
        else
        {
            tenantDbContext.EventShopItemLanguages.Update(shopLanguage);
        }

        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }
}