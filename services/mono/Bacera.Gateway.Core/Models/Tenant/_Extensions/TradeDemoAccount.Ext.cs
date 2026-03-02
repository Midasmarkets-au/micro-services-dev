using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class TradeDemoAccount
{
    public bool IsEmpty() => Id == default;

    public static TradeDemoAccount Build(long partyId, int serviceId, long accountNumber = 0, string email = "")
        => new()
        {
            PartyId = partyId,
            ServiceId = serviceId,
            AccountNumber = accountNumber,
            ExpireOn = DateTime.UtcNow.AddMonths(1),
            Name = string.Empty,
            Email = email,
            PhoneNumber = string.Empty,
            CountryCode = string.Empty,
            ReferralCode = string.Empty,
        };

    public class CreateSpec
    {
        [Required] public int Leverage { get; set; }
        
        [Required] public PlatformTypes Platform { get; set; }
        [Required] public long Amount { get; set; } = 10000;
        public AccountTypes AccountType { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
    }

    public class CreateSpecV2
    {
        [Required] public int ServiceId { get; set; }
        [Required] public int Leverage { get; set; }
        [Required] public long Amount { get; set; } = 10000;
        public AccountTypes AccountType { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
    }

    public class DemoCreateSpec
    {
        [Required] public int Leverage { get; set; }
        [Required] public int Amount { get; set; }
        [Required] public PlatformTypes Platform { get; set; }
        public long TenantId { get; set; }

        public AccountTypes AccountType { get; set; }
        public CurrencyTypes CurrencyId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        
    }

    public class ClientResponseModel
    {
        public long Id { get; set; }
        public int ServiceId { get; set; }
        public long AccountNumber { get; set; }
        public DateTime ExpireOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public double Balance { get; set; }
        public long BalanceInCents { get; set; }
        public int Leverage { get; set; }
        public int CurrencyId { get; set; }
        public short Type { get; set; }
        public string ReferralCode { get; set; } = string.Empty;

        public static ClientResponseModel Build(TradeDemoAccount model)
            => new()
            {
                Id = model.Id,
                Type = model.Type,
                ServiceId = model.ServiceId,
                AccountNumber = model.AccountNumber,
                ExpireOn = model.ExpireOn,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                Balance = model.Balance,
                Leverage = model.Leverage,
                CurrencyId = model.CurrencyId,
                BalanceInCents = (long)Math.Round(model.Balance * 100, 0),
                ReferralCode = model.ReferralCode,
            };
    }
}