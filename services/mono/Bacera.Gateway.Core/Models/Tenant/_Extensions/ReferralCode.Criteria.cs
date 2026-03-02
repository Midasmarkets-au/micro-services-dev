using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = ReferralCode;

partial class ReferralCode : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public ReferralServiceTypes? ServiceType { get; set; }
        public long? AccountId { get; set; }
        public string? Code { get; set; }
        public ReferralCodeStatusTypes? Status { get; set; }
        public long? ParentAccountUid { get; set; }
        public long? ChildAccountUid { get; set; }
        public long? Level { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == (int)Status!, Status.HasValue && Status.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.AccountId == AccountId, AccountId.IsTangible());
            pool.Add(x => x.Account.Level == Level, Level != null);
            pool.Add(x => x.Code.Equals(Code!.Trim()), Code is { Length: > 2 });
            pool.Add(x => x.ServiceType == (int)ServiceType!, ServiceType.HasValue && ServiceType.IsTangible());

            pool.Add(x => x.Account.Uid == ParentAccountUid, ParentAccountUid.IsTangible() && ChildAccountUid == null);
            pool.Add(
                x => x.Account.ReferPath.Contains(ParentAccountUid!.Value.ToString())
                     && x.Account.ReferPath.Contains(ChildAccountUid!.Value.ToString()),
                ChildAccountUid != null && ParentAccountUid != null);
        }
    }
}