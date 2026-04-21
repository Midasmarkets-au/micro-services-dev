using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Services;
using Bacera.Gateway.Web.Services.BcrTask;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;
using Bacera.Gateway;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class CommandJob
{
    private readonly ServiceProvider _serviceProvider;
    private readonly WebApplicationBuilder _webApplicationBuilder;
    private readonly string[] _args;

    public CommandJob(ServiceProvider serviceProvider, WebApplicationBuilder me, string[] args)
    {
        _args = args;
        _webApplicationBuilder = me;
        _serviceProvider = serviceProvider;
    }

    /**
     *
     * @ Important!!!!
     * @ Yixuan
     * @ 02/28/2024
     * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
     * !!! Danger Zone                                                                   !!!
     * !!! Do Not Touch Below Command Methods If You Don't Know What You Are Doing       !!!
     * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
     *
     */
    public async Task<bool> RunAsync()
    {
        Console.WriteLine("🔍 [DEBUG] CommandJob.RunAsync() started");
        Console.WriteLine($"🔍 [DEBUG] Command arguments: [{string.Join(", ", _args)}]");
        
        try
        {
            Console.WriteLine("🔍 [DEBUG] Applying configurations...");
            var configResult = await ApplyConfigurations();
            Console.WriteLine($"🔍 [DEBUG] Configuration result: {configResult}");
            
            if (!configResult) 
            {
                Console.WriteLine("❌ [ERROR] Configuration failed, returning false");
                return false;
            }

            var cts = new CancellationTokenSource();
            Console.WriteLine($"🔍 [DEBUG] Executing command: {_args[2]}");
            
            bool result;
            switch (_args[2])
            {
                // [MIGRATED] pull-event-trade-queue removed — BCREventTrade SQS consumer replaced by
                // NATS BCR_EVENT_TRADE consumer in scheduler/src/jobs/event_trade_handler.rs.
                // case "pull-event-trade-queue":
                //     result = await RunPullEventTradeQueue(cts);
                //     break;
                // [MIGRATED] poll-send-message-queue removed — BCRSendMessage SQS consumer replaced by
                // NATS BCR_SEND_MESSAGE consumer in scheduler/src/jobs/send_message_handler.rs.
                // case "poll-send-message-queue":
                //     result = await RunSendMessageQueue(cts);
                //     break;
                // [MIGRATED] pull-trade-queue and mt-monitor commands removed.
                // BCRTrade pipeline replaced by scheduler NATS JetStream (trade_monitor.rs + trade_handler.rs).
                // case "pull-trade-queue":
                //     result = await RunPullTradeQueue(cts);
                //     break;
                // case "mt-monitor":
                //     result = await RunMetaTradeMonitor(cts);
                //     break;
                case "bcr-task":
                    result = await RunBcrTask(cts);
                    break;
                case "test":
                    result = await RunTest(cts);
                    break;
                default:
                    Console.WriteLine($"❌ [ERROR] Unknown command: {_args[2]}");
                    return false;
            }

            Console.WriteLine($"🔍 [DEBUG] Command execution completed with result: {result}");
            await JobEnd(result);
            Console.WriteLine("🔍 [DEBUG] JobEnd completed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ERROR] Exception in CommandJob.RunAsync(): {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"❌ [ERROR] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    // [MIGRATED] RunSendMessageQueue removed — BCRSendMessage SQS consumer replaced by
    // NATS BCR_SEND_MESSAGE consumer in scheduler/src/jobs/send_message_handler.rs.
    // private async Task<bool> RunSendMessageQueue(CancellationTokenSource cts)
    // {
    //     var handler = _serviceProvider.GetRequiredService<PollSendMessageHandler>();
    //     return await WithCtsWrapper(handler.RunAsync, cts);
    // }

    // [MIGRATED] RunPullEventTradeQueue removed — BCREventTrade SQS consumer replaced by
    // NATS BCR_EVENT_TRADE consumer in scheduler/src/jobs/event_trade_handler.rs.
    // private async Task<bool> RunPullEventTradeQueue(CancellationTokenSource cts)
    // {
    //     var pollEventTradeService = _serviceProvider.GetRequiredService<PollEventTradeHandler>();
    //     return await WithCtsWrapper(pollEventTradeService.PollEventTradeAsync, cts);
    // }

    // [MIGRATED] RunPullTradeQueue and RunMetaTradeMonitor removed.
    // BCRTrade pipeline has been replaced by scheduler NATS JetStream:
    //   - Producer: scheduler/src/jobs/trade_monitor.rs (polls MT5 DB, publishes to NATS)
    //   - Consumer: scheduler/src/jobs/trade_handler.rs (writes to trade_rebate_k8s)
    //
    // private async Task<bool> RunPullTradeQueue(CancellationTokenSource cts)
    // {
    //     var svc = _serviceProvider.GetRequiredService<PollMetaTradeHandler>();
    //     return await WithCtsWrapper(svc.PollRebateTradeAsync, cts);
    // }
    //
    // private async Task<bool> RunMetaTradeMonitor(CancellationTokenSource cts)
    // {
    //     var metaTradeMonitorService = _serviceProvider.GetRequiredService<TradeMonitorService>();
    //     try { return await WithCtsWrapper(metaTradeMonitorService.ExecuteJobAsync, cts); }
    //     finally { metaTradeMonitorService.Dispose(); }
    // }


    private async Task<bool> RunBcrTask(CancellationTokenSource cts)
    {
        var bcrTask = new BcrTask(_serviceProvider, _args);
        Console.WriteLine("BcrTask Start.");
        await WithCtsWrapper(bcrTask.RunAsync, cts);
        Console.WriteLine("BcrTask End.");
        return true;
    }

    /// <summary>
    /// Run test
    /// </summary>
    /// <returns></returns>
    private async Task<bool> RunTest(CancellationTokenSource cts)
    {
        var testService = new CmdTestService(_serviceProvider, _webApplicationBuilder, _args);
        Console.WriteLine("TestService Start.");
        await WithCtsWrapper(testService.RunAsync, cts);
        Console.WriteLine("TestService End.");
        return true;
    }

    private static async Task<bool> WithCtsWrapper(Func<CancellationToken, Task> action,
        CancellationTokenSource cts)
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Prevent the process from terminating immediately.
            // ReSharper disable once AccessToDisposedClosure
            cts.Cancel(); // Send the cancellation signal.
            Console.WriteLine("Cancellation requested...");
        };

        try
        {
            await action(cts.Token);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine($"Task cancelled when receiving messages from queue, {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error receiving messages from queue, {e.Message}");
        }
        finally
        {
            cts.Dispose(); // Ensure the CancellationTokenSource is disposed once no longer needed.
        }

        return true;
    }

    private async Task<bool> ApplyConfigurations()
    {
        Console.WriteLine("🔍 [DEBUG] ApplyConfigurations() started");
        
        await Task.Delay(0);
        if (_args.Length < 2)
        {
            Console.WriteLine("❌ [ERROR] Insufficient arguments! Do nothing!");
            return false;
        }
        
        Console.WriteLine($"🔍 [DEBUG] Arguments count: {_args.Length}, Command: {_args[2]}");

        try
        {
            // Keep only essential validation, avoid noisy Postgres logs
            using var scope = _serviceProvider.CreateScope();
            var centralDbContext = scope.ServiceProvider.GetService<CentralDbContext>();
            if (centralDbContext == null)
            {
                Console.WriteLine("❌ [ERROR] CentralDbContext is null");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ERROR] Exception in ApplyConfigurations(): {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"❌ [ERROR] Stack trace: {ex.StackTrace}");
            return false;
        }

        ConfigLog();
        Console.WriteLine("================================== CommandJob Start ==================================");
        return true;
    }

    private static async Task JobEnd(bool result)
    {
        await Task.Delay(0);
        Console.WriteLine(
            $"================================ CommandJob End: {result} ================================");
    }

    private void ConfigLog()
    {
        var isSqlLogEnabled = _args.Any(x => x == "sql-log");
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Logger(
                isSqlLogEnabled
                    ? x => x.WriteTo.Console()
                    : x => x.WriteTo.Console()
                        .Filter.ByExcluding(e => e.Level == LogEventLevel.Information &&
                                                 e.Properties.ContainsKey("SourceContext") &&
                                                 e.Properties["SourceContext"].ToString()
                                                     .Contains("Microsoft.EntityFrameworkCore.Database.Command"))
            );
        var dir = Directory.GetCurrentDirectory();
        Log.Logger = loggerConfiguration.CreateLogger();
        _webApplicationBuilder.Configuration.SetBasePath(dir).AddJsonFile("appsettings.json");
        _webApplicationBuilder.Host.UseSerilog();
        _webApplicationBuilder.Services.AddLogging();
    }
}