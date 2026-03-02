using M = Bacera.Gateway.Auth.UserAudit;
namespace Bacera.Gateway.Auth;

partial class UserAudit
{
    public sealed class Criteria : EntityCriteria<UserAudit>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public AuditTypes? Type { get; set; }
        public AuditActionTypes? Action { get; set; }
        public long? RowId { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.RowId == RowId, RowId.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.Type == (int)Type!, Type != null && Type.IsTangible());
            pool.Add(x => x.Action == (int)Action!, Action != null && Action.IsTangible());
        }
    }
}