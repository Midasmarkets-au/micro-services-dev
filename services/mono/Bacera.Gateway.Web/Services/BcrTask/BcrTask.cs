using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Extension;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace Bacera.Gateway.Web.Services.BcrTask;

public class BcrTask(IServiceProvider serviceProvider, params string?[] args)
{
    public async Task RunAsync(CancellationToken token)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("action for bcr-task is required");
            return;
        }


        var action = args[3];
        switch (action)
        {
            case "wt-daily":
                await GenerateWalletDailySnapshotAsync(token);
                break;
            case "write-files":
                await TestWriteFiles(token);
                break;
            default:
                Console.WriteLine("action for bcr-task is invalid");
                break;
        }
    }

    private async Task TestWriteFiles(CancellationToken token)
    {
        var path = Path.Combine("/home/forge/logs/crypto", $"test_file_{DateTime.UtcNow:yyyyMMddHH}");
        // var path = Path.Combine("/Users/yixuan/Desktop/ddev", $"test_file_{DateTime.UtcNow:yyyyMMddHH}");
        await File.AppendAllTextAsync(path, $"{DateTime.UtcNow:yyyyMMddHHmmss}" + Environment.NewLine, token);
    }

    private async Task GenerateWalletDailySnapshotAsync(CancellationToken token)
    {
        if (args.Length < 6)
        {
            Console.WriteLine("tenantId and date are required");
            return;
        }

        var tenantId = long.Parse(args[4]!);
        var date = Utils.ParseToUTC(args[5]!).Date;
        var tenantCtx = serviceProvider.GetDbPool().CreateTenantDbContext(tenantId);
        var walletIds = await tenantCtx.Wallets
            .Where(x => x.Party.Status == (int)PartyStatusTypes.Active)
            .Where(x => x.Balance != 0)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken: token);
        await tenantCtx.DisposeAsync();

        Console.WriteLine($"Generating wallet daily snapshot for {walletIds.Count} wallets");
        var bar = new ProgressBar(walletIds.Count, "Generating wallet daily snapshot");

        const int threadCount = 10;
        var countPerThread = (int)Math.Ceiling(walletIds.Count / (double)threadCount);
        await Task.WhenAll(Enumerable.Range(0, threadCount).Select(async i =>
        {
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var acctSvc = scope.ServiceProvider.GetRequiredService<AcctService>();
            foreach (var id in walletIds.Skip(i * countPerThread).Take(countPerThread))
            {
                bar.Tick();
                await acctSvc.GenerateWalletDailySnapshotAsync(id, date, true);
            }
        }));
    }
}