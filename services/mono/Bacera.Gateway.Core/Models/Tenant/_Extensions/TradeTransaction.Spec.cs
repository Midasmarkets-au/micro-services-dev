namespace Bacera.Gateway;

partial class TradeTransaction
{
    public class TenantRebateResponseModel
    {
        public int Cmd { get; set; }
        public int Volume { get; set; }
        public long Ticket { get; set; }
        public string Symbol { get; set; } = null!;
        public static TenantRebateResponseModel Empty() => new();
    }
}