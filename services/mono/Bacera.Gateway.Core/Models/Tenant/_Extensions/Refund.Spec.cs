using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Refund;

partial class Refund
{
    public class CreateSpec
    {
        [Required] public long TargetId { get; set; }
        [Required] public RefundTargetTypes TargetType { get; set; }
        [Required] public long Amount { get; set; }
        [Required] public string Comment { get; set; } = string.Empty;
        [Required] public int TransferCurrencyId { get; set; }
    }
}