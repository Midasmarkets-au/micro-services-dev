using Bacera.Gateway.Auth;

namespace Bacera.Gateway.ViewModels.Parent;

public class UserBasicForParentViewModel : HasDisplayName
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long PartyId { get; set; }

    public long Uid { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;

    public static UserBasicForParentViewModel Empty() => new();
}

public static class AgentUserBasicViewModelExtension
{
    public static IQueryable<UserBasicForParentViewModel> ToParentViewModel(this IQueryable<User> query)
        => query.Select(x => new UserBasicForParentViewModel
        {
            Uid = x.Uid,
            Avatar = x.Avatar,
            PartyId = x.PartyId,
            LastName = x.LastName,
            FirstName = x.FirstName,
            NativeName = x.NativeName,
            Email = x.Email ?? string.Empty,
        });
}