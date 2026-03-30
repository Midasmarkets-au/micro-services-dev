using System.Text;
using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using StackExchange.Redis;

// ReSharper disable AccessToDisposedClosure

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[Tags("Tenant/Statistic")]
[Route("api/" + VersionTypes.V2 + "/[Area]/statistic")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class StatisticControllerV2(
    MybcrDbContext bcrCtx,
    IHttpClientFactory clientFactory,
    MonitorService monitorSvc,
    IMyCache cache,
    MyDbContextPool pool
) : TenantBaseControllerV2
{
    [HttpGet("server")]
    public async Task<IActionResult> ServerIndex([FromQuery] Server.Criteria? criteria)
    {
        criteria ??= new Server.Criteria();
        var items = await bcrCtx.Servers
            .PagedFilterBy<Server, ulong>(criteria)
            .ToTenantPageModel()
            .OrderBy(x => x.Stat)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return Ok(Result.Of(items));
    }

    [HttpGet("server/metrics")]
    public async Task<IActionResult> ServerMetrics([FromQuery] ulong id)
    {
        var server = await bcrCtx.Servers
            .Where(x => x.Id == id)
            .ToTenantPageModel()
            .SingleAsync();

        var metrics = new Server.TenantServerMetricModel();
        using var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        var baseUrl = $"http://{server.Ip}:60618/bcrstat/api/4";
        var endpoints = new Dictionary<string, Action<string>>
        {
            ["/cpu"]  = json => metrics.CpuJson  = json,
            ["/load"] = json => metrics.LoadJson  = json,
            ["/mem"]  = json => metrics.MemoryJson = json,
            ["/fs"]   = json => metrics.DiskJson  = json
        };

        await Task.WhenAll(endpoints.Select(async x =>
        {
            string json;
            try
            {
                var response = await client.GetAsync(baseUrl + x.Key);
                json = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                json = "{\"error\":\"Request timeout\"}";
            }
            catch (Exception)
            {
                json = "{\"error\":\"error\"}";
            }
            x.Value(json);
        }));

        server.Metrics = metrics;
        return Ok(server);
    }

    [HttpGet("online-admins")]
    public async Task<IActionResult> TodayActiveUser()
    {
        var rawInfos = await monitorSvc.GetOnlineAdminAsync();
        var results = rawInfos
            .Select(x => x.ToString()!.Split('_'))
            .GroupBy(x => x[2])
            .Select(x => new
            {
                TenantId = x.Key,
                Users = x.Select(y => new
                {
                    PartyId = y[1],
                    Email = y[0],
                    LastRequest = $"{(DateTime.UtcNow - Utils.ParseToUTC(y[3])).TotalSeconds}s"
                })
            })
            .ToList();

        return Ok(results);
    }

    [HttpGet("online-users")]
    public async Task<IActionResult> OnlineUsers()
    {
        var db = cache.GetDatabase();
        var result = await Task.WhenAll(pool.GetTenantIds().Select(async tenantId =>
        {
            var deviceStat = new Dictionary<string, long>();
            var clientStat = new Dictionary<string, long>();

            var pattern = $"portal_online:{tenantId}:*";
            var total = 0;
            long cursor = 0;
            do
            {
                var scanResult = await db.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "100");
                var redisResults = (RedisResult[])scanResult!;

                cursor = long.Parse((string)redisResults[0]!);
                var resultKeys = (string[])redisResults[1]!;
                foreach (var key in resultKeys)
                {
                    total += 1;
                    var parts = key.Split(':').TakeLast(3).ToArray();
                    var clientKey = parts[1];
                    var device = parts[2];

                    deviceStat.TryAdd(device, 0);
                    clientStat.TryAdd(clientKey, 0);

                    deviceStat[device] += 1;
                    clientStat[clientKey] += 1;
                }
            } while (cursor != 0);

            return new { tenantId, total, deviceStat, clientStat };
        }));

        return Ok(result);
    }
}
