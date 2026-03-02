using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public partial class AccountReportGroup
{
    public long Id { get; set; }

    [StringLength(20)] public string Group { get; set; } = string.Empty;
    [StringLength(20)] public string Category { get; set; } = string.Empty;
    public string MetaData { get; set; } = "{}";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public long OperatorPartyId { get; set; }
    public long? ParentId { get; set; }

    public virtual Party OperatorParty { get; set; } = null!;

    public virtual AccountReportGroup? Parent { get; set; }

    public virtual ICollection<AccountReportGroup> Children { get; set; } = new List<AccountReportGroup>();

    public virtual ICollection<AccountReportGroupLogin> AccountReportGroupLogins { get; set; } =
        new List<AccountReportGroupLogin>();
}