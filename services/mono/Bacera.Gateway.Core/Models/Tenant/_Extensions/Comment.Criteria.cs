using M = Bacera.Gateway.Comment;
namespace Bacera.Gateway;

partial class Comment
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public CommentTypes? Type { get; set; }
        public long? RowId { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.RowId == RowId, RowId.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.Type == (int)Type!, Type != null && Type.IsTangible());
        }
    }
}