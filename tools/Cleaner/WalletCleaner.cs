using Bacera.Gateway;
using Microsoft.EntityFrameworkCore;

namespace Cleaner;

public static class WalletCleaner
{
    public static async Task<Tuple<long, int>> SortTransactions(TenantDbContext dbContext, long walletId)
    {
        var transactions = await dbContext.WalletTransactions
            .Where(x => x.WalletId == walletId)
            .ToListAsync();

        if (!transactions.Any()) return Tuple.Create(walletId, 0);

        var prevBalance = 0L;
        foreach (var item in transactions.OrderBy(x => x.Id))
        {
            item.PrevBalance = prevBalance;
            prevBalance = item.PrevBalance + item.Amount;
        }

        dbContext.WalletTransactions.UpdateRange(transactions);

        await dbContext.SaveChangesAsync();

        return Tuple.Create(walletId, transactions.Count);
    }

    public static async Task<Tuple<Wallet, int>> RemoveDuplicatedTransactionsAsync(TenantDbContext dbContext,
        long walletId)
    {
        var wallet = await dbContext.Wallets
            .FirstOrDefaultAsync(x => x.Id == walletId);

        //.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (wallet == null)
            return Tuple.Create(new Wallet(), 0);

        var existsTransactions = new List<WalletTransaction>();
        var removeTransactions = new List<WalletTransaction>();

        var transactions = await dbContext.WalletTransactions
            .Where(x => x.WalletId == walletId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        if (!transactions.Any()) return Tuple.Create(new Wallet(), 0);

        foreach (var transaction in transactions)
        {
            var exists = existsTransactions.Any(x =>
                x.WalletId == transaction.WalletId &&
                x.MatterId == transaction.MatterId &&
                x.Amount == transaction.Amount
            );
            if (!exists)
            {
                existsTransactions.Add(transaction);
                continue;
            }

            removeTransactions.Add(transaction);
        }

        if (!removeTransactions.Any()) return Tuple.Create(new Wallet(), 0);

        wallet.Balance = existsTransactions.Sum(x => x.Amount);
        dbContext.Wallets.Update(wallet);

        // var transCount = existsTransactions.Count;
        var removeCount = removeTransactions.Count;

        var prevBalance = 0L;
        foreach (var item in transactions.OrderBy(x => x.Id))
        {
            item.PrevBalance = prevBalance;
            prevBalance = item.PrevBalance + item.Amount;
        }

        dbContext.WalletTransactions.RemoveRange(removeTransactions);
        dbContext.WalletTransactions.UpdateRange(existsTransactions);

        await dbContext.SaveChangesAsync();

        return Tuple.Create(wallet, removeCount);
    }

    public static List<long> WalletList => new List<long>
    {
        218144
    };
}