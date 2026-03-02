using System.Security;
using Bacera.Gateway.Auth;

namespace Bacera.Gateway.ViewModels.Tenant;

public class UserAdminViewModel : HasDisplayName
{
    public long Id { get; set; }
    public long Uid { get; set; }
    public long PartyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<long> Roles { get; set; } = new();
}

public static class UserAdminViewModelExtension
{
    public static IQueryable<UserAdminViewModel> ToUserAdminViewModel(this IQueryable<User> query)
        => query.Select(x => new UserAdminViewModel
        {
            Id = x.Id,
            Uid = x.Uid,
            Avatar = x.Avatar,
            PartyId = x.PartyId,
            Language = x.Language,
            LastName = x.LastName,
            FirstName = x.FirstName,
            NativeName = x.NativeName,
            Email = x.Email ?? string.Empty,
            Roles = x.UserRoles.Select(r => r.RoleId).ToList(),
        });
}