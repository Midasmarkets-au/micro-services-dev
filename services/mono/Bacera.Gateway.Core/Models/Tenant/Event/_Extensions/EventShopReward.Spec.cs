using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class EventShopReward
{
    public sealed class CreateSpec
    {
        public string ShopItemHashId { get; set; } = string.Empty;
    }

    
}