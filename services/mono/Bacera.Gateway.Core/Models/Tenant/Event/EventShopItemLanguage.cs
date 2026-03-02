namespace Bacera.Gateway;

public partial class EventShopItemLanguage
{
    public long Id { get; set; }

    public long EventShopItemId { get; set; }
    public string Language { get; set; } = "en-us";
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Images { get; set; } = "{}";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual EventShopItem EventShopItem { get; set; } = null!;
}