using Newtonsoft.Json;

namespace Bacera.Gateway;

public class ParentUserBasicModel : HasDisplayName
{
    public long Uid { get; set; }
    [JsonIgnore] public string EmailRaw { get; set; } = string.Empty;
    public string Email => Utils.HideEmail(EmailRaw);
    public string Avatar { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;

    // public string Phone { get; set; } = string.Empty;
    public static ParentUserBasicModel Empty() => new();
}

public static class AgentUserBasicModelExtension
{
    public static ParentUserBasicModel ToParentBasicViewModel(this Party party) => new()
    {
        Uid = party.Uid,
        Avatar = party.Avatar,
        Language = party.Language,
        LastName = party.LastName,
        FirstName = party.FirstName,
        NativeName = party.NativeName,
        // Phone = party.PhoneNumber,
        EmailRaw = party.Email,
    };

    public static IQueryable<ParentUserBasicModel> ToParentBasicViewModel(this IQueryable<Party> query)
        => query.Select(x => new ParentUserBasicModel
        {
            Uid = x.Uid,
            Avatar = x.Avatar,
            Language = x.Language,
            LastName = x.LastName,
            FirstName = x.FirstName,
            NativeName = x.NativeName,
            // Phone = x.PhoneNumber,
            EmailRaw = x.Email,
        });
}