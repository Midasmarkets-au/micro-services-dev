using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Bacera.Gateway.Event;

public partial class Event : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public EventStatusTypes? Status { get; set; }
        public DateTime? StartFrom { get; set; }
        public DateTime? StartTo { get; set; }
        public DateTime? EndFrom { get; set; }
        public DateTime? EndTo { get; set; }
        public DateTime? ApplyStartFrom { get; set; }
        public DateTime? ApplyStartTo { get; set; }
        public DateTime? ApplyEndFrom { get; set; }
        public DateTime? ApplyEndTo { get; set; }
        public long? ParticipatedPartyId { get; set; }
        public List<long>? ParticipatedPartyIds { get; set; }
        public string? Role { get; set; }
        public long? IdLargerThan { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Id > IdLargerThan, IdLargerThan != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => x.StartOn >= StartFrom, StartFrom != null);
            pool.Add(x => x.StartOn <= StartTo, StartTo != null);
            pool.Add(x => x.EndOn >= EndFrom, EndFrom != null);
            pool.Add(x => x.EndOn <= EndTo, EndTo != null);
            pool.Add(x => x.ApplyStartOn >= ApplyStartFrom, ApplyStartFrom != null);
            pool.Add(x => x.ApplyStartOn <= ApplyStartTo, ApplyStartTo != null);
            pool.Add(x => x.ApplyEndOn >= ApplyEndFrom, ApplyEndFrom != null);
            pool.Add(x => x.ApplyEndOn <= ApplyEndTo, ApplyEndTo != null);
            pool.Add(x => x.AccessRoles.Contains(Role!), Role != null);

            pool.Add(x => x.EventParties.Any(y => y.PartyId == ParticipatedPartyId), ParticipatedPartyId != null);
            pool.Add(x => x.EventParties.Any(y => ParticipatedPartyIds!.Contains(y.PartyId)), ParticipatedPartyIds != null);
        }
    }
}