namespace Bacera.Gateway.Vendor.BigPay.Models;

public class BigPayCallbackViewModel
{
    public string OrderRef { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public bool IsVerifying { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    // public int Amount { get; set; }
    public long OrderAmount { get; set; }
    public string SecretKey { get; set; } = string.Empty;
}