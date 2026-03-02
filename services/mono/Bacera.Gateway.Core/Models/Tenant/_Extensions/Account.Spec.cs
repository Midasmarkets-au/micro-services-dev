using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class Account
{
    public class UpdateAliasSpec
    {
        public long Uid { get; set; }
        [Required] public string Alias { get; set; } = string.Empty;
    }

    public class UpdateTag
    {
        public long Id { get; set; }
        public List<string> TagNames { get; set; } = new();
    }

    public class UpdateType
    {
        public long Id { get; set; }
        [Required] public AccountTypes Type { get; set; }
    }

    public class UpdateSite
    {
        public long Id { get; set; }
        [Required] public SiteTypes Site { get; set; }
    }

    public class UpdateGroupName
    {
        public long Id { get; set; }

        // required and not empty
        [Required] [MinLength(4)] public string GroupName { get; set; } = string.Empty;
    }


    public sealed class TradeAccountCreateSpec
    {
        public string ReferCode { get; set; } = string.Empty;
        public AccountTypes AccountType { get; set; }
        public int ServiceId { get; set; }
        public PlatformTypes Platform { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public int Leverage { get; set; }

        public ApplicationSupplement ToApplicationSupplement() => new()
        {
            Role = AccountRoleTypes.Client,
            ReferCode = ReferCode,
            AccountType = AccountType,
            ServiceId = ServiceId,
            Platform = Platform,
            CurrencyId = (int)CurrencyId,
            Leverage = Leverage
        };

        public static TradeAccountCreateSpec FromJson(string json) => Utils.JsonDeserializeObjectWithDefault<TradeAccountCreateSpec>(json);
    }
}