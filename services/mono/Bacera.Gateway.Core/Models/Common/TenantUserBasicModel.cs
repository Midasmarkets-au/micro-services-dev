using Newtonsoft.Json;

namespace Bacera.Gateway;

public class TenantUserBasicModel : HasDisplayName
{
    public long Id { get; set; }
    public long Uid { get; set; }
    public long PartyId { get; set; }
    [JsonIgnore] public string EmailRaw { get; set; } = string.Empty;
    [JsonIgnore] public bool HideEmail { get; set; } = false;
    public string Email => HideEmail ? Utils.HideEmail(EmailRaw) : EmailRaw;
    public string Avatar { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LastLoginIp { get; set; } = string.Empty;
    public string RegisteredIp { get; set; } = string.Empty;
    public int Status { get; set; }
    public bool HasComment { get; set; }
    public bool IsInIpBlackList { get; set; }
    public bool IsInUserBlackList { get; set; }
    public bool HasUSCAccount { get; set; }
    public List<string> PartyTags { get; set; } = [];
    public static TenantUserBasicModel Empty() => new();
}

public static class TenantUserBasicModelExtension
{
    public static TenantUserBasicModel ToTenantBasicViewModel(this Party party, bool hideEmail = false) => new()
    {
        Id = party.Id,
        Uid = party.Uid,
        Avatar = party.Avatar,
        PartyId = party.Id,
        Language = party.Language,
        LastName = party.LastName,
        FirstName = party.FirstName,
        NativeName = party.NativeName,
        Phone = party.PhoneNumber,
        IdNumber = party.IdNumber,
        LastLoginIp = party.LastLoginIp,
        RegisteredIp = party.RegisteredIp,
        EmailRaw = party.Email,
        HideEmail = hideEmail,
        Status = party.Status,
        PartyTags = party.Tags.Select(t => t.Name).ToList(),
        HasComment = party.PartyComments.Any(),
    };

    public static IQueryable<TenantUserBasicModel> ToTenantBasicViewModel(this IQueryable<Party> query)
        => query.Select(x => new TenantUserBasicModel
        {
            Id = x.Id,
            Uid = x.Uid,
            Avatar = x.Avatar,
            PartyId = x.Id,
            Language = x.Language,
            LastName = x.LastName,
            FirstName = x.FirstName,
            NativeName = x.NativeName,
            Phone = x.PhoneNumber,
            IdNumber = x.IdNumber,
            LastLoginIp = x.LastLoginIp,
            RegisteredIp = x.RegisteredIp,
            EmailRaw = x.Email,
            Status = x.Status,
            PartyTags = x.Tags.Select(t => t.Name).ToList(),
            HasComment = x.PartyComments.Any(),
        });
}