using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSG = Bacera.Gateway.ResultMessage.Verification;

namespace Bacera.Gateway.Web.Controllers;

[Tags("User Verification")]
[Route("api/" + VersionTypes.V1 + "/trade-account/{tenantId:long}")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TradeAccountController(
    IServiceProvider serviceProvider,
    ILogger<TradeAccountController> logger)
    : BaseController
{
    /// <summary>
    /// Validate OTP for change trade account password
    /// </summary>
    /// <param name="spec"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePasswordVerifyToken([FromBody] ChangeTradeAccountPasswordRequest spec,
        long tenantId)
    {
        using var scope = serviceProvider.CreateScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantSetter>();
        setter.SetTenantId(tenantId);
        var tradingService = scope.ServiceProvider.GetRequiredService<TradingService>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IApplicationTokenService>();

        spec.ReferenceType = TokenTypes.TradeAccountChangePasswordToken;
        var (exists, accountId) = await tradingService.AccountUidExistsForPartyAsync(spec.ReferenceId, spec.PartyId);
        if (!exists)
            return NotFound();

        var tokenResult = await tokenService.VerifyTokenAsync(spec);
        if (!tokenResult)
        {
            logger.LogInformation("Invalid Token : {AccountId}", accountId);
            return BadRequest(Result.Error(ResultMessage.Common.InvalidToken));
        }

        //var account = await _tradingSvc.AccountGetAsync(accountId);
        //if (account.Status != (short)AccountStatusTypes.Activate)
        //    return BadRequest(Result.Error(ResultMessage.Account.AccountInactivated));

        try
        {
            var result = await tradingService.TradeAccountChangePassword(accountId, spec.Password);
            if (result)
            {
                logger.LogInformation("Change Trade Account Password By Token Success : {AccountId}", accountId);
                await tokenService.RemoveTokenAsync(spec);
                return NoContent();
            }

            logger.LogWarning("Change Trade Account Password By Token Fail : {AccountId}", accountId);
            return BadRequest(Result.Error(ResultMessage.Register.ChangePasswordFail));
        }
        catch (Exception)
        {
            logger.LogWarning("System Error Change Trade Account Password By Token Fail : {AccountId}", accountId);
            return BadRequest(Result.Error(ResultMessage.Register.ChangePasswordFail));
        }
    }
}