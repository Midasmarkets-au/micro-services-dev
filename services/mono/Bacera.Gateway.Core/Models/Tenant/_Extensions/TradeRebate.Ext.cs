namespace Bacera.Gateway;

partial class TradeRebate
{
    public bool ClosedLessThanOneMinute() => ClosedOn - OpenedOn <= TimeSpan.FromMinutes(1);

    /// <summary>
    /// Opened on Wednesday and closed on the immediately following Thursday after 00:05 (MT5 server time).
    /// Used for Event Shop double points promotion.
    /// </summary>
    public bool IsWednesdayOpenThursdayClose() =>
        OpenedOn.DayOfWeek == DayOfWeek.Wednesday &&
        ClosedOn.DayOfWeek == DayOfWeek.Thursday &&
        ClosedOn.Date == OpenedOn.Date.AddDays(1) &&
        ClosedOn.TimeOfDay >= TimeSpan.FromMinutes(5);

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