using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Report.Models;
using Bacera.Gateway.Vendor.PayPal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    public async Task<Tuple<bool, Medium?>> ProcessRequestAsync(ReportRequest request)
    {
        var (result, medium) = (ReportRequestTypes)request.Type switch
        {
            ReportRequestTypes.Rebate => await ProcessRebateRequestAsync(request),
            ReportRequestTypes.DepositForTenant => await ProcessDepositForTenantRequestAsync(request),
            ReportRequestTypes.WithdrawForTenant => await ProcessWithdrawalForTenantRequestAsync(request),
            ReportRequestTypes.TransactionForTenant => await ProcessTransactionForTenantRequestAsync(request),
            ReportRequestTypes.SalesRebateForTenant => await ProcessSalesRebateForTenantRequestAsync(request),
            ReportRequestTypes.SalesRebateSumByAccountForTenant =>
                await ProcessSalesRebateSumByAccountForTenantRequestAsync(request),
            ReportRequestTypes.WithdrawPendingForTenant => await ProcessWithdrawalPendingTenantRequestAsync(request),
            ReportRequestTypes.WalletOverviewForTenant => await ProcessWalletOverviewForTenantRequestAsync(request),
            ReportRequestTypes.WalletDailySnapshot => await ProcessWalletDailySnapshotForTenantRequestAsync(request),
            ReportRequestTypes.WalletTransactionForTenant => await ProcessWalletTransactionForTenantRequestAsync(
                request),
            ReportRequestTypes.AccountSearchForTenant => await ProcessAccountSearchForTenantRequestAsync(request),
            ReportRequestTypes.DemoAccount => await ProcessWeeklyDemoAccountRequestAsync(request),
            ReportRequestTypes.DailyEquity => await ProcessDailyEquityRequestAsync(request),
            ReportRequestTypes.DailyEquityMonthly => await ProcessMonthlyDailyEquityRequestAsync(request),
            _ => new Tuple<ReportRequest, Medium?>(request, null)
        };

        if (!request.IsGenerated) return new Tuple<bool, Medium?>(false, null);

        tenantDbContext.ReportRequests.Update(result);
        await tenantDbContext.SaveChangesAsync();
        
        // 如果是 Rebate 或 WalletTransactionForTenant 类型，查找配对报告并更新 Query 字段（在 ProcessRequestAsync 层面也调用一次，确保两个报告都能更新）
        if (result.Type == (int)ReportRequestTypes.Rebate || result.Type == (int)ReportRequestTypes.WalletTransactionForTenant)
        {
            await UpdatePairedReportQueryAsync(result);
        }
        
        return new Tuple<bool, Medium?>(true, medium);
    }


    public static bool ValidateQueryString(ReportRequest request)
        => (ReportRequestTypes)request.Type switch
        {
            ReportRequestTypes.Rebate => ValidateQuery<Rebate.Criteria>(request.Query),
            ReportRequestTypes.DepositForTenant => ValidateQuery<Deposit.Criteria>(request.Query),
            ReportRequestTypes.WithdrawForTenant => ValidateQuery<Withdrawal.Criteria>(request.Query),
            ReportRequestTypes.TransactionForTenant => ValidateQuery<Transaction.Criteria>(request.Query),
            ReportRequestTypes.WithdrawPendingForTenant => ValidateQuery<Withdrawal.Criteria>(request.Query),
            ReportRequestTypes.WalletDailySnapshot => ValidateQuery<WalletDailySnapshot.Criteria>(request.Query),
            ReportRequestTypes.WalletOverviewForTenant => ValidateQuery<Wallet.Criteria>(request.Query),
            ReportRequestTypes.WalletTransactionForTenant => ValidateQuery<WalletTransaction.Criteria>(request.Query),
            ReportRequestTypes.SalesRebateForTenant => ValidateQuery<SalesRebate.Criteria>(request.Query),
            ReportRequestTypes.SalesRebateSumByAccountForTenant => ValidateQuery<SalesRebate.Criteria>(request.Query),
            ReportRequestTypes.AccountSearchForTenant => ValidateQuery<Account.Criteria>(request.Query),
            ReportRequestTypes.DailyEquity => ValidateQuery<DailyEquity.Criteria>(request.Query),
            ReportRequestTypes.DailyEquityMonthly => ValidateQuery<DailyEquity.Criteria>(request.Query),
            _ => false
        };

    private async Task<Tuple<ReportRequest, Medium?>> ProcessDailyEquityRequestAsync(ReportRequest request)
    {
        var criteria = JsonConvert.DeserializeObject<DailyEquity.Criteria>(request.Query, Utils.AppJsonSerializerSettings);
        if (criteria == null) return new Tuple<ReportRequest, Medium?>(request, null);

        var hoursGap = await configSvc.GetHoursGapForMT5Async(criteria.From);
        
        // 时区转换: 用户输入的时间是GMT+x，需要转换为GMT+0 (PostgreSQL时区)
        // GMT+x 时间 - hoursGap小时 = GMT+0 时间
        var fromDTMinus2H = criteria.From?.AddHours(-hoursGap);
        var toDTMinus2H = criteria.To?.AddHours(-hoursGap);

        // 判断是否使用 ClosedTime 逻辑
        // Job入口 (IsFromApi == 0): 始终使用 ClosedTime
        // API入口 (IsFromApi == 1): 根据 criteria.UseClosingTime 决定
        var useClosingTime = request.IsFromApi == 0 
            ? true  // Job入口始终使用ClosedTime
            : (criteria.UseClosingTime == true);  // API入口根据开关决定

        // Create adjusted criteria with GMT+0 time for PostgreSQL queries
        // AND original time for TradeRebate.ClosedOn (MT5 time)
        var adjustedCriteria = new DailyEquity.Criteria
        {
            From = fromDTMinus2H,
            To = toDTMinus2H,
            UseClosingTime = useClosingTime,
            ClosedOnFrom = criteria.From,  // Original time for TradeRebate.ClosedOn
            ClosedOnTo = criteria.To,      // Original time for TradeRebate.ClosedOn
            Page = 1,
            Size = 500000
        };

        var records = await DailyEquityReportQueryAsync(adjustedCriteria, hoursGap);

        // Persist snapshot for later comparison with monthly report.
        // Only save for single-day reports (<=25h span) to maintain daily granularity.
        // Multi-day reports like "Sat-Mon" must NOT overwrite individual daily snapshots,
        // otherwise the monthly aggregation would double-count weekend data.
        var reportSpan = (criteria.To ?? DateTime.UtcNow) - (criteria.From ?? DateTime.UtcNow);
        if (reportSpan.TotalHours <= 25)
        {
            try
            {
                var snapshotDate = ((criteria.To ?? DateTime.UtcNow).AddSeconds(-1)).Date;
                var snapshotVersion = useClosingTime
                    ? EquityReportVersion.ClosingTimeBased
                    : EquityReportVersion.ReleasedTimeBased;
                await SaveDailyEquitySnapshotAsync(records, snapshotDate, snapshotVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[DailyEquitySnapshot] Failed to save snapshot — report generation continues");
            }
        }
        else
        {
            logger.LogInformation(
                "[DailyEquitySnapshot] Skipping snapshot save for multi-day report ({Hours:F1}h span, {From} to {To})",
                reportSpan.TotalHours, criteria.From, criteria.To);
        }

        var aggregateByOffice = criteria.AggregateByOffice == true;

        // Generate CSV file with Excel-like formatting
        var fileName = !string.IsNullOrEmpty(request.Name) 
            ? $"{request.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
            : $"daily_equity_report_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        var tempDirectory = Path.GetTempPath();
        var tempFilePath = Path.Combine(tempDirectory, Path.GetRandomFileName() + ".csv");
        await using var writer = new StreamWriter(tempFilePath, false, new UTF8Encoding(true));
        
        // Write title row - use the 'To' date as report date (not From)
        // The report is FOR the period, so use the end date
        var reportDate = criteria.To?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (aggregateByOffice)
        {
            var perOfficeRecords = AggregateByOffice(records);
            await writer.WriteLineAsync($"Daily Equity Per Office {reportDate},,,,,,,,,,,,,");
            await writer.WriteLineAsync(""); // Blank line

            // Write CSV header
            await writer.WriteLineAsync(DailyEquityPerOfficeRecord.Header());

            // Write CSV rows with enhanced formatting
            foreach (var record in perOfficeRecords)
            {
                await writer.WriteLineAsync(record.ToCsv());
            }
        }
        else
        {
            var mapping = await configSvc.GetDailyEquityOfficeMergeMappingAsync();
            var perSalesRecords = mapping != null
                ? MergeRecordsByOfficeMapping(records, mapping)
                : records;

            await writer.WriteLineAsync($"Daily Equity Report {reportDate},,,,,,,,,,,,,,");
            await writer.WriteLineAsync(""); // Blank line
        
            // Write CSV header
            await writer.WriteLineAsync(DailyEquityRecord.Header());
        
            // Write CSV rows with enhanced formatting
            string? lastCurrency = null;
            var currencyPrefix = mapping?.CurrencyPrefix;
            foreach (var record in perSalesRecords)
            {
                await writer.WriteLineAsync(record.ToCsvGrouped(ref lastCurrency, currencyPrefix));
            }
        }

        writer.Close();

        // Upload file
        var now = DateTime.UtcNow;
        await using var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var medium = await storageService.UploadFileAndSaveMediaAsync(
            fileStream, fileName, ".csv", "report-request", request.Id,
            "text/csv", 0, request.PartyId
        );
        
        request.GeneratedOn = now;
        request.FileName = medium.Guid;
        tenantDbContext.ReportRequests.Update(request);
        await tenantDbContext.SaveChangesAsync();
        
        return new Tuple<ReportRequest, Medium?>(request, medium);
    }

    private async Task<Tuple<ReportRequest, Medium?>> ProcessMonthlyDailyEquityRequestAsync(ReportRequest request)
    {
        var criteria = JsonConvert.DeserializeObject<DailyEquity.Criteria>(request.Query, Utils.AppJsonSerializerSettings);
        if (criteria == null) return new Tuple<ReportRequest, Medium?>(request, null);

        var useClosingTime = request.IsFromApi == 0
            ? true
            : (criteria.UseClosingTime == true);

        var reportVersion = useClosingTime
            ? EquityReportVersion.ClosingTimeBased
            : EquityReportVersion.ReleasedTimeBased;

        // Use stored daily snapshots instead of re-running the CTE query.
        // This guarantees monthly totals = sum of daily snapshots exactly.
        var records = await AggregateSnapshotsForMonthlyAsync(
            criteria.From!.Value, criteria.To!.Value, reportVersion);
        var reportDateFrom = criteria.From?.ToString("yyyy-MM-dd") ?? "";
        var reportDateTo = criteria.To?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
        var reportDateRange = $"{reportDateFrom} - {reportDateTo}";
        var now = DateTime.UtcNow;

        // --- Report 1: Per-Sales ---
        var mapping = await configSvc.GetDailyEquityOfficeMergeMappingAsync();
        var perSalesRecords = mapping != null
            ? MergeRecordsByOfficeMapping(records, mapping)
            : records;

        var perSalesFileName = !string.IsNullOrEmpty(request.Name)
            ? $"{request.Name.Replace(" ", "_")}_{now:yyyyMMdd-HHmmss}.csv"
            : $"daily_equity_monthly_report_{now:yyyyMMdd-HHmmss}.csv";

        var perSalesTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");
        await using (var writer = new StreamWriter(perSalesTempPath, false, new UTF8Encoding(true)))
        {
            await writer.WriteLineAsync($"Monthly Equity Report {reportDateRange},,,,,,,,,,,,,,");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync(DailyEquityRecord.Header());
            string? lastCurrency = null;
            var currencyPrefix = mapping?.CurrencyPrefix;
            foreach (var record in perSalesRecords)
            {
                await writer.WriteLineAsync(record.ToCsvGrouped(ref lastCurrency, currencyPrefix));
            }
        }

        await using (var fileStream = new FileStream(perSalesTempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var medium = await storageService.UploadFileAndSaveMediaAsync(
                fileStream, perSalesFileName, ".csv", "report-request", request.Id,
                "text/csv", 0, request.PartyId);
            request.GeneratedOn = now;
            request.FileName = medium.Guid;
            tenantDbContext.ReportRequests.Update(request);
            await tenantDbContext.SaveChangesAsync();
        }

        // --- Report 2: Per-Office ---
        var perOfficeRecords = AggregateByOffice(records);

        var perOfficeName = request.Name?.Contains("Per Office") == true
            ? request.Name
            : request.Name + " (Per Office)";

        var perOfficeQueryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.Query)
                                 ?? new Dictionary<string, object>();
        perOfficeQueryDict["aggregateByOffice"] = true;
        var perOfficeQuery = JsonConvert.SerializeObject(perOfficeQueryDict, Utils.AppJsonSerializerSettings);

        var companionRequest = new ReportRequest
        {
            PartyId = request.PartyId,
            Type = request.Type,
            Name = perOfficeName,
            Query = perOfficeQuery,
            IsFromApi = request.IsFromApi
        };
        tenantDbContext.ReportRequests.Add(companionRequest);
        await tenantDbContext.SaveChangesAsync();

        var perOfficeFileName = $"{perOfficeName.Replace(" ", "_")}_{now:yyyyMMdd-HHmmss}.csv";
        var perOfficeTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");
        await using (var writer = new StreamWriter(perOfficeTempPath, false, new UTF8Encoding(true)))
        {
            await writer.WriteLineAsync($"Monthly Equity Per Office {reportDateRange},,,,,,,,,,,,,");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync(DailyEquityPerOfficeRecord.Header());
            foreach (var record in perOfficeRecords)
            {
                await writer.WriteLineAsync(record.ToCsv());
            }
        }

        await using (var fileStream = new FileStream(perOfficeTempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var medium = await storageService.UploadFileAndSaveMediaAsync(
                fileStream, perOfficeFileName, ".csv", "report-request", companionRequest.Id,
                "text/csv", 0, companionRequest.PartyId);
            companionRequest.GeneratedOn = now;
            companionRequest.FileName = medium.Guid;
            tenantDbContext.ReportRequests.Update(companionRequest);
            await tenantDbContext.SaveChangesAsync();
        }

        return new Tuple<ReportRequest, Medium?>(request, null);
    }

    private Task<Tuple<ReportRequest, Medium?>> ProcessAccountSearchForTenantRequestAsync(ReportRequest request)
        => ProcessRequestAsync<Account, Account.Criteria, AccountSearchRecord>(
            request,
            $"account_search_report_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv",
            async criteria =>
            {
                var records = await tenantDbContext.Accounts
                    .PagedFilterBy(criteria)
                    .ToRecords()
                    .ToListAsync();

                var wallets = await tenantDbContext.Wallets
                    .Where(x => records.Select(y => y.PartyId).Contains(x.PartyId))
                    .Select(x => new { x.Id, x.PartyId, x.CurrencyId, x.FundType })
                    .ToListAsync();

                records.ForEach(x =>
                {
                    x.Server = pool.GetServiceNameByServiceId(x.ServiceId);
                    x.WalletId = wallets
                        .Where(y => y.PartyId == x.PartyId)
                        .FirstOrDefault(y => y.CurrencyId == x.CurrencyId && y.FundType == x.FundType)?.Id ?? 0;
                });
                // await FulfillClientNameAsync(records);
                return records;
            }
        );

    private Task<Tuple<ReportRequest, Medium?>> ProcessWeeklyDemoAccountRequestAsync(ReportRequest request)
        => ProcessRequestAsync<DemoAccountRecord, DemoAccountRecord.QueryCriteria, DemoAccountRecord>(
            request,
            $"demo_account_report_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv",
            async criteria =>
            {
                var sql = GetDemoAccountQueryString(criteria);
                var records = await tenantCon.ToListAsync<DemoAccountRecord>(sql);
                return records;
            }
        );


    private async Task<Tuple<ReportRequest, Medium?>> ProcessRebateRequestAsync(ReportRequest request)
    {
        var parsedCriteria = JsonConvert.DeserializeObject<Rebate.Criteria>(request.Query, Utils.AppJsonSerializerSettings);
        var hoursGapForHeader = await configSvc.GetHoursGapForMT5Async(parsedCriteria?.From);
        var fileName = !string.IsNullOrEmpty(request.Name)
            ? $"{request.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
            : $"rebate_report_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return await ProcessRequestAsync<Rebate, Rebate.Criteria, RebateRecord>(
            request,
            fileName,
            async criteria =>
            {
                var page = criteria.Page;
                var size = criteria.Size;
                var fromDT = criteria.From;
                var toDT = criteria.To;
                var hoursGap = hoursGapForHeader;
                var fromDTMinus2H = fromDT?.AddHours(-hoursGap);
                var toDTMinus2H = toDT?.AddHours(-hoursGap);
                var stateId = criteria.StateId;
                // TradeRebate.ClosedOn should be between (from + hoursGap) and (to + hoursGap), currently MT5 server fixed GMT+2 for ReportJob
                var hoursToAdd = hoursGap; // Configurable via HoursGapForMT5
                var fromPlus2H = fromDT?.AddHours(hoursToAdd);
                var toPlus2H = toDT?.AddHours(hoursToAdd);

                // 判断是否使用MT5 ClosingTime逻辑
                // Job入口 (IsFromApi == 0): 始终使用MT5 ClosingTime
                // API入口 (IsFromApi == 1): 根据 criteria.UseClosingTime 决定
                //   - UseClosingTime == true: 使用MT5 ClosingTime逻辑
                //   - UseClosingTime == false 或 null: 不使用MT5 ClosingTime逻辑（仅使用StatedOn）
                var useClosingTime = request.IsFromApi == 0 
                    ? true  // Job入口始终使用ClosingTime
                    : (criteria.UseClosingTime == true);  // API入口根据开关决定

                // Extended 'to' with 5 minutes for Matter.StatedOn validation (m.StatedOn < to + 5min) for ReportJob
                // 如果使用ClosingTime逻辑，且to接近半夜12点，需要额外加5分钟
                var minutesToAddForMatter = 5; // 默认5分钟

                if (useClosingTime && toDT.HasValue)
                {
                    // 检查to是否接近半夜12点（23:55:00到00:00:00之间）
                    var timeOfDay = toDT.Value.TimeOfDay;
                    var isNearMidnight = (timeOfDay >= TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(55)) && timeOfDay <= TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59)));
                    
                    if (isNearMidnight)
                    {
                        minutesToAddForMatter += 5; // 额外加5分钟，总共10分钟
                    }
                }
                
                var toDTMinus2HPlusMinutes = toDTMinus2H?.AddMinutes(minutesToAddForMatter);

                IQueryable<RebateRecord> rebateQuery;
                
                if (useClosingTime)
                {
                    // 使用MT5 ClosingTime逻辑：根据 tr.ClosedOn + Matter.StatedOn 双重过滤
                    // Use explicit joins to ensure TradeRebate.Account.CurrencyId is accessible for SourceCurrencyId
                    // EF Core ignores .Include() when using .Select() projections, so we need explicit joins
                    rebateQuery = from r in tenantDbContext.Rebates
                                  join tr in tenantDbContext.TradeRebates on r.TradeRebateId equals tr.Id
                                  join a in tenantDbContext.Accounts on r.AccountId equals a.Id
                                  join tra in tenantDbContext.Accounts on tr.AccountId equals tra.Id into traj
                                  from tra in traj.DefaultIfEmpty()
                                  // Completed 或者 SkippedWithOpenCloseTimeLessThanOneMinute
                                  where (tr.Status == (int)TradeRebateStatusTypes.Completed || tr.Status == (int)TradeRebateStatusTypes.SkippedWithOpenCloseTimeLessThanOneMinute)
                                      && (fromDT == null || tr.ClosedOn >= fromDT)
                                      && (toDT == null || tr.ClosedOn <= toDT)
                                      && (fromDTMinus2H == null || r.IdNavigation.StatedOn >= fromDTMinus2H)
                                      && (toDTMinus2HPlusMinutes == null || r.IdNavigation.StatedOn < toDTMinus2HPlusMinutes)
                                      && (stateId == null || r.IdNavigation.StateId == (int)stateId)
                                  orderby r.Id descending
                                  select new RebateRecord
                                  {
                                      Id = r.Id,
                                      PartyId = r.PartyId,
                                      Ticket = tr.Ticket,
                                      Symbol = tr.Symbol,
                                      AccountNumber = tr.AccountNumber,
                                      AccountUid = a.Uid,
                                      ClientName = a.Name,
                                      ClientCode = a.Code,
                                      CurrencyId = r.CurrencyId,
                                      SourceCurrencyId = tra != null ? tra.CurrencyId : r.CurrencyId,
                                      Volume = tr.Volume,
                                      RebateValue = r.Amount,
                                      ClosedOn = tr.ClosedOn,
                                      Information = r.Information,
                                      //和ClosedOn统一时区, 即GMT+2 
                                      ReleasedOn = r.IdNavigation.StatedOn.AddHours(hoursToAdd),
                                  };
                }
                else
                {
                    // 不使用MT5 ClosingTime逻辑：仅根据 Matter.StatedOn 过滤（API入口，UseClosingTime=false）
                    // Use explicit joins to ensure TradeRebate.Account.CurrencyId is accessible for SourceCurrencyId
                    // EF Core ignores .Include() when using .Select() projections, so we need explicit joins
                    rebateQuery = from r in tenantDbContext.Rebates
                                  join tr in tenantDbContext.TradeRebates on r.TradeRebateId equals tr.Id
                                  join a in tenantDbContext.Accounts on r.AccountId equals a.Id
                                  join tra in tenantDbContext.Accounts on tr.AccountId equals tra.Id into traj
                                  from tra in traj.DefaultIfEmpty()
                                  // Completed 或者 SkippedWithOpenCloseTimeLessThanOneMinute
                                  where (tr.Status == (int)TradeRebateStatusTypes.Completed || tr.Status == (int)TradeRebateStatusTypes.SkippedWithOpenCloseTimeLessThanOneMinute)
                                      && (fromDTMinus2H == null || r.IdNavigation.StatedOn >= fromDTMinus2H)
                                      && (toDTMinus2H == null || r.IdNavigation.StatedOn <= toDTMinus2H)
                                      && (stateId == null || r.IdNavigation.StateId == (int)stateId)
                                  orderby r.Id descending
                                  select new RebateRecord
                                  {
                                      Id = r.Id,
                                      PartyId = r.PartyId,
                                      Ticket = tr.Ticket,
                                      Symbol = tr.Symbol,
                                      AccountNumber = tr.AccountNumber,
                                      AccountUid = a.Uid,
                                      ClientName = a.Name,
                                      ClientCode = a.Code,
                                      CurrencyId = r.CurrencyId,
                                      SourceCurrencyId = tra != null ? tra.CurrencyId : r.CurrencyId,
                                      Volume = tr.Volume,
                                      RebateValue = r.Amount,
                                      ClosedOn = tr.ClosedOn,
                                      Information = r.Information,
                                      //和ClosedOn统一时区, 即GMT+2 
                                      ReleasedOn = r.IdNavigation.StatedOn.AddHours(hoursToAdd),
                                  };
                }


                var rebateRecords = await rebateQuery
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                var rebateIds = rebateRecords.Select(x => x.Id).ToList();
                var idToCreateOnDict = await tenantDbContext.WalletTransactions
                    .Where(x => rebateIds.Contains(x.MatterId))
                    .GroupBy(x => x.MatterId)
                    .Select(x => new { MatterId = x.Key, CreatedOn = x.Max(y => y.CreatedOn) })
                    .ToDictionaryAsync(x => x.MatterId, x => x.CreatedOn);

                foreach (var rebateRecord in rebateRecords)
                {
                    //和ClosedOn统一时区, 即GMT+2 
                    if (idToCreateOnDict.TryGetValue(rebateRecord.Id, out var createdOn))
                        rebateRecord.CreatedOn = createdOn.AddHours(hoursToAdd);
                    rebateRecord.MtGmtOffsetHoursForCsv = hoursGapForHeader;
                }

                return rebateRecords;
            },
            RebateRecord.Header(hoursGapForHeader));
    }

    private async Task<Tuple<ReportRequest, Medium?>> ProcessWalletOverviewForTenantRequestAsync(ReportRequest request)
        => await ProcessRequestAsync<Wallet, Wallet.Criteria, WalletOverviewRecord>(
            request,
            $"client_wallet_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv",
            async criteria =>
            {
                criteria.HasBalance = true;
                var items = await tenantDbContext.Wallets
                    // .Where(x => x.Party.Status == (short)PartyStatusTypes.Active) // dealing said export all reports regardless of the party status, 05/19/2025
                    .PagedFilterBy(criteria)
                    .ToRecords()
                    .ToListAsync();

                await FulfillClientNameEmailAsync(items);
                return items;
            }
        );

    private async Task<Dictionary<long, long>> GetWalletBalancesAtTimeAsync(DateTime cutoffTime)
    {
        // Query the latest transaction for each wallet before cutoffTime
        var walletBalances = await (
            from wt in tenantDbContext.WalletTransactions
            where wt.CreatedOn < cutoffTime
            group wt by wt.WalletId into g
            select new
            {
                WalletId = g.Key,
                LatestTransaction = g.OrderByDescending(x => x.CreatedOn)
                                     .ThenByDescending(x => x.Id)
                                     .Select(x => new { x.PrevBalance, x.Amount })
                                     .FirstOrDefault()
            }
        ).ToListAsync();

        return walletBalances
            .Where(x => x.LatestTransaction != null)
            .ToDictionary(
                x => x.WalletId,
                x => x.LatestTransaction!.PrevBalance + x.LatestTransaction.Amount
            );
    }

    private async Task<Tuple<ReportRequest, Medium?>> ProcessWalletDailySnapshotForTenantRequestAsync(
        ReportRequest request)
        => await ProcessRequestAsync<WalletDailySnapshot, WalletDailySnapshot.Criteria, WalletOverviewRecord>(
            request,
            $"client_wallet_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv",
            async criteria =>
            {
                criteria.HasBalance = true;

                // Check if useMT5Time flag is set
                var useMT5Time = criteria.UseMT5Time ?? false;

                logger.LogInformation("ProcessWalletDailySnapshotForTenantRequestAsync - UseMT5Time: {UseMT5Time}, SnapshotDate: {SnapshotDate}", 
                    useMT5Time, criteria.SnapshotDate);

                if (useMT5Time)
                {
                    // NEW VERSION: Calculate real-time balance at 23:59:59 from WalletTransactions
                    // Calculate cutoff time: 23:59:59 of the snapshot date
                    var cutoffTime = criteria.SnapshotDate?.Date.AddDays(1).AddSeconds(-1) ?? DateTime.UtcNow;

                    logger.LogInformation("WalletDailySnapshot RealTime - CutoffTime: {CutoffTime}, Page: {Page}, Size: {Size}", cutoffTime, criteria.Page, criteria.Size);

                    // Get real-time wallet balances from WalletTransactions (for ALL wallets, not paginated yet)
                    var walletBalances = await GetWalletBalancesAtTimeAsync(cutoffTime);

                    logger.LogInformation("WalletDailySnapshot RealTime - Found {Count} wallets with balances", walletBalances.Count);

                    // Query wallets with filters
                    var walletsQuery = tenantDbContext.Wallets
                        .Where(w => criteria.PartyId == null || w.PartyId == criteria.PartyId)
                        .Where(w => criteria.FundType == null || w.FundType == (int)criteria.FundType)
                        .Where(w => criteria.CurrencyId == null || w.CurrencyId == (int)criteria.CurrencyId)
                        .Where(w => criteria.Email == null || w.Party.Email == criteria.Email)
                        // Only include wallets that have transactions
                        .Where(w => walletBalances.Keys.Contains(w.Id));

                    // Apply HasBalance filter on calculated balance
                    if (criteria.HasBalance != null)
                    {
                        var filteredWalletIds = walletBalances
                            .Where(kvp => (bool)criteria.HasBalance ? kvp.Value != 0 : kvp.Value == 0)
                            .Select(kvp => kvp.Key)
                            .ToHashSet();
                        
                        walletsQuery = walletsQuery.Where(w => filteredWalletIds.Contains(w.Id));
                    }

                    // Apply pagination
                    var page = criteria.Page < 1 ? 1 : criteria.Page;
                    var size = criteria.Size < 1 ? 20 : criteria.Size;
                    
                    var records = await walletsQuery
                        .Skip((page - 1) * size)
                        .Take(size)
                        .ToRecords()
                        .ToListAsync();

                    logger.LogInformation("WalletDailySnapshot RealTime - Retrieved {Count} wallet records (Page {Page})", records.Count, page);

                    // Update balance from calculated real-time balances
                    foreach (var record in records)
                    {
                        if (walletBalances.TryGetValue(record.Id, out var balance))
                        {
                            record.Amount = balance;
                        }
                    }

                    logger.LogInformation("WalletDailySnapshot RealTime - Final item count: {Count}", records.Count);

                    await FulfillClientNameEmailAsync(records);
                    return records;
                }
                else
                {
                    // ORIGINAL VERSION: Use pre-generated snapshots at 22:00 from WalletDailySnapshots table
                    var hoursGap = await configSvc.GetHoursGapForMT5Async(criteria.SnapshotDate);
                    criteria.SnapshotDate = DateHelper.MinusMT5GMTHours(criteria.SnapshotDate, hoursGap);

                    logger.LogInformation("WalletDailySnapshot SnapshotTable - Adjusted SnapshotDate: {SnapshotDate}, HoursGap: {HoursGap}", 
                        criteria.SnapshotDate, hoursGap);

                    var items = await tenantDbContext.WalletDailySnapshots
                        // .Where(x => x.Wallet.Party.Status == (short)PartyStatusTypes.Active) // dealing said export all reports regardless of the party status, 05/19/2025
                        .PagedFilterBy(criteria)
                        .ToRecords()
                        .ToListAsync();

                    logger.LogInformation("WalletDailySnapshot SnapshotTable - Retrieved {Count} items", items.Count);

                    await FulfillClientNameEmailAsync(items);
                    return items;
                }
            }
        );

    private async Task<Tuple<ReportRequest, Medium?>> ProcessDepositForTenantRequestAsync(ReportRequest request)
        => await ProcessRequestAsync<Deposit, Deposit.Criteria, DepositRecord>(
            request,
            $"deposit_report_{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv",
            async criteria =>
            {
                var query = tenantDbContext.Deposits
                    // .Where(x => x.Party.Status == (short)PartyStatusTypes.Active) // dealing said export all reports regardless of the party status, 05/19/2025
                    .FilterBy(criteria)
                    .ToRecords()
                    .OrderByDescending(x => x.CreatedOn);
                var items = await query
                    .Skip((criteria.Page - 1) * criteria.Size)
                    .Take(criteria.Size)
                    .ToListAsync();
                criteria.Total = await query.CountAsync();

                await FulfillClientNameAsync(items);
                return items;
            }
        );

    private Task<Tuple<ReportRequest, Medium?>> ProcessWithdrawalForTenantRequestAsync(ReportRequest request)
        => ProcessRequestAsync<Withdrawal, Withdrawal.Criteria, WithdrawalRecord>(
            request,
            $"withdrawal_report_{DateTime.UtcNow:yyyyMMdd-HHmms}.csv",
            async criteria =>
            {
                var query = tenantDbContext.Withdrawals
                    .AsQueryable();
                // .Where(x => x.Party.Status == (short)PartyStatusTypes.Active); 

                if (criteria.From != null)
                    query = query.Where(x => x.ApprovedOn >= criteria.From);

                if (criteria.To != null)
                    query = query.Where(x => x.ApprovedOn <= criteria.To);

                criteria.Total = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.ApprovedOn)
                    .Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size)
                    .ToRecords()
                    .ToListAsync();

                await FulfillClientNameAsync(items);
                return items;
            }
        );

    private async Task<Tuple<ReportRequest, Medium?>> ProcessWithdrawalPendingTenantRequestAsync(ReportRequest request)
        => await ProcessRequestAsync<Withdrawal, Withdrawal.Criteria, WithdrawalRecord>(
            request,
            $"withdrawal_pending_report_{DateTime.UtcNow:yyyyMMdd-HHmms}.csv",
            async criteria =>
            {
                var items = await tenantDbContext.Withdrawals
                    // .Where(x => x.Party.Status == (short)PartyStatusTypes.Active)
                    .Where(x => x.IdNavigation.StateId == (int)StateTypes.WithdrawalCreated)
                    .Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size)
                    .ToRecords()
                    .ToListAsync();
                return items;
            });

    private async Task<Tuple<ReportRequest, Medium?>> ProcessTransactionForTenantRequestAsync(ReportRequest request)
        => await ProcessRequestAsync<Transaction, Transaction.Criteria, TransactionRecord>(
            request,
            $"transaction_report_{DateTime.UtcNow:yyyyMMdd-HHmms}.csv",
            async criteria =>
            {
                var query = from transaction in tenantDbContext.Transactions
                        // .Where(x => x.Party.Status == (short)PartyStatusTypes.Active)
                        .PagedFilterBy(criteria)
                    join st in tenantDbContext.TradeAccounts
                        on transaction.SourceAccountId equals st.Id into sourceGroup
                    from sourceTradeAccount in sourceGroup.DefaultIfEmpty()
                    join tt in tenantDbContext.TradeAccounts
                        on transaction.TargetAccountId equals tt.Id into targetGroup
                    from targetTradeAccount in targetGroup.DefaultIfEmpty()
                    select new TransactionRecord
                    {
                        PartyId = transaction.PartyId,
                        Amount = transaction.Amount,
                        CreatedOn = transaction.IdNavigation.PostedOn.AddHours(2),
                        CurrencyId = (CurrencyTypes)transaction.CurrencyId,
                        State = (StateTypes)transaction.IdNavigation.StateId,
                        SourceAccountNumber = sourceTradeAccount != null ? sourceTradeAccount.AccountNumber : 0,
                        TargetAccountNumber = targetTradeAccount != null ? targetTradeAccount.AccountNumber : 0,
                    };

                var items = await query.ToListAsync();
                await FulfillClientNameAsync(items);
                return items;
            });

    private async Task<Tuple<ReportRequest, Medium?>> ProcessWalletTransactionForTenantRequestAsync(
        ReportRequest request)
    {
        var parsedCriteria = JsonConvert.DeserializeObject<WalletTransaction.Criteria>(request.Query, Utils.AppJsonSerializerSettings);
        var hoursGapForHeader = await configSvc.GetHoursGapForMT5Async(parsedCriteria?.From);
        return await ProcessRequestAsync<WalletTransaction, WalletTransaction.Criteria, WalletTransactionRecord>(
            request,
            $"wallet_transaction_report_{DateTime.UtcNow:yyyyMMdd-HHmms}.csv",
            async criteria =>
            {
                var fromDT = criteria.From;
                var toDT = criteria.To;
                var hoursGap = hoursGapForHeader;
                var fromDTMinus2H = fromDT?.AddHours(-hoursGap);
                var toDTMinus2H = toDT?.AddHours(-hoursGap);
                
                // 确定是否使用MT5关仓时间逻辑
                // Job入口 (IsFromApi == 0): 始终使用ClosingTime
                // API入口 (IsFromApi == 1): 根据 criteria.UseClosingTime 决定
                var useClosingTime = request.IsFromApi == 0
                    ? true  // Job入口始终使用ClosingTime
                    : (criteria.UseClosingTime == true);  // API入口根据开关决定
                
                // Extended 'to' with 5 minutes for Matter.StatedOn validation (m.StatedOn < to + 5min) for ReportJob
                var minutesToAddForMatter = 5; // 默认5分钟

                if (useClosingTime && toDT.HasValue)
                {
                    // 检查to是否接近半夜12点（23:55:00到00:00:00之间）
                    var timeOfDay = toDT.Value.TimeOfDay;
                    var isNearMidnight = (timeOfDay >= TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(55)) && timeOfDay <= TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59)));
                    
                    if (isNearMidnight)
                    {
                        minutesToAddForMatter += 5; // 额外加5分钟，总共10分钟
                    }
                }
                
                var toDTMinus2HPlusMinutes = toDTMinus2H?.AddMinutes(minutesToAddForMatter);

                var walletAdjust = tenantDbContext.WalletAdjusts
                    .Select(x => new WalletTransactionRecord
                    {
                        Id = x.Id,
                        SourceAmount = x.Amount,
                        Amount = x.Amount,
                        CurrencyId = (CurrencyTypes)x.Wallet.CurrencyId,
                        SourceCurrencyId = (CurrencyTypes)x.Wallet.CurrencyId,
                        FundType = FundTypes.Wire,
                        PartyId = x.Wallet.PartyId,
                        Source = x.SourceType,
                        Target = 0,
                        RebateTargetAccountUid = 0,
                        ReleasedOn = x.CreatedOn
                    });

                //入金 bank account -> account
                var deposit = from d in tenantDbContext.Deposits
                    select new WalletTransactionRecord
                    {
                        Id = d.Id,
                        SourceCurrencyId = (CurrencyTypes)d.Payment.CurrencyId,
                        CurrencyId = (CurrencyTypes)d.CurrencyId,
                        SourceAmount = d.Payment.Amount,
                        Amount = d.Amount,
                        FundType = FundTypes.Wire,
                        PartyId = d.PartyId,
                        Source = 0,
                        Target = 0,
                        RebateTargetAccountUid = 0,
                        ReleasedOn = DateTime.UtcNow
                    };

                //出金 wallet/account -> bank account
                var withdrawal = from w in tenantDbContext.Withdrawals
                    join sa in tenantDbContext.Accounts on w.SourceAccountId equals sa.Id into saj
                    from sa in saj.DefaultIfEmpty()
                    join sw in tenantDbContext.Wallets on w.SourceWalletId equals sw.Id into swj
                    from sw in swj.DefaultIfEmpty()
                    select new WalletTransactionRecord
                    {
                        Id = w.Id,
                        // SourceCurrencyId: from Account if SourceAccountId is set, otherwise from Wallet if SourceWalletId is set, else fallback to withdrawal's CurrencyId
                        SourceCurrencyId = w.SourceAccountId != null
                            ? (CurrencyTypes)(sa != null ? sa.CurrencyId : w.CurrencyId)
                            : (w.SourceWalletId != null
                                ? (sw != null ? (CurrencyTypes)sw.CurrencyId : (CurrencyTypes)w.CurrencyId)
                                : (CurrencyTypes)w.CurrencyId),
                        CurrencyId = (CurrencyTypes)w.Payment.CurrencyId,
                        SourceAmount = w.Amount,
                        Amount = w.Payment.Amount,
                        FundType = FundTypes.Wire,
                        PartyId = w.PartyId,
                        Source = 0,
                        Target = 0,
                        RebateTargetAccountUid = 0,
                        ReleasedOn = w.ApprovedOn,
                    };

                var rebate = from r in tenantDbContext.Rebates
                    join tr in tenantDbContext.TradeRebates on r.TradeRebateId equals tr.Id
                    join a in tenantDbContext.Accounts on r.AccountId equals a.Id
                    join tra in tenantDbContext.Accounts on tr.AccountId equals tra.Id into traj
                    from tra in traj.DefaultIfEmpty()
                    select new WalletTransactionRecord
                    {
                        Id = r.Id,
                        SourceCurrencyId = tra != null ? (CurrencyTypes)tra.CurrencyId : (CurrencyTypes)r.CurrencyId,
                        CurrencyId = (CurrencyTypes)r.CurrencyId,
                        SourceAmount = r.Amount, // Will be updated after parsing JSON from Rebates table
                        Amount = r.Amount,
                        FundType = FundTypes.Wire,
                        PartyId = r.PartyId,
                        Source = (int)tr.Ticket,
                        Target = 0,
                        RebateTargetAccountUid = a.Uid,
                        ReleasedOn = DateTime.UtcNow
                    };

                // 规则: wallet-wallet （usd-usd），wallet-accout（usd-account currencyId），account-account（account currencyId-account currencyId
                var transaction = from t in tenantDbContext.Transactions
                    join sa in tenantDbContext.Accounts on t.SourceAccountId equals sa.Id into saj
                    from sa in saj.DefaultIfEmpty()
                    join ta in tenantDbContext.Accounts on t.TargetAccountId equals ta.Id into taj
                    from ta in taj.DefaultIfEmpty()
                    join sw in tenantDbContext.Wallets on t.SourceAccountId equals sw.Id into swj
                    from sw in swj.DefaultIfEmpty()
                    join tw in tenantDbContext.Wallets on t.TargetAccountId equals tw.Id into twj
                    from tw in twj.DefaultIfEmpty()
                    where (t.SourceAccountType == (int)TransactionAccountTypes.Wallet && t.TargetAccountType == (int)TransactionAccountTypes.Wallet)
                       || (t.SourceAccountType == (int)TransactionAccountTypes.Wallet && t.TargetAccountType == (int)TransactionAccountTypes.Account)
                       || (t.SourceAccountType == (int)TransactionAccountTypes.Account && t.TargetAccountType == (int)TransactionAccountTypes.Wallet)
                       || (t.SourceAccountType == (int)TransactionAccountTypes.Account && t.TargetAccountType == (int)TransactionAccountTypes.Account)
                    select new WalletTransactionRecord
                    {
                        Id = t.Id,
                        // CurrencyId: Target currency
                        // wallet-account: target account currencyId
                        // account-account: target account currencyId
                        // wallet-wallet: target wallet currency (USD)
                        // account-wallet: target wallet currency (USD)
                        CurrencyId = t.TargetAccountType == (int)TransactionAccountTypes.Account
                            ? (ta != null ? (CurrencyTypes)ta.CurrencyId : CurrencyTypes.USD)
                            : CurrencyTypes.USD,
                        // SourceCurrencyId: Source currency
                        // account-wallet: source account currencyId
                        // account-account: source account currencyId
                        // wallet-wallet: source wallet currency (USD)
                        // wallet-account: source wallet currency (USD)
                        SourceCurrencyId = t.SourceAccountType == (int)TransactionAccountTypes.Account
                            ? (sa != null ? (CurrencyTypes)sa.CurrencyId : CurrencyTypes.USD)
                            : CurrencyTypes.USD,
                        // For wallet transactions, SourceAmount and Amount will be updated to WalletTransaction.Amount in final query
                        SourceAmount = t.Amount,
                        Amount = t.Amount, 
                        FundType = FundTypes.Wire,
                        PartyId = t.PartyId,
                        Source = (int)(t.SourceAccountType == (int)TransactionAccountTypes.Account
                            ? (sa != null ? sa.AccountNumber : 0)
                            : 0),
                        Target = (int)(t.TargetAccountType == (int)TransactionAccountTypes.Account
                            ? (ta != null ? ta.AccountNumber : 0)
                            : 0),
                        RebateTargetAccountUid = 0,
                        ReleasedOn = DateTime.UtcNow
                    };

                var refunds = from refund in tenantDbContext.Refunds
                    select new WalletTransactionRecord
                    {
                        Id = refund.Id,
                        SourceAmount = refund.Amount,
                        Amount = refund.Amount,
                        CurrencyId = (CurrencyTypes)refund.CurrencyId,
                        SourceCurrencyId = (CurrencyTypes)refund.CurrencyId,
                        FundType = FundTypes.Wire,
                        PartyId = refund.PartyId,
                        Source = 0,
                        Target = 0,
                        RebateTargetAccountUid = 0,
                        ReleasedOn = DateTime.UtcNow
                    };

                var unionQuery = deposit.Concat(withdrawal).Concat(transaction).Concat(rebate).Concat(refunds)
                    .Concat(walletAdjust);

                IQueryable<WalletTransaction> walletTransactionBaseQuery = tenantDbContext.WalletTransactions
                    // .Where(x => x.Wallet.Party.Status == (short)PartyStatusTypes.Active) // dealing said export all reports regardless of the party status, 05/19/2025
                    // .Where(x => x.Wallet.PartyId == 328864)
                    .OrderBy(x => x.Id);

                // Api入口, **** 根据用户自定义criteria查询 *****
                if (request.IsFromApi == 1)
                {
                    walletTransactionBaseQuery = walletTransactionBaseQuery
                        .Where(x => fromDTMinus2H == null || x.UpdatedOn >= fromDTMinus2H!.Value.ToUniversalTime())
                        .Where(x => toDTMinus2H == null || x.UpdatedOn <= toDTMinus2H!.Value.ToUniversalTime());
                }
                // else: ReportJob入口 - 根据MT5关仓时间 + Master状态时间双重过滤,no filtering here, done via Matter.StatedOn and TradeRebate.ClosedOn in subsequent join

                var query = from wt in walletTransactionBaseQuery
                        .Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size)
                    // join w in tenantDbContext.Wallets on wt.WalletId equals w.Id
                    join m in tenantDbContext.Matters on wt.MatterId equals m.Id
                    // Join TradeRebate for Rebate type filtering
                    join r in tenantDbContext.Rebates on m.Id equals r.Id into rj
                    from r in rj.DefaultIfEmpty()
                    join tr in tenantDbContext.TradeRebates on r != null ? r.TradeRebateId : (long?)null equals tr.Id into trj
                    from tr in trj.DefaultIfEmpty()
                    // Apply filtering based on useClosingTime flag
                    // Api入口 (IsFromApi == 1):
                    //   - useClosingTime == true: 根据 TradeRebate.ClosedOn + Matter.StatedOn 双重过滤（仅Rebate类型）
                    //   - useClosingTime == false: 仅根据 Matter.StatedOn 过滤
                    // Job入口 (IsFromApi == 0):
                    //   - 始终使用ClosingTime: 根据 TradeRebate.ClosedOn + Matter.StatedOn 双重过滤（仅Rebate类型）
                    where request.IsFromApi == 1 && !useClosingTime
                        ? // API入口且不使用ClosingTime: 仅根据 Matter.StatedOn 过滤
                          ((fromDTMinus2H == null || m.StatedOn >= fromDTMinus2H) &&
                           (toDTMinus2H == null || m.StatedOn < toDTMinus2H))
                        : // Job入口或API入口使用ClosingTime: 根据 TradeRebate.ClosedOn + Matter.StatedOn 双重过滤（仅Rebate类型）
                          (m.Type == (int)MatterTypes.Rebate && r != null && r.TradeRebateId != null && tr != null
                              ? ((fromDT == null || tr.ClosedOn >= fromDT) &&
                                 (toDT == null || tr.ClosedOn <= toDT) &&
                                 (fromDTMinus2H == null || m.StatedOn >= fromDTMinus2H) &&
                                 (toDTMinus2HPlusMinutes == null || m.StatedOn < toDTMinus2HPlusMinutes))
                              : ((fromDTMinus2H == null || m.StatedOn >= fromDTMinus2H) &&
                                 (toDTMinus2HPlusMinutes == null || m.StatedOn < toDTMinus2HPlusMinutes)))
                    join unionTr in unionQuery on m.Id equals unionTr.Id
                    // Join Transaction table to check SourceAccountType and TargetAccountType for InternalTransfer
                    join t in tenantDbContext.Transactions on m.Id equals t.Id into tj
                    from t in tj.DefaultIfEmpty()
                    select new
                    {
                        Record = new WalletTransactionRecord
                        {
                            Id = unionTr.Id,
                            WalletId = wt.WalletId,
                            PartyId = wt.Wallet.PartyId,
                            SourceCurrencyId = unionTr.SourceCurrencyId,
                            CurrencyId = (CurrencyTypes)unionTr.CurrencyId,
                            FundType = (FundTypes)wt.Wallet.FundType,
                            Source = unionTr.Source,
                            Target = unionTr.Target,
                            // SourceAmount: Use WalletTransaction.Amount only if source is Wallet
                            SourceAmount = m.Type == (int)MatterTypes.InternalTransfer && t != null && t.SourceAccountType == (int)TransactionAccountTypes.Wallet  
                                ? Math.Abs(wt.Amount) // Source is Wallet, use WalletTransaction.Amount
                                : unionTr.SourceAmount, // Source is Account or not InternalTransfer, use from union query
                            // Amount: Use WalletTransaction.Amount only if target is Wallet
                            Amount = m.Type == (int)MatterTypes.InternalTransfer && t != null && t.TargetAccountType == (int)TransactionAccountTypes.Wallet
                                ? Math.Abs(wt.Amount) // Target is Wallet, use WalletTransaction.Amount
                                : unionTr.Amount, // Target is Account or not InternalTransfer, use from union query
                            MatterType = (MatterTypes)m.Type,
                            StateId = (StateTypes)m.StateId,
                            // 同步成MT5时间, 即GMT+hoursGap (configurable)
                            CreatedOn = m.PostedOn.AddHours(hoursGap),
                            ReleasedOn = m.Type != (int)MatterTypes.Withdrawal ? m.StatedOn.AddHours(hoursGap) : unionTr.ReleasedOn.AddHours(hoursGap),
                            RebateTargetAccountUid = unionTr.RebateTargetAccountUid
                        },
                        MatterType = (MatterTypes)m.Type,
                        Information = r != null ? r.Information : string.Empty // Get Information from Rebates table for rebate processing
                    };

                List<(WalletTransactionRecord Record, MatterTypes MatterType, string Information)> queryResults;
                await using (var trans = await tenantDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await tenantDbContext.Database.ExecuteSqlRawAsync("SET ENABLE_NESTLOOP TO OFF;");
                        var anonymousResults = await query.ToListAsync();
                        // Convert anonymous type to tuple after query execution
                        queryResults = anonymousResults.Select(x => (x.Record, x.MatterType, x.Information)).ToList();
                        await tenantDbContext.Database.ExecuteSqlRawAsync("SET ENABLE_NESTLOOP TO ON;");
                        await trans.CommitAsync();
                    }
                    catch
                    {
                        await trans.RollbackAsync();
                        throw;
                    }
                }

                // Process results: extract records and update SourceAmount for rebates using exchangeRate from Information JSON
                var items = queryResults.Select(x =>
                {
                    var record = x.Record;
                    
                    // For rebates, parse exchangeRate from Information JSON and calculate SourceAmount
                    if (x.MatterType == MatterTypes.Rebate && !string.IsNullOrEmpty(x.Information))
                    {
                        try
                        {
                            var infoObj = JsonConvert.DeserializeObject<dynamic>(x.Information);
                            var exchangeRate = (decimal?)(infoObj?.exchangeRate);
                            
                            if (exchangeRate.HasValue && exchangeRate.Value > 0 && record.SourceCurrencyId != record.CurrencyId)
                            {
                                // exchangeRate converts from source currency to wallet currency (target)
                                // So SourceAmount = Amount / exchangeRate
                                record.SourceAmount = Math.Round(record.Amount / exchangeRate.Value, 2, MidpointRounding.AwayFromZero);
                            }
                            else
                            {
                                // Same currency or no exchangeRate, SourceAmount = Amount
                                record.SourceAmount = record.Amount;
                            }
                        }
                        catch
                        {
                            // If JSON parsing fails, fallback to Amount
                            record.SourceAmount = record.Amount;
                        }
                    }
                    
                    return record;
                }).ToList();

                await FulfillClientNameAsync(items);

                foreach (var item in items)
                    item.MtGmtOffsetHoursForCsv = hoursGapForHeader;

                // var withdrawals = items.Where(x => x.MatterType == MatterTypes.Withdrawal).ToList();
                // var activities = await _tenantDbContext.Activities
                //     .Where(x => withdrawals.Select(y => y.Id).Contains(x.MatterId))
                //     .GroupBy(x => x.MatterId)
                //     .ToDictionaryAsync(x => x.Key,
                //         x => x
                //             .Select(y => new { y.ToStateId, y.PerformedOn })
                //             .ToList()
                //     );
                //
                // foreach (var withdrawalRecord in withdrawals)
                // {
                //     var acs = activities.GetValueOrDefault(withdrawalRecord.Id);
                //     if (acs == null) continue;
                //
                //     var approveActivity = acs.FirstOrDefault(x => x.ToStateId == (int)StateTypes.WithdrawalTenantApproved);
                //     if (approveActivity == null) continue;
                //
                //     // var finalActivity = acs.MaxBy(x => x.PerformedOn);
                //     var completeActivity = acs.FirstOrDefault(x => x.ToStateId == (int)StateTypes.WithdrawalCompleted);
                //     // if not same day, remove
                //     if (criteria is { From: not null, To: not null } && completeActivity?.PerformedOn > criteria.To.Value)
                //     {
                //         items.Remove(withdrawalRecord);
                //         continue;
                //     }
                //
                //     withdrawalRecord.CreatedOn = approveActivity.PerformedOn;
                // }

                return items;
            },
            WalletTransactionRecord.Header(hoursGapForHeader));
    }

    private async Task<Tuple<ReportRequest, Medium?>> ProcessSalesRebateForTenantRequestAsync(ReportRequest request)
        => await ProcessRequestAsync<SalesRebate, SalesRebate.Criteria, SalesRebateRecord>(
            request,
            $"sales_rebate_report_{DateTime.UtcNow:yyyyMMdd-HHmms}.csv",
            async criteria =>
            {
                var hoursGap = await configSvc.GetHoursGapForMT5Async(criteria.From);
                (criteria.From, criteria.To) = DateHelper.MinusMT5GMTHours(criteria.From, criteria.To, hoursGap);

                var query = tenantDbContext.SalesRebates
                    .FilterBy(criteria)
                    .ToRecords()
                    .OrderByDescending(x => x.Id);

                var items = await query
                    .Skip((criteria.Page - 1) * criteria.Size)
                    .Take(criteria.Size)
                    .ToListAsync();

                criteria.Total = await query.CountAsync();

                return items;
            }
        );

    private async Task<Tuple<ReportRequest, Medium?>> ProcessSalesRebateSumByAccountForTenantRequestAsync(
        ReportRequest request)
        => await ProcessRequestAsync<SalesRebate, SalesRebate.Criteria, SalesRebateSumByAccountRecord>(
            request,
            $"sales_rebate_sumUp_report_{DateTime.UtcNow:yyyyMMdd-HHmms}.csv",
            async criteria =>
            {
                var query = tenantDbContext.SalesRebates
                    .FilterBy(criteria)
                    .GroupBy(x => x.TradeAccountNumber)
                    .Select(group => new SalesRebateSumByAccountRecord
                    {
                        TradeAccountNumber = group.Key,
                        Amount = group.Sum(x => x.Amount)
                    })
                    .OrderByDescending(x => x.TradeAccountNumber);

                var items = await query
                    .Skip((criteria.Page - 1) * criteria.Size)
                    .Take(criteria.Size)
                    .ToListAsync();

                criteria.Total = await query.CountAsync();

                return items;
            }
        );

    private static bool ValidateQuery<T>(string query) where T : class
    {
        try
        {
            JsonConvert.DeserializeObject<T>(query, Utils.AppJsonSerializerSettings);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Tuple<ReportRequest, Medium?>> ProcessRequestAsync<T, TB, TBk>(ReportRequest request,
        string fileName,
        Func<TB, Task<List<TBk>>> getItemsAsync,
        string? csvHeaderOverride = null)
        where T : class, IEntity<long>
        where TB : Criteria<T>
        where TBk : ICanExportToCsv
    {
        var criteria = JsonConvert.DeserializeObject<TB>(request.Query, Utils.AppJsonSerializerSettings);
        if (criteria == null) return new Tuple<ReportRequest, Medium?>(request, null);

        criteria.Page = 1;
        criteria.Size = 500000;
        criteria.SortField = "Id";
        criteria.SortFlag = false;

        var tempDirectory = Path.GetTempPath();
        var tempFilePath = Path.Combine(tempDirectory, Path.GetRandomFileName() + ".csv");
        await using var writer = new StreamWriter(tempFilePath, false, new UTF8Encoding(true));
        // Write CSV content to the file
        await writer.WriteLineAsync(csvHeaderOverride ?? TBk.Header());

        while (true)
        {
            var items = await getItemsAsync(criteria);
            if (!items.Any())
                break;
            criteria.Page++;
            logger.LogInformation($"ReportService>>ProcessRequestAsync: {criteria.Page}");
            foreach (var item in items)
                await writer.WriteLineAsync(item.ToCsv());
        }

        writer.Close();

        var now = DateTime.UtcNow;
        await using var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var medium = await storageService.UploadFileAndSaveMediaAsync(
            fileStream, fileName, ".csv", "report-request", request.Id,
            "text/csv", 0, request.PartyId
        );
        request.GeneratedOn = now;
        request.FileName = medium.Guid;
        tenantDbContext.ReportRequests.Update(request);
        await tenantDbContext.SaveChangesAsync();
        return new Tuple<ReportRequest, Medium?>(request, medium);
    }

    /// <summary>
    /// 更新配对报告的 Query 字段，添加 pairFileName
    /// </summary>
    private async Task UpdatePairedReportQueryAsync(ReportRequest currentReport)
    {
        // 确定报告类型
        var reportType = currentReport.Type;
        if (reportType != (int)ReportRequestTypes.Rebate && reportType != (int)ReportRequestTypes.WalletTransactionForTenant)
            return;

        // 优先从 Query 中读取 UseClosingTime，如果不存在则从名称判断
        bool? useClosingTime = null;
        try
        {
            var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(currentReport.Query);
            if (queryDict != null && queryDict.ContainsKey("UseClosingTime"))
            {
                useClosingTime = Convert.ToBoolean(queryDict["UseClosingTime"]);
            }
        }
        catch
        {
            // 如果解析失败，继续使用名称判断
        }

        // 如果 Query 中没有 UseClosingTime，则从名称或 IsFromApi 判断
        var currentName = currentReport.Name;
        var isClosingTimeBased = useClosingTime == true 
            || (useClosingTime == null && currentName.Contains("MT5 ClosingTime Based"));
        var isReleasedTimeBased = useClosingTime == false 
            || (useClosingTime == null && currentName.Contains("ReleasedTime Based"));

        // 如果既不是 ClosingTime 也不是 ReleasedTime，根据 IsFromApi 判断
        // Job入口 (IsFromApi == 0) 默认是 ClosingTime，API入口需要检查 UseClosingTime
        if (!isClosingTimeBased && !isReleasedTimeBased)
        {
            if (currentReport.IsFromApi == 0)
            {
                isClosingTimeBased = true; // Job入口默认使用ClosingTime
            }
            else
            {
                // API入口：如果 UseClosingTime 未设置或为 false，则使用 ReleasedTime
                isReleasedTimeBased = useClosingTime != true;
            }
        }

        // 提取日期部分（从 Query 或名称）
        string? dateStr = null;
        DateTime? reportDate = null;
        
        // 优先从 Query 中提取日期
        try
        {
            var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(currentReport.Query);
            if (queryDict != null && queryDict.ContainsKey("to"))
            {
                reportDate = DateTime.Parse(queryDict["to"].ToString()!);
                dateStr = reportDate.Value.ToString("yyyy-MM-dd");
            }
        }
        catch
        {
            // 如果从 Query 解析失败，尝试从名称中提取日期
        }

        if (string.IsNullOrEmpty(dateStr))
        {
            var dateMatch = System.Text.RegularExpressions.Regex.Match(currentName, @"(\d{4}-\d{2}-\d{2})");
            if (dateMatch.Success)
            {
                dateStr = dateMatch.Groups[1].Value;
            }
            else
            {
                return; // 无法提取日期，无法配对
            }
        }

        // 查找配对报告（重新从数据库加载，确保获取最新状态）
        ReportRequest? pairedReport = null;
        if (isClosingTimeBased)
        {
            // 当前是 MT5 ClosingTime Based (IsFromApi=0 或 UseClosingTime=true)，查找 ReleasedTime Based 配对报告 (IsFromApi=1 且 UseClosingTime=false)
            // 配对逻辑：通过 Query 中的 from/to 日期和 IsFromApi 来匹配
            pairedReport = await tenantDbContext.ReportRequests
                .FirstOrDefaultAsync(x =>
                    x.Type == reportType
                    && x.IsFromApi == 1
                    && x.PartyId == currentReport.PartyId
                    && x.Id != currentReport.Id
                    && x.Query.Contains(dateStr)); // 通过日期字符串匹配
            
            // 进一步验证：检查 Query 中的 UseClosingTime 或名称
            if (pairedReport != null)
            {
                try
                {
                    var pairedQueryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(pairedReport.Query);
                    var pairedUseClosingTime = pairedQueryDict != null && pairedQueryDict.ContainsKey("UseClosingTime") 
                        ? Convert.ToBoolean(pairedQueryDict["UseClosingTime"])
                        : (bool?)null;
                    
                    // 如果配对报告的 UseClosingTime 是 true，或者名称包含 "MT5 ClosingTime Based"，则不是正确的配对
                    if (pairedUseClosingTime == true || pairedReport.Name.Contains("MT5 ClosingTime Based"))
                    {
                        pairedReport = null; // 不是正确的配对，继续查找
                    }
                }
                catch
                {
                    // 如果解析失败，检查名称
                    if (pairedReport.Name.Contains("MT5 ClosingTime Based"))
                    {
                        pairedReport = null;
                    }
                }
            }
        }
        else if (isReleasedTimeBased)
        {
            // 当前是 ReleasedTime Based (IsFromApi=1 且 UseClosingTime=false)，查找 MT5 ClosingTime Based 配对报告 (IsFromApi=0 或 UseClosingTime=true)
            pairedReport = await tenantDbContext.ReportRequests
                .FirstOrDefaultAsync(x =>
                    x.Type == reportType
                    && (x.IsFromApi == 0 || x.Name.Contains("MT5 ClosingTime Based"))
                    && x.PartyId == currentReport.PartyId
                    && x.Id != currentReport.Id
                    && x.Query.Contains(dateStr)); // 通过日期字符串匹配
            
            // 进一步验证：检查 Query 中的 UseClosingTime
            if (pairedReport != null)
            {
                try
                {
                    var pairedQueryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(pairedReport.Query);
                    var pairedUseClosingTime = pairedQueryDict != null && pairedQueryDict.ContainsKey("UseClosingTime") 
                        ? Convert.ToBoolean(pairedQueryDict["UseClosingTime"])
                        : (bool?)null;
                    
                    // 如果配对报告的 UseClosingTime 是 false，且 IsFromApi=1，则不是正确的配对
                    if (pairedReport.IsFromApi == 1 && pairedUseClosingTime == false)
                    {
                        pairedReport = null; // 不是正确的配对
                    }
                }
                catch
                {
                    // 如果解析失败，使用 IsFromApi 判断
                    if (pairedReport.IsFromApi == 1 && !pairedReport.Name.Contains("MT5 ClosingTime Based"))
                    {
                        pairedReport = null;
                    }
                }
            }
        }

        if (pairedReport != null)
        {
            // 重新加载当前报告和配对报告，确保获取最新状态（包括 GeneratedOn 和 FileName）
            var currentReportFresh = await tenantDbContext.ReportRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentReport.Id);
            var pairedReportFresh = await tenantDbContext.ReportRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == pairedReport.Id);
            
            if (currentReportFresh == null || pairedReportFresh == null)
                return;

            // 如果配对报告已生成，更新当前报告的 Query 字段
            if (pairedReportFresh.GeneratedOn != null && !string.IsNullOrEmpty(pairedReportFresh.FileName))
            {
                // 重新加载当前报告用于更新（不使用 AsNoTracking）
                var currentReportForUpdate = await tenantDbContext.ReportRequests
                    .FirstOrDefaultAsync(x => x.Id == currentReport.Id);
                if (currentReportForUpdate != null)
                {
                    var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(currentReportForUpdate.Query) 
                        ?? new Dictionary<string, object>();
                    // 只有当 pairFileName 不存在或不同时才更新
                    if (!queryDict.ContainsKey("pairFileName") || queryDict["pairFileName"]?.ToString() != pairedReportFresh.FileName)
                    {
                        queryDict["pairFileName"] = pairedReportFresh.FileName;
                        queryDict["pairReportName"] = pairedReportFresh.Name; // 同时存储配对报告的名称
                        currentReportForUpdate.Query = JsonConvert.SerializeObject(queryDict, Utils.AppJsonSerializerSettings);
                        tenantDbContext.ReportRequests.Update(currentReportForUpdate);
                        await tenantDbContext.SaveChangesAsync();
                        logger?.LogInformation($"Updated ReportRequest {currentReportForUpdate.Id} Query with pairFileName: {pairedReportFresh.FileName}");
                    }
                }
            }

            // 如果当前报告已生成，更新配对报告的 Query 字段
            if (currentReportFresh.GeneratedOn != null && !string.IsNullOrEmpty(currentReportFresh.FileName))
            {
                // 重新加载配对报告用于更新（不使用 AsNoTracking）
                var pairedReportForUpdate = await tenantDbContext.ReportRequests
                    .FirstOrDefaultAsync(x => x.Id == pairedReport.Id);
                if (pairedReportForUpdate != null)
                {
                    var pairedQueryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(pairedReportForUpdate.Query) 
                        ?? new Dictionary<string, object>();
                    // 只有当 pairFileName 不存在或不同时才更新
                    if (!pairedQueryDict.ContainsKey("pairFileName") || pairedQueryDict["pairFileName"]?.ToString() != currentReportFresh.FileName)
                    {
                        pairedQueryDict["pairFileName"] = currentReportFresh.FileName;
                        pairedQueryDict["pairReportName"] = currentReportFresh.Name; // 同时存储配对报告的名称
                        pairedReportForUpdate.Query = JsonConvert.SerializeObject(pairedQueryDict, Utils.AppJsonSerializerSettings);
                        tenantDbContext.ReportRequests.Update(pairedReportForUpdate);
                        await tenantDbContext.SaveChangesAsync();
                        logger?.LogInformation($"Updated ReportRequest {pairedReportForUpdate.Id} Query with pairFileName: {currentReportFresh.FileName}");
                    }
                }
            }
        }
    }

    private async Task FulfillClientNameAsync<T>(List<T> items) where T : class, ICanExportToCsv
    {
        var partyIds = items.Select(x => x.PartyId).Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => partyIds.Contains(x.PartyId) && tenantGetter.GetTenantId() == x.TenantId)
            .ToListAsync();
        foreach (var item in items)
        {
            var user = users.FirstOrDefault(x => x.PartyId.Equals(item.PartyId));
            if (user == null) return;
            item.ClientName = user.GuessUserNativeName();
        }
    }

    private async Task FulfillClientNameEmailAsync<T>(List<T> items) where T : WalletOverviewRecord
    {
        var partyIds = items.Select(x => x.PartyId).Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => partyIds.Contains(x.PartyId) && tenantGetter.GetTenantId() == x.TenantId)
            .ToListAsync();
        foreach (var item in items)
        {
            var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
            if (user == null) continue;
            item.ClientName = user.GuessUserNativeName();
            item.Email = user.Email;
        }
    }

    private static string GetDemoAccountQueryString(DemoAccountRecord.QueryCriteria criteria) => $"""
         SELECT demo."AccountNumber",
                demo."ExpireOn",
                demo."Leverage",
                demo."Balance",
                CASE
                    WHEN demo."Type" = 4 THEN 'STD'
                    WHEN demo."Type" = 6 THEN 'ALPHA'
                    WHEN demo."Type" = 13 THEN 'Sea STD'
                    WHEN demo."Type" = 11 THEN 'Advantage'
                    ELSE CAST(demo."Type" AS VARCHAR)
                    END as "Type",
                CASE
                    WHEN demo."CurrencyId" = 36 THEN 'AUD'
                    WHEN demo."CurrencyId" = 840 THEN 'USD'
                    ELSE CAST(demo."CurrencyId" AS VARCHAR)
                    END as "Currency",
                CASE
                    WHEN demo."PartyId" > 1 THEN p."NativeName"
                    ELSE demo."Name"
                    END AS "NativeName",
                CASE
                    WHEN demo."PartyId" > 1 THEN p."FirstName"
                    ELSE ''
                    END AS "FirstName",
                CASE
                    WHEN demo."PartyId" > 1 THEN p."LastName"
                    ELSE ''
                    END AS "LastName",
                CASE
                    WHEN demo."PartyId" > 1 THEN p."CountryCode"
                    ELSE demo."CountryCode"
                    END AS "CountryCode",
                CASE
                    WHEN demo."PartyId" > 1 THEN p."PhoneNumber"
                    ELSE demo."PhoneNumber"
                    END AS "PhoneNumber",
                demo."Email",
                CASE
                    WHEN demo."PartyId" > 1 THEN p."Language"
                    ELSE ''
                    END AS "Language",
                demo."CreatedOn"
         FROM trd."_TradeDemoAccount" demo
                  LEFT JOIN core."_Party" p ON demo."PartyId" = p."Id"
         WHERE demo."CreatedOn" > '{criteria.Date.AddDays(-7):yyyy-MM-dd HH:mm:ss}'
           AND (
             demo."PartyId" = 1
                 OR (demo."PartyId" > 1
                 AND NOT EXISTS (SELECT 1
                                 FROM trd."_Account" acc
                                 WHERE acc."PartyId" = demo."PartyId"))
             )
         ORDER BY demo."CreatedOn"
         OFFSET {(criteria.Page - 1) * criteria.Size} LIMIT {criteria.Size};
         """;
}