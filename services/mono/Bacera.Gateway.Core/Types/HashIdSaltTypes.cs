namespace Bacera.Gateway.Core.Types;

public static class HashIdSaltTypes
{
    public static readonly Dictionary<string, string> Dictionary = new()
    {
        { "BCREventShopItem", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "BCREventShopOrder", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "BCREventShopReward", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "BCRUserAddress", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "BCREventShopRewardRebate", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "BCRDiscoverPost", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "Wallet", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "Deposit", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "Withdrawal", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "Transaction", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "Verification", "ABCDEFGHJKMNPQRSTUVWXYZ23456789" },
        { "BCRPartyId", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "BCRUserId", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "BCRPaymentMethod", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "BCRPaymentInfo", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "BCRPayment", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "Medium", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "PayoutRecord", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
        { "Chat", "ABCDEFGHJKMNPQRSTUVWXYZ0123456789" },
    };

    public const string EventShopItem = "BCREventShopItem";
    public const string UserAddress = "BCRUserAddress";
    public const string EventShopOrder = "BCREventShopOrder";
    public const string EventShopReward = "BCREventShopReward";
    public const string EventShopRewardRebate = "BCREventShopRewardRebate";
    public const string BCRDiscoverPost = "BCRDiscoverPost";
    public const string Wallet = "Wallet";
    public const string Chat = "Chat";
    public const string Medium = "Medium";
    public const string Deposit = "Deposit";
    public const string Withdrawal = "Withdrawal";
    public const string PayoutRecord = "PayoutRecord";
    public const string Transaction = "Transaction";
    public const string Verification = "Verification";
    public const string BCRPartyId = "BCRPartyId";
    public const string BCRUserId = "BCRUserId";
    public const string BCRPaymentMethod = "BCRPaymentMethod";
    public const string BCRPaymentInfo = "BCRPaymentInfo";
    public const string BCRPayment = "BCRPayment";
}