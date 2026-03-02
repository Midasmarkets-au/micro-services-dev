using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.Magick;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = UserRoleTypesString.SuperAdmin)]
[Tags("Tenant/Super Admin")]
[Route("api/" + VersionTypes.V1 + "/[Area]/super-admin")]
public class SuperAdminController(
    MyDbContextPool pool,
    ITenantService tenantService,
    ISendMessageService sendMsgSvc,
    IServiceProvider provider,
    IMyCache cache,
    IBackgroundJobClient client)
    : TenantBaseController
{
    [HttpPost("document/upload")]
    public async Task<IActionResult> Upload(IFormFile file, string? type = null)
    {
        var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        // convert file to byte array
        var bytes = stream.ToArray();
        stream.Position = 0;
        await MagickService.CompressImageAsync(stream);
        // return file stream

        // using var stream = new MemoryStream();
        stream.Position = 0;
        return File(stream, file.ContentType, file.Name);
    }

    [HttpPut("do-job")]
    public async Task<IActionResult> TenantConnection()
    {
        client.Enqueue<SuperAdminJob>(HangFireQueues.IntensiveJob, x => x.TJob());
        // await JJob();
        return Ok();
    }


    [HttpGet("test-popup-ws")]
    public async Task<IActionResult> TestPopupWs()
    {
        await sendMsgSvc.SendReadMessageToManagerAsync(GetTenantId(), "Account auto created");
        return Ok();
    }

    [HttpGet("test-read-ws")]
    public async Task<IActionResult> TestReadWs()
    {
        // await sendMsgSvc.SendPopupToManagerAsync()
        await Task.Delay(0);
        return Ok();
    }

    [HttpGet("trade-monitor-key")]
    public async Task<IActionResult> GetTradeMonitorKey()
    {
        var serviceIds = pool.GetServiceNameDict().Select(x => x.Key).ToList();
        var result = new Dictionary<int, string?[]>();
        foreach (var serviceId in serviceIds)
        {
            string? timeKey = null;
            string? ticketKey = null;
            //if (pool.GetPlatformByServiceId(serviceId) == PlatformTypes.MetaTrader5)
            //{
                timeKey = TradeMonitorService.GetMt5LastTradeTimeKey(serviceId);
                ticketKey = TradeMonitorService.GetMt5LastTicketKey(serviceId);
            //}
            //else if (pool.GetPlatformByServiceId(serviceId) == PlatformTypes.MetaTrader4)
            //{
            //    timeKey = TradeMonitorService.GetMt4LastTradeTimeKey(serviceId);
            //    ticketKey = TradeMonitorService.GetMt4LastTicketKey(serviceId);
            //}

            if (timeKey == null || ticketKey == null) continue;
            
            var time = await cache.GetStringAsync(timeKey);
            var ticket = await cache.GetStringAsync(ticketKey);
            
            var instanceName = cache.GetInstanceName();
            result.Add(serviceId, [$"{instanceName}{timeKey}", time, $"{instanceName}{ticketKey}", ticket]);
        }

        return Ok(result);
    }


    [HttpPut("trade-monitor-key")]
    public async Task<IActionResult> ResetTradeMonitorKey([FromQuery] int serviceId, [FromQuery] DateTime date)
    {
        string timeKey;
        string ticketKey;
        if (pool.GetPlatformByServiceId(serviceId) == PlatformTypes.MetaTrader5)
        {
            timeKey = TradeMonitorService.GetMt5LastTradeTimeKey(serviceId);
            ticketKey = TradeMonitorService.GetMt5LastTicketKey(serviceId);
        }
        //else if (pool.GetPlatformByServiceId(serviceId) == PlatformTypes.MetaTrader4)
        //{
        //    timeKey = TradeMonitorService.GetMt4LastTradeTimeKey(serviceId);
        //    ticketKey = TradeMonitorService.GetMt4LastTicketKey(serviceId);
        //}
        else
        {
            return BadRequest();
        }

        await cache.SetStringAsync(timeKey, date.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
        await cache.KeyDeleteAsync(ticketKey);

        return Ok();
    }
}