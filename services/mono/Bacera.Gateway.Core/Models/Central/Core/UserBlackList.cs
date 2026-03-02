namespace Bacera.Gateway;

public partial class UserBlackList
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string IdNumber { get; set; } = null!;
    public string OperatorName { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}