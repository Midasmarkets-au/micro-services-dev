using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class MetaTrade
{
    public bool ClosedLessThanOneMinute() => CloseAt.HasValue && CloseAt.Value - OpenAt < TimeSpan.FromMinutes(1);

    public sealed class ReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Period { get; set; }

        public string DateTime => new DateTime(Year, Month, Day)
            .AddHours(Hour)
            .ToString("yyyy-MM-dd HH:00:00");

        public int TotalTransaction { get; set; }

        public long TotalValue { get; set; }
    }

    public static bool TryParse(string json, out MetaTrade trade)
    {
        trade = new MetaTrade();
        try
        {
            trade = JsonConvert.DeserializeObject<MetaTrade>(json, Utils.UtcJsonSerializerSettings)!;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public TradeRebate ToTradeRebate()
        => new()
        {
            TradeServiceId = ServiceId,
            Ticket = Ticket,
            AccountNumber = AccountNumber,
            CurrencyId = (int)CurrencyTypes.Invalid,
            Volume = VolumeOriginal,
            Status = (int)TradeRebateStatusTypes.Created,
            OpenedOn = DateTime.SpecifyKind(OpenAt ?? DateTime.MinValue, DateTimeKind.Utc),
            ClosedOn = DateTime.SpecifyKind(CloseAt ?? DateTime.MinValue, DateTimeKind.Utc),
            RuleType = 199,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            Symbol = Symbol,
            TimeStamp = TimeStamp,
            Action = Cmd,
            DealId = Ticket,
            Commission = Commission,
            Swaps = Swaps,
            Reason = Reason,
            OpenPrice = OpenPrice ?? 0,
            ClosePrice = ClosePrice ?? 0,
            Profit = Profit
        };
}