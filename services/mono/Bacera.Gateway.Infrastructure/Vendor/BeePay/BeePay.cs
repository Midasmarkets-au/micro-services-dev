namespace Bacera.Gateway.Vendor.BeePay;

public class BeePay
{
    public sealed class RequestClient
    {
        public long AccountId { get; set; }
        public long Amount { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}