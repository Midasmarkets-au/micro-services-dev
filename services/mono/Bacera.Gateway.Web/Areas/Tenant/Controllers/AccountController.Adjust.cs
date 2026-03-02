using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

public partial class AccountController
{
    [HttpGet("adjust-batch")]
    public async Task<IActionResult> BatchAdjustIndex([FromQuery] AdjustBatch.Criteria? criteria)
    {
        criteria ??= new AdjustBatch.Criteria();
        return Ok(await tradingService.AdjustBatchQueryAsync(criteria));
    }

    [HttpGet("adjust-record")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AdjustRecordIndex([FromQuery] AdjustRecord.Criteria? criteria)
    {
        criteria ??= new AdjustRecord.Criteria();
        return Ok(await tradingService.AdjustRecordQueryAsync(criteria));
    }

    [HttpPost("adjust-record")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAdjustRecord([FromBody] AdjustRecord.CreateSpec spec)
    {
        var (result, msg) =
            await tradingService.CreateAccountAdjustAsync(spec.Type, spec.AccountId, spec.Amount, spec.Comment,
                GetPartyId());
        if (!result) return BadRequest(Result.Error(msg));
        return Ok(new { Ticket = msg });
    }

    [HttpPost("batch-adjust/create")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessAccountInBatch([FromForm] AdjustBatch.CreateSpec spec)
    {
        if (spec.File.Length < 1)
            return BadRequest(Result.Error("__INVALID_FILE__"));

        var (result, msg, response) = await tradingService.CreateProcessAdjustAccountBatch(GetPartyId(), GetTenantId(),
            spec.ServiceId, spec.Type, spec.Note, spec.File);

        if (!result) return BadRequest(Result.Error(msg));

        return Ok(response);
    }

    [HttpPost("batch-adjust/{adjustBatchId:long}/confirm")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmAccountBatchProcess(long adjustBatchId)
    {
        var result = await tradingService.ConfirmAccountBatchProcessingByIdAsync(adjustBatchId);
        if (!result) return BadRequest();

        backgroundJobClient.Enqueue<ITradeAccountJob>("intensive-job", x =>
            x.AdjustCreditOrBalanceByBatchId(GetTenantId(), adjustBatchId));
        return Ok();
    }

    
    [HttpPost("batch-adjust/{adjustBatchId:long}/confirm/{recordId:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmAccountBatchProcessByRecordId(long adjustBatchId, long recordId)
    {
        var batch = await tenantCtx.AdjustBatches
            .Where(x => x.Id == adjustBatchId)
            .Select(x => new { x.ServiceId, x.Type })
            .SingleOrDefaultAsync();

        if (batch == null) return NotFound(Result.Error("__BATCH_NOT_FOUND__"));

        var record = await tenantCtx.AdjustRecords
            .Where(x => x.Id == recordId)
            .SingleOrDefaultAsync();

        if (record == null) return NotFound(Result.Error("__RECORD_NOT_FOUND__"));

        // amount has been scaled with *10000 already and will be convert back in ChangeBalance() and ChangeCredit()
        var amount = Math.Round(((decimal)record.Amount / 100), 2);

        var (result, ticket) = batch.Type switch
        {
            (short)AdjustTypes.Agent or (short)AdjustTypes.Adjust => await tradingApiService.ChangeBalance(
                batch.ServiceId, record.AccountNumber, amount, record.Comment),

            (short)AdjustTypes.Credit => await tradingApiService.ChangeCredit(batch.ServiceId,
                record.AccountNumber, amount, record.Comment),

            _ => new Tuple<bool, string>(false, "Invalid adjust type")
        };

        if (!result)
        {
            record.Status = (short)AdjustRecordStatusTypes.Failed;
            BcrLog.Slack($"Failed to adjust for account {record.AccountNumber}. Ticket: {ticket}");
        }
        else
        {
            record.Ticket = long.TryParse(ticket, out var res) ? res : 0;
            record.Status = (short)AdjustRecordStatusTypes.Completed;
        }

        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    // private static string GetCreditComment(decimal amount, string comment)
    //     => "Credit " + (amount > 0 ? "in" : "out") + " " + comment.Trim();
}