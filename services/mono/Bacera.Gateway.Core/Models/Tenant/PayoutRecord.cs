using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public partial class PayoutRecord
{
    public long Id { get; set; }
    public long OperatorPartyId { get; set; }
    public long PaymentMethodId { get; set; }

    public string BatchUid { get; set; } = "";
    public short Status { get; set; }

    [MaxLength(100)] public string BankName { get; set; } = "";

    [MaxLength(30)] public string BankCode { get; set; } = "";

    [MaxLength(100)] public string BranchName { get; set; } = "";

    [MaxLength(30)] public string AccountName { get; set; } = "";

    [MaxLength(100)] public string BankNumber { get; set; } = "";

    [MaxLength(10)] public string Currency { get; set; } = "";
    public decimal Amount { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public string Info { get; set; } = "{}";

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
}