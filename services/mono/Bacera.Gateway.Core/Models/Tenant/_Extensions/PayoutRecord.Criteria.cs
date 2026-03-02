using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = PayoutRecord;

public partial class PayoutRecord : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
            SortFlag = false;
        }

        public string? BatchUid { get; set; }
        public long? PaymentMethodId { get; set; }
        public PayoutRecordStatusTypes? Status { get; set; }
        public List<PayoutRecordStatusTypes>? Statuses { get; set; }
        public string? BankName { get; set; }
        public string? BankCode { get; set; }
        public string? BranchName { get; set; }
        public string? AccountName { get; set; }
        public string? BankNumber { get; set; }
        public string? Currency { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.BatchUid == BatchUid, BatchUid != null);
            pool.Add(x => x.PaymentMethodId == PaymentMethodId, PaymentMethodId != null);
            pool.Add(x => x.Status == (int)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((PayoutRecordStatusTypes)x.Status), Statuses != null);
            pool.Add(x => x.BankName.Contains(BankName!), BankName != null);
            pool.Add(x => x.BankCode.Contains(BankCode!), BankCode != null);
            pool.Add(x => x.BranchName.Contains(BranchName!), BranchName != null);
            pool.Add(x => x.AccountName.Contains(AccountName!), AccountName != null);
            pool.Add(x => x.BankNumber.Contains(BankNumber!), BankNumber != null);
            pool.Add(x => x.Currency == Currency, Currency != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);
        }
    }
}