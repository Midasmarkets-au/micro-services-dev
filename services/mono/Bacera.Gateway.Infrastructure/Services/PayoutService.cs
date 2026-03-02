using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.OFAPay;
using Bacera.Gateway.Vendor.Pay247;
using Bacera.Gateway.Vendor.ExLinkCashier;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class PayoutService(TenantDbContext tenantCtx, ILogger<PayoutService> logger, IHttpClientFactory clientFactory)
{
    public async Task<(long, long)> BatchPayoutAsync(string batchUid, long operatorPartyId = 1)
    {
        int success = 0, failed = 0;
        var query = tenantCtx.PayoutRecords
            .Include(x => x.PaymentMethod)
            .OrderBy(x => x.Id)
            .Where(x => x.BatchUid == batchUid);

        const int size = 10;
        var page = 0;
        while (true)
        {
            var items = await query.Skip(page * size).Take(size).ToListAsync();
            foreach (var item in items)
            {
                if (item.Status == (short)PayoutRecordStatusTypes.Completed)
                {
                    success++;
                    continue;
                }

                // if (item.Status == (short)PayoutRecordStatusTypes.Failed)
                // {
                //     failed++;
                //     continue;
                // }

                var response = await PayoutAsync(item, item.PaymentMethod, operatorPartyId);
                if (response == null || response.IsSuccess == false)
                {
                    item.Status = (short)PayoutRecordStatusTypes.Failed;
                    failed++;
                }
                else
                {
                    item.Status = (short)PayoutRecordStatusTypes.Completed;
                    success++;
                }
            }

            await tenantCtx.SaveChangesAsync();
            if (items.Count < size)
                break;

            page += 1;
        }

        return (success, failed);
    }

    public async Task<PayoutResponseModel?> PayoutAsync(PayoutRecord record, PaymentMethod method,
        long operatorPartyId = 1)
    {
        PayoutResponseModel? response = null;
        if (method.Platform == (int)PaymentPlatformTypes.Pay247)
        {
            var options = Pay247Options.FromJson(method.Configuration);
            var client = new Pay247.PayoutRequestClient
            {
                Amount = record.Amount,
                AccountName = record.AccountName,
                BankNumber = record.BankNumber,
                PaymentNumber = record.HashId,
                BankCode = record.BankCode,
                Currency = record.Currency,
                Logger = logger,
                Options = options,
                Client = clientFactory.CreateClient(),
            };

            response = await client.RequestAsync();
        }
        else if (method.Platform == (int)PaymentPlatformTypes.OFAPay)
        {
            var options = OFAPayOptions.FromJson(method.Configuration);
            var client = new OFAPay.DFRequestClient
            {
                Amount = record.Amount,
                AccountName = record.AccountName,
                BankNumber = record.BankNumber,
                BankName = record.BankName,
                BankCode = record.BankCode,
                NotifyUrl = options.CallbackUrl + $"unfinished/{PayoutRecord.HashDecode(record.HashId)}",
                PaymentNumber = record.HashId,
                Options = options,
                Logger = logger,
                Client = clientFactory.CreateClient(),
            };

            response = await client.RequestAsync();
        }
        else if (method.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
        {
            var options = ExLinkCashierOptions.FromJson(method.Configuration);
            
            // Validate ExLink account balance before withdrawal
            var balanceResponse = await ExLinkCashier.QueryAccountBalanceAsync(
                options.Uid,
                options.SecretKey,
                clientFactory.CreateClient(),
                logger
            );

            if (balanceResponse != null)
            {
                var currencyBalance = balanceResponse.Data
                    .FirstOrDefault(x => x.CoinName.Equals(record.Currency, StringComparison.OrdinalIgnoreCase));
                
                if (currencyBalance != null)
                {
                    decimal availableBalance = currencyBalance.Amount - currencyBalance.FrozenAmount;
                    
                    if (record.Amount > availableBalance)
                    {
                        logger.LogWarning("ExLink balance insufficient: {Currency} balance={Balance}, frozen={Frozen}, requested={Requested}",
                            record.Currency, currencyBalance.Amount, currencyBalance.FrozenAmount, record.Amount);
                        
                        return new PayoutResponseModel
                        {
                            IsSuccess = false,
                            Message = $"Insufficient ExLink balance. Available: {availableBalance:F2} {record.Currency}, Requested: {record.Amount:F2}",
                            ResponseJson = JsonConvert.SerializeObject(balanceResponse)
                        };
                    }
                    
                    logger.LogInformation("ExLink balance check passed: {Currency} available={Available}, requested={Requested}",
                        record.Currency, availableBalance, record.Amount);
                }
                else
                {
                    logger.LogWarning("ExLink balance check skipped: Currency {Currency} not found in balance response", record.Currency);
                }
            }
            else
            {
                logger.LogWarning("ExLink balance check skipped: Failed to query account balance");
            }
            
            var client = new ExLinkCashier.WithdrawalRequestClient
            {
                PaymentNumber = record.HashId,
                Amount = record.Amount,
                Currency = record.Currency,
                PaymentType = options.PaymentType,
                BankName = record.BankName,
                BankCode = record.BankCode,
                BankBranchName = string.IsNullOrEmpty(record.BranchName) ? record.BankName : record.BranchName, // Fallback to BankName if BranchName is empty
                AccountName = record.AccountName,
                BankNumber = record.BankNumber,
                Memo = string.Empty, // Optional memo field
                Options = options,
                Client = clientFactory.CreateClient(),
                Logger = logger
            };
            
            response = await client.RequestAsync();
        }
        else if (method.Platform == (int)PaymentPlatformTypes.ExLink)
        {
        }

        if (response == null) return null;

        var info = record.GetInfoModel();
        info.RequestHistory.Add(Utils.JsonDeserializeDynamic(response.ResponseJson));
        record.Info = JsonConvert.SerializeObject(info);
        record.OperatorPartyId = operatorPartyId;
        return response;
    }
}