using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = EventShopRewardRebate;

public partial class EventShopRewardRebate
{
    public static M Build(long rewardId, long ticket, long amount,
        EventShopRewardRebateStatusTypes status = EventShopRewardRebateStatusTypes.Pending) => new()
    {
        EventShopRewardId = rewardId,
        Ticket = ticket,
        Amount = amount,
        Status = (short)status,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow
    };
}