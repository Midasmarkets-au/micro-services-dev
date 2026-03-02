namespace Bacera.Gateway;

public partial class Event
{
    public long Id { get; set; }
    public DateTime ApplyStartOn { get; set; }
    public DateTime ApplyEndOn { get; set; }
    public DateTime StartOn { get; set; }
    public DateTime EndOn { get; set; }
    public string AccessRoles { get; set; } = "[]";
    public string AccessSites { get; set; } = "[]";
    public string Key { get; set; } = "";
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual ICollection<EventLanguage> EventLanguages { get; set; } = new List<EventLanguage>();

    // public virtual ICollection<EventAccount> EventAccounts { get; set; } = new List<EventAccount>();
    public virtual ICollection<EventParty> EventParties { get; set; } = new List<EventParty>();
    public virtual ICollection<EventShopItem> EventShopItems { get; set; } = new List<EventShopItem>();
}