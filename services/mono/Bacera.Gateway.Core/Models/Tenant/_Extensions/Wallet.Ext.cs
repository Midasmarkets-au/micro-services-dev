using System.Globalization;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

partial class Wallet
{
    public virtual long BalanceInCents => Balance;
    public bool IsEmpty() => Id == 0;

    private static readonly Hashids Hashids = new(HashIdSaltTypes.Wallet, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.Wallet]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    
    
    public static Wallet Build(long partyId, CurrencyTypes currency, FundTypes type = FundTypes.Wire, bool isPrimary = false)
        => new()
        {
            Balance = 0,
            FundType = (int)type,
            PartyId = partyId,
            CurrencyId = (int)currency,
            TalliedOn = DateTime.UtcNow,
            Sequence = currency.GetWalletSequence(),
            Number = WalletExtensions.GenerateNumber(partyId, currency, type),
            IsPrimary = (short)(isPrimary ? 1 : 0), // Default to not primary, will be set by AccountingService
        };

    public Wallet AddBalance(long amount)
    {
        Balance += amount;
        TalliedOn = DateTime.UtcNow;
        return this;
    }

    public class ResponseModel
    {
        public long Id { get; set; }
        public FundTypes FundType { get; set; }
        public long Balance { get; set; }
        public int Sequence { get; set; }
        public int CurrencyId { get; set; }
        public DateTime TalliedOn { get; set; }
        public string Number { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}

public static class WalletExtensions
{
    public static string GenerateNumber(long partyId, CurrencyTypes currencyType, FundTypes fundType) => 
        "w" +
        DateTime.UtcNow.ToString("yy") +
        ISOWeek.GetWeekOfYear(DateTime.UtcNow.Date) +
        $"f{(int)fundType}00{(int)currencyType}00{partyId}";

    public static Wallet.ResponseModel ToResponseModel(this Wallet wallet)
        => new()
        {
            Id = wallet.Id,
            FundType = (FundTypes)wallet.FundType,
            Balance = wallet.Balance,
            Sequence = wallet.Sequence,
            CurrencyId = wallet.CurrencyId,
            TalliedOn = wallet.TalliedOn,
            Number = wallet.Number,
            IsPrimary = wallet.IsPrimary == 1? true : false,
        };

    public static IQueryable<Wallet.ResponseModel> ToResponseModels(this IQueryable<Wallet> query)
        => query.Select(x => new Wallet.ResponseModel
        {
            Id = x.Id,
            FundType = (FundTypes)x.FundType,
            Balance = x.Balance,
            Sequence = x.Sequence,
            CurrencyId = x.CurrencyId,
            TalliedOn = x.TalliedOn,
            Number = x.Number,
            IsPrimary = x.IsPrimary == 1 ? true : false,
        });
}