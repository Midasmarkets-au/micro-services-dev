using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

//using MSG = ResultMessage.Payment;

[Tags("Client/Payment")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class PaymentController(
    AccountingService accountingService,
    UserService userSvc,
    ConfigService configSvc,
    TenantDbContext tenantCtx) : ClientBaseController
{
    /// <summary>
    /// Payment Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Payment.ClientResponseModel>, Payment.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] Payment.Criteria? criteria)
    {
        criteria ??= new Payment.Criteria();
        criteria.PartyId = GetPartyId();
        return Ok(await accountingService.PaymentQueryAsync(GetPartyId(), criteria));
    }

    ///// <summary>
    ///// Process Wire Payment for a payment
    ///// </summary>
    ///// <remarks>
    ///// </remarks>
    ///// <param name="paymentId"></param>
    ///// <param name="model"></param>
    ///// <returns></returns>
    //[HttpPut("{paymentId:long}/p-100/process")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    //public async Task<IActionResult> ProcessWire(long paymentId, [FromBody] WirePaymentModel model)
    //{
    //    var item = await _accountingSvc.PaymentGetAsync(paymentId);
    //    if (item.Id == 0 || item.PartyId != GetPartyId() || item.CanExecute())
    //    {
    //        return NotFound();
    //    }

    //    if (item.PaymentService.Platform != (int)PaymentPlatformTypes.Wire)
    //    {
    //        return BadRequest(Result.Error(MSG.PaymentPlatformNotMatched));
    //    }

    //    return await _paymentSvc.ProcessPaymentAsync(paymentId, model)
    //        ? NoContent()
    //        : Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
    //}

    /// <summary>
    /// Get user payment service accesses
    /// </summary>
    /// <returns></returns>
    [HttpGet("service")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentService()
    {
        var services = await tenantCtx.PaymentServices
            .Where(x => x.IsActivated == 1)
            .ToResponse()
            .ToListAsync();

        var result = new PaymentService.AccessResponseModel
        {
            Deposit = services.Where(x => x.CanDeposit == 1).ToList(),
            Withdrawal = services.Where(x => x.CanWithdraw == 1).ToList()
        };
        return Ok(result);
    }


    [HttpGet("{paymentHashId}/guide")]
    public async Task<IActionResult> GetGuide(string paymentHashId)
    {
        var id = Payment.HashDecode(paymentHashId);
        var user = await userSvc.GetPartyAsync(GetPartyId());

        var item = await tenantCtx.Payments
            .Where(x => x.Id == id)
            .Select(x => new { x.PaymentMethod.Platform, x.PaymentServiceId })
            .SingleOrDefaultAsync();
        if (item == null) return BadRequest("Payment not found");

        var data = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
            .Where(x => x.RowId == item.PaymentServiceId)
            .Select(x => x.Data)
            .SingleOrDefaultAsync() ?? "{}";

        var instructions = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, string>>(data);
        var instruction = instructions.TryGetValue(user.Language, out var value) ? value :
            instructions.TryGetValue(LanguageTypes.English, out value) ? value : "";
        if (item.Platform == (int)PaymentPlatformTypes.Crypto)
        {
            var crypto = await tenantCtx.Cryptos
                .Where(x => x.InUsePaymentId == id)
                .Select(x => new { x.Address, x.Status, x.UpdatedOn })
                .SingleOrDefaultAsync();

            if (crypto == null) return Ok(new { instruction, crypto });
            var cryptoSetting = await configSvc.GetAsync<Crypto.Setting>(nameof(Public), 0, ConfigKeys.CryptoSetting);
            var expireTime = crypto.UpdatedOn.AddMinutes(cryptoSetting!.PayExpiredTimeInMinutes);
            var remainMinutes = Math.Max((int)(expireTime - DateTime.UtcNow).TotalMinutes, 0);
            var cryptoInfo = new { instruction, crypto = new { crypto.Address, remainMinutes } };
            return Ok(cryptoInfo);
        }

        var result = new { instruction };
        return Ok(result);
    }
}