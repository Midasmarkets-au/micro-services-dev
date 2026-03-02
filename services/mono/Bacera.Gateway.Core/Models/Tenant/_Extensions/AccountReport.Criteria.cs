using Bacera.Gateway.Core.Types;
using M = Bacera.Gateway.AccountReport;

namespace Bacera.Gateway;

public partial class AccountReport : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? TenantId { get; set; }
        public long? AccountId { get; set; }
        public long? AccountNumber { get; set; }
        public int? ServiceId { get; set; }
        public DateTime? TryTime { get; set; }
        public int? Tries { get; set; }
        public AccountReportStatusTypes? Status { get; set; }
        public AccountReportTypes? Type { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.TenantId == TenantId, TenantId != null);
            pool.Add(x => x.AccountId == AccountId, AccountId != null);
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber != null);
            pool.Add(x => x.ServiceId == ServiceId, ServiceId != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => x.Type == (int)Type!, Type != null);

            pool.Add(x => x.Date == Date!.Value.Date, Date != null);
            pool.Add(x => x.Date >= From, From != null);
            pool.Add(x => x.Date <= To, To != null);

            // 确定重试次数
            pool.Add(x => x.TryTime == null || x.TryTime <= TryTime, TryTime != null);
            // pool.Add(x => x.Tries < Tries, Tries != null);
        }
    }
}