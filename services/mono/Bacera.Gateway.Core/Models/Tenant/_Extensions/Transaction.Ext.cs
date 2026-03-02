using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

using M = Transaction;

partial class Transaction : ICloneable, IHasMatter
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Transaction, 8, HashIdSaltTypes.Dictionary[HashIdSaltTypes.Transaction]);
    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public bool IsBetweenTradeAccounts() => SourceAccountType == (short)TransactionAccountTypes.Account &&
                                            TargetAccountType == (short)TransactionAccountTypes.Account;

    public bool IsBetweenWallets() => SourceAccountType == (short) TransactionAccountTypes.Wallet &&
                                      TargetAccountType == (short)TransactionAccountTypes.Wallet;


    public Transaction()
    {
        ReferenceNumber = string.Empty;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public M DeepCopy()
    {
        var copyObj = (M)MemberwiseClone();
        return copyObj;
    }

    public bool IsEmpty() => Id == 0;

    public static M Build(long senderPartyId,
        TransactionAccountTypes senderAccountType,
        long senderAccountId,
        long receiverPartyId,
        TransactionAccountTypes receiverAccountType,
        long receiverAccountId,
        LedgerSideTypes ledgerSide, long amount,
        FundTypes fundType,
        CurrencyTypes currency = CurrencyTypes.USD)
        => new()
        {
            PartyId = senderPartyId,
            Amount = amount,
            CurrencyId = (int)currency,
            SourceAccountId = senderAccountId,
            SourceAccountType = (short)senderAccountType,
            TargetAccountId = receiverAccountId,
            TargetAccountType = (short)receiverAccountType,
            CreatedOn = DateTime.UtcNow,
            FundType = (int)fundType,
            IdNavigation = Matter.Build().Transaction().SetState(StateTypes.TransferAwaitingApproval),
        };

    public class ClientResponseModel
    {
        public long Id { get; set; }
        public int SourceAccountType { get; set; }
        public long SourceAccountNumber { get; set; }
        public int TargetAccountType { get; set; }
        public long TargetAccountNumber { get; set; }
        public FundTypes FundType { get; set; }
        public int CurrencyId { get; set; }
        public long Amount { get; set; }
        public int StateId { get; set; }
        public DateTime StatedOn { get; set; }
        public string FlowType { get; set; }

        public static ClientResponseModel From(M model)
            => new()
            {
                Id = model.Id,
                FundType = (FundTypes)model.FundType,
                SourceAccountType = model.SourceAccountType,
                TargetAccountType = model.TargetAccountType,
                CurrencyId = model.CurrencyId,
                Amount = model.Amount,
                StateId = model.IdNavigation.StateId,
                StatedOn = model.IdNavigation.StatedOn,
            };
    }

    public class TradeAccountTransactionResponseModel
    {
        public long Id { get; set; }

        public FundTypes FundType { get; set; }
        public long? SourceAccountUid { get; set; }
        public long? TargetAccountUid { get; set; }
        public int CurrencyId { get; set; }
        public long Amount { get; set; }
        public int Status { get; set; }
    }

    public class ClientRequestModel
    {
        public long WalletId { get; set; }
        public long TradeAccountUid { get; set; }
        public long Amount { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
    }

    public class RequestModel
    {
        public long PartyId { get; set; }
        public long Amount { get; set; }
        public long WalletId { get; set; }
        public long TradeAccountId { get; set; }
    }

    /// <summary>
    /// Request model for verification code
    /// </summary>
    public class RequestCodeModel
    {
        public string? AuthType { get; set; }
    }
}

public static class TransactionExt
{
    public static IQueryable<M.ClientResponseModel> ToClientResponseModels(this IQueryable<M> query, long partyId)
        => query.Select(x =>
            new M.ClientResponseModel
            {
                Id = x.Id,
                SourceAccountType = x.SourceAccountType,
                TargetAccountType = x.TargetAccountType,
                FundType = (FundTypes)x.FundType,
                CurrencyId = x.CurrencyId,
                Amount = x.Amount,
                StateId = x.IdNavigation.StateId,
                StatedOn = x.IdNavigation.StatedOn
            }
        );
}