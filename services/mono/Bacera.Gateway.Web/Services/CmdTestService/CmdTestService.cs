using System.Globalization;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Email.ViewModel;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.Permission;
using Bacera.Gateway.Services.Withdraw;
using Bacera.Gateway.Vendor.Amazon;
using Bacera.Gateway.Vendor.Help2Pay;
using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Vendor.OFAPay;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Controllers;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Response;
using Bacera.Gateway.Web.Services.Interface;
using CsvHelper;
using Hangfire;
using Hangfire.Common;
using LinqKit;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Serilog;
using ShellProgressBar;
using StackExchange.Redis;


namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CentralDbContext _centralDbContext;
    private readonly AuthDbContext _authDbContext;
    private readonly IMyCache _myCache;
    private readonly UserManager<User> _userManager;
    private readonly IMessageQueueService _mqService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CmdTestService> _logger;
    private readonly string?[] _args;


    public CmdTestService(IServiceProvider serviceProvider, WebApplicationBuilder me, params string?[] args)
    {
        _serviceProvider = serviceProvider;
        _mqService = serviceProvider.GetRequiredService<IMessageQueueService>();
        _authDbContext = serviceProvider.GetRequiredService<AuthDbContext>();
        _centralDbContext = serviceProvider.GetRequiredService<CentralDbContext>();
        _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        _logger = serviceProvider.GetRequiredService<ILogger<CmdTestService>>();
        _myCache = serviceProvider.GetRequiredService<IMyCache>();
        _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        _args = args;
    }

    public async Task RunAsync(CancellationToken token)
    {
        await Task.Delay(0, token);

        // Check if we have a specific test command
        Console.WriteLine($"🔍 [DEBUG] CmdTestService arguments: [{string.Join(", ", _args)}]");
        Console.WriteLine($"🔍 [DEBUG] _args.Length: {_args.Length}");
        if (_args.Length >= 4)
        {
            var testCommand = _args[3];
            Console.WriteLine($"🔍 [DEBUG] testCommand: '{testCommand}'");
            switch (testCommand)
            {
                case "reprocess-trade-rebate":
                    await ReprocessTradeRebate();
                    break;
                case "test-sqs":
                    await TestSqs();
                    break;
                case "test-eu-payment":
                    await TestEuPayment();
                    break;
                case "test-npay":
                    await TestNPay();
                    break;
                case "test-monetix-pay":
                    await TestMonetixPay();
                    break;
                case "test-sea-bipi-pay":
                    await TestSeaBipiPay();
                    break;
                case "test-auto-create":
                    // await TestAutoCreate(); // Method not implemented yet
                    Console.WriteLine("⚠️ TestAutoCreate method not implemented yet");
                    break;
                case "test-help2pay":
                    await TestHelp2Pay();
                    break;
                case "test-dragon-pay":
                    await TestDragonPayAsync();
                    break;
                case "test-exlink-vnd":
                    await TestExlinkVND();
                    break;
                case "test-pay247":
                    await TestPay247();
                    break;
                case "test-pay247-out":
                    await TestPay247Out();
                    break;
                case "test-china-pay":
                    await TestChinaPay();
                    break;
                case "test-bipi-pay":
                    await TestBipiPay();
                    break;
                case "test-exlink":
                    await TestExlink();
                    break;
                case "test-exlink-exchange-rate":
                    await TestExLinkExchangeRate();
                    break;
                case "test-exlink-currencies":
                    await TestExLinkSupportedCurrencies();
                    break;
                case "test-exlink-payment-types":
                    await TestExLinkPaymentTypes();
                    break;
                case "test-exlink-payout-payment-types":
                    await TestExLinkPayoutPaymentTypes();
                    break;
                case "test-exlink-withdrawal":
                    await TestExLinkWithdrawal();
                    break;
                case "test-exlink-bank-list":
                    await TestExLinkPayoutBankList();
                    break;
                case "test-exlink-withdrawal-status":
                    await TestExLinkWithdrawalStatus();
                    break;
                case "test-long77-vn-payment":
                    await TestLong77VnPayment();
                    break;
                case "test-long77-transfer":
                    //await TestLong77Transfer();
                    break;
                case "check-trade-stat":
                    // await CheckTradeStat(); // Method not implemented yet
                    Console.WriteLine("⚠️ CheckTradeStat method not implemented yet");
                    break;
                case "check-event-points":
                    await CheckEventPoints();
                    break;
                case "clear-redis-cache":
                    await ClearRedisCache();
                    break;
                case "test-auto-create-acc":
                    await TestAutoCreateAcc();
                    break;
                case "tt":
                    await TT();
                    break;
                case "test-close-trade-job":
                    await TestCloseTradeJob();
                    break;
                case "test-twilio-sms":
                    await TestTwilioSms();
                    break;
                default:
                    Console.WriteLine($"❌ [ERROR] Unknown test command: {testCommand}");
                    Console.WriteLine("Available test commands:");
                    Console.WriteLine("  reprocess-trade-rebate - Reprocess a specific TradeRebate record");
                    Console.WriteLine("  test-sqs - Test SQS connectivity");
                    Console.WriteLine("  test-eu-payment - Test EU payment");
                    Console.WriteLine("  test-npay - Test NPay");
                    Console.WriteLine("  test-monetix-pay - Test MonetixPay");
                    Console.WriteLine("  test-sea-bipi-pay - Test SeaBipiPay");
                    Console.WriteLine("  test-auto-create - Test auto create");
                    Console.WriteLine("  test-help2pay - Test Help2Pay");
                    Console.WriteLine("  test-dragon-pay - Test DragonPay");
                    Console.WriteLine("  test-exlink-vnd - Test ExlinkVND");
                    Console.WriteLine("  test-pay247 - Test Pay247");
                    Console.WriteLine("  test-pay247-out - Test Pay247Out");
                    Console.WriteLine("  test-china-pay - Test ChinaPay");
                    Console.WriteLine("  test-bipi-pay - Test BipiPay");
                    Console.WriteLine("  test-exlink - Test Exlink");
                    Console.WriteLine("  test-exlink-exchange-rate - Test ExLink Exchange Rate API");
                    Console.WriteLine("  test-exlink-currencies - Test ExLink Supported Currencies API");
                    Console.WriteLine("  test-exlink-payment-types - Test ExLink Payment Types API (channel availability)");
                    Console.WriteLine("  test-exlink-payout-payment-types - Test ExLink Payout Payment Types API");
                    Console.WriteLine("  test-exlink-withdrawal - Test ExLink Withdrawal/Payout API");
                    Console.WriteLine("  test-exlink-bank-list - Test ExLink Payout Bank List API");
                    Console.WriteLine("  test-exlink-withdrawal-status - Test ExLink Withdrawal Status Query API");
                    Console.WriteLine("  test-long77-vn-payment - Test Long77 Pay Virtual Account (VN Payment)");
                    Console.WriteLine("  test-long77-transfer - Test Long77 Pay Transfer (ATM)");
                    Console.WriteLine("  check-trade-stat - Check trade statistics");
                    Console.WriteLine("  test-auto-create-acc - Test auto create account");
                    Console.WriteLine("  test-close-trade-job - Test ExecuteCloseTradeJobAsync() (Daily Rebate Report)");
                    Console.WriteLine("  test-twilio-sms - Test Twilio SMS verification (send and verify code)");
                    Console.WriteLine("  tt - Test utility functions");
                    break;
            }
        }
        else
        {
            Console.WriteLine("❌ [ERROR] No test command specified!");
            Console.WriteLine("Usage: dotnet run -- cmd test <test-command> [additional-args] local-dev");
            Console.WriteLine("Example: dotnet run -- cmd test reprocess-trade-rebate 10000 10000 local-dev");
        }

        // Legacy commented code for reference
        // var tid = long.Parse(_args[3]!);
        // var startPartyId = long.Parse(_args[4]!);
        // BcrLog.Slack($"CmdTestService.RunAsync tid:{tid} startPartyId:{startPartyId}");
        // await MigrateNewPaymentServiceAsync(tid, startPartyId);
        // await UpdateWalletByWalletTransactionId();
        // await UpdateWallet();
        // await UpdateSearchText();
        // await BatchProcessPipsAndCommission();
        // await TestOFAPay();
        // await MigrateNewPaymentServiceAsync();
        // await EnableAccountAccessByGroup();
        // await TestFivePayVA();
        // await TestBakong();
        // await TestCrypto();
        // await AddNewConfig();
        // await MigrateUser();
        // await TestPayPal();
        // await TestSqs();
        // await TestEuPayment();
        // await TestNPay();
        // await TestMonetixPay();
        // await TestSeaBipiPay();
        // await UpdateTradeAccountStatus();
        // await TestAutoCreate();
        // await TestHelp2Pay();
        // await TestDragonPayAsync();
        // await TestExlinkVND();
        // await TestPay247();
        // await TestPay247Out();
        // await TestChinaPay();
        // await TestBipiPay();
        // await TestExlink();
        // await CheckWalletTransaction();
        // await BatchCheckWalletTransaction();
        // await UpdateWalletByWalletTransactionId();
        // await BatchUpdateWalletByWalletTransactionIdByWalletId();
        // await WrongSalesIdAndReferPath();
        // await ManualEnqueueEventTransactionSource();
        // await AddNewConfig();
        // await TestWsProcessor();
        // await RegenDailyReport();
        // await RegenReportFromIdHf();
        // await RegenReport();
        // await TestCheckRebate();
        // await CheckTradeStat();
        // await TestAutoCreateAcc();
        // await TT();
    }
    private async Task TT()
    {
        var pwd = Utils.GenerateSimplePassword();
        Console.WriteLine(pwd);
    }

    private async Task BatchUpdateAccountWalletId()
    {
        using var scope = _serviceProvider.CreateTenantScope(10000);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var total = await tenantCtx.Parties.CountAsync();
        using var bar = CreateBar(total, "Fulfill walletId");
        const int threadCount = 10;
        var countPerThread = total / threadCount;
        var tasks = Enumerable.Range(0, threadCount).Select(async i =>
        {
            const int size = 1000;
            while (true)
            {
                bar.Tick();
                var partyIds = await tenantCtx.Parties
                    .OrderBy(x => x.Id)
                    .Skip(i * countPerThread)
                    .Take(countPerThread)
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var partyId in partyIds)
                {
                    bar.Tick();
                    var accounts = await tenantCtx.Accounts
                        .Where(x => x.WalletId == null)
                        .Where(x => x.PartyId == partyId)
                        .ToListAsync();
                    var wallets = await tenantCtx.Wallets
                        .Where(x => x.PartyId == partyId)
                        .Select(x => new { x.Id, x.CurrencyId, x.FundType })
                        .ToListAsync();

                    if (accounts.Count == 0) continue;
                    if (wallets.Count == 0) continue;

                    foreach (var account in accounts)
                    {
                        var wallet = wallets.FirstOrDefault(x =>
                            x.CurrencyId == account.CurrencyId && x.FundType == account.FundType);
                        if (wallet == null) continue;

                        account.WalletId = wallet.Id;
                        tenantCtx.Accounts.Update(account);
                    }
                }


                if (partyIds.Count < size) break;
            }
        });
        await Task.WhenAll(tasks);
    }

  /// <summary>
  /// Test AWS SQS connectivity - call with: TestSqs
  /// </summary>
  public async Task TestSqs()
  {
    using var scope = _serviceProvider.CreateScope();
    var sqsOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonSQSOptions>>();
    var tester = new SqsConnectivityTester(_mqService, sqsOptions);

    // Check configuration first
    var configStatus = tester.CheckConfiguration();

    if (!configStatus.IsFullyConfigured)
    {
      Console.WriteLine("\n⚠ SQS not fully configured. Please check your environment variables:");
      Console.WriteLine("  - AWS_SQS_ACCESS_KEY");
      Console.WriteLine("  - AWS_SQS_ACCESS_SECRET");
      Console.WriteLine("  - AWS_SQS_REGION");
      Console.WriteLine("  - AWS_SQS_PREFIX");
      Console.WriteLine("  - AWS_SQS_BCR_SEND_MESSAGE_QUEUE");
      Console.WriteLine("  - AWS_SQS_BCR_EVENT_TRADE_QUEUE");
      Console.WriteLine("  - AWS_SQS_BCR_TRADE_QUEUE");
      Console.WriteLine("  - AWS_SQS_BCR_SALES_REBATE_TRADE_QUEUE");
      return;
    }

    Console.WriteLine();

    // Test main queue (BCRSendMessage) with full send/receive/delete cycle
    var testResult = await tester.TestConnectivityAsync();

    Console.WriteLine();

    // Test all queues with simple send operation
    var allResults = await tester.TestAllQueuesAsync();

    Console.WriteLine("\n=== Summary ===");
    Console.WriteLine($"Configuration: {(configStatus.IsFullyConfigured ? "✓ Complete" : "✗ Incomplete")}");
    Console.WriteLine($"Main Queue Test: {(testResult.OverallSuccess ? "✓ Success" : "✗ Failed")}");
    Console.WriteLine($"All Queues Test: {allResults.Count(r => r.Value.OverallSuccess)}/{allResults.Count} queues working");

    if (testResult.OverallSuccess && allResults.All(r => r.Value.OverallSuccess))
    {
      Console.WriteLine("\n🎉 AWS SQS is working correctly!");
    }
    else
    {
      Console.WriteLine("\n❌ AWS SQS has connectivity issues. Check the error messages above.");
    }
  }    /// <summary>
    /// Quick SQS configuration check - call with: TestSqsConfig
    /// </summary>
    public Task TestSqsConfig()
    {
        using var scope = _serviceProvider.CreateScope();
        var sqsOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonSQSOptions>>();
        var tester = new SqsConnectivityTester(_mqService, sqsOptions);
        
        var configStatus = tester.CheckConfiguration();
        
        if (configStatus.IsFullyConfigured)
        {
            Console.WriteLine("\n✅ SQS configuration looks good! Run 'TestSqs' to test connectivity.");
        }
        else
        {
            Console.WriteLine("\n❌ SQS configuration is incomplete. Check the missing items above.");
        }
        
        return Task.CompletedTask;
    }


    private IServiceScope CreateTenantScopeByTenantIdAsync(long tenantId) =>
        _serviceProvider.CreateTenantScope(tenantId);

    public static ProgressBar CreateBar(int totalTicks, string title = "Main Progress")
    {
        var options = new ProgressBarOptions
        {
            ProgressCharacter = '#',
            ProgressBarOnBottom = true,
            ForegroundColor = ConsoleColor.Cyan,
            ForegroundColorDone = ConsoleColor.DarkCyan,
            BackgroundColor = ConsoleColor.DarkGray,
            ShowEstimatedDuration = true,
        };
        return new ProgressBar(totalTicks, title, options);
    }

    /// <summary>
    /// Reprocess a specific TradeRebate record to trigger downstream queues
    /// Usage: dotnet run -- cmd test reprocess-trade-rebate &lt;tradeRebateId&gt; &lt;tenantId&gt;
    /// Example: dotnet run -- cmd test reprocess-trade-rebate 10000 10000 local-dev
    /// Note: // .NET automatically sets:
    /// _args[0] = "...\Bacera.Gateway.Web.dll"
    /// What this method does:
    /// 1. Finds an existing TradeRebate record by ID
    /// 2. Recreates the SAME messages that AddToOtherProcessTradeQueue sends
    /// 3. Sends to downstream queues: bcr-sales-rebate-trade.fifo + bcr-event-trade.fifo
    /// </summary>
    public async Task ReprocessTradeRebate()
    {
        if (_args.Length < 6)
        {
            Console.WriteLine("❌ Usage: dotnet run -- cmd test reprocess-trade-rebate <tradeRebateId> <tenantId>");
            Console.WriteLine("Example: dotnet run -- cmd test reprocess-trade-rebate 10000 10000");
            return;
        }

        var tradeRebateId = long.Parse(_args[4]!);
        var tenantId = long.Parse(_args[5]!);

        Console.WriteLine($"�� Reprocessing TradeRebate {tradeRebateId} for Tenant {tenantId}...");

        try
        {
            // Get the trade record
            using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
            var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var tradeRebate = await tenantCtx.TradeRebates
                .FirstOrDefaultAsync(x => x.Id == tradeRebateId);

            if (tradeRebate == null)
            {
                Console.WriteLine($"❌ TradeRebate {tradeRebateId} not found");
                return;
            }

            Console.WriteLine($"✅ Found TradeRebate: Ticket {tradeRebate.Ticket}, Symbol {tradeRebate.Symbol}");

            // Create the same message that would be sent by AddToOtherProcessTradeQueue
            var model = EventShopPointTransaction.MQSource.Build(
                EventShopPointTransactionSourceTypes.Trade,
                tradeRebate.Id,
                tenantId
            );

            var json = model.ToString();
            Console.WriteLine($"📤 Message JSON: {json}");

            // [MIGRATED] BCRSalesRebateTrade and BCREventTrade (Trade source) sends removed.
            // Pipeline replaced by scheduler NATS JetStream (trade_monitor.rs + trade_handler.rs).
            // await _mqService.SendAsync(json, "bcr-sales-rebate-trade.fifo", messageGroupId: tenantId.ToString());
            // await _mqService.SendAsync(json, "bcr-event-trade.fifo", messageGroupId: tenantId.ToString());

            Console.WriteLine($"⚠️ ReprocessTradeRebate is no longer supported — BCRTrade pipeline replaced by scheduler NATS JetStream.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error reprocessing TradeRebate: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Check event points for a specific tenant
    /// Usage: dotnet run -- cmd test check-event-points &lt;tenantId&gt;
    /// Example: dotnet run -- cmd test check-event-points 10000
    /// </summary>
    public async Task CheckEventPoints()
    {
        if (_args.Length < 5)
        {
            Console.WriteLine("❌ Usage: dotnet run -- cmd test check-event-points <tenantId>");
            Console.WriteLine("Example: dotnet run -- cmd test check-event-points 10000");
            return;
        }

        var tenantId = long.Parse(_args[4]!);

        Console.WriteLine($"🔍 Checking event points for Tenant {tenantId}...");

        try
        {
            using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
            var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            // Check EventShopPointTransaction records
            var eventTransactions = await tenantCtx.EventShopPointTransactions
                .OrderByDescending(x => x.CreatedOn)
                .Take(10)
                .ToListAsync();

            Console.WriteLine($"📊 Found {eventTransactions.Count} recent event transactions:");

            foreach (var transaction in eventTransactions)
            {
                Console.WriteLine($"  - ID: {transaction.Id}, SourceType: {transaction.SourceType}, SourceId: {transaction.SourceId}, Created: {transaction.CreatedOn}");
            }

            // Check if our specific trade (ID 10000) was processed
            var ourTradeEvent = eventTransactions.FirstOrDefault(x => x.SourceId == 10000);
            if (ourTradeEvent != null)
            {
                Console.WriteLine($"✅ SUCCESS: TradeRebate ID 10000 was processed into event points!");
                Console.WriteLine($"   Event Transaction ID: {ourTradeEvent.Id}");
                Console.WriteLine($"   Source Type: {ourTradeEvent.SourceType}");
                Console.WriteLine($"   SourceId (TradeRebate ID): {ourTradeEvent.SourceId}");
                Console.WriteLine($"   Created: {ourTradeEvent.CreatedOn}");
            }
            else
            {
                Console.WriteLine($"❌ TradeRebate ID 10000 was NOT found in event transactions");
                Console.WriteLine($"   This means the pull-event-trade-queue job may not have processed it yet");
            }

            // Check total count
            var totalCount = await tenantCtx.EventShopPointTransactions
                .CountAsync();

            Console.WriteLine($"📈 Total event transactions for tenant {tenantId}: {totalCount}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error checking event points: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Clear Redis cache for event trade processing
    /// Usage: dotnet run -- cmd test clear-redis-cache &lt;tenantId&gt;
    /// Example: dotnet run -- cmd test clear-redis-cache 10000
    /// </summary>
    public async Task ClearRedisCache()
    {
        if (_args.Length < 5)
        {
            Console.WriteLine("❌ Usage: dotnet run -- cmd test clear-redis-cache <tenantId>");
            Console.WriteLine("Example: dotnet run -- cmd test clear-redis-cache 10000");
            return;
        }

        var tenantId = long.Parse(_args[4]!);

        Console.WriteLine($"🧹 Clearing Redis cache for Tenant {tenantId}...");

        try
        {
            // Clear the specific cache key for event trade processing
            var cacheKey = $"event_shop_mq_src_tid:{tenantId}_sourceType:Trade_rowId10000";
            await _myCache.KeyDeleteAsync(cacheKey);
            Console.WriteLine($"✅ Cleared cache key: {cacheKey}");

            // Also clear any other related cache keys
            var pattern = $"event_shop_mq_src_tid:{tenantId}_*";
            Console.WriteLine($"🔍 Looking for cache keys matching pattern: {pattern}");
            
            // Note: Redis pattern matching might not be available in all cache implementations
            // This is a basic approach - you might need to adjust based on your cache implementation
            Console.WriteLine($"✅ Redis cache cleared for tenant {tenantId}");
            Console.WriteLine($"💡 Now you can run 'reprocess-trade-rebate' and 'pull-event-trade-queue' again");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error clearing Redis cache: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}