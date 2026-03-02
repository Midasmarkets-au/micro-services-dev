using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class Tenant
{
    public bool IsEmpty() => Id == 0;
    public bool IsNotEmpty() => Id > 0;

    public sealed class UpdateSpec
    {
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; } = null!;
    }

    public class CreateSpec
    {
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; } = null!;

        [Required, MinLength(3), MaxLength(64)]
        public string DatabaseName { get; set; } = null!;


        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(DatabaseName);
    }
}