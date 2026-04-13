
﻿using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneNumbers;
using MSG = Bacera.Gateway.ResultMessage.Verification;

namespace Bacera.Gateway.Web.Controllers;

[Tags("User Verification")]
[Route("api/" + VersionTypes.V1 + "/user/verification")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class VerificationController : BaseController
{
    private readonly AuthDbContext _authDbContext;
    private readonly ISmsVerification _smsVerification;
    private readonly ILogger<VerificationController> _logger;

    public VerificationController(
        AuthDbContext authDbContext,
        ISmsVerification smsVerification,
        ILogger<VerificationController> logger
    )
    {
        _logger = logger;
        _authDbContext = authDbContext;
        _smsVerification = smsVerification;
    }

    /// <summary>
    /// Send sms verification
    /// </summary>
    /// <returns></returns>
    [HttpPost("mobile/{regionCode}/{phone}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendVerificationCode(string regionCode, string phone)
    {
        var combinedPhoneNumber = "+" + regionCode.Trim() + phone.Trim();
        if (regionCode == "852")
        {
            if (phone.Length != 8)
                return BadRequest(Result.Error(ResultMessage.Verification.PhoneNumberInvalid));
        }
        else
        {
            if (!IsValidPhoneNumber(combinedPhoneNumber))
                return BadRequest(Result.Error(ResultMessage.Verification.PhoneNumberInvalid));
        }

        if (!PhoneNumberRegionCodeTypes.All.Contains(regionCode))
            return BadRequest(Result.Error(MSG.RegionCodeInvalid));

        if (_smsVerification.HasReachedLimit(GetPartyId().ToString()))
            return new StatusCodeResult(StatusCodes.Status429TooManyRequests);

        try
        {
            var result = await _smsVerification.Verification(combinedPhoneNumber, GetPartyId().ToString());
            return result ? NoContent() : BadRequest(MSG.VerificationFail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Send SMS Verification Code Error : {Message}", ex.Message);
            return BadRequest(Result.Error(ex.Message));
        }
    }

    /// <summary>
    /// Check sms verification
    /// </summary>
    /// <returns></returns>
    [HttpPut("mobile/{regionCode}/{phone}/{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckVerificationCode(string regionCode, string phone, string code)
    {
        var combinedPhoneNumber = "+" + regionCode.Trim() + phone.Trim();

        // 09/13/2023 Disable profile phone number verification request by Jim.
        // if (!isValidPhoneNumber(combinedPhoneNumber))
        //     return BadRequest(Result.Error(MSG.PhoneNumberInvalid));

        if (!PhoneNumberRegionCodeTypes.All.Contains(regionCode))
            return BadRequest(Result.Error(MSG.RegionCodeInvalid));

        try
        {
            // 09/13/2023 Disable profile phone update code verification request by Jim.
            // var (result, formattedPhoneNumber) = await _smsVerification.VerificationCheck(combinedPhoneNumber, code);
            // if (result == false) return BadRequest(Result.Error(MSG.VerificationFail));
            var tenantId = GetTenantId();
            var partyId = GetPartyId();
            var user = await _authDbContext.Users.SingleOrDefaultAsync(x =>
                x.PartyId == partyId && x.TenantId == tenantId);
            if (user == null) return BadRequest(Result.Error(ResultMessage.Common.UserNotFound));

            user.CCC = regionCode;
            user.PhoneNumber = phone;
            user.PhoneNumberConfirmed = true;
            user.UpdatedOn = DateTime.UtcNow;

            await _authDbContext.SaveChangesWithAuditAsync(GetPartyId());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Check SMS Verification Code Error : {Message}", ex.Message);
            return BadRequest(Result.Error(ex.Message));
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        PhoneNumber phone;
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            phone = util.ParseAndKeepRawInput(phoneNumber, null);
        }
        catch (Exception)
        {
            return false;
        }

        return util.IsValidNumber(phone);
    }
}