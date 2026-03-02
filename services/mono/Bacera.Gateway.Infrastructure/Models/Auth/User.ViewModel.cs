using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway.Auth;

partial class User
{
    public class TwoFactorAuthenticationResponseModel
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public IList<string?>? ExternalLogins { get; set; }
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
    }

    public sealed class UserNameModel
    {
        public string NativeName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string GuessNativeName() =>
            string.IsNullOrWhiteSpace(NativeName) ? $"{FirstName} {LastName}" : NativeName;
    }

    public sealed class EmailNameModel
    {
        public long PartyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string GuessNativeName() =>
            string.IsNullOrWhiteSpace(NativeName) ? $"{FirstName} {LastName}" : NativeName;
    }

    public class TenantPageModel
    {
        public long Id { get; set; }
        public long Uid { get; set; }
        public long PartyId { get; set; }
        public string NativeName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string CCC { get; set; } = string.Empty;
        [JsonIgnore] public string PhoneNumberRaw { get; set; } = string.Empty;
        public string PhoneNumber => HideEmail ? Utils.HidePhoneNumber(PhoneNumberRaw) : PhoneNumberRaw;

        [JsonIgnore] public bool HideEmail { get; set; }
        [JsonIgnore] public string EmailRaw { get; set; } = string.Empty;
        public string Email => HideEmail ? Utils.HideEmail(EmailRaw) : EmailRaw;
        public string CountryCode { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public int SiteId { get; set; }
        public PartyStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

        public bool HasComment { get; set; }
        public List<string> PartyTags { get; set; } = [];
    }

    public class TenantDetailModel : TenantPageModel
    {
        public string TimeZone { get; set; } = string.Empty;
        public string ReferCode { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public DateOnly Birthday { get; set; } = default;
        public string Citizen { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IdIssuer { get; set; } = string.Empty;
        public DateOnly IdIssuedOn { get; set; } = default;
        public DateOnly IdExpireOn { get; set; } = default;
        public DateTime? LockoutEnd { get; set; }
        public int IdType { get; set; }
        public int Gender { get; set; }
        public int Status { get; set; }

        public List<string> Tags { get; set; } = [];

        public string LastLoginIp { get; set; } = string.Empty;
        public string RegisteredIp { get; set; } = string.Empty;

        public string GuessNativeName() =>
            string.IsNullOrWhiteSpace(NativeName) ? $"{FirstName} {LastName}" : NativeName;
        
        public string GetUserName() => $"{FirstName} {LastName}";

        public bool IsIdExpired() => IdExpireOn < DateOnly.FromDateTime(DateTime.UtcNow);
    }
}

public static class UserViewModelExtension
{
    public static IQueryable<User.TenantPageModel> ToTenantPageModel(this IQueryable<Party> parties,
        bool hideEmail = false)
        => parties.Select(x => new User.TenantPageModel
        {
            Id = x.Id,
            Uid = x.Uid,
            PartyId = x.Id,
            NativeName = x.NativeName,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Language = x.Language,
            Avatar = x.Avatar,
            HideEmail = hideEmail,
            EmailRaw = x.Email,
            CCC = x.CCC,
            PhoneNumberRaw = x.PhoneNumber,
            CountryCode = x.CountryCode,
            EmailConfirmed = x.EmailConfirmed,
            SiteId = x.SiteId,
            Status = (PartyStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            HasComment = x.PartyComments.Any(),
            PartyTags = x.Tags.Select(y => y.Name).ToList()
        });

    public static IQueryable<User.TenantDetailModel> ToTenantDetailModel(this IQueryable<Party> parties,
        bool hideEmail = false)
        => parties.Select(x => new User.TenantDetailModel
        {
            Id = x.Id,
            Uid = x.Uid,
            PartyId = x.Id,
            Address = x.Address,
            CCC = x.CCC,
            Citizen = x.Citizen,
            CountryCode = x.CountryCode,
            IdType = x.IdType,
            IdIssuedOn = x.IdIssuedOn,
            IdExpireOn = x.IdExpireOn,
            IdNumber = x.IdNumber,
            IdIssuer = x.IdIssuer,
            CreatedOn = x.CreatedOn,
            Birthday = x.Birthday,
            Avatar = x.Avatar,
            Gender = x.Gender,
            Status = x.Status,
            Tags = x.Tags.Select(y => y.Name).ToList(),
            HasComment = x.PartyComments.Any(),
            NativeName = x.NativeName,
            FirstName = x.FirstName,
            LastName = x.LastName,
            EmailConfirmed = x.EmailConfirmed,
            HideEmail = hideEmail,
            EmailRaw = x.Email,
            Language = x.Language,
            TimeZone = x.TimeZone,
            ReferCode = x.ReferCode,
            Currency = x.Currency,
            PhoneNumberRaw = x.PhoneNumber,
            SiteId = x.SiteId,
            LastLoginIp = x.LastLoginIp,
            RegisteredIp = x.RegisteredIp,
            UpdatedOn = x.UpdatedOn,
        });

    public static User ToResponse(this User model)
    {
        model.SecurityStamp = null;
        model.PasswordHash = null;
        model.ConcurrencyStamp = null;
        return model;
    }

    public static IQueryable<User.UserNameModel> ToUserNameModel(this IQueryable<User> users)
        => users.Select(x => new User.UserNameModel
        {
            NativeName = x.NativeName,
            FirstName = x.FirstName,
            LastName = x.LastName
        });

    public static IQueryable<User.EmailNameModel> ToEmailNameModel(this IQueryable<User> users)
        => users.Select(x => new User.EmailNameModel
        {
            PartyId = x.PartyId,
            Email = x.Email ?? "",
            Language = x.Language,
            NativeName = x.NativeName,
            FirstName = x.FirstName,
            LastName = x.LastName
        });

    public static IQueryable<UserInfo> ToUserInfo(this IQueryable<User> users)
        => users.Select(x => new UserInfo
        {
            Id = x.Id,
            Uid = x.Uid,
            PartyId = x.PartyId,
            Avatar = x.Avatar,
            FirstName = x.FirstName,
            LastName = x.LastName,
            EmailString = x.Email ?? "",
            NativeName = x.NativeName
        });
}