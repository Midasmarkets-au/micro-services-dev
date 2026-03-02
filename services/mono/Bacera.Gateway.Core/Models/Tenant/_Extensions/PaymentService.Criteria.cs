namespace Bacera.Gateway;

using M = PaymentService;

partial class PaymentService : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Sequence);
            SortFlag = false;
        }

        public PaymentPlatformTypes? Platform { get; set; }
        public bool? IsActivated { get; set; }
        public string? Keyword { get; set; }
        public bool? CanDeposit { get; set; }
        public bool? CanWithdraw { get; set; }
        public FundTypes? FundType { get; set; }
        public string? CategoryName { get; set; }
        public bool? IsShowAll { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Id > 0 && x.Platform >= 100, IsShowAll is not true);
            pool.Add(x => x.IsActivated == 1, IsActivated is true);
            pool.Add(x => x.IsActivated == 0, IsActivated is false);
            pool.Add(x => x.CanDeposit == 1, CanDeposit is true);
            pool.Add(x => x.CanDeposit == 0, CanDeposit is false);
            pool.Add(x => x.CanWithdraw == 1, CanWithdraw is true);
            pool.Add(x => x.CanWithdraw == 0, CanWithdraw is false);
            pool.Add(x => x.Platform == (short)Platform!, Platform.HasValue);
            pool.Add(x => x.FundTypes.Any(t => t.Id == (int)FundType!), FundType.HasValue);
            pool.Add(x => x.Name.Contains(Keyword!), Keyword is { Length: > 3 });
            pool.Add(x => x.CategoryName == CategoryName!, CategoryName is { Length: > 1 });
        }
    }
}