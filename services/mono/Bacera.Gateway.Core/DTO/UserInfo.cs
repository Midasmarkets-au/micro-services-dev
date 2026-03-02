namespace Bacera.Gateway;

public class UserInfo
{
    public long Uid { get; set; }
    public long Id { get; set; }
    public List<long> AccountUids { get; set; } = new();
    public string Avatar { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(NativeName) ? $"{FirstName} {LastName}" : NativeName;

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public string EmailString { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public long PartyId { get; set; }

    // public string Email => string.IsNullOrEmpty(EmailString)
    //     ? string.Empty
    //     : Utility.ObfuscateEmail(EmailString);
    public string Email => EmailString;
    public bool? HasComment { get; set; }
}