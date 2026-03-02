using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Lead;

partial class Lead : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public string? Keyword { get; set; }
        public long? AssignedAccountId { get; set; }
        public long? AssignedAccountUid { get; set; }

        public bool? IsAssigned { get; set; }
        public bool? HasReferCode { get; set; }
        public LeadIsArchivedTypes? IsArchived { get; set; }
        public LeadStatusTypes? Status { get; set; }
        public int? SourceType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public bool? HasTradeAccount { get; set; }
        public bool? HasDeposit { get; set; }
        public bool? HasUtm { get; set; }
        public List<string>? Utms { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == (int)Status!, Status.HasValue);
            pool.Add(x => x.SourceType == SourceType!, SourceType.HasValue);
            pool.Add(x => x.UpdatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.UpdatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());

            pool.Add(x => x.Accounts.Any() == IsAssigned, IsAssigned.HasValue);

            pool.Add(x => x.Party != null && x.Party.Accounts.Any(a => a.HasTradeAccount == HasTradeAccount), HasTradeAccount != null);
            pool.Add(x => x.Party != null && x.Party.Deposits.Any(), HasDeposit.HasValue && HasDeposit.Value);
            pool.Add(x => x.Party == null || !x.Party.Deposits.Any(), HasDeposit.HasValue && !HasDeposit.Value);

            pool.Add(x => x.Party != null && x.Party.ReferCode != "", HasReferCode.HasValue && HasReferCode.Value);
            pool.Add(x => x.Party == null || x.Party.ReferCode == "", HasReferCode.HasValue && !HasReferCode.Value);

            pool.Add(x => IsArchived != null && x.IsArchived == (int)IsArchived,
                IsArchived.HasValue && IsArchived.IsTangible());

            pool.Add(x => x.Name.Contains(Keyword!) || x.Email.Contains(Keyword!) || x.PhoneNumber.Contains(Keyword!),
                !string.IsNullOrEmpty(Keyword));

            pool.Add(x => x.Accounts.Any(a => a.Id == AssignedAccountId),
                AssignedAccountId.HasValue && AssignedAccountId.IsTangible());
            pool.Add(x => x.Accounts.Any(a => a.Uid == AssignedAccountUid),
                AssignedAccountUid.HasValue && AssignedAccountUid.IsTangible());

        }
    }
}