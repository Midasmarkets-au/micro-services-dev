namespace Bacera.Gateway;

public partial class EventLanguage
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public string Language { get; set; } = "";
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Images { get; set; } = "{}";
    public string? Term { get; set; }
    public string Instruction { get; set; } = "{}";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public virtual Event Event { get; set; } = null!;
}