using M = Bacera.Gateway.Audit;

namespace Bacera.Gateway;

partial class Audit
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public AuditTypes? Type { get; set; }
        public AuditActionTypes? Action { get; set; }
        public string? AdjustType { get; set; }
        public long? RowId { get; set; }
        public long? AccountNumber { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.RowId == RowId, RowId.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.Type == (int)Type!, Type != null && Type.IsTangible());
            pool.Add(x => x.Action == (int)Action!, Action != null && Action.IsTangible());
            pool.Add(x => x.Data.Contains(AdjustType!),
                Type == AuditTypes.TradeAccountBalance && AdjustType != null && AdjustType.IsTangible());
        }
    }

    public static Audit Build(AuditTypes type, AuditActionTypes action, long partyId, long rowId, string data,
        string? environment = null)
        => new()
        {
            PartyId = partyId,
            RowId = rowId,
            Type = (int)type,
            Action = (int)action,
            CreatedOn = DateTime.UtcNow,
            Environment = environment ?? string.Empty,
            Data = data
        };
}