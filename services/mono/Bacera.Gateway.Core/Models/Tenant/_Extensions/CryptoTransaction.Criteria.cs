using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = CryptoTransaction;

partial class CryptoTransaction : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public long? CryptoId { get; set; }
        public bool? Confirmed { get; set; }
        public CryptoTransactionStatusTypes? Status { get; set; }
        public List<CryptoTransactionStatusTypes>? Statuses { get; set; }
        public long? PaymentId { get; set; }
        public string? FromAddress { get; set; }
        public string? TransactionHash { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CryptoId == CryptoId, CryptoId != null);
            pool.Add(x => x.Confirmed == Confirmed, Confirmed != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((CryptoTransactionStatusTypes)x.Status), Statuses != null);

            pool.Add(x => x.PaymentId == PaymentId, PaymentId != null);
            pool.Add(x => x.FromAddress.Contains(FromAddress!), FromAddress != null);
            pool.Add(x => x.TransactionHash.Contains(TransactionHash!), TransactionHash != null);
        }
    }
}