using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway
{
    public class ResendConfirmEmailRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;

        [Required] public string ConfirmUrl { get; set; } = null!;
    }
}