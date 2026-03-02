using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using M = AuthCode;

public partial class AuthCode
{
    public sealed class CreateSpec
    {
        [Required] [EmailAddress] public string Email { get; set; } = null!;
        public long? TenantId { get; set; }

    }

    public sealed class ConfirmPasswordSpec
    {
        [Required] [EmailAddress] public string Email { get; set; } = null!;
        [Required] public string Code { get; set; } = null!;
        [Required] public string NewPassword { get; set; } = null!;
    }

    public sealed class ConfirmEmailLoginSpec
    {
        [Required] [EmailAddress] public string Email { get; set; } = null!;
        [Required] public string Code { get; set; } = null!;

        [Required]
        [RegularExpression("^(api|app)$", ErrorMessage = "Invalid grant type")]
        public string GrantType { get; set; } = null!;
    }

    public sealed class Update2FaSpec
    {
        public string? Code { get; set; } 
    }
}