using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = WalletAdjust;

public partial class WalletAdjust
{
    public sealed class ManualAdjustCreateSpec
    {
        public long Amount { get; set; }
        public string Comment { get; set; } = "";
        public short SourceType { get; set; }
        
        public M ToEntity(long walletId) => new()
        {
            WalletId = walletId,
            Amount = Amount,
            Comment = Comment,
            SourceType = SourceType,
            IdNavigation = Matter.Build().WalletAdjustCompleted(),
        };
    }
}