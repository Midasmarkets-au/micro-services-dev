using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class TradeAccount : ILeadable<int>
{
    public bool IsEmpty() => Id == 0;
    public int Status => IdNavigation?.Status ?? 0;
    public static TradeAccount Empty() => new();

    public static TradeAccount Build(long accountId, CurrencyTypes currencyType = CurrencyTypes.USD)
        => new()
        {
            Id = accountId,
            CurrencyId = (int)currencyType,
            TradeAccountStatus = new TradeAccountStatus
            {
                Currency = Enum.GetName(typeof(CurrencyTypes), currencyType) ?? "USD",
                ModifiedOn = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
            },
        };

    public sealed class CreatedTradeAccountInfo
    {
        public long AccountId { get; set; }
        public long AccountNumber { get; set; }
        public string Password { get; set; } = string.Empty;
        public string PasswordInvestor { get; set; } = string.Empty;
        public string PasswordPhone { get; set; } = string.Empty;

        public bool IsEmpty() => AccountId == 0 || AccountNumber == 0;
    }
    
    public sealed class WholesaleReferralInfo
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public long AccountNumber { get; set; }
    }

    [Serializable]
    public sealed class ClientResponseModel
    {
        public long Uid { get; set; }
        public int ServiceId { get; set; }
        public short ServicePlatformId { get; set; }
        public long AccountNumber { get; set; }
        public int CurrencyId { get; set; }
        public int FundType { get; set; }
        public DateTime? LastSyncedOn { get; set; }

        // referenced from TradeAccountStatus
        public int Leverage { get; set; }
        public double Balance { get; set; }
        public double Credit { get; set; }
        public double Equity { get; set; }
        public double Margin { get; set; }

        public long BalanceInCents => Balance.ToAmountInCents();
        public long CreditInCents => Credit.ToAmountInCents();
        public long EquityInCents => Equity.ToAmountInCents();

        public bool IsEmpty() => AccountNumber == 0;
    }

    public sealed class ChangeBalanceSpec
    {
        public string AdjustType { get; set; } = string.Empty;
        public string Ticket { get; set; } = string.Empty;
        [Required] public long AccountId { get; set; }
        [Required] public decimal Amount { get; set; }
        [Required] public string Comment { get; set; } = string.Empty;
    }
}

public static class TradeAccountExt
{
    public static IQueryable<TradeAccount.ClientResponseModel> ToClientResponseModels(
        this IQueryable<TradeAccount> query)
        => query
            .Select(x => new TradeAccount.ClientResponseModel
            {
                Uid = x.IdNavigation.Uid,
                ServiceId = x.ServiceId,
                CurrencyId = x.CurrencyId,
                FundType = x.IdNavigation.FundType,
                LastSyncedOn = x.LastSyncedOn,
                AccountNumber = x.AccountNumber,
                Balance = (x.TradeAccountStatus != null) ? x.TradeAccountStatus.Balance : 0,
                Leverage = (x.TradeAccountStatus != null) ? x.TradeAccountStatus.Leverage : 0,
                ServicePlatformId = x.Service.Platform,
            });
}