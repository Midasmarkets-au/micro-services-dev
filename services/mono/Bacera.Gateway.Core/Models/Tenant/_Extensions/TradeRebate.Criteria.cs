namespace Bacera.Gateway;

public partial class TradeRebate : IEntity
{
    public sealed class Criteria : EntityCriteria<TradeRebate>
    {
        public Criteria()
        {
            SortField = nameof(TradeRebate.Id);
        }

        public long? Uid { get; set; }
        public long? PartyId { get; set; }
        public long? AccountNumber { get; set; }
        public long? AccountId { get; set; }
        public int? TradeServiceId { get; set; }

        public List<int>? Statuses { get; set; }
        public TradeRebateStatusTypes? Status { get; set; }
        public long? Ticket { get; set; }
        public string? Symbol { get; set; }
        public TradeCommandTypes? Command { get; set; }
        public List<int>? Commands { get; set; }

        public DateTime? OpenedFrom { get; set; }
        public DateTime? OpenedTo { get; set; }

        public DateTime? ClosedFrom { get; set; }
        public DateTime? ClosedTo { get; set; }

        public bool? Completed { get; set; }


        protected override void OnCollect(ICriteriaPool<TradeRebate> pool)
        {
            pool.Add(x => x.AccountId != null && x.Account!.Uid.Equals(Uid!), Uid is not null);
            pool.Add(x => x.AccountId != null && x.Account!.PartyId == PartyId,
                PartyId.HasValue && PartyId.IsTangible());

            pool.Add(x => x.Action == (int)Command!, Command.HasValue);
            pool.Add(x => Commands!.Contains(x.Action), Commands != null && Commands.Any());
            pool.Add(x => x.TradeServiceId == TradeServiceId, TradeServiceId.HasValue && TradeServiceId.IsTangible());
            pool.Add(x => x.Ticket == Ticket, Ticket.HasValue && Ticket.IsTangible());
            pool.Add(x => x.AccountId == AccountId, AccountId.HasValue && AccountId.IsTangible());
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.HasValue && AccountNumber.IsTangible());
            pool.Add(x => x.Symbol.Equals(Symbol!), !string.IsNullOrEmpty(Symbol) && Symbol.IsTangible());

            pool.Add(x => x.OpenedOn >= OpenedFrom!.Value.ToUniversalTime(),
                OpenedFrom != null && OpenedFrom.IsTangible());
            pool.Add(x => x.OpenedOn <= OpenedTo!.Value.ToUniversalTime(), OpenedTo != null && OpenedTo.IsTangible());

            pool.Add(x => x.ClosedOn >= ClosedFrom!.Value.ToUniversalTime(),
                ClosedFrom != null && ClosedFrom.IsTangible());
            pool.Add(x => x.ClosedOn <= ClosedTo!.Value.ToUniversalTime(), ClosedTo != null && ClosedTo.IsTangible());

            pool.Add(x => x.Status == (int)Status!, Status.HasValue);
            pool.Add(x => Statuses!.Contains(x.Status), Statuses != null && Statuses.Any());

            pool.Add(x => Completed!.Value
                ? x.Status == (int)TradeRebateStatusTypes.Completed
                : x.Status != (int)TradeRebateStatusTypes.Completed, Completed != null);
        }
    }
}