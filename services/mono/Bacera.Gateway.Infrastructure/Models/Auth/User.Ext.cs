using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway.Auth;

partial class User
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.BCRUserId, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.BCRUserId]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    public bool IsEmpty() => Id == 0;

    public void ApplyToParty(ref Party party)
    {
        party.NativeName = GuessUserNativeName();
        party.PhoneNumber = PhoneNumber ?? string.Empty;
        party.Email = Email ?? string.Empty;
        party.FirstName = FirstName;
        party.LastName = LastName;
        party.Language = Language;
        party.Avatar = Avatar;
        party.EmailConfirmed = EmailConfirmed;
        party.TimeZone = TimeZone;
        party.ReferCode = ReferCode;
        party.CountryCode = CountryCode;
        party.Currency = Currency;
        party.CCC = CCC;
        party.Birthday = Birthday;
        party.Gender = Gender;
        party.Citizen = Citizen;
        party.Address = Address;
        party.IdType = IdType;
        party.IdNumber = IdNumber;
        party.IdIssuer = IdIssuer;
        party.IdIssuedOn = IdIssuedOn;
        party.IdExpireOn = IdExpireOn;
        party.RegisteredIp = RegisteredIp;
    }

    public static User Create(string email)
    {
        var user = new User
        {
            Email = email.Trim().ToLower(),
            Language = LanguageTypes.English,

            FirstName = string.Empty,
            LastName = string.Empty,
            PhoneNumber = string.Empty,
            ReferCode = string.Empty,
            Currency = string.Empty,
            CountryCode = string.Empty,
            RegisteredIp = string.Empty,
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
        };
        return user;
    }

    public User Tenant(long tenantId)
    {
        TenantId = tenantId;
        UserName = tenantId + "_" + Email;
        return this;
    }

    public User Party(long partyId)
    {
        PartyId = partyId;
        return this;
    }

    public User Ip(string ipAddress)
    {
        RegisteredIp = ipAddress;
        return this;
    }

    public User SetCountryCode(string countryCode)
    {
        CountryCode = countryCode.Trim();
        return this;
    }

    public User SetCurrency(string currencyName)
    {
        Currency = currencyName.Trim();
        return this;
    }

    public User SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber.Trim();
        return this;
    }

    public User SetLanguage(string? language)
    {
        language ??= LanguageTypes.English;
        Language = LanguageTypes.All.Contains(language) ? language : LanguageTypes.English;
        return this;
    }

    public User SetName(string firstname, string lastname = "")
    {
        FirstName = firstname.Trim();
        LastName = lastname.Trim();
        return this;
    }

    public User SetCcc(string ccc)
    {
        CCC = ccc.Trim();
        return this;
    }

    public User SetReferCode(string code)
    {
        ReferCode = code.Trim();
        return this;
    }

    public User SetUid(long uid)
    {
        Uid = uid;
        return this;
    }

    public string GuessUserName()
    {
        if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
        {
            return (FirstName + " " + LastName).Trim();
        }

        if (!string.IsNullOrEmpty(NativeName))
        {
            return NativeName;
        }

        return (Email ?? Uid.ToString()).Split('@')[0];
    }

    public string GuessUserNativeName()
    {
        if (!string.IsNullOrEmpty(NativeName))
        {
            return NativeName;
        }

        if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
        {
            return (FirstName + " " + LastName).Trim();
        }

        return (Email ?? Uid.ToString()).Split('@')[0];
    }

   
    public class SocialMediaType
    {
        public string Name { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string? ConnectId { get; set; }
        public string? StaffName { get; set; }
    }
}

public static class UserExtension
{
    
}