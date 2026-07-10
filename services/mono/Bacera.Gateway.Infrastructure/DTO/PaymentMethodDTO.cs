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
    /// One per-row deposit/withdrawal variant exposed to the client as a step-3 dropdown entry.
    /// Used on deposit-group-info / withdrawal-info for groups that fan out into multiple rows
    /// (ExLink Global per currency, Help2Pay per channel x currency).
    /// </summary>
    public sealed class GroupPaymentMethodInfo
    {
        public CurrencyTypes CurrencyId { get; set; }
        public string HashId { get; set; } = null!;
        public long[] Range { get; set; } = null!;
        public string PaymentMethodName { get; set; } = null!;

        /// <summary>
        /// Optional per-row bank whitelist (Help2Pay only). Null/empty for ExLink Global and other
        /// platforms that don't surface a payer-bank dropdown. The client renders this as the
        /// `bank` request-key dropdown ("CODE - Name") when populated.
        /// </summary>
        public List<BankOption>? Banks { get; set; }
    }

    public sealed class BankOption
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
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
        /// Per-row variants for groups that fan out (ExLink Global per primary currency, Help2Pay per channel x currency).
        /// Each entry carries its own HashId + Range and is rendered as a single dropdown option in step 3.
        /// <see cref="CurrencyRates"/> stays driven by the opened method's configured currency list (may be a subset).
        /// </summary>
        public List<GroupPaymentMethodInfo> PaymentMethods { get; set; } = [];

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
