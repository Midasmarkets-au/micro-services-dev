namespace Bacera.Gateway;

public partial class CentralParty
{
    public long Id { get; set; }


    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public int SiteId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string NativeName { get; set; } = null!;
    public long Uid { get; set; }
    public long Tid { get; set; }
    public long TenantId { get; set; }

    public string Email { get; set; } = null!;

    public string Note { get; set; } = null!;
}

public partial class CentralParty
{
    public Party ToParty()
        => new()
        {
            Id = Id,
            Uid = Uid,
            Name = Name,
            Note = Note,
            Code = Code,
            SiteId = SiteId,
            NativeName = NativeName,
            Email = Email,
            CreatedOn = CreatedOn,
            UpdatedOn = UpdatedOn,
        };
}