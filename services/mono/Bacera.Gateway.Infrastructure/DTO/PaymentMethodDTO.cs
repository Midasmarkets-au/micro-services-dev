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

    public sealed class GroupInfo
    {
        public string HashId { get; set; } = null!;
        public string Policy { get; set; } = null!;
        public PlatformTypes Platform { get; set; }
        public string Instruction { get; set; } = null!;
        public long[] Range { get; set; } = null!;

        public List<CurrencyRate> CurrencyRates { get; set; } = [];

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
    }
}