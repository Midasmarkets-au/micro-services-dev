using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway.DTO;

public sealed class PaymentMethodDTO
{
    public sealed class AccessManagement
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public CurrencyTypes CurrencyId { get; set; } 
        public PaymentMethodStatusTypes Status { get; set; }
        public PaymentMethodAccessStatusTypes AccessStatus { get; set; }
        public bool IsDisplay { get; set; }
    }

    public sealed class TenantAccessManagement
    {
        public List<AccessManagement> Deposit { get; set; } = [];
        public List<AccessManagement> Withdrawal { get; set; } = [];
    }

    public sealed class DepositGuideInfo
    {
        public string PaymentMethodName { get; set; } = null!;
        public PlatformTypes Platform { get; set; }
        public string Instruction { get; set; } = null!;
        public object Info { get; set; } = new();
    }

    /// <summary>
    /// One ExLink Global deposit variant (tenant-configured rail). Used on deposit-group-info when group is ExLink Global.
    /// </summary>
    public sealed class ExLinkGlobalPaymentMethodInfo
    {
        public CurrencyTypes CurrencyId { get; set; }
        public string HashId { get; set; } = null!;
        public long[] Range { get; set; } = null!;
        public string PaymentMethodName { get; set; } = null!;
    }

    public sealed class GroupInfo
    {
        public string HashId { get; set; } = null!;
        public string Policy { get; set; } = null!;
        public PlatformTypes Platform { get; set; }
        public string Instruction { get; set; } = null!;
        public long[] Range { get; set; } = null!;

        public List<CurrencyRate> CurrencyRates { get; set; } = [];

        /// <summary>
        /// All active named ExLink Global deposit methods for this tenant (hashId + range per primary CurrencyId).
        /// <see cref="CurrencyRates"/> stays driven by the opened method's configured currency list (may be a subset).
        /// </summary>
        public List<ExLinkGlobalPaymentMethodInfo> PaymentMethods { get; set; } = [];

        public List<string> RequestKeys { get; set; } = [];

        public object RequestValues { get; set; } = new();

        public static GroupInfo Build(string hashId, PlatformTypes platform, string policy, string instruction,
            long[] range, List<CurrencyRate> currencyRates, List<string> requestKeys)
            => new()
            {
                HashId = hashId,
                Policy = policy,
                Platform = platform,
                Instruction = instruction,
                Range = range,
                CurrencyRates = currencyRates,
                RequestKeys = requestKeys,
            };
    }

    public sealed class CurrencyRate
    {
        public CurrencyTypes CurrencyId { get; set; }
        public decimal Rate { get; set; }

        /// <summary>
        /// HashId of the specific payment method for this currency (ExLinkGlobal only).
        /// </summary>
        public string? HashId { get; set; }

        /// <summary>
        /// [min, max] deposit range in base units for this currency method (ExLinkGlobal only).
        /// </summary>
        public long[]? Range { get; set; }
    }
}
