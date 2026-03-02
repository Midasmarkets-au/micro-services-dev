using M = Bacera.Gateway.Activity;

namespace Bacera.Gateway;

partial class Activity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public long? PartyId { get; set; }
        public long? MatterId { get; set; }
        public ActionTypes? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public StateTypes? ToState { get; set; }

        // public StateTypes? StateId { get; set; }
        public StateTypes? OnState { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.MatterId == MatterId, MatterId.IsTangible());
            // pool.Add(x => x.Matter.StateId == (int)StateId!, StateId.IsTangible());
            pool.Add(x => x.ActionId == (int)Action!, Action != null && Action.IsTangible());
            pool.Add(x => x.ToStateId == (int)ToState!, ToState.IsTangible());
            pool.Add(x => x.OnStateId == (int)OnState!, OnState.IsTangible());
            pool.Add(x => x.PerformedOn <= To, To.IsTangible());
            pool.Add(x => x.PerformedOn >= From, From.IsTangible());
        }
    }
}