namespace Bacera.Gateway;

public partial class IpBlackList
{
    public long Id { get; set; }
    public string Ip { get; set; } = null!;
    public bool Enabled { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string OperatorName { get; set; } = null!;
    public string Note { get; set; } = null!;
}