using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class TradeAccountJob(IServiceProvider serviceProvider)
    : ITradeAccountJob
{
    public async Task<Dictionary<long, double>> CheckTradeAccountBalanceAsync(long tenantId, long partyId)
    {
        var dict = new Dictionary<long, double>();

        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var svc = scope.ServiceProvider.GetRequiredService<TradingService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var tradeAccounts = await ctx.TradeAccounts
            .Where(x => x.IdNavigation.PartyId == partyId)
            .Select(x => new TradeAccount { Id = x.Id })
            .ToListAsync();
        foreach (var tradeAccount in tradeAccounts)
        {
            try
            {
                var (success, balance) = await svc.TradeAccountCheckBalance(tradeAccount.Id);
                dict.Add(tradeAccount.Id, balance);
            }
            catch
            {
                dict.Add(tradeAccount.Id, -99);
            }
        }

        return dict;
    }

    public async Task<bool> AdjustCreditOrBalanceByBatchId(long tenantId, long adjustBatchId)
    {
        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var apiSvc = scope.ServiceProvider.GetRequiredService<ITradingApiService>();

        var adjustBatch = await ctx.AdjustBatches
            .Where(x => x.Status != (short)AdjustBatchStatusTypes.Processing)
            .SingleOrDefaultAsync(x => x.Id == adjustBatchId);

        if (adjustBatch is null)
            return false;

        adjustBatch.Status = (short)AdjustBatchStatusTypes.Processing;
        ctx.AdjustBatches.Update(adjustBatch);
        await ctx.SaveChangesAsync();
        var successCount = 0;

        const int size = 100;
        var page = 1;
        do
        {
            var records = await ctx.AdjustRecords
                .Where(x => x.AdjustBatchId == adjustBatchId)
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * size).Take(size)
                .ToListAsync();

            foreach (var record in records)
            {
                if (record.Status == (int)AdjustRecordStatusTypes.Completed)    
                    continue;

                // amount has been scaled with 10000 already and will be convert back in ChangeBalance() and ChangeCredit()
                var amount = Math.Round(((decimal)record.Amount / 100), 2);

                try
                {
                    var (result, ticket) = adjustBatch.Type switch
                    {
                        (short)AdjustTypes.Agent or (short)AdjustTypes.Adjust => await apiSvc.ChangeBalance(
                            adjustBatch.ServiceId, record.AccountNumber, amount, record.Comment),

                        (short)AdjustTypes.Credit => await apiSvc.ChangeCredit(adjustBatch.ServiceId, record.AccountNumber, amount,
                            GetCreditComment(record.Amount, record.Comment)),

                        _ => new Tuple<bool, string>(false, "Invalid adjust type")
                    };

                    if (!result)
                    {
                        BcrLog.Slack($"AdjustCreditOrBalanceByBatchId_Failed_to_adjust_for_account {record.AccountNumber}. Ticket: {ticket}");
                        record.Status = (short)AdjustRecordStatusTypes.Failed;
                    }
                    else
                    {
                        successCount++;
                        record.Ticket = long.TryParse(ticket, out var res) ? res : 0;
                        record.Status = (short)AdjustRecordStatusTypes.Completed;
                    }
                }
                catch (Exception e)
                {
                    BcrLog.Slack($"AdjustCreditOrBalanceByBatchId_Failed_to_adjust_for_account {record.AccountNumber}. Error: {e}");
                    record.Status = (short)AdjustRecordStatusTypes.Failed;
                }

                ctx.AdjustRecords.Update(record);
                await ctx.SaveChangesAsync();
                await Task.Delay(100);
            }

            if (records.Count < size)
                break;

            page++;
        } while (true);

        var batchResult = adjustBatch.GetResult();
        batchResult.SuccessCount = successCount;
        adjustBatch.Result = JsonConvert.SerializeObject(batchResult);
        adjustBatch.Status = (short)AdjustBatchStatusTypes.Completed;
        ctx.AdjustBatches.Update(adjustBatch);
        await ctx.SaveChangesAsync();
        return true;
    }

    private static string GetCreditComment(decimal amount, string comment)
        => (comment.Contains("credit", StringComparison.CurrentCultureIgnoreCase) ? "" : "Credit ")
           + (amount > 0 ? "in " : "out ")
           + comment.Trim();
    //
    // private static string GetAdjustComment(decimal amount, string comment) => comment.Trim();
}