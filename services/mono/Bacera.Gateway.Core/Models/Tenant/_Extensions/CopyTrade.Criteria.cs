namespace Bacera.Gateway;

using M = CopyTrade;

partial class CopyTrade : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public long? PartyId { get; set; }
        public string? Mode { get; set; }
        public long? TargetAccountId { get; set; }
        public long? TargetAccountNumber { get; set; }
        public long? SourceAccountId { get; set; }
        public long? SourceAccountNumber { get; set; }
        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.UpdatedOn <= UpdatedTo!.Value.ToUniversalTime(), UpdatedTo.IsTangible());
            pool.Add(x => x.UpdatedOn >= UpdatedFrom!.Value.ToUniversalTime(), UpdatedFrom.IsTangible());
            pool.Add(x => x.SourceAccountId == SourceAccountId, SourceAccountId.IsTangible());
            pool.Add(x => x.SourceAccountNumber == SourceAccountNumber, SourceAccountNumber.IsTangible());
            pool.Add(x => x.TargetAccountId == TargetAccountId, TargetAccountId.IsTangible());
            pool.Add(x => x.TargetAccountNumber == TargetAccountNumber, TargetAccountNumber.IsTangible());
            pool.Add(x => x.Mode.ToUpper().Equals(Mode!.ToUpper()),
                Mode != null && Mode.IsTangible());
        }
    }
}