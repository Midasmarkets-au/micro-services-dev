using System.Runtime.InteropServices;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.TronProCrypto;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class CryptoService(
    TenantDbContext tenantCtx,
    IServiceProvider provider,
    ConfigService cfgSvc,
    IMyCache cache,
    IHttpClientFactory clientFactory,
    MyDbContextPool pool,
    ITenantGetter tenantGetter)
{
    private readonly long _tenantId = tenantGetter.GetTenantId();

    public async Task<Crypto?> GetRandomCryptoForPaymentAsync()
    {
        var cryptos = await tenantCtx.Cryptos
            .Where(x => !x.IsDeleted && x.Status == (int)CryptoStatusTypes.Idle && x.InUsePaymentId == null)
            .OrderBy(x => x.UpdatedOn)
            .ToListAsync();
        
        foreach (var crypto in cryptos)
        {
            var key = CacheKeys.CryptoWalletKey(crypto.Address);
            var value = await cache.GetStringAsync(key);
            if (value != null) continue;
            return crypto;
        }

        return null;
    }

    public async Task<List<long>> TronProSyncTransactionAsync(Crypto crypto)
    {
        var client = clientFactory.CreateClient(HttpClientHandlerTypes.TronPro);
        var url = $"{TronProCryptoUrl}{crypto.Address}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            // BcrLog.Slack($"TronProSyncTransactionAsync_cache_error: request not success_{response.StatusCode}");
            return [];
        }

        var json = await response.Content.ReadAsStringAsync();

        // try
        // {
        //     // Cache the response for 55 minutes
        //     var hourKey = DateTime.UtcNow.ToString("yyyyMMddHH");
        //     // var key = CacheKeys.CryptoWalletKey(crypto.Address);
        //     // var requestKey = $"{key}_{hourKey}";
        //     // var unixTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        //     var obj = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, object>>(json);
        //     obj["request_on_bcr"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        //     var value = JsonConvert.SerializeObject(obj);
        //     // await cache.AddToSortedSetAsync(requestKey, value, unixTime, TimeSpan.FromMinutes(55));
        //
        //     // write to file /home/forge/logs/crypto/request_logs/{crypto.Address}_{hourKey}
        //
        //     // var path = Path.Combine("/home/forge/logs/crypto", $"{crypto.Address}_{hourKey}");
        //     var path = Path.Combine("/home/forge/logs", $"{crypto.Address}_{hourKey}");
        //     // var path = Path.Combine("/Users/yixuan/Desktop/ddev", $"{crypto.Address}_{hourKey}");
        //     // await File.WriteAllTextAsync(path, value);
        //     // append
        //     await File.AppendAllTextAsync(path, value + Environment.NewLine);
        // }
        // catch (Exception e)
        // {
        //     BcrLog.Slack($"TronProSyncTransactionAsync_cache_error: {e.Message}");
        // }
        
        if (!TronProCryptoApiResponse.TryParse(json, out var result)) return [];

        var matchedDepositIds = new List<long>();
        foreach (var item in result.Data)
        {
            var transaction = await TryMatchTransactionByDataAsync(crypto, item);
            if (transaction == null) continue;

            var depositId = await MatchDepositByTransactionAsync(transaction);
            if (depositId != 0) matchedDepositIds.Add(depositId);
        }

        return matchedDepositIds;
    }

    private async Task<CryptoTransaction?> TryMatchTransactionByDataAsync(Crypto crypto, TransactionData item)
    {
        var transferToAddress = item.To;
        if (transferToAddress != crypto.Address) return null;

        var transactionHash = item.Hash;
        var transaction = await tenantCtx.CryptoTransactions.SingleOrDefaultAsync(x => x.TransactionHash == transactionHash);
        if (transaction != null)
        {
            if (transaction.Status == (short)CryptoTransactionStatusTypes.Pending)
                transaction.Confirmed = item.Confirmed == 1;
        }
        else
        {
            var amount = (long)(long.Parse(item.Amount) * Math.Pow(10, -(item.Decimals - 2)));
            // var createdOn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(item.BlockTimestamp);
            var confirmed = item.Confirmed == 1;
            var fromAddress = item.From;
            var data = JsonConvert.SerializeObject(item);
            transaction = new CryptoTransaction
            {
                CryptoId = crypto.Id,
                Amount = amount,
                CreatedOn = DateTime.UtcNow,
                Confirmed = confirmed,
                FromAddress = fromAddress,
                Status = (short)CryptoTransactionStatusTypes.Pending,
                TransactionHash = transactionHash,
                Data = data
            };
            tenantCtx.CryptoTransactions.Add(transaction);
            await tenantCtx.SaveChangesAsync();
        }

        return transaction;
    }

    public async Task<long> MatchDepositByTransactionAsync(CryptoTransaction transaction)
    {
        if (!transaction.Confirmed || transaction.Status == (short)CryptoTransactionStatusTypes.Completed) return 0;
        var setting = await cfgSvc.GetAsync<Crypto.Setting>(nameof(Public), 0, ConfigKeys.CryptoSetting);
        var crypto = transaction.Crypto;
        if (DateTime.UtcNow - transaction.CreatedOn >= TimeSpan.FromMinutes(setting!.PayExpiredTimeInMinutes))
        {
            transaction.Status = (short)CryptoTransactionStatusTypes.Completed;
            await tenantCtx.SaveChangesAsync();
            return 0;
        }

        var depositId = 0L;
        if (crypto.InUsePaymentId == null) return depositId;

        var payment = await tenantCtx.Payments.SingleOrDefaultAsync(x => x.Id == crypto.InUsePaymentId);
        if (payment != null && payment.Amount.ToCentsFromScaled() == transaction.Amount)
        {
            var hkey = CacheKeys.CryptoWalletPaymentHKey(crypto.Address, payment.Amount);
            var value = await cache.HGetStringAsync(hkey, transaction.TransactionHash);
            if (value != null)
            {
                BcrLog.Slack($"MatchDepositByTransactionAsync_same_amount_crypaddr-{crypto.Address}_pid1-{value}_pid2-{payment.Id}(discard)");
                transaction.Status = (short)CryptoTransactionStatusTypes.Completed;
            }
            else
            {
                await cache.HSetStringAsync(hkey, transaction.TransactionHash, payment.Id.ToString(),
                    TimeSpan.FromMinutes(setting.PayExpiredTimeInMinutes));

                transaction.PaymentId = payment.Id;
                
                depositId = await tenantCtx.Deposits
                    .Where(x => x.PaymentId == transaction.PaymentId)
                    .Where(x => x.IdNavigation.StateId == (short)StateTypes.DepositCreated)
                    .Select(x => x.Id)
                    .SingleOrDefaultAsync();

                if (depositId != 0)
                {
                    using var scope = provider.CreateTenantScope(_tenantId);
                    var acctSvc = scope.ServiceProvider.GetRequiredService<AcctService>();
                    var (res0, _) = await acctSvc.DepositCompletePaymentAsync(depositId, 1, "Crypto Complete Deposit Payment");
                    var (res1, _) = await acctSvc.DepositCallbackCompleteAsync(depositId, 1, "Crypto Complete Deposit");
                    if (!res0 || !res1)
                    {
                        transaction.Status = (short)CryptoTransactionStatusTypes.Failed;
                    }
                    else
                    {
                        transaction.Status = (short)CryptoTransactionStatusTypes.Completed;
                        payment.ReferenceNumber = transaction.TransactionHash;
                        tenantCtx.Entry(payment).Property(x => x.ReferenceNumber).IsModified = true;
                    }
                }
            }

            await cache.KeyDeleteAsync(CacheKeys.CryptoWalletKey(crypto.Address));
        }

        crypto.Release();
        await tenantCtx.SaveChangesAsync();
        return depositId;
    }

    public async Task<Crypto> CreateCryptoAsync(long operatorPartyId, string name, string type, string address)
    {
        var model = new Crypto
        {
            OperatorPartyId = operatorPartyId,
            Address = address,
            Name = name,
            Type = type,
            Status = (short)CryptoStatusTypes.Idle,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };
        tenantCtx.Cryptos.Add(model);
        await tenantCtx.SaveChangesAsync();
        return model;
    }

    public async Task<bool> UpdateCryptoAsync(long cryptoId, string address, long operatorPartyId)
    {
        var model = await tenantCtx.Cryptos.SingleOrDefaultAsync(x => x.Id == cryptoId);
        if (model == null) return false;

        model.Address = address;
        model.OperatorPartyId = operatorPartyId;
        model.UpdatedOn = DateTime.UtcNow;
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCryptoAsync(long cryptoId)
    {
        var model = await tenantCtx.Cryptos.SingleOrDefaultAsync(x => x.Id == cryptoId);
        if (model == null) return false;

        tenantCtx.Cryptos.Remove(model);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Soft delete a crypto wallet
    /// </summary>
    public async Task<(bool success, string message)> SoftDeleteCryptoAsync(long cryptoId)
    {
        var model = await tenantCtx.Cryptos
            .SingleOrDefaultAsync(x => x.Id == cryptoId && !x.IsDeleted);
        
        if (model == null) 
            return (false, "Crypto wallet not found or already deleted");

        if (model.Status == (int)CryptoStatusTypes.InUse || model.InUsePaymentId != null)
            return (false, "Cannot delete a crypto wallet that is currently in use");

        model.IsDeleted = true;
        model.UpdatedOn = DateTime.UtcNow;
        
        tenantCtx.Cryptos.Update(model);
        await tenantCtx.SaveChangesAsync();
        
        return (true, "Crypto wallet deleted successfully");
    }

    /// <summary>
    /// Restore a soft-deleted crypto wallet
    /// </summary>
    public async Task<(bool success, string message)> RestoreCryptoAsync(long cryptoId)
    {
        var model = await tenantCtx.Cryptos
            .SingleOrDefaultAsync(x => x.Id == cryptoId && x.IsDeleted);
        
        if (model == null) 
            return (false, "Crypto wallet not found or not deleted");

        model.IsDeleted = false;
        model.UpdatedOn = DateTime.UtcNow;
        
        tenantCtx.Cryptos.Update(model);
        await tenantCtx.SaveChangesAsync();
        
        return (true, "Crypto wallet restored successfully");
    }

    private const string QueryUrl = "https://apilist.tronscan.org/api/contract/events?address=";

    private const string TronProCryptoUrl =
        "https://apilist.tronscanapi.com/api/token_trc20/transfers-with-status?limit=10&start=0&trc20Id=TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t&direction=2&address=";

}

// public async Task<List<long>> SyncTransactionAsync(Crypto crypto)
// {
//     using var client = new HttpClient();
//     var url = $"{QueryUrl}{crypto.Address}";
//     var response = await client.GetAsync(url);
//     if (!response.IsSuccessStatusCode)
//     {
//         // var msg = await response.Content.ReadAsStringAsync();
//         // BcrLog.Slack($"SyncCryptoError_msg:{msg}");
//         return [];
//     }
//
//     var matchedDepositIds = new List<long>();
//     var content = await response.Content.ReadAsStringAsync();
//     foreach (var item in Utils.JsonDeserializeDynamic(content).data)
//     {
//         string transferToAddress = item.transferToAddress;
//         if (transferToAddress != crypto.Address)
//             continue;
//
//         string transactionHash = item.transactionHash;
//         var cryptoTransaction = await tenantCtx.CryptoTransactions.SingleOrDefaultAsync(x => x.TransactionHash == transactionHash);
//         if (cryptoTransaction != null)
//         {
//             if (cryptoTransaction.Status != (short)CryptoTransactionStatusTypes.Pending)
//                 continue;
//
//             cryptoTransaction.Confirmed = item.confirmed;
//         }
//         else
//         {
//             var amount = (long)(long.Parse((string)item.amount) * Math.Pow(10, -((int)item.decimals - 2)));
//             var createdOn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)item.timestamp);
//             bool confirmed = item.confirmed;
//             string fromAddress = item.transferFromAddress;
//             string data = JsonConvert.SerializeObject(item);
//             cryptoTransaction = new CryptoTransaction
//             {
//                 CryptoId = crypto.Id,
//                 Amount = amount,
//                 CreatedOn = createdOn,
//                 Confirmed = confirmed,
//                 FromAddress = fromAddress,
//                 Status = (short)CryptoTransactionStatusTypes.Pending,
//                 TransactionHash = transactionHash,
//                 Data = data
//             };
//             tenantCtx.CryptoTransactions.Add(cryptoTransaction);
//         }
//
//         if (cryptoTransaction.Confirmed)
//         {
//             cryptoTransaction.Status = (short)CryptoTransactionStatusTypes.Completed;
//             var payment = await tenantCtx.Payments.SingleOrDefaultAsync(x => x.Id == crypto.InUsePaymentId);
//             if (payment != null && payment.Amount == cryptoTransaction.Amount)
//             {
//                 cryptoTransaction.PaymentId = payment.Id;
//
//                 var depositId = await tenantCtx.Deposits
//                     .Where(x => x.PaymentId == cryptoTransaction.PaymentId)
//                     .Select(x => x.Id)
//                     .SingleOrDefaultAsync();
//
//                 if (depositId == 0) continue;
//
//                 var (res0, _) = await acctSvc.DepositCompletePaymentAsync(depositId, 1, "Crypto Complete Deposit Payment");
//                 var (res1, _) = await acctSvc.DepositCallbackCompleteAsync(depositId, 1, "Crypto Complete Deposit");
//                 if (!res0 || !res1)
//                 {
//                     cryptoTransaction.Status = (short)CryptoTransactionStatusTypes.Failed;
//                     crypto.Status = (int)CryptoStatusTypes.Idle;
//                     crypto.UpdatedOn = DateTime.UtcNow;
//                     await tenantCtx.SaveChangesAsync();
//                     continue;
//                 }
//
//                 cryptoTransaction.Status = (short)CryptoTransactionStatusTypes.Completed;
//                 crypto.Status = (int)CryptoStatusTypes.Idle;
//                 crypto.InUsePaymentId = null;
//                 crypto.UpdatedOn = DateTime.UtcNow;
//                 payment.ReferenceNumber = cryptoTransaction.TransactionHash;
//                 tenantCtx.Payments.Update(payment);
//                 tenantCtx.Cryptos.Update(crypto);
//                 tenantCtx.CryptoTransactions.Update(cryptoTransaction);
//                 matchedDepositIds.Add(depositId);
//             }
//         }
//
//         await tenantCtx.SaveChangesAsync();
//     }
//
//     return matchedDepositIds;
// }