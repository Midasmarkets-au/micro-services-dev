using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = EventShopItem;

public partial class EventShopItem : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Point);
            SortFlag = true;
        }

        public EventShopItemStatusTypes? Status { get; set; }
        public EventShopItemStatusTypes? StatusExclude { get; set; }
        public List<EventShopItemStatusTypes>? Statuses { get; set; }
        public long? EventId { get; set; }
        public string? EventKey { get; set; }
        //public bool? IncludeAll { get; set; }
        public List<long>? EventIds { get; set; }

        public EventShopItemTypes? Type { get; set; }
        public string? Title { get; set; }
        public short? Category { get; set; }
        public List<short>? Categories { get; set; }  // 改为 short 类型，支持动态分类
        public List<EventShopItemTypes>? Types { get; set; }

        public SiteTypes? AccessSite { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => x.Status != (short)StatusExclude!, StatusExclude != null);
            pool.Add(x => x.EventId == EventId, EventId != null);
            pool.Add(x => x.Event.Key == EventKey, EventKey != null);
            pool.Add(x => x.Category == Category!.Value, Category != null);
            pool.Add(x => Categories!.Contains(x.Category), Categories != null);
            pool.Add(x => Types!.Contains((EventShopItemTypes)x.Type), Types != null);
            pool.Add(x => x.Type == (short)Type!, Type != null);
            pool.Add(x => x.EventShopItemLanguages.Any(y => y.Title.Contains(Title!)), Title != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);
            pool.Add(x => EventIds!.Contains(x.EventId), EventIds != null);

            //pool.Add(x => EventShopItemCategoryTypesExtensions.AvailableTypes.Contains(
            //    (EventShopItemCategoryTypes)x.Category), IncludeAll != true);
        }
    }
}