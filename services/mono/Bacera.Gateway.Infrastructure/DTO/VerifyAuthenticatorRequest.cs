using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway.Web;

public class VerifyAuthenticatorRequest : IVerificationRequest
{
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Verification Code")]
    public string VerificationCode { get; set; } = null!;
}

public interface IVerificationRequest
{
}