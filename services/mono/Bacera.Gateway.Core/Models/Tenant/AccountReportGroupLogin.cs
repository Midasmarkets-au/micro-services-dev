namespace Bacera.Gateway;

public class AccountReportGroupLogin
{
    public long AccountReportGroupId { get; set; }

    public long Login { get; set; }

    public virtual AccountReportGroup AccountReportGroup { get; set; } = null!;
}