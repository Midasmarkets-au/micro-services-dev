using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = WalletAdjust;

public partial class WalletAdjust : IHasMatter
{
    public static M Build(long walletId, long amount, string comment) => new()
    {
        WalletId = walletId,
        Amount = amount,
        Comment = comment,
        SourceType = (short)WalletAdjustSourceTypes.ManualAdjust,
        IdNavigation = Matter.Build().WalletAdjustCompleted(),
    };
    
    
    public static M BuildFromEventReward(long walletId, long amount, string comment) => new()
    {
        WalletId = walletId,
        Amount = amount,
        Comment = comment,
        SourceType = (short)WalletAdjustSourceTypes.EventRewardCard,
        IdNavigation = Matter.Build().WalletAdjustCompleted(),
    };
}