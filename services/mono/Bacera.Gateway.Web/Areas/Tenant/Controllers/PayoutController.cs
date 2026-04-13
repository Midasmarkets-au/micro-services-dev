
﻿using System.Text;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Payout")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class PayoutController(
    TenantDbContext tenantCtx,
    MyDbContextPool pool,
    PaymentMethodService paymentMethodSvc,
    IServiceProvider serviceProvider,
    PayoutService payoutSvc,
    ITenantGetter getter)
    : TenantBaseController
{
    private readonly long _tenantId = getter.GetTenantId();

    /// <summary>
    /// Payout pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PayoutRecord.Criteria? criteria)
    {
        criteria ??= new PayoutRecord.Criteria();
        var items = await tenantCtx.PayoutRecords
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Create Payout by csv file
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("batch-create")]
    public async Task<IActionResult> BatchCreate([FromForm] PayoutRecord.BatchCreateSpec spec)
    {
        var partyId = GetPartyId();
        var methodExists = await tenantCtx.PaymentMethods.AnyAsync(x => x.Id == spec.PaymentMethodId);
        if (!methodExists) return NotFound("Payment method not found");

        var batchUid = Guid.NewGuid().ToString()[..8];

        using var stream = new MemoryStream();
        await spec.File.CopyToAsync(stream);
        stream.Position = 0; // Reset stream position to the beginning

        using var reader = new StreamReader(stream, Encoding.UTF8);
        await using var taskCtx = pool.CreateTenantDbContext(_tenantId);
        await using var transaction = await taskCtx.Database.BeginTransactionAsync();
        try
        {
            var headerStr = await reader.ReadLineAsync();
            if (headerStr != "BankName,BankCode,BranchName,AccountName,BankNumber,Currency,Amount")
                return BadRequest("Invalid header format");

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                    continue;

                Console.WriteLine(line);
                var fields = line.Split(',');
                var record = new PayoutRecord
                {
                    PaymentMethodId = spec.PaymentMethodId,
                    BatchUid = batchUid,
                    Status = (short)PayoutRecordStatusTypes.Created,
                    BankName = fields[0].Trim(),
                    BankCode = fields[1].Trim(),
                    BranchName = fields[2].Trim(),
                    AccountName = fields[3].Trim(),
                    BankNumber = fields[4].Trim(),
                    Currency = fields[5].Trim(),
                    Amount = decimal.Parse(fields[6].Trim()),
                    OperatorPartyId = partyId,
                };

                taskCtx.PayoutRecords.Add(record);
            }

            await taskCtx.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            BcrLog.Slack($"Payout BatchCreate json parse error: {e.Message}");
            return BadRequest(e.Message);
        }

        return Ok(batchUid);
    }

    /// <summary>
    /// Confirm payout by id
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromForm] PayoutRecord.IdSpec spec)
    {
        var item = await tenantCtx.PayoutRecords.FindAsync(spec.Id);
        if (item == null) return NotFound("Record not found");

        var method = await paymentMethodSvc.GetMethodByIdAsync(item.PaymentMethodId);
        if (method == null) return NotFound("Payment method not found");

        var partyId = GetPartyId();
        var result = await payoutSvc.PayoutAsync(item, method, partyId);
        if (result == null) return BadRequest("Invalid payment method");

        item.Status = (short)(result.IsSuccess ? PayoutRecordStatusTypes.Completed : PayoutRecordStatusTypes.Failed);
        await tenantCtx.SaveChangesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Batch confirm payout by batch uid
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("batch-confirm")]
    public async Task<IActionResult> BatchConfirm([FromForm] PayoutRecord.BatchConfirmSpec spec)
    {
        await Task.Delay(0);
        var partyId = GetPartyId();
        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateTenantScope(_tenantId);
            var svc = scope.ServiceProvider.GetRequiredService<PayoutService>();
            var msgSvc = scope.ServiceProvider.GetRequiredService<ISendMessageService>();
            var (success, failed) = await svc.BatchPayoutAsync(spec.BatchUid, partyId);

            const string title = "Batch Payout Result";
            var text = $"Batch payout result: {success} success, {failed} failed";
            var dto = MessagePopupDTO.BuildInfo(title, text);
            await msgSvc.SendPopupToPartyAsync(_tenantId, partyId, dto);
        });
        return Ok("Confirmed, will get notice when job done");
    }
}