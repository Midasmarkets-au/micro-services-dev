using System.Text.Json.Serialization;
using Bacera.Gateway.Auth;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public sealed class UserResponseModel
{
    public long Uid { get; set; }
    public long LastCheckedEventId { get; set; }

    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    public string NativeName { get; set; } = string.Empty;

    [JsonProperty("avatar"), JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.MinValue;
    public string Email { get; set; } = string.Empty;
    public bool TwoFactorEnabled { get; set; }
    public string?[]? Roles { get; set; } = Array.Empty<string?>();

    public string?[]? Permissions { get; set; } = Array.Empty<string?>();
    public string?[]? IbAccount { get; set; } = Array.Empty<string?>();
    public string?[]? SalesAccount { get; set; } = Array.Empty<string?>();
    public string?[]? RepAccount { get; set; } = Array.Empty<string?>();
    public string?[]? BrokerAccount { get; set; } = Array.Empty<string?>();
    public List<Configuration.ClientMeViewModel> Configurations { get; set; } = [];

    //public long TenantId { get; set; }
    //public long PartyId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [JsonProperty("language"), JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonProperty("timezone"), JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonProperty("countryCode"), JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonProperty("currency"), JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("ccc"), JsonPropertyName("ccc")]
    public string CCC { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberConfirmed { get; set; }
    public string Tenancy { get; set; } = string.Empty;

    public string DefaultAgentAccount { get; set; } = string.Empty;
    public string DefaultSalesAccount { get; set; } = string.Empty;

    public static UserResponseModel Of(User model)
        => new()
        {
            Uid = model.Uid,
            Name = GetDisplayName(model),
            Avatar = model.Avatar,
            CreatedOn = model.CreatedOn,
            Email = model.Email ?? "",
            PhoneNumber = model.PhoneNumber ?? "",
            PhoneNumberConfirmed = model.PhoneNumberConfirmed,
            //PartyId = model.PartyId,
            //TenantId = model.TenantId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Language = model.Language,
            Timezone = model.TimeZone,
            CountryCode = model.CountryCode,
            Currency = model.Currency,
            CCC = model.CCC,

            // Roles = Array.Empty<string>(),
            // Roles = model.UserRoles?.Select(x => x.Role.Name).ToArray() ?? Array.Empty<string>(),
        };


    private static string GetDisplayName(User model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName) && string.IsNullOrWhiteSpace(model.LastName))
        {
            return model.Email ?? "";
        }

        return (model.FirstName + " " + model.LastName).Trim();
    }
}