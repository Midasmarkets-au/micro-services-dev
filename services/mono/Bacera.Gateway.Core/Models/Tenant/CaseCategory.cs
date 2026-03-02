namespace Bacera.Gateway;

public partial class CaseCategory
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string Name { get; set; } = "";
    public short Status { get; set; }
    public short Role { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    public virtual CaseCategory? Parent { get; set; }
    public virtual ICollection<CaseCategory> ChildCaseCategories { get; set; } = new List<CaseCategory>();
}