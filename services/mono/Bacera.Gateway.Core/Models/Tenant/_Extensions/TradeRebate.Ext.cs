namespace Bacera.Gateway;

partial class TradeRebate
{
    public bool ClosedLessThanOneMinute() => ClosedOn - OpenedOn <= TimeSpan.FromMinutes(1);

    public class SummaryResponseModel
    {
        public long Id { get; set; }
        public long AccountNumber { get; set; }
        public long AccountUid { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public long Ticket { get; set; }
        public string Symbol { get; set; } = null!;
        public int Volume { get; set; }
        public DateTime? CloseAt { get; set; }
        public long CurrencyId { get; set; }
    }
}