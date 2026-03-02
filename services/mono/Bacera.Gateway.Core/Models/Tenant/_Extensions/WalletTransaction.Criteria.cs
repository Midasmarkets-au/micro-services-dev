using M = Bacera.Gateway.WalletTransaction;

namespace Bacera.Gateway
{
    partial class WalletTransaction : IEntity
    {
        public sealed class Criteria : EntityCriteria<M>
        {
            public Criteria()
            {
                SortField = nameof(Id);
            }

            public long? PartyId { get; set; }
            public long? WalletId { get; set; }
            public long? MatterId { get; set; }
            public MatterTypes? MatterType { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public string? Email { get; set; }

            /// <summary>
            /// 是否基于MT5关仓时间（ClosingTime）进行查询
            /// true: 使用 tr.ClosedOn 过滤（MT5关仓时间）
            /// false: 使用 StatedOn 过滤（Master状态时间）
            /// </summary>
            public bool? UseClosingTime { get; set; }


            protected override void OnCollect(ICriteriaPool<M> pool)
            {
                pool.Add(x => x.MatterId == MatterId, MatterId.IsTangible());
                pool.Add(x => x.WalletId == WalletId, WalletId.IsTangible());
                pool.Add(x => x.Matter.Type == (int)MatterType!, MatterType.HasValue);
                pool.Add(x => x.Wallet.PartyId == PartyId, PartyId.IsTangible());
                pool.Add(x => x.UpdatedOn >= From!.Value.ToUniversalTime(), From != null && From.IsTruthy());
                pool.Add(x => x.UpdatedOn < To!.Value.ToUniversalTime(), To != null && To.IsTruthy());
            }
        }
        
        public sealed class ClientCriteria : BaseEntityCriteria<M>
        {
            public ClientCriteria()
            {
                SortField = nameof(Id);
            }

            public long? PartyId { get; set; }
            public long? WalletId { get; set; }
            public MatterTypes? MatterType { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }

            protected override void OnCollect(ICriteriaPool<M> pool)
            {
                pool.Add(x => x.WalletId == WalletId, WalletId.IsTangible());
                pool.Add(x => x.Matter.Type == (int)MatterType!, MatterType.HasValue);
                pool.Add(x => x.Wallet.PartyId == PartyId, PartyId.IsTangible());
                pool.Add(x => x.UpdatedOn >= From!.Value.ToUniversalTime(), From != null && From.IsTruthy());
                pool.Add(x => x.UpdatedOn < To!.Value.ToUniversalTime(), To != null && To.IsTruthy());
            }
        }
    }
}