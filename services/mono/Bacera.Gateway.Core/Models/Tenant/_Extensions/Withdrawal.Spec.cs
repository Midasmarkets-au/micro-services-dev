using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class Withdrawal
{
    public sealed class IdSpec
    {
        public long Id { get; set; }
    }
    public sealed class RejectSpec
    {
        public long Id { get; set; }
        [Required] public string Reason { get; set; } = null!;
    }
    
    public sealed class ApproveSpec
    {
        [Required] public string Comment { get; set; } = null!;
    }
}