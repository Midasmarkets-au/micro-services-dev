namespace Bacera.Gateway;

using M = Topic;

partial class Topic : IEntity<int>
{
    public sealed class Criteria : EntityCriteria<M, int>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public TopicTypes? Type { get; set; }
        public bool? IsEffective { get; set; }
        public string? Language { get; set; }
        public string? Title { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type != null && Type.IsTangible());
            pool.Add(x => x.Title.ToUpper().Contains(Title!.ToUpper()),
                Title != null && Title.IsTangible() && Title.Length > 2);
            pool.Add(x => x.EffectiveFrom <= DateTime.UtcNow && x.EffectiveTo >= DateTime.UtcNow, IsEffective.HasValue);
            pool.Add(x => x.TopicContents.Any(y => y.Language.ToUpper().Equals(Language!.ToUpper())),
                !string.IsNullOrEmpty(Language) && LanguageTypes.IsExists(Language));
        }
    }
    
    public sealed class ClientCriteria : BaseEntityCriteria<M, int>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        public TopicTypes? Type { get; set; }
        public bool? IsEffective { get; set; }
        public string? Language { get; set; }
        public string? Title { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type != null && Type.IsTangible());
            pool.Add(x => x.Title.ToUpper().Contains(Title!.ToUpper()),
                Title != null && Title.IsTangible() && Title.Length > 2);
            pool.Add(x => x.EffectiveFrom <= DateTime.UtcNow && x.EffectiveTo >= DateTime.UtcNow, IsEffective.HasValue);
            pool.Add(x => x.TopicContents.Any(y => y.Language.ToUpper().Equals(Language!.ToUpper())),
                !string.IsNullOrEmpty(Language) && LanguageTypes.IsExists(Language));
        }
    }
}