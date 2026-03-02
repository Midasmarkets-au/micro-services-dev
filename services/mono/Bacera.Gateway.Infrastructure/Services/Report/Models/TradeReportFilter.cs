namespace Bacera.Gateway.Services.Report.Models;

public class TradeReportFilter
{
    public long AccountUid { get; set; }
    // 0: self only
    // 1: sub accounts only
    public ScopeType Scope { get; set; } = ScopeType.ForSelf;
    public DateTime ClosedFrom { get; set; }
    public DateTime ClosedTo { get; set; }


}
public enum ScopeType
{
    ForSelf = 0,
    ForSubAccounts = 1,
}