using Newtonsoft.Json;

namespace Bacera.Gateway;

public class ClientUserBasicModel : HasDisplayName
{
    // [JsonIgnore] public string EmailRaw { get; set; } = string.Empty;
    // public string Email => EmailRaw;
    public string Avatar { get; set; } = string.Empty;
}

public static class ClientUserBasicModelExtension
{
    public static ClientUserBasicModel ToClientBasicViewModel(this Party party) => new()
    {
        Avatar = party.Avatar,
        LastName = party.LastName,
        FirstName = party.FirstName,
        NativeName = party.NativeName,
        // EmailRaw = party.Email,
    };

    public static IQueryable<ClientUserBasicModel> ToClientBasicViewModel(this IQueryable<Party> query)
        => query.Select(x => new ClientUserBasicModel
        {
            Avatar = x.Avatar,
            LastName = x.LastName,
            FirstName = x.FirstName,
            NativeName = x.NativeName,
            // EmailRaw = x.Email,
        });
}