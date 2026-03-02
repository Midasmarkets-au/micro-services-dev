using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public partial class AccountReportGroup
{
    public sealed class CreateReportSchemaSpec
    {
        [Required] public string Group { get; set; } = null!;
        [Required] public string Category { get; set; } = null!;
        [Required] public string Title { get; set; } = null!;
        [Required] public string ReceiverEmail { get; set; } = null!;
        [Required] public List<string> Bccs { get; set; } = null!;
        [Required] public List<string> Ccs { get; set; } = null!;
    }

    public sealed class UpdateReportSchemaSpec
    {
        [Required] public long Id { get; set; }
        [Required] public string Group { get; set; } = null!;
        [Required] public string Category { get; set; } = null!;
        [Required] public string Title { get; set; } = null!;
        [Required] public string ReceiverEmail { get; set; } = null!;
        [Required] public List<string> Bccs { get; set; } = null!;
        [Required] public List<string> Ccs { get; set; } = null!;
    }


    public sealed class UpdateGroupCategorySpec
    {
        [Required] public long Id { get; set; }
        [Required] public string Group { get; set; } = null!;
        [Required] public string Category { get; set; } = null!;
    }

    public sealed class AddGroupCategorySpec
    {
        [Required] public long ParentId { get; set; }
        [Required] public string Group { get; set; } = null!;
        [Required] public string Category { get; set; } = null!;
    }

    public sealed class UpdateAccountNumberSpec
    {
        [Required] public long Id { get; set; }
        public List<long> AccountNumbers { get; set; } = [];
    }
}