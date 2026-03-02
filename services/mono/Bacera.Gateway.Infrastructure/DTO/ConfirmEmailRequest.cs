using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway
{
    public class ConfirmEmailRequest
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;
    }
}
