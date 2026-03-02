using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.Report.Models;
using Bacera.Gateway.Web.Services;
using Bacera.Gateway.Web.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class ReportJob(
    IServiceProvider serviceProvider,
    CentralDbContext centralDbContext,
    ILogger<ReportJob> logger,
    IMyCache myCache)
    : IReportJob
{
    public async Task<List<Tuple<long, string>>> GenerateDailyEquityReport()
    {
        var tenants = await centralDbContext.Tenants.ToListAsync();
        var result = new List<Tuple<long, string>>();
        var date = DateTime.UtcNow;
        foreach (var tenant in tenants)
        {
            var item = await ProcessEquityReportAsync(tenant.Id, date);
            result.Add(Tuple.Create(tenant.Id, item.Item2));
        }

        return result;
    }

    public async Task GenerateAccountDailyConfirmationReport(CancellationToken cancellationToken)
    {
        BcrLog.Slack($"============= GenerateAccountDailyConfirmationReport Start =============");
        // var pool = serviceProvider.GetDbPool();
        // var mt4ServiceIds = await centralDbContext.CentralTradeServices
        //     .Where(x => x.Platform == (int)PlatformTypes.MetaTrader4)
        //     .Select(x => x.Id)
        //     .ToListAsync(cancellationToken: cancellationToken);
        //
        // foreach (var serviceId in mt4ServiceIds)
        // {
        //     const string sql = "insert into MT4_OPENTRADES (select * from MT4_TRADES where CLOSE_TIME = '1970-01-01 00:00:00')";
        //     await using var mt4Ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
        //     await mt4Ctx.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        // }
        
        var startTime = DateTime.UtcNow;
        var tenantIds = await centralDbContext.Tenants
            .Select(x => x.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        // var fromStr = "2025-03-31";
        // var from = Utils.ParseToUTC(fromStr);

        var from = DateTime.UtcNow.Date;

        var tasks = tenantIds.Select(x => ProcessAccountDailyConfirmationReport(x, cancellationToken, from));
        await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var durationInMinutes = (endTime - startTime).TotalMinutes;
        BcrLog.Slack($"============= GenerateAccountDailyConfirmationReport End, Duration in minutes: {durationInMinutes} =============");
    }

    public async Task<Tuple<long, string>> GenerateDailyEquityReportForTenant(long tenantId,
        long rowId, List<string>? mailTos = null, DateTime? date = null)
    {
        date ??= DateTime.UtcNow;
        var item = await ProcessEquityReportAsync(tenantId, date.Value, mailTos, rowId);
        return Tuple.Create(tenantId, item.Item2);
    }

    public async Task<bool> ProcessReportRequest(long tenantId, long requestId)
    {
        using var scope = serviceProvider.CreateTenantScope(tenantId);
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var wsMessageProcessor = scope.ServiceProvider.GetRequiredService<WsMessageProcessor>();

        var request = await tenantCtx.ReportRequests.SingleOrDefaultAsync(x => x.Id == requestId);
        if (request == null) throw new Exception("__REPORT_REQUEST_NOT_FOUND__");
        var (result, _) = await reportSvc.ProcessRequestAsync(request);
        if (!result) return result;

        var operatorPartyGroup = NotificationHub.GetPartyGroupName(tenantId, request.PartyId);
        var messagePopup = MessagePopupDTO.BuildInfo("Report Process Finished", request.Name);
        wsMessageProcessor.AddMessage(WsSendDTO.Build("SendMessageToGroup", [operatorPartyGroup, "ReceivePopup", messagePopup.ToJson()]));
        return result;
    }


    public async Task ExecuteCloseTradeJobAsync()
    {
        // *** MT4-specific position snapshot not needed for MT5 architecture ***
        // MT5 uses dedicated mt5_positions table for open positions
        //await GenerateMT4CurrentPositionSnapshotAsync();
        //await Task.Delay(5 * 60 * 1000);
        await GenerateCloseTradeReportAsync();
    }

    private async Task GenerateMT4CurrentPositionSnapshotAsync()
    {
        var pool = serviceProvider.GetDbPool();
        var mt4ServiceIds = await centralDbContext.CentralTradeServices
            .Where(x => x.Platform == (int)PlatformTypes.MetaTrader4)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var serviceId in mt4ServiceIds)
        {
            const string sql = """
                               delete from MT4_OPENTRADES; 
                               insert into MT4_OPENTRADES (select * from MT4_TRADES where CLOSE_TIME = '1970-01-01 00:00:00');
                               """;
            await using var mt4Ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
            await mt4Ctx.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private Task GenerateCloseTradeReportAsync() => Task.WhenAll(serviceProvider.GetDbPool().GetTenantIds()
        .Where(x => new long[] { 10000, 10004 }.Contains(x))
        .Select(async tenantId =>
        {
            try
            {
                logger.LogInformation("GenerateCloseTradeReport: Starting processing for tenant {TenantId}", tenantId);
                var utcNow = DateTime.UtcNow.Date;

                using var outerScope = serviceProvider.CreateTenantScope(tenantId);
                var tenantCtx = outerScope.ServiceProvider.GetRequiredService<TenantDbContext>();
                var configSvc = outerScope.ServiceProvider.GetRequiredService<ConfigService>();
                var hoursGap = await configSvc.GetHoursGapForMT5Async();

                var date = utcNow.Date
                    .AddHours(Utils.IsCurrentDSTLosAngeles(utcNow) ? 20 : 21) // 20 : 21
                    .AddHours(hoursGap) // MT5 GMT+2 (configurable)
                    .AddMinutes(59)
                    .AddSeconds(59);
                var walletIds = await tenantCtx.Wallets
                    //最优先判定的是否有余额，如果有余额，Closed也要取
                    //.Where(x => x.Party.Status == (int)PartyStatusTypes.Active)
                    .Where(x => x.Balance != 0)
                    .Select(x => x.Id)
                    .ToListAsync();

                const int threadCount = 10;
                var countPerThread = (int)Math.Ceiling(walletIds.Count / (double)threadCount);
                await Task.WhenAll(Enumerable.Range(0, threadCount).Select(async i =>
                {
                    var ids = walletIds.Skip(i * countPerThread).Take(countPerThread).ToList();
                    using var scope = serviceProvider.CreateTenantScope(tenantId);
                    var acctSvc = scope.ServiceProvider.GetRequiredService<AcctService>();
                    foreach (var walletId in ids)
                    {
                        await acctSvc.GenerateWalletDailySnapshotAsync(walletId, date, true);
                    }
                }));

                // Find a valid party ID for this tenant instead of hardcoding Party ID 1
                var validPartyId = await tenantCtx.Parties
                    .Where(p => p.Status == (int)PartyStatusTypes.Active)
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync();
                    
                if (validPartyId == 0)
                {
                    logger.LogWarning("GenerateCloseTradeReport: No active party found in tenant {TenantId}, skipping", tenantId);
                    BcrLog.Slack($"GenerateCloseTradeReport: No active party found in tenant {tenantId}, skipping");
                    return; // Skip this tenant, but continue with other tenants
                }
                
                logger.LogInformation("GenerateCloseTradeReport: Found valid party {PartyId} for tenant {TenantId}, proceeding with report generation", validPartyId, tenantId);

                // 确保日期时间为 UTC 格式（两个版本共享）
                var fromDateUtc = DateTime.SpecifyKind(date.AddDays(-1), DateTimeKind.Utc);
                var toDateUtc = DateTime.SpecifyKind(date, DateTimeKind.Utc);

                var requests = new List<ReportRequest>
                {
                // WalletDailySnapshot Version 1: Snapshot Table (22:00 GMT+2) - Original
                ReportRequest.Build(validPartyId
                    , ReportRequestTypes.WalletDailySnapshot
                    , $"Wallet Daily Snapshot (UTC Time Based) {date:yyyy-MM-dd}"
                    , JsonConvert.SerializeObject(new { snapshotDate = date, useMT5Time = false })),

                // WalletDailySnapshot Version 2: Real-Time Balance (23:59:59 GMT+2) - New
                ReportRequest.Build(validPartyId
                    , ReportRequestTypes.WalletDailySnapshot
                    , $"Wallet Daily Snapshot (MT5 Time Based) {date:yyyy-MM-dd}"
                    , JsonConvert.SerializeObject(new { snapshotDate = date, useMT5Time = true })),

                    // WalletTransactionForTenant 版本1：基于 MT5 ClosingTime（关仓时间）- Job入口，IsFromApi=0（默认）
                    new ReportRequest
                    {
                        PartyId = validPartyId,
                        Type = (int)ReportRequestTypes.WalletTransactionForTenant,
                        Name = $"Wallet Daily Transaction (MT5 ClosingTime Based) {date:yyyy-MM-dd}",
                        Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                        IsFromApi = 0 // 默认值，但明确设置
                    },

                    // WalletTransactionForTenant 版本2：基于发放时间（ReleasedTime）- 设置为API入口，IsFromApi=1
                    new ReportRequest
                    {
                        PartyId = validPartyId,
                        Type = (int)ReportRequestTypes.WalletTransactionForTenant,
                        Name = $"Wallet Daily Transaction (ReleasedTime Based) {date:yyyy-MM-dd}",
                        Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                        IsFromApi = 1 // 标记为API入口，基于StatedOn分发时间
                    },

                    // Rebate 版本1：基于 MT5 ClosingTime（关仓时间）- Job入口，IsFromApi=0（默认）
                    ReportRequest.Build(validPartyId
                        , ReportRequestTypes.Rebate
                        , $"Rebate Daily Record (MT5 ClosingTime Based) {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings)),
                    
                    // Rebate 版本2：基于发放时间（ReleasedTime）- 设置为API入口，IsFromApi=1
                    ReportRequest.Build(validPartyId
                        , ReportRequestTypes.Rebate
                        , $"Rebate Daily Record (ReleasedTime Based) {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings)
                        , 1), // 标记为API入口，基于StatedOn分发时间
                    
                    // Daily Equity 版本1：基于 ClosedTime（关仓时间）- Job入口，IsFromApi=0（默认）
                    ReportRequest.Build(validPartyId
                        , ReportRequestTypes.DailyEquity
                        , $"Daily Equity Report (MT5 ClosingTime Based) {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings)),
                    
                    // Daily Equity 版本2：基于发放时间（ReleasedTime）- 设置为API入口，IsFromApi=1
                    ReportRequest.Build(validPartyId
                        , ReportRequestTypes.DailyEquity
                        , $"Daily Equity Report (ReleasedTime Based) {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings)
                        , 1) // 标记为API入口，基于StatedOn分发时间
                };

                // check if it is Tuesday
                if (date.DayOfWeek == DayOfWeek.Tuesday)
                {
                    // Daily Equity Report for last 3 days (Saturday + Sunday + Monday)
                    var fromDate3Day = DateTime.SpecifyKind(date.AddDays(-4), DateTimeKind.Utc); // Friday 22:00 GMT+0
                    var toDate3Day = DateTime.SpecifyKind(date.AddDays(-1), DateTimeKind.Utc);   // Monday 22:00 GMT+0
                    
                    // Daily Equity 版本1：基于 ClosedTime（关仓时间）- Job入口，IsFromApi=0（默认）
                    requests.Add(ReportRequest.Build(validPartyId
                        , ReportRequestTypes.DailyEquity
                        , $"Daily Equity Report (Sat-Mon) (MT5 ClosingTime Based) {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new { from = fromDate3Day, to = toDate3Day }, Utils.AppJsonSerializerSettings)));

                    // Daily Equity 版本2：基于发放时间（ReleasedTime）- 设置为API入口，IsFromApi=1
                    requests.Add(ReportRequest.Build(validPartyId
                        , ReportRequestTypes.DailyEquity
                        , $"Daily Equity Report (Sat-Mon) (ReleasedTime Based) {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new { from = fromDate3Day, to = toDate3Day }, Utils.AppJsonSerializerSettings)
                        , 1)); // 标记为API入口，基于StatedOn分发时间
                }

                // check if it is Friday
                if (date.DayOfWeek == DayOfWeek.Friday)
                {
                    requests.Add(ReportRequest.Build(validPartyId
                        , ReportRequestTypes.DemoAccount
                        , $"Demo Account Report {date:yyyy-MM-dd}"
                        , JsonConvert.SerializeObject(new DemoAccountRecord.QueryCriteria { Date = date })));
                }

                if (date.AddDays(1).Day == 1)
                {
                    var lastMonthEnd = new DateTime(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second, DateTimeKind.Utc)
                        .AddDays(-1);

                    requests.Add(ReportRequest.Build(validPartyId
                        , ReportRequestTypes.WalletTransactionForTenant
                        , $"Wallet Monthly Transaction {date:yyyy-MM}"
                        , JsonConvert.SerializeObject(new { from = lastMonthEnd, to = date })));

                    requests.Add(ReportRequest.Build(validPartyId
                        , ReportRequestTypes.SalesRebateForTenant
                        , $"Sales Rebate Monthly Record {date:yyyy-MM}"
                        , JsonConvert.SerializeObject(new { from = lastMonthEnd, to = date, IsFromDirectClient = true })));

                    requests.Add(ReportRequest.Build(validPartyId
                        , ReportRequestTypes.Rebate
                        , $"Rebate Monthly Record {date:yyyy-MM}"
                        , JsonConvert.SerializeObject(new { from = lastMonthEnd, to = date })));
                }

                tenantCtx.ReportRequests.AddRange(requests);
                await tenantCtx.SaveChangesAsync();

                await Task.WhenAll(requests.Select(async request =>
                {
                    using var scope = serviceProvider.CreateTenantScope(tenantId);
                    var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
                    //await reportSvc.ProcessRequestAsync(request);
                    try
                    {
                        await reportSvc.ProcessRequestAsync(request);
                    }
                    catch (Exception e)
                    {
                        BcrLog.Slack(
                            $"GenerateCloseTradeReport Error:_{e.Message}_tid:{tenantId}_reportRequestId_{request.Id}_reportName_{request.Name}");
                    }
                }));
                
                logger.LogInformation("GenerateCloseTradeReport: Completed processing for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                // Log error but don't stop other tenants from processing
                logger.LogError(ex, "GenerateCloseTradeReport Error for tenant {TenantId}: {Message}", tenantId, ex.Message);
                BcrLog.Slack($"GenerateCloseTradeReport Error for tenant {tenantId}: {ex.Message}");
            }
        }));

    public async Task ProcessAccountDailyConfirmationReport(long tenantId, CancellationToken token,
        DateTime? from = null)
    {
        var tasks = new List<Task>
        {
            GenerateAccountTask(tenantId, token, from),
            ProcessAccountReportTask(tenantId, token, from),
            SendMailTask(tenantId, token, from)
        };
        await Task.WhenAll(tasks);
    }

    public static string GenerateAccountTaskKey(long tenantId, DateTime date) =>
        $"generate_account_task_tid:{tenantId}_date:{date:yyyy-MM-dd}";

    public const string TodayKey = "date_account_daily_report";

    public async Task GenerateAccountTask(long tenantId, CancellationToken token, DateTime? from)
    {
        logger.LogInformation("AccountDailyReport> Generate Account Task Start {TenantId}", tenantId);
        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();

        if (from == null)
        {
            var todayStr = await myCache.GetStringAsync(TodayKey);
            if (todayStr == null)
            {
                todayStr = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                await myCache.SetStringAsync(TodayKey, todayStr, TimeSpan.FromHours(10));
            }

            from = Utils.ParseToUTC(todayStr);
        }
        await myCache.SetStringAsync(GenerateAccountTaskKey(tenantId, from.Value), "Started", TimeSpan.FromHours(10));
        var to = from.Value
            .AddHours(Utils.IsCurrentDSTLosAngeles(from.Value) ? 20 : 21)
            .AddMinutes(59)
            .AddSeconds(59);

        if (to > DateTime.UtcNow)
        {
            await myCache.SetStringAsync(GenerateAccountTaskKey(tenantId, from.Value), "Ended", TimeSpan.FromHours(10));
            return;
        }

        await reportSvc.GenerateAccountToSendConfirmationReport(tenantId, from.Value, to);
        await myCache.SetStringAsync(GenerateAccountTaskKey(tenantId, from.Value), "Ended", TimeSpan.FromHours(10));
    }

    private const int AccountReportDelay = 10 * 1000; // 10 seconds

    public static string ProcessAccountReportTaskKey(long tenantId, DateTime date) =>
        $"process_account_report_task_tid:{tenantId}_date:{date:yyyy-MM-dd}";

    public async Task ProcessAccountReportTask(long tenantId, CancellationToken token, DateTime? from)
    {
        // _logger.LogInformation("AccountDailyReport> ProcessAccountReportTask Start {TenantId}", tenant.Id);
        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();

        var date = DateTime.UtcNow.Date;
        while (!token.IsCancellationRequested)
        {
            if (from == null)
            {
                var dateStr = await myCache.GetStringAsync(TodayKey);
                if (dateStr == null) continue;
                date = Utils.ParseToUTC(dateStr);
            }
            else
            {
                date = from.Value;
            }
            await myCache.SetStringAsync(ProcessAccountReportTaskKey(tenantId, date), "Started", TimeSpan.FromHours(10));
            // await Task.Delay(AccountReportDelay, token);
            var remains = await reportSvc.ProcessAccountReportModel(date);

            var isCompleted = await myCache.GetStringAsync(GenerateAccountTaskKey(tenantId, date)) == "Ended";
            if (!isCompleted || remains != 0) continue;

            await myCache.SetStringAsync(ProcessAccountReportTaskKey(tenantId, date), "Ended",
                TimeSpan.FromHours(10));
            break;
        }

        await myCache.SetStringAsync(ProcessAccountReportTaskKey(tenantId, date), "Ended", TimeSpan.FromHours(10));
    }


    public static string SendMailTaskKey(long tenantId, DateTime date) =>
        $"send_email_task_tid:{tenantId}_date:{date:yyyy-MM-dd}";

    private async Task SendMailTask(long tenantId, CancellationToken token, DateTime? from)
    {
        // _logger.LogInformation("AccountDailyReport>SendMailTask> SendMailTask Start {tenantId}", tenant.Id);
        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
        var date = DateTime.UtcNow.Date;
        while (!token.IsCancellationRequested)
        {
            if (from == null)
            {
                var dateStr = await myCache.GetStringAsync(TodayKey);
                if (dateStr == null) continue;

                date = Utils.ParseToUTC(dateStr);
            }
            else
            {
                date = from.Value;
            }
            await myCache.SetStringAsync(SendMailTaskKey(tenantId, date), "Started", TimeSpan.FromHours(10));

            await Task.Delay(AccountReportDelay, token);
            var remains = await reportSvc.ProcessSendAccountDailyReportEmail(date);
          
            var isCompleted = await myCache.GetStringAsync(GenerateAccountTaskKey(tenantId, date)) == "Ended"
                              && await myCache.GetStringAsync(ProcessAccountReportTaskKey(tenantId, date)) == "Ended";
            if (isCompleted && remains == 0)
                break;
        }

        await myCache.SetStringAsync(SendMailTaskKey(tenantId, date), "Ended", TimeSpan.FromHours(10));
        // _logger.LogInformation("AccountDailyReport>SendMailTask> SendMailTask End");
    }

    // private async Task<Tuple<bool, string>> ProcessAccountDailyConfirmationReport2(long tenantId,
    //     bool generateWhenEmpty = false)
    // {
    //     var tenant = await _centralDbContext.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId);
    //     if (tenant == null)
    //         return Tuple.Create(false, "__TENANT_NOT_FOUND__");
    //
    //     using var scope = _serviceProvider.CreateScope();
    //     var s = scope.ServiceProvider.GetRequiredService<TenancyResolver>();
    //     s.SetTenant(tenant);
    //
    //     var configurationSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
    //     if (true != await configurationSvc.GetAccountDailyReportToggleSwitchAsync())
    //         return Tuple.Create(false, "");
    //
    //     var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
    //     var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    //     var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
    //
    //     var sentAccountNumbers = new List<long>();
    //     await foreach (var report in reportSvc.GenerateAccountDailyConfirmationReport(null, generateWhenEmpty,
    //                        tenant.Id))
    //     {
    //         _logger.LogInformation("Report generated: {report}", report);
    //         var user = await authDbContext.Users.Where(x => x.PartyId == report.PartyId).FirstOrDefaultAsync();
    //         if (user == null) continue;
    //
    //
    //         report.Email = user.Email ?? string.Empty;
    //         // report.Email = "dealing@thebcr.com";
    //         // report.BccEmails = new List<string> { "jiehe@thebcr.com", "report@" };
    //         report.NativeName = user.GuessUserNativeName();
    //         _logger.LogInformation(
    //             "Prepare to send account daily confirmation report to partyId: {PartyId} email:{Email}",
    //             user.PartyId,
    //             report.Email);
    //         backgroundJobClient.Enqueue<ISendMailJob>(x =>
    //             x.SendAndSaveAccountActivityReportAsync(tenantId, report, user.Language));
    //
    //         sentAccountNumbers.Add(report.AccountNumber);
    //     }
    //
    //     return Tuple.Create(true, sentAccountNumbers.Any() ? string.Join(",", sentAccountNumbers) : "");
    // }


    public async Task<Tuple<bool, string>> ProcessEquityReportAsync(long tenantId, DateTime dateTime,
        List<string>? mailTos = null, long? rowId = null)
    {
        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var configurationSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();

        var reportConfigurations = await GetReportConfiguration(tenantCtx, logger, rowId);
        if (!reportConfigurations.Any())
            return Tuple.Create(false, "__REPORT_NOT_FOUND__");

        var sentReports = new List<string>();
        foreach (var reportCfg in reportConfigurations)
        {
            var options = await GetMt4DbOptions(tenantCtx, reportCfg.ServiceId);
            if (options == null)
                return Tuple.Create(false, "__MT4_DATABASE_NOT_FOUND__");

            await using var mt4DbCtx = new MetaTrade4DbContext(options);

            reportCfg.From = dateTime;
            reportCfg.To = reportCfg.From.AddDays(1);

            // var dailyReport = await reportSvc.GenerateMT4EquityDailyReport(mt4DbCtx, reportCfg);
            // var monthlyReport = await reportSvc.GenerateMT4EquityMonthlyReport(mt4DbCtx, reportCfg);
            //
            // var title = reportCfg.Name + " " + reportCfg.From.ToString("yyyy-MM-dd");
            // var html = reportSvc.GenerateMT4EquityReportHtml(title, dailyReport, monthlyReport);
            var html = await GenerateEquityReportHtmlAsync(reportSvc, reportCfg);

            var emailTitle = reportCfg.From.ToString("yyyy-MM-dd") + " " + reportCfg.Name + " Daily Equity Report";
            var emailAddress = await configurationSvc.GetDefaultEmailAddressAsync();
            var emailDisplayName = await configurationSvc.GetDefaultEmailDisplayNameAsync();

            await sendMailSvc.SendEmailAsync(mailTos ?? reportCfg.MailTo, emailTitle, html,
                emailAddress, emailDisplayName,
                mailTos == null ? reportCfg.MailCc : null);
            sentReports.Add(reportCfg.Name);
        }

        return Tuple.Create(true, string.Join(",", sentReports));
    }

    public static async Task<string> GenerateEquityReportHtmlAsync(ReportService reportSvc,
        ReportConfiguration reportCfg)
    {
        var dailyReportTask = reportSvc.GenerateMT4EquityDailyReport(reportCfg.ServiceId, reportCfg);
        var monthlyReportTask = reportSvc.GenerateMT4EquityMonthlyReport(reportCfg.ServiceId, reportCfg);
        var reports = await Task.WhenAll(dailyReportTask, monthlyReportTask);
        var title = reportCfg.Name + " " + reportCfg.From.ToString("yyyy-MM-dd");
        var html = reportSvc.GenerateMT4EquityReportHtml(title, reports[0], reports[1]);
        return html;
    }

    private static async Task<List<ReportConfiguration>> GetReportConfiguration(TenantDbContext ctx,
        ILogger logger, long? rowId = null)
    {
        var query = ctx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DailyEquityReport);
        if (rowId.HasValue)
            query = query.Where(x => x.RowId == rowId.Value);

        var supplements = await query.ToListAsync();
        var reportConfigurations = new List<ReportConfiguration>();
        foreach (var supplement in supplements)
        {
            try
            {
                var item = JsonConvert.DeserializeObject<ReportConfiguration>(supplement.Data);
                if (item != null && item.MailTo.Any() && item.Items.Any() && item.ServiceId > 0)
                    reportConfigurations.Add(item);
            }
            catch (Exception e)
            {
                logger.LogError("[{Id}]Report configuration deserializeObject error: {Message}", supplement.Id,
                    e.Message);
            }
        }

        return reportConfigurations;
    }

    private static async Task<DbContextOptions<MetaTrade4DbContext>?> GetMt4DbOptions(TenantDbContext ctx,
        long tradeServiceId)
    {
        var mt4Services = await ctx.TradeServices
            .Where(x => x.Platform == (int)PlatformTypes.MetaTrader4)
            .Where(x => x.Id == tradeServiceId)
            .FirstOrDefaultAsync();
        if (mt4Services == null)
            return null;

        var traderServiceOptions = mt4Services.GetOptions<TradeServiceOptions>();
        if (!traderServiceOptions.IsDatabaseValidated())
            return null;

        var options = new DbContextOptionsBuilder<MetaTrade4DbContext>();
        options.UseMySql(traderServiceOptions.Database!.ConnectionString, ServerVersion.Parse("5.7.38-mysql"));
        return options.Options;
    }
}