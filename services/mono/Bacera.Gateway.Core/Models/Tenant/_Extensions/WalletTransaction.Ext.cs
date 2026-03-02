namespace Bacera.Gateway;

partial class WalletTransaction
{
    public static WalletTransaction Build(long walletId, long matterId, long prevBalance, long amount)
        => new()
        {
            WalletId = walletId,
            MatterId = matterId,
            PrevBalance = prevBalance,
            Amount = amount,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

    public class ResponseModel
    {
        public long Id { get; set; }
        public long WalletId { get; set; }
        public long? InvoiceId { get; set; }
        public long PrevBalance { get; set; }
        public long Amount { get; set; }
        public int StateId { get; set; }
        public DateTime StatedOn { get; set; }
    }
}

public static class WalletTransactionExt
{
    public static WalletTransaction TransactionByMatter(this Wallet wallet, long matterId, long prevBalance, long amount)
        => WalletTransaction.Build(wallet.Id, matterId, prevBalance, amount);

    public static IQueryable<WalletTransaction.ResponseModel> ToResponseModels(this IQueryable<WalletTransaction> query)
        => query.Select(x => new WalletTransaction.ResponseModel
        {
            Id = x.Id,
            WalletId = x.WalletId,
            InvoiceId = x.InvoiceId,
            PrevBalance = x.PrevBalance,
            Amount = x.Amount,
            StateId = x.Matter.StateId,
            StatedOn = x.Matter.StatedOn,
        });
}