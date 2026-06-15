using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class  ContactRequest
{
    public class CreateSpec
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Content { get; set; } = null!;
    }

    public class SendEmailSpec
    {
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public string Subtitle { get; set; } = string.Empty;
        [Required] public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = LanguageTypes.English;
    }
}
