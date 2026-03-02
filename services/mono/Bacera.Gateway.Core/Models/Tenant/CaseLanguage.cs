namespace Bacera.Gateway;

public partial class CaseLanguage
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public string Language { get; set; } = null!;
    public string Content { get; set; } = "";
    public virtual Case Case { get; set; } = null!;
}