using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using MSG = ResultMessage.Payment;

[Tags("Tenant/Payment")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class PaymentController(AccountingService accountingService) : TenantBaseController
{
    /// <summary>
    /// Payment pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Payment>, Payment.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<Payment>, Payment.Criteria>>> Index(
        [FromQuery] Payment.Criteria? criteria = null)
    {
        criteria ??= new Payment.Criteria();
        var items = await accountingService.PaymentQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Get Payment
    /// </summary>
    /// <param name="id" example="10"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(Payment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<List<Payment>, Payment.Criteria>>> Get(long id)
    {
        var item = await accountingService.PaymentGetAsync(id);
        return item.Id == 0 ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Cancel Payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(long id)
    {
        var pid = GetPartyId();
        var (result, _) = await accountingService.PaymentCancelAsync(id, $"Canceled manually by user:{pid}");
        return result switch
        {
            1 => NoContent(),
            -1 => NotFound(),
            -2 => BadRequest(Result.Error(MSG.PaymentNotFoundForCurrentAction)),
            _ => BadRequest(Result.Error(MSG.PaymentCancelFailed))
        };
    }

    /// <summary>
    /// Execute Payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/execute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Execute(long id)
    {
        var (result, _) = await accountingService.PaymentExecuteAsync(id);
        return result ? NoContent() : BadRequest(Result.Error(MSG.PaymentExecuteFailed));
    }

    /// <summary>
    /// Complete Payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(long id)
    {
        var pid = GetPartyId();
        var (result, _) = await accountingService.PaymentCompleteAsync(id, $"Completed manually by user:{pid}");
        return result switch
        {
            1 => NoContent(),
            -1 => NotFound(),
            -2 => BadRequest(Result.Error(MSG.PaymentNotFoundForCurrentAction)),
            _ => BadRequest(Result.Error(MSG.PaymentCompleteFailed))
        };
    }
}