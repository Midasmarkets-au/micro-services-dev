using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public class ForgetPasswordRequest
{
    [Required] public string Email { get; set; } = null!;

    [Required] public string ResetUrl { get; set; } = null!;
}