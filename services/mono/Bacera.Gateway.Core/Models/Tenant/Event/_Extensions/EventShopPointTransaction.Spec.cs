using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = EventShopPointTransaction;

public partial class EventShopPointTransaction
{
    public sealed class ManualAdjustSpec
    {
        [Required] public long EventPartyId { get; set; }
        [Required] public long Point { get; set; }
        public string Comment { get; set; } = string.Empty;

        public string ToTransactionSource() => JsonConvert.SerializeObject(new { Comment });
    }
}