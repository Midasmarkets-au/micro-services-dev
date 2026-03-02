using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}