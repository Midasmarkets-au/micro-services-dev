using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway
{
    public class ChangePhoneNumberRequestModel
    {
        [Required]
        public string ccc { get; set; } = null!;
        [Required]
        public string phoneNumber { get; set; } = null!;
    }
}