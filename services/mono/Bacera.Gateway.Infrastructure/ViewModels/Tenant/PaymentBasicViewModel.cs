namespace Bacera.Gateway.ViewModels.Tenant;
public class PaymentBasicViewModel
{
    public long Id { get; set; }

    public long? Pid { get; set; }

    public long PartyId { get; set; }

    public short LedgerSide { get; set; }

    public long PaymentServiceId { get; set; }
    public string PaymentServiceName { get; set; } = string.Empty;
    public PaymentPlatformTypes PaymentServicePlatformId { get; set; }

    public string Number { get; set; } = null!;

    public CurrencyTypes CurrencyId { get; set; }

    public long Amount { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public PaymentStatusTypes Status { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public string ExchangeRate { get; set; } = null!;

    public static PaymentBasicViewModel Empty() => new();
}