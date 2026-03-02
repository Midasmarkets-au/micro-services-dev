namespace Bacera.Gateway;

public partial class Case
{
    public long Id { get; set; }
    public long? ReplyId { get; set; }
    public long PartyId { get; set; }
    public string CaseId { get; set; } = "";
    public long CategoryId { get; set; }
    public bool IsAdmin { get; set; }
    public long? AdminPartyId { get; set; }
    public string Data { get; set; } = "{}";
    public string Content { get; set; } = "";
    public string Files { get; set; } = "[]";
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }

    public virtual Party Party { get; set; } = null!;
    public virtual Party? AdminParty { get; set; }
    public virtual CaseCategory Category { get; set; } = null!;

    public virtual Case? Reply { get; set; }
    public virtual ICollection<Case> InverseReply { get; set; } = new List<Case>();
    public virtual ICollection<CaseLanguage> CaseLanguages { get; set; } = new List<CaseLanguage>();
}