using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = EventParty;

public partial class EventParty : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public EventPartyStatusTypes? Status { get; set; }
        public string? Email { get; set; }
        public string? NativeName { get; set; }
        public long? PointsGreaterThan { get; set; }
        public long? PointsLessThan { get; set; }
        public long? PartyId { get; set; }

        public long? FrozenPointsGreaterThan { get; set; }
        public long? FrozenPointsLessThan { get; set; }
        public long? TotalPointsGreaterThan { get; set; }
        public long? TotalPointsLessThan { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => x.Party.Email == Email, Email != null);
            pool.Add(x => x.Party.NativeName == NativeName, NativeName != null);
            pool.Add(x => x.EventShopPoint.TotalPoint >= PointsGreaterThan!, PointsGreaterThan != null);
            pool.Add(x => x.EventShopPoint.TotalPoint <= PointsLessThan!, PointsLessThan != null);
            pool.Add(x => x.EventShopPoint.FrozenPoint >= FrozenPointsGreaterThan!, FrozenPointsGreaterThan != null);
            pool.Add(x => x.EventShopPoint.FrozenPoint <= FrozenPointsLessThan!, FrozenPointsLessThan != null);
            pool.Add(x => x.EventShopPoint.Point >= TotalPointsGreaterThan!, TotalPointsGreaterThan != null);
            pool.Add(x => x.EventShopPoint.Point <= TotalPointsLessThan!, TotalPointsLessThan != null);
        }
    }
}