using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.Services;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace Bacera.Gateway;

public partial class TradingService
{
    public async Task<Result<List<AdjustBatch.TenantResponseModel>, AdjustBatch.Criteria>> AdjustBatchQueryAsync(
        AdjustBatch.Criteria criteria)
    {
        var result = await dbContext.AdjustBatches
            .PagedFilterBy(criteria)
            .ToTenantResponseModel()
            .ToListAsync();

        return Result<List<AdjustBatch.TenantResponseModel>, AdjustBatch.Criteria>.Of(result, criteria);
    }

    public async Task<Result<List<AdjustRecord.TenantResponseModel>, AdjustRecord.Criteria>> AdjustRecordQueryAsync(
        AdjustRecord.Criteria criteria)
    {
        var result = await dbContext.AdjustRecords
            .PagedFilterBy(criteria)
            .ToTenantResponseModel()
            .ToListAsync();

        return Result<List<AdjustRecord.TenantResponseModel>, AdjustRecord.Criteria>.Of(result, criteria);
    }

    public async Task<(bool result, string msg, AdjustBatch.AdjustBatchResult response)>
        CreateProcessAdjustAccountBatch(long partyId, long tenantId, int serviceId, AdjustTypes type, string note, IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName);
        if (fileExtension != ".csv")
        {
            _logger.LogError("Invalid file extension. {FileExtension}", fileExtension);
            return (false, "__INVALID_FILE_EXTENSION__", new AdjustBatch.AdjustBatchResult());
        }

        var adjustBatch = new AdjustBatch
        {
            Note = note,
            Type = (short)type,
            ServiceId = serviceId,
            Status = (short)AdjustBatchStatusTypes.Created,
            OperatorPartyId = partyId
        };
        dbContext.AdjustBatches.Add(adjustBatch);
        await dbContext.SaveChangesAsync();
        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var (uploadResult, msg) = await storageSvc.UploadFileAsync(memoryStream,
                adjustBatch.GetFileDirInS3(tenantId),
                adjustBatch.GetFileNameInS3(), fileExtension, "text/csv", tenantId, partyId);

            if (!uploadResult)
            {
                // BcrLog.Slack($"Upload account batch process csv file fail. msg:{msg}");
                return (false, "__UPLOAD_FAIL__", new AdjustBatch.AdjustBatchResult());
            }
        }
        catch
        {
            // BcrLog.Slack($"Process account batch process csv file fail.");
            return (false, "__PROCESS_FILE_FAIL__", new AdjustBatch.AdjustBatchResult());
        }

        adjustBatch.File = adjustBatch.GetFileFullPathInS3(tenantId);
        dbContext.AdjustBatches.Update(adjustBatch);
        await dbContext.SaveChangesAsync();

        var adjustRecords = new List<AdjustRecord>();
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0; // Reset stream position to the beginning

            using var reader = new StreamReader(stream, Encoding.UTF8);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                    continue;

                var adjustRecord = AdjustRecord.ReadFromCsvLine(line, type);
                adjustRecord.AdjustBatchId = adjustBatch.Id;
                adjustRecord.OperatorPartyId = partyId;
                adjustRecord.Type = (short)type;
                adjustRecord.Status = (short)AdjustRecordStatusTypes.Created;
                adjustRecords.Add(adjustRecord);
            }
        }
        catch
        {
            // _logger.LogError("Process account batch process csv file fail.");
            return (false, "__PROCESS_FILE_FAIL__", new AdjustBatch.AdjustBatchResult());
        }

        var accounts = await dbContext.Accounts
            .Where(x => adjustRecords.Select(r => r.AccountNumber).Contains(x.AccountNumber))
            .Select(x => new { x.AccountNumber, x.Id })
            .ToListAsync();

        adjustRecords.ForEach(x => x.AccountId = accounts.FirstOrDefault(a => a.AccountNumber == x.AccountNumber)?.Id);
        dbContext.AdjustRecords.AddRange(adjustRecords);
        await dbContext.SaveChangesAsync();

        var total = adjustRecords.Count;
        var totalAmount = adjustRecords.Sum(x => x.Amount);
        var accountsInOurSystem = adjustRecords.Count(x => x.AccountId != null);
        var response = AdjustBatch.AdjustBatchResult.Build(adjustBatch.Id, total, accountsInOurSystem, totalAmount);

        adjustBatch.Result = JsonConvert.SerializeObject(response);
        dbContext.AdjustBatches.Update(adjustBatch);
        await dbContext.SaveChangesAsync();

        return (true, "__SUCCESS__", response);
    }

    public async Task<bool> ConfirmAccountBatchProcessingByIdAsync(long adjustBatchId)
    {
        var adjustBatch = await dbContext.AdjustBatches
            .Include(x => x.AdjustRecords)
            .SingleOrDefaultAsync(x => x.Id == adjustBatchId);
        if (adjustBatch is not { Status: (short)AdjustBatchStatusTypes.Created })
        {
            _logger.LogError("Invalid adjust batch status. {Status}", adjustBatch?.Status);
            return false;
        }

        adjustBatch.AdjustRecords.ForEach(x => x.Status = (short)AdjustRecordStatusTypes.Processing);
        adjustBatch.Status = (short)AdjustBatchStatusTypes.Pending;
        dbContext.AdjustBatches.Update(adjustBatch);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(bool result, string msg)> CreateAccountAdjustAsync(AdjustTypes type, long accountId,
        decimal amount, string comment, long operatorPartyId)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
        if (account == null)
            return (false, "__ACCOUNT_NOT_FOUND__");
        // if (amount is 0 or < -500000m or > 500000m)
        //     return (false, "__INVALID_AMOUNT__");

        // *** Scale 10000 due to TradeAccountChangeCreditAsync() and TradeAccountChangeBalanceAndUpdateStatus() will be convert back of *10000 scale *** //
        amount = amount.ToScaledFromCents();

        var (success, ticket) = type switch
        {
            // It will call apiService.ChangeCredit() which rollbacks scale *10000
            AdjustTypes.Credit => await TradeAccountChangeCreditAsync(accountId, amount / 100, GetCreditComment(amount, comment)),

            // It will call apiService.ChangeBalance() which rollbacks scale *10000
            AdjustTypes.Adjust => await TradeAccountChangeBalanceAndUpdateStatus(accountId, amount / 100, GetAdjustComment(amount, comment)),
            _ => new Tuple<bool, string>(false, "__INVALID_ADJUST_TYPE__")
        };
        if (!success)
            return (false, "__TRADE_ACCOUNT_CHANGE_ADJUST_OR_CREDIT_FAIL__");

        var adjustRecord = new AdjustRecord
        {
            AccountNumber = account.AccountNumber,
            Amount = (long)amount,
            Comment = comment,
            AccountId = accountId,
            Type = (short)type,
            Ticket = long.Parse(ticket),
            Status = (short)AdjustRecordStatusTypes.Completed,
            OperatorPartyId = operatorPartyId
        };
        account.AccountLogs.Add(Account.BuildLog(operatorPartyId,
            "Account" + (Enum.GetName(type) ?? type.ToString()) + "Finished"
            , ""
            , $"Ticket: {ticket}, Amount: {(amount / 100).ToCentsFromScaled()}, Comment: {comment}"));
        dbContext.Accounts.Update(account);
        dbContext.AdjustRecords.Add(adjustRecord);
        await dbContext.SaveChangesAsync();
        return (true, ticket);
    }

    public async Task<long> GetExistingAdjustRecordTicket(long serviceId, long accountNumber, string comment,
        double amount)
    {
        var trdSvc = await GetMetaTradeDatabaseOptions(serviceId);
        if (trdSvc == null)
        {
            _logger.LogError("Trade account service not found");
            return 0;
        }

        switch (trdSvc.Platform)
        {
            case (short)PlatformTypes.MetaTrader4:
            {
                var mt4Ctx = await GetMetaTrade4DbContext(trdSvc.Id, true);
                // if long type of accountNumber exceeds int, return 0
                if (accountNumber > int.MaxValue)
                {
                    _logger.LogError("Account number exceeds int");
                    return 0;
                }

                var login = (int)accountNumber;
                var ticket = mt4Ctx.Mt4Trades.Where(x =>
                        x.Login == login && x.Comment == comment && Math.Abs(x.Profit - amount) < 0.001)
                    .Select(x => x.Ticket)
                    .FirstOrDefault();
                return ticket;
            }
            case (short)PlatformTypes.MetaTrader5:
                return 0;
            default:
                return 0;
        }
    }

    private static string GetCreditComment(decimal amount, string comment)
        => "Credit " + (amount > 0 ? "in" : "out") + " " + comment.Trim();

    private static string GetAdjustComment(decimal amount, string comment)
        => comment.Contains("Adjust") ? comment : "Adjust " + comment.Trim();
}