using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway
{
    public class RegistrationRequest
    {
        [Required] [EmailAddress] public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required] public string ConfirmUrl { set; get; } = null!;

        // [RegularExpression(PhoneNumberRegionCodeTypes.AllRegRex)]
        [StringLength(10), JsonProperty("ccc"), JsonPropertyName("ccc")]
        public string CCC { get; set; } = string.Empty;

        [StringLength(10)] public string CountryCode { get; set; } = string.Empty;
        [StringLength(10)] public string Currency { get; set; } = string.Empty;
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;

        // [Required] public string Phone { get; set; } = null!;
        public string Phone { get; set; } = string.Empty;
        [Required] public string Otp { get; set; } = null!;

        [StringLength(20)] public string ReferCode { get; set; } = string.Empty;

        // [RegularExpression(LanguageTypes.AllLanguageRegEx)]
        public string? Language { get; set; } = LanguageTypes.English;
        public string? SourceComment { get; set; }
        public int? SiteId { get; set; }
        public long? TenantId { get; set; }
    }
}