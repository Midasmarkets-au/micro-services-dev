#nullable enable
namespace Bacera.Gateway;

using M = Referral;

partial class Referral : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public string? Code { get; set; }
        public string? ReferrerCode { get; set; }
        public string? Module { get; set; }
        public long? ReferrerPartyId { get; set; }
        public long? ReferredPartyId { get; set; }
        public long? ReferrerAccountId { get; set; }
        public bool? IsUnverified { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Code.Equals(Code!), Code is { Length: > 2 });
            pool.Add(x => x.ReferralCode.Code == ReferrerCode, ReferrerCode is { Length: > 2 });
            pool.Add(x => x.ReferrerPartyId == ReferrerPartyId, ReferrerPartyId.IsTangible());
            pool.Add(x => x.ReferredPartyId == ReferredPartyId, ReferredPartyId.IsTangible());
            pool.Add(x => x.Module == Module, Module != null);
            pool.Add(x => x.ReferralCode.AccountId == ReferrerAccountId, ReferrerAccountId.IsTangible());
            pool.Add(
                x => x.ReferredParty.Verifications.All(y => y.Type != (int)VerificationTypes.Verification)
                     || x.ReferredParty.Verifications.Any(v =>
                         v.Type == (int)VerificationTypes.Verification &&
                         v.Status < (int)VerificationStatusTypes.Approved),
                IsUnverified == true);
        }
    }
}