using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services.Email.ViewModel;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Web.Services;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    private static IQueryable<Account> GetAccountQuery(TenantDbContext tenantDbContext)
        => tenantDbContext.Accounts
            .Where(x => x.AccountNumber > 0 && x.ServiceId != 0)
            .Where(x => x.IsClosed == 0 && x.Status == 0)
            .Where(x => x.Role == (short)AccountRoleTypes.Client)
            // .Where(x => x.AccountTags.Any(y => y.Name == AccountTagTypes.DailyConfirmEmail))
            .OrderByDescending(x => x.Id);

    public async Task GenerateAccountToSendConfirmationReport(long tenantId, DateTime from, DateTime to)
    {
        const int threadCount = 10;
        var total = await GetAccountQuery(tenantDbContext).CountAsync();
        BcrLog.Slack($"Tenant: {tenantId} Today's daily report total: {total}");
        var numPerThread = (int)Math.Ceiling((double)total / threadCount);
        var tasks = new List<Task>();
        var processed = 0;
        var shouldSendCount = 0;
        for (var i = 0; i < threadCount; i++)
        {
            var start = i * numPerThread;
            var end = Math.Min((i + 1) * numPerThread, total);
            if (start >= end) break;
            var task = Task.Run(async () =>
            {
                await using var ctx = myDbContextPool.CreateTenantDbContext(tenantId);
                var maxId = await GetAccountQuery(ctx).Skip(start).Take(end - start).MaxAsync(x => x.Id);
                var minId = await GetAccountQuery(ctx).Skip(start).Take(end - start).MinAsync(x => x.Id);
                var pool = new Dictionary<int, DbContext>();
                var page = 1;
                const int size = 200;
                do
                {
                    var accounts = await GetAccountQuery(ctx)
                        .Where(x => x.Id >= minId && x.Id <= maxId)
                        .Skip((page - 1) * size).Take(size)
                        .Select(x => new { x.Id, x.PartyId, x.ServiceId, x.AccountNumber })
                        .ToListAsync();

                    foreach (var account in accounts)
                    {
                        Interlocked.Increment(ref processed);
                        var shouldSend = false;
                        var platform = myDbContextPool.GetPlatformByServiceId(account.ServiceId);
                        if (platform == PlatformTypes.MetaTrader4)
                        {
                            if (!pool.ContainsKey(account.ServiceId))
                            {
                                pool[account.ServiceId] = myDbContextPool.CreateCentralMT4DbContextAsync(account.ServiceId);
                            }

                            var mt4Ctx = (MetaTrade4DbContext)pool[account.ServiceId];

                            var hasTrade = await mt4Ctx.Mt4Trades
                                .Where(x => x.Login == account.AccountNumber)
                                .Where(x => x.CloseTime == Mt4Trade.DefaultDateTime ||
                                            (x.CloseTime >= from && x.CloseTime <= to))
                                .Where(x => x.Cmd == 0 || x.Cmd == 1 || x.Cmd == 6 || x.Cmd == 7)
                                .AnyAsync();

                            shouldSend = hasTrade;
                        }
                        else if (platform == PlatformTypes.MetaTrader5)
                        {
                            if (!pool.ContainsKey(account.ServiceId))
                            {
                                pool[account.ServiceId] = myDbContextPool.CreateCentralMT5DbContextAsync(account.ServiceId);
                            }

                            var mt5Ctx = (MetaTrade5DbContext)pool[account.ServiceId];

                            var hasTrade = await mt5Ctx.Mt5Deals2025s
                                .Where(x => x.Login == (ulong)account.AccountNumber)
                                .Where(x => x.Time >= from && x.Time <= to)
                                .Where(x => new uint[] { 2, 3 }.Contains(x.Action) || // 2 adjust, 3 credit
                                            (new uint[] { 0, 1 }.Contains(x.Action) && x.VolumeClosed > 0))
                                .AnyAsync();

                            shouldSend = hasTrade;
                            if (!shouldSend)
                            {
                                var hasOpenTrades = await mt5Ctx.Mt5Positions
                                    .Where(x => x.Login == (ulong)account.AccountNumber)
                                    .AnyAsync();
                                shouldSend = hasOpenTrades;
                            }
                        }

                        if (!shouldSend) continue;

                        Interlocked.Increment(ref shouldSendCount);
                        var exists = await ctx.AccountReports
                            .Where(x => x.Type == (int)AccountReportTypes.DailyConfirmation)
                            .Where(x => x.AccountNumber == account.AccountNumber)
                            .Where(x => x.ServiceId == account.ServiceId)
                            .Where(x => x.Date == from)
                            .AnyAsync();
                        if (exists) continue;

                        try
                        {
                            var model = AccountReport.Build(tenantId, account.PartyId, account.Id, account.ServiceId, account.AccountNumber, from,
                                AccountReportTypes.DailyConfirmation);

                            ctx.AccountReports.Add(model);
                            await ctx.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            BcrLog.Slack($"GenerateAccountToSendConfirmationReport_Error_Save_Model => {e.Message}");
                        }

                        // Console.WriteLine(
                        // $"{account.AccountNumber},{account.ServiceId}, remains: {total - processed}, threadId: {Environment.CurrentManagedThreadId}");
                    }

                    if (accounts.Count < size)
                        break;
                    page++;
                } while (true);

                foreach (var (_, mtCtx) in pool)
                {
                    await mtCtx.DisposeAsync();
                }
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        BcrLog.Slack(
            $" Tenant: {tenantId} Today's daily report processed: {processed}, should send count: {shouldSendCount}");

        // var query = _tenantDbContext.Accounts
        //     .Where(x => x.AccountNumber > 0 && x.ServiceId != 0)
        //     .Where(x => x.IsClosed == 0 && x.Status == 0)
        //     .Where(x => x.Role == (short)AccountRoleTypes.Client)
        //     .Where(x => x.AccountTags.Any(y => y.Name == AccountTagTypes.DailyConfirmEmail))
        //     .OrderByDescending(x => x.Id);
        //
        // // var cc = await query.CountAsync();
        // var page = 1;
        // const int size = 200;
        // var pool = new Dictionary<int, DbContext>();
        //
        // do
        // {
        //     var accounts = await query
        //         .Skip((page - 1) * size).Take(size)
        //         .Select(x => new { x.Id, x.PartyId, x.ServiceId, x.AccountNumber })
        //         .ToListAsync();
        //     foreach (var account in accounts)
        //     {
        //         // if (!await ShouldSendStatementReport(account.ServiceId, account.AccountNumber, from, to))
        //         //     continue;
        //
        //         var shouldSend = false;
        //         var platform = _myDbContextPool.GetPlatformByServiceId(account.ServiceId);
        //         if (platform == PlatformTypes.MetaTrader4)
        //         {
        //             if (!pool.ContainsKey(account.ServiceId))
        //             {
        //                 var ctx = _myDbContextPool.CreateCentralMT4DbContextAsync(account.ServiceId);
        //                 pool[account.ServiceId] = ctx;
        //             }
        //
        //             var mt4Ctx = (MetaTrade4DbContext)pool[account.ServiceId];
        //
        //             var hasTrade = await mt4Ctx.Mt4Trades
        //                 .Where(x => x.Login == account.AccountNumber)
        //                 .Where(x => x.CloseTime == Mt4Trade.DefaultDateTime ||
        //                             (x.CloseTime >= from && x.CloseTime <= to))
        //                 .Where(x => x.Cmd == 0 || x.Cmd == 1 || x.Cmd == 6 || x.Cmd == 7)
        //                 .AnyAsync();
        //
        //             shouldSend = hasTrade;
        //         }
        //         else if (platform == PlatformTypes.MetaTrader5)
        //         {
        //             if (!pool.ContainsKey(account.ServiceId))
        //             {
        //                 var ctx = _myDbContextPool.CreateCentralMT5DbContextAsync(account.ServiceId);
        //                 pool[account.ServiceId] = ctx;
        //             }
        //
        //             var mt5Ctx = (MetaTrade5DbContext)pool[account.ServiceId];
        //
        //             var hasTrade = await mt5Ctx.Mt5Deals
        //                 .Where(x => x.Login == (ulong)account.AccountNumber)
        //                 .Where(x => x.Time >= from && x.Time <= to)
        //                 .Where(x => x.Action == 0 || x.Action == 1 || x.Action == 2 || x.Action == 7)
        //                 .AnyAsync();
        //
        //             shouldSend = hasTrade;
        //             if (!shouldSend)
        //             {
        //                 var hasOpenTrades = await mt5Ctx.Mt5Positions
        //                     .Where(x => x.Login == (ulong)account.AccountNumber)
        //                     .AnyAsync();
        //                 shouldSend = hasOpenTrades;
        //             }
        //
        //             if (!shouldSend)
        //             {
        //                 var info = await _tradingApiSvc.GetDailyReport(account.ServiceId
        //                     , account.AccountNumber, from, to);
        //                 if (info.Count > 0 && info[0].Positions.Count > 0) shouldSend = true;
        //             }
        //         }
        //
        //         if (!shouldSend) continue;
        //         // await AddAccountRecordOfDay(tenantId, account, from);
        //         var exists = await _tenantDbContext.AccountReports
        //             .Where(x => x.AccountNumber == account.AccountNumber)
        //             .Where(x => x.ServiceId == account.ServiceId)
        //             .Where(x => x.Date == from)
        //             .AnyAsync();
        //         if (exists) continue;
        //
        //         var model = AccountReport.Build(tenantId, account.PartyId, account.Id, account.ServiceId,
        //             account.AccountNumber, from);
        //
        //         _tenantDbContext.AccountReports.Add(model);
        //         await _tenantDbContext.SaveChangesAsync();
        //     }
        //
        //     if (accounts.Count < size)
        //         break;
        //     page++;
        // } while (true);
        //
        // foreach (var (_, ctx) in pool)
        // {
        //     await ctx.DisposeAsync();
        // }
    }

    /// <summary>
    /// Process AccountReportModel
    /// </summary>
    /// <returns> Whether db has remaining unprocessed model </returns>
    public async Task<long> ProcessAccountReportModel(DateTime? date = null, bool? replaceInS3 = false)
    {
        var criteria = new AccountReport.Criteria
        {
            Tries = 5,
            Size = 100,
            Type = AccountReportTypes.DailyConfirmation,
            TryTime = DateTime.UtcNow,
            Status = AccountReportStatusTypes.Initialed,
            Date = date
        };

        var query = tenantDbContext.AccountReports.PagedFilterBy(criteria);
        var reports = await query.ToListAsync();
        foreach (var report in reports)
        {
            await GenerateAndSaveAccountDailyReport(report, replaceInS3 ?? false);
        }

        return await query.CountAsync();
    }

    public async Task<long> ProcessSendAccountDailyReportEmail(DateTime? date = null)
    {
        var criteria = new AccountReport.Criteria
        {
            Size = 100,
            Tries = 10,
            Date = date,
            Type = AccountReportTypes.DailyConfirmation,
            TryTime = DateTime.UtcNow,
            Status = AccountReportStatusTypes.Generated,
        };
        var query = tenantDbContext.AccountReports
            .Where(x => x.Account.Tags.Any(y => y.Name == AccountTagTypes.DailyConfirmEmail))
            .PagedFilterBy(criteria);
        var reports = await query.ToListAsync();
        foreach (var report in reports)
        {
            var (result, msg) = await SendAccountReportEmail(report);
            if (!result)
            {
                report.Tries++;
                report.TryTime = DateTime.UtcNow.AddMinutes(5);
                report.UpdatedOn = DateTime.UtcNow;
                tenantDbContext.AccountReports.Update(report);
                await tenantDbContext.SaveChangesAsync();
                continue;
            }

            report.Status = (short)AccountReportStatusTypes.MailSent;
            report.UpdatedOn = DateTime.UtcNow;
            tenantDbContext.AccountReports.Update(report);
            await tenantDbContext.SaveChangesAsync();
        }

        return await query.CountAsync();
    }

    public async Task<AccountReport> AddAccountRecordOfDay(long tenantId, Account account, DateTime day)
    {
        var exists = await tenantDbContext.AccountReports
            .Where(x => x.AccountNumber == account.AccountNumber)
            .Where(x => x.ServiceId == account.ServiceId)
            .Where(x => x.Date == day)
            .AnyAsync();
        if (exists)
        {
            logger.LogInformation(
                "AccountDailyReport> Account report already exist: {AccountNumber}", account.AccountNumber);
            return new AccountReport();
        }

        var model = AccountReport.Build(tenantId, account.PartyId, account.Id, account.ServiceId,
            account.AccountNumber, day, AccountReportTypes.DailyConfirmation);

        tenantDbContext.AccountReports.Add(model);
        await tenantDbContext.SaveChangesAsync();
        logger.LogInformation(
            "AccountDailyReport> Account daily report created: {AccountNumber} {Day}", account.AccountNumber, day);
        return model;
    }

    public async Task<bool> GenerateAndSaveAccountDailyReport(AccountReport report, bool replaceInS3 = false,
        GenerateDailyReportOptions? options = null)
    {
        var filePath = report.GetFilePathInS3();

        AccountDailyConfirmationViewModel? model;
        if (!replaceInS3)
        {
            model = await GetAccountDailyReportModelByPathFromS3(filePath);
            if (model != null)
            {
                logger.LogInformation("AccountDailyReport> Model exist in S3 {filePath}", filePath);
                report.DataFile = filePath;
                report.Status = (short)AccountReportStatusTypes.Generated;
                report.UpdatedOn = DateTime.UtcNow;
                tenantDbContext.AccountReports.Update(report);
                await tenantDbContext.SaveChangesAsync();
            }
        }

        try
        {
            model = await GenerateAccountReportModel(report, options);
            if (model == null)
            {
                report.Tries++;
                report.TryTime = DateTime.UtcNow.AddMinutes(5);
                report.UpdatedOn = DateTime.UtcNow;
                tenantDbContext.AccountReports.Update(report);
                await tenantDbContext.SaveChangesAsync();
                logger.LogInformation(
                    "AccountDailyReport>Model can't be generated ReportId: {Id}, retry: {Tries}", report.Id,
                    report.Tries);
                return false;
            }

            await SaveAccountReportToS3(report, model);
            report.DataFile = filePath;
            report.Status = (short)AccountReportStatusTypes.Generated;
        }
        catch (Exception e)
        {
            report.Status = (short)AccountReportStatusTypes.Failed;
            BcrLog.Slack(
                $"GenerateAndSaveAccountDailyReport_Failed => {report.AccountNumber} => {report.ReportDate} => {e.Message}");
        }


        report.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.AccountReports.Update(report);
        await tenantDbContext.SaveChangesAsync();
        logger.LogInformation(
            "AccountDailyReport>Service>ProcessAccountReportModel> Model generated ReportId: {ReportId}", report.Id);
        return true;
    }

    public async Task SaveAccountReportToS3(AccountReport report, AccountDailyConfirmationViewModel model)
    {
        var modelJson = JsonConvert.SerializeObject(model);
        var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(modelJson));
        await storageService.UploadFileAsync(jsonStream, report.GetFileDirInS3(), report.GetFileNameInS3(), "json",
            "application/json", report.TenantId,
            report.PartyId, false);
    }

    public async Task<(bool, string)> SendAccountReportEmailById(long reportId, string? senderEmail = null,
        string? receiverEmail = null)
    {
        var report = await tenantDbContext.AccountReports
            .Where(x => x.Id == reportId)
            .FirstOrDefaultAsync();
        if (report == null) return (false, "Report not found");
        return await SendAccountReportEmail(report, senderEmail, receiverEmail);
    }

    public async Task<(bool, string)> SendAccountReportEmail(AccountReport report, string? senderEmail = null,
        string? receiverEmail = null)
    {
        var model = await GetAccountDailyReportModelByPathFromS3(report.DataFile);
        if (model == null) return (false, "Model in S3 not found");

        var html = await GenerateReportHtmlForEmail(model, report);
        if (html == null) return (false, "Html generation failed");
        var receiverEmailAddress = receiverEmail ?? model.Email;

        var isMatch = EmailViewModel.EmailRegex().IsMatch(receiverEmailAddress);
        if (!isMatch)
        {
            BcrLog.Slack(
                $"SendAccountReportEmail_Invalid email: {receiverEmailAddress}, nativeName:{model.NativeName}");
            return (false, "Invalid email address");
        }

        // var receiverEmailAddress = "dealing@bacera.com";
        var senderEmailAddress = Environment.GetEnvironmentVariable("AWS_SES_FROM_ADDRESS") ?? "statements@thebcr.com";
        var senderDisplayName = await configurationSvc.GetDefaultEmailDisplayNameAsync();

        var bcc = new List<string> { senderEmailAddress };
        var sendEmailEnabled = await configurationSvc.GetAccountDailyReportToggleSwitchAsync();

        try
        {
            try
            {
                await messageSvc.AddSendEmailAsync(receiverEmailAddress, model.TemplateTitle,
                    html, report.PartyId, report.Id);
            }
            catch (Exception e)
            {
                BcrLog.Slack($"SendAccountReportEmail_AddSendEmailAsync_Error => {e.Message}");
            }

            if (sendEmailEnabled)
            {
                var (emailResult, msg) = await emailSender.SendEmailAsync(receiverEmailAddress, model.TemplateTitle,
                    html,
                    senderEmailAddress, senderDisplayName, bcc);
                return (emailResult, msg);
            }

            return (true, "Email send disabled");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Email send failed");
            return (false, e.Message);
        }
    }

    private async Task<string?> GenerateReportHtmlForEmail(AccountDailyConfirmationViewModel model,
        AccountReport report)
    {
        var user = await authDbContext.Users
            .Where(x => x.TenantId == report.TenantId && x.PartyId == model.PartyId)
            .ToEmailNameModel()
            .SingleOrDefaultAsync();
        if (user == null) return string.Empty;

        model.Email = user.Email;
        model.NativeName = user.GuessNativeName();

        var template = await sendMailSvc.GetTemplate(model.TemplateTitle, user.Language);
        try
        {
            var html = await sendMailSvc.ApplyVariablesInTemplate(template.Content, model);
            return html;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"GenerateReportHtmlForEmail_Error => {e.Message}");
            return null;
        }
    }

    public async Task<AccountDailyConfirmationViewModel?> GenerateAccountReportModel(AccountReport report,
        GenerateDailyReportOptions? options = null)
    {
        var platform = myDbContextPool.GetPlatformByServiceId(report.ServiceId);
        var summary = new Summary();
        var closedTradeSummary = new TradeSummary();
        var closedTrades = new List<TradeViewModel>();
        var openTradeSummary = new TradeSummary();
        var openTrades = new List<TradeViewModel>();

        var depositCloseTrade = new List<TradeViewModel>();
        switch (platform)
        {
            case PlatformTypes.MetaTrader4:
            {
                await using var mt4DbContext = myDbContextPool.CreateCentralMT4DbContextAsync(report.ServiceId);

                var summaryInfo = await mt4DbContext.Mt4Dailies
                    .Where(x => x.Login == report.AccountNumber)
                    .Where(x => x.Time == report.ReportDate)
                    .Select(x => new { x.BalancePrev, x.Balance, x.Credit, x.Equity, x.Margin, x.MarginFree, })
                    .FirstOrDefaultAsync();
                if (summaryInfo == null) return null;

                var trades = await mt4DbContext.Mt4Trades
                    .Where(x => x.Login == report.AccountNumber)
                    .Where(x => x.CloseTime >= report.FromDate && x.CloseTime <= report.ToDate)
                    .Where(x => x.Cmd == 0 || x.Cmd == 1 || x.Cmd == 6 || x.Cmd == 7)
                    .OrderByDescending(x => x.CloseTime)
                    .Take(500)
                    .ToTradeViewModel()
                    .ToListAsync();

                closedTrades = trades.Where(x => x.Cmd is 0 or 1).ToList();
                depositCloseTrade = trades.Where(x => x.Cmd == 6).ToList();
                closedTradeSummary.Credit = trades.Where(x => x.Cmd == 7).Sum(x => x.Profit);

                if (options != null)
                {
                    if (options.FileDir.EndsWith("/"))
                    {
                        options.FileDir = options.FileDir[..^1];
                    }

                    var filePath = $"{options.FileDir}/{report.AccountNumber}_mail.json";
                    openTrades = await ReadOpenTradeFromFileAsync(filePath, report.AccountNumber);
                }
                else
                {
                    openTrades = await mt4DbContext.Mt4OpenTrades
                        .Where(x => x.Login == report.AccountNumber)
                        .Where(x => x.CloseTime <= Mt4Trade.DefaultDateTime)
                        .OrderByDescending(x => x.OpenTime)
                        .Take(500)
                        .ToTradeViewModel()
                        .ToListAsync();
                }

                summary.Credit = summaryInfo.Credit;
                summary.Equity = summaryInfo.Equity;
                summary.Margin = summaryInfo.Margin;
                summary.Balance = summaryInfo.Balance;
                summary.PrevBalance = summaryInfo.BalancePrev;
                summary.AvailableMargin = summaryInfo.MarginFree;
                break;
            }
            case PlatformTypes.MetaTrader5:
            {
                var info = await tradingApiSvc.GetDailyReport(report.ServiceId, report.AccountNumber, report.FromDate,
                    report.ToDate);
                if (info.Count > 0)
                {
                    var dailyReport = info.OrderByDescending(x => x.DatetimePrev).First();
                    summary.Profit = dailyReport.Profit;
                    summary.Credit = dailyReport.Credit;
                    summary.Balance = dailyReport.Balance;
                    summary.PrevBalance = dailyReport.BalancePrevDay;
                    summary.Equity = dailyReport.ProfitEquity;
                    summary.Margin = dailyReport.Margin;
                    summary.AvailableMargin = dailyReport.MarginFree;
                    openTrades = dailyReport.Positions.ToViewModel().ToList();
                }
                else
                {
                    return null;
                }

                await using var mt5DbContext = myDbContextPool.CreateCentralMT5DbContextAsync(report.ServiceId);

                var trades = await mt5DbContext.Mt5Deals2025s
                    .Where(x => x.Login == (ulong)report.AccountNumber)
                    .Where(x => x.Time >= report.FromDate && x.Time <= report.ToDate)
                    .Where(x => new uint[] { 2, 3, 4 }.Contains(x.Action) || // 2 adjust, 3 credit, 4 charge, 6 balance
                                (new uint[] { 0, 1 }.Contains(x.Action) && x.VolumeClosed > 0))
                    .OrderByDescending(x => x.Time)
                    .Take(500)
                    .ToTradeViewModel()
                    .ToListAsync();

                closedTrades = trades.Where(x => x.Cmd is 0 or 1).ToList();
                depositCloseTrade = trades.Where(x => x.Cmd is 2 or 4).ToList();
                depositCloseTrade.ForEach(x => x.Cmd = 6);

                closedTradeSummary.Credit = trades.Where(x => x.Cmd == 3).Sum(x => x.Profit);

                var positionId = closedTrades
                    .Where(x => x.Position != null)
                    .Select(x => x.Position)
                    .ToList();

                var positions = await mt5DbContext.Mt5Deals2025s
                    .Where(x => positionId.Contains((long)x.PositionId))
                    .Where(x => x.VolumeClosed == 0)
                    .Select(x => new { x.PositionId, x.TimeMsc, x.Price })
                    .ToListAsync();

                foreach (var item in closedTrades)
                {
                    if (item.Position == null) continue;
                    var pos = positions.FirstOrDefault(x => (long)x.PositionId == item.Position);
                    if (pos == null)
                    {
                        item.OpenAt = DateTime.MinValue;
                        item.OpenPrice = 0;
                    }

                    item.OpenAt = pos.TimeMsc;
                    item.OpenPrice = pos.Price;
                }

                closedTradeSummary.Credit = trades.Where(x => x.Cmd == 7).Sum(x => x.Profit);
                break;
            }
        }

        closedTradeSummary.Deposit = depositCloseTrade.Sum(x => x.Profit);
        summary.Deposit = closedTradeSummary.Deposit;
        closedTradeSummary.Swaps = closedTrades.Sum(x => x.Swaps);
        closedTradeSummary.Commission = closedTrades.Sum(x => x.Commission);
        // closedTradeSummary.Profit = closedTrades.Sum(x => x.Profit);
        closedTradeSummary.Profit = closedTrades.Sum(x => x.Profit);
        closedTradeSummary.TotalProfit = closedTradeSummary.Profit + closedTradeSummary.Swaps +
                                         closedTradeSummary.Commission;

        closedTrades.AddRange(depositCloseTrade);

        openTradeSummary.Swaps = openTrades.Sum(x => x.Swaps);
        openTradeSummary.Commission = openTrades.Sum(x => x.Commission);
        // openTradeSummary.Profit = openTrades.Sum(x => x.Profit);
        openTradeSummary.Profit = openTrades.Sum(x => x.Profit);
        openTradeSummary.TotalProfit = openTradeSummary.Profit + openTradeSummary.Swaps +
                                       openTradeSummary.Commission;


        var model = AccountDailyConfirmationViewModel.Build(report.ReportDate, report.ReportTime, report.TenantId,
            report.PartyId,
            report.AccountNumber, report.AccountId
            , openTrades
            , closedTrades
            , openTradeSummary
            , closedTradeSummary
            , new List<TradeViewModel>()
            , summary);

        return model;
    }

    // public async Task<bool> ShouldSendStatementReport(int serviceId, long accountNumber, DateTime from, DateTime to)
    // {
    //     var platform = _myDbContextPool.GetPlatformByServiceId(serviceId);
    //     if (platform == PlatformTypes.MetaTrader4)
    //     {
    //         await using var mt4DbContext = _myDbContextPool.CreateCentralMT4DbContextAsync(serviceId);
    //
    //         var hasTrade = await mt4DbContext.Mt4Trades
    //             .Where(x => x.Login == accountNumber)
    //             .Where(x => x.CloseTime == Mt4Trade.DefaultDateTime || (x.CloseTime >= from && x.CloseTime <= to))
    //             .Where(x => x.Cmd == 0 || x.Cmd == 1 || x.Cmd == 6 || x.Cmd == 7)
    //             .AnyAsync();
    //         return hasTrade;
    //     }
    //
    //     if (platform == PlatformTypes.MetaTrader5)
    //     {
    //         await using var mt5DbContext = _myDbContextPool.CreateCentralMT5DbContextAsync(serviceId);
    //
    //         var hasTrade = await mt5DbContext.Mt5Deals
    //             .Where(x => x.Login == (ulong)accountNumber)
    //             .Where(x => x.Time >= from && x.Time <= to)
    //             .Where(x => x.Action == 0 || x.Action == 1 || x.Action == 2)
    //             .AnyAsync();
    //
    //         if (hasTrade) return true;
    //
    //         var hasOpenTrades = await mt5DbContext.Mt5Positions.AnyAsync(x => x.Login == (ulong)accountNumber);
    //         if (hasOpenTrades) return true;
    //
    //         var info = await _tradingApiSvc.GetDailyReport(serviceId, accountNumber, from, to);
    //         if (info.Count > 0 && info[0].Positions.Count > 0)
    //             return true;
    //     }
    //
    //     return false;
    // }

    // private async Task<bool> ShouldSendConfirmationReport(Account account,
    //     AccountReport.CriteriaForPeriod reportCriteria, MetaTrade4DbContext? mt4Ctx = null,
    //     MetaTrade5DbContext? mt5Ctx = null, GenerateDailyReportOptions? options = null)
    // {
    //     if (!_myDbContextPool.IsServiceExisted(account.ServiceId))
    //     {
    //         // _logger.LogError("AccountDailyReport> ShouldSendConfirmationReport> Service not found");
    //         return false;
    //     }
    //     var platform = _myDbContextPool.GetPlatformByServiceId(account.ServiceId);
    //
    //     switch (platform)
    //     {
    //         case PlatformTypes.MetaTrader4:
    //         {
    //             if (mt4Ctx == null)
    //             {
    //                 _logger.LogError("AccountDailyReport> ShouldSendConfirmationReport> MT4 context is null");
    //                 return false;
    //             }
    //             var closedTime = Utils.ParseToUTC("1970-01-01 00:00:00");
    //             var hasTrade = await mt4Ctx.Mt4Trades
    //                 .Where(x => x.Login == account.AccountNumber)
    //                 .Where(x =>
    //                     x.CloseTime == closedTime ||
    //                     (x.CloseTime >= reportCriteria.ClosedTradeCriteria.ClosedFrom &&
    //                      x.CloseTime <= reportCriteria.ClosedTradeCriteria.ClosedTo))
    //                 .Where(x => x.Cmd == 0 || x.Cmd == 1 || x.Cmd == 6)
    //                 .AnyAsync();
    //
    //             if (hasTrade) return true;
    //
    //             if (options != null)
    //             {
    //                 var filePath = $"{options.FileDir}/{account.AccountNumber}_mail.json";
    //
    //                 try
    //                 {
    //                     var json = await File.ReadAllTextAsync(filePath);
    //                     var lists = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json)!;
    //                     if (lists.Any())
    //                     {
    //                         Console.WriteLine($"{account.AccountNumber} has open MT4 trade");
    //                         return true;
    //                     }
    //                 }
    //                 catch
    //                 {
    //                     Console.WriteLine($"Open trade file not found for {account.AccountNumber}");
    //                 }
    //             }
    //             
    //             break;
    //         }
    //         case PlatformTypes.MetaTrader5:
    //         {
    //             if (mt5Ctx == null)
    //             {
    //                 _logger.LogError("AccountDailyReport> ShouldSendConfirmationReport> MT5 context is null");
    //                 return false;
    //             }
    //
    //             var hasClosedTrades = await mt5Ctx.Mt5Deals
    //                 .Where(x => x.Login == (ulong)account.AccountNumber)
    //                 .Where(x => x.Time >= reportCriteria.ClosedTradeCriteria.From &&
    //                             x.Time <= reportCriteria.ClosedTradeCriteria.To)
    //                 .Where(x => x.Action == 0 || x.Action == 1 || x.Action == 2)
    //                 .AnyAsync();
    //             if (hasClosedTrades) return true;
    //
    //             // TODO: remove before production
    //             if (options != null)
    //             {
    //                 var info = await _tradingApiSvc.GetDailyReport(account.ServiceId, account.AccountNumber,
    //                     options.From,
    //                     options.To);
    //                 if (info.Count > 0)
    //                 {
    //                     var dailyReport = info[0];
    //
    //                     var hasOpenTrade = dailyReport.Positions.Count > 0;
    //                     if (hasOpenTrade)
    //                     {
    //                         Console.WriteLine($"{account.AccountNumber} has open MT5 trade");
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 var hasOpenTrades =
    //                     await mt5Ctx.Mt5Positions.AnyAsync(x => x.Login == (ulong)account.AccountNumber);
    //                 if (hasOpenTrades) return true;
    //             }
    //             break;
    //         }
    //     }
    //
    //     return false;
    // }

    public async Task<AccountDailyConfirmationViewModel?> GetAccountDailyReportModelByPathFromS3(string path)
    {
        try
        {
            var fileSteam = await storageService.GetObjectByFilenameAsync(path);
            if (fileSteam == null) return null;

            var json = await new StreamReader(fileSteam).ReadToEndAsync();
            return JsonConvert.DeserializeObject<AccountDailyConfirmationViewModel>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> TryGetAccountDailyReportModelPreviewHtml(long reportId)
    {
        var report = await tenantDbContext.AccountReports
            .Where(x => x.Id == reportId)
            .FirstOrDefaultAsync();
        if (report == null) return null;

        var model = await GetAccountDailyReportModelByPathFromS3(report.DataFile);
        if (model == null) return null;

        return await GenerateReportHtmlForEmail(model, report);
    }

    public static async Task<List<TradeViewModel>> ReadOpenTradeFromFileAsync(string path, long accountNumber)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path);
            var lists = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json)!;
            return lists.Select(x => new TradeViewModel
            {
                AccountNumber = accountNumber,
                OpenAt = Utils.ParseToUTC(x["OpenTime"]),
                Ticket = long.Parse(x["Ticket"].Replace(" ", "")),
                Cmd = x["Type"] == "buy" ? 0 : 1,
                Volume = double.Parse(x["Size"].Replace(" ", "")),
                Symbol = x["Item"].ToUpper(),
                OpenPrice = double.Parse(x["Price"].Replace(" ", "")),
                CurrentPrice = double.Parse(x["Current"].Replace(" ", "")),
                Sl = double.Parse(x["SL"].Replace(" ", "")),
                Tp = double.Parse(x["TP"].Replace(" ", "")),
                Comment = x["Comment"],
                Commission = double.Parse(x["Commission"].Replace(" ", "")),
                Swaps = double.Parse(x["Swap"].Replace(" ", "")),
                Profit = double.Parse(x["PL"].Replace(" ", "")),
            }).ToList();
        }
        catch
        {
            return [];
        }
    }
}

public sealed class GenerateDailyReportOptions
{
    public string FileDir { get; set; } = string.Empty;
    public DateTime From { get; set; }

    public DateTime To => From
        .AddHours(Utils.IsCurrentDSTLosAngeles(From) ? 20 : 21)
        .AddMinutes(59)
        .AddSeconds(59);
}