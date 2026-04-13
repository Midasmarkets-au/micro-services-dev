
﻿using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using U = Bacera.Gateway.Auth.User;

namespace Bacera.Gateway.Web.Controllers;

using MSG = ResultMessage.TwoFactor;

[Tags("Two Factor Authentication")]
[Route("api/" + VersionTypes.V1 + "/2fa")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class TwoFactorAuthenticationController : BaseController
{
    private readonly UrlEncoder _urlEncoder;
    private readonly UserManager<U> _userManager;
    private readonly SignInManager<U> _signInManager;
    private readonly ILogger<TwoFactorAuthenticationController> _logger;

    public TwoFactorAuthenticationController(
        UrlEncoder urlEncoder,
        UserManager<U> userManager,
        SignInManager<U> signInManager,
        ILogger<TwoFactorAuthenticationController> logger
    )
    {
        _logger = logger;
        _urlEncoder = urlEncoder;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Get Two Factor Authentication Detail
    /// </summary>
    /// <returns></returns>
    [HttpGet("detail")]
    [ProducesResponseType(typeof(U.TwoFactorAuthenticationResponseModel), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Detail()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("User not found");
            return Unauthorized(ToErrorResult(MSG.InvalidUser));
        }

        var logins = await _userManager.GetLoginsAsync(user);
        var result = new U.TwoFactorAuthenticationResponseModel
        {
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            UserName = user.UserName,
            ExternalLogins = logins.Select(login => login.ProviderDisplayName).ToList(),
            TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
            // TwoFactorClientRemembered = await _userMgr.IsTwoFactorClientRememberedAsync(user),
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
        };
        return Ok(result);
    }

    // [HttpGet]
    // public IActionResult AesKey()
    // {
    //     return Ok(EncryptProvider.CreateAesKey().Key);
    // }

    /// <summary>
    /// Show Two Factor Authentication
    /// </summary>
    /// <returns></returns>
    [HttpGet("authenticator/setup")]
    public async Task<IActionResult> SetupAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(ToErrorResult(MSG.InvalidUser));
        var authenticatorDetails = await GetAuthenticatorDetailsAsync(user);
        return Ok(authenticatorDetails);
    }

    /// <summary>
    /// Generate Two Factor Authentication Codes
    /// </summary>
    /// <returns></returns>
    [HttpGet("authenticator/codes")]
    public async Task<IActionResult> ValidCodes()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("User not found");
            return Unauthorized(ToErrorResult(MSG.InvalidUser));
        }

        var validCodes = new List<int>();
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (key == null) return BadRequest(ToErrorResult(MSG.InvalidAuthenticatorKey));

        var hash = new HMACSHA1(Identity.Internals.Base32.FromBase32(key));
        var unixTimestamp =
            Convert.ToInt64(Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
        var timestamp = Convert.ToInt64(unixTimestamp / 30);
        // Allow codes from 90s in each direction (we could make this configurable?)
        for (var i = -2; i <= 2; i++)
        {
            var expectedCode =
                Identity.Internals.Rfc6238AuthenticationService.ComputeTotp(hash, (ulong)(timestamp + i),
                    modifier: null);
            validCodes.Add(expectedCode);
        }

        _logger.LogInformation("Valid codes: {Codes} for User: {User}", validCodes, user.UserName);
        return Ok(validCodes);
    }

    /// <summary>
    /// Verify Two Factor Authentication
    /// </summary>
    /// <param name="verifyAuthenticator"></param>
    /// <returns></returns>
    [HttpPost("authenticator/verify")]
    public async Task<IActionResult> VerifyAuthenticator([FromBody] VerifyAuthenticatorRequest verifyAuthenticator)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(ToErrorResult(MSG.InvalidUser));

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid VerifyAuthenticatorRequest");
            var errors = GetErrors(ModelState);
            return BadRequest(ToErrorResult(MSG.InvalidData, errors));
        }

        var verificationCode =
            verifyAuthenticator.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2FaTokenValid)
        {
            _logger.LogWarning("Invalid authenticator code entered");
            return BadRequest(ToErrorResult(MSG.InvalidAuthenticatorCode));
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        var result = ToSuccessResult(MSG.Success);

        _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app",
            user.Id);
        if (await _userManager.CountRecoveryCodesAsync(user) != 0) return Ok(result);

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        result.SetData(new { recoveryCodes });
        return Ok(result);
    }

    /// <summary>
    /// Reset Two Factor Authentication
    /// </summary>
    /// <returns></returns>
    [HttpPost("authenticator/reset")]
    public async Task<IActionResult> ResetAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(ToErrorResult(MSG.InvalidUser));

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User with id '{UserId}' has reset their authentication app key", user.Id);
        return Ok(ToSuccessResult(MSG.ResetSuccess));
    }

    /// <summary>
    /// Disable Two Factor Authentication
    /// </summary>
    /// <returns></returns>
    [HttpPut("disable")]
    public async Task<IActionResult> Disable2Fa([FromBody] VerifyAuthenticatorRequest verifyAuthenticator)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(ToErrorResult(MSG.InvalidUser));

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogWarning("2FA is not enabled for user");
            return BadRequest(ToErrorResult(MSG.CannotDisable2Fa));
        }

        var verificationCode =
            verifyAuthenticator.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2FaTokenValid)
        {
            _logger.LogWarning("Invalid authenticator code entered");
            return BadRequest(ToErrorResult(MSG.InvalidAuthenticatorCode));
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa", user.Id);
            return Ok(ToSuccessResult(MSG.Disable2FaSuccess));
        }

        _logger.LogWarning("Failed to disable 2FA for user");
        return BadRequest(ToErrorResult(MSG.Disable2FaFail, result.Errors.Select(e => e.Description)));
    }

    /// <summary>
    /// Enable Two Factor Authentication
    /// </summary>
    /// <returns></returns>
    [HttpPut("enable")]
    public async Task<IActionResult> Enable2Fa()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(ToErrorResult(MSG.InvalidUser));

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogWarning("2FA has enabled for user");
            return BadRequest(ToErrorResult(MSG.CannotEnable2Fa));
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' has enable 2fa", user.Id);
            return Ok(ToSuccessResult(MSG.Enable2FaSuccess));
        }

        _logger.LogWarning("Failed to enable 2FA for user");
        return BadRequest(ToErrorResult(MSG.Enable2FaFail, result.Errors.Select(e => e.Description)));
    }

    // [HttpPost("GenerateRecoveryCodes")]
    // public async Task<IActionResult> GenerateRecoveryCodes()
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return Unauthorized(Result.Error("Invalid user"));
    //
    //     var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
    //
    //     if (!isTwoFactorEnabled)
    //     {
    //         return Ok(Result.Error("Cannot generate recovery codes as you do not have 2FA enabled"));
    //     }
    //
    //     var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
    //     return Ok(Result.Success("You have generated new recovery codes", new { recoveryCodes }));
    // }

    // [HttpGet("AesKey")]
    // public IActionResult AesKey()
    //     => Ok(EncryptProvider.CreateAesKey().Key);

    private static IEnumerable<string> GetErrors(ModelStateDictionary modelState)
    {
        var errors = new List<string>();

        foreach (var state in modelState.Values)
        {
            errors.AddRange(state.Errors.Select(error => error.ErrorMessage));
        }

        return errors;
    }

    private async Task<object> GetAuthenticatorDetailsAsync(U user)
    {
        // Load the authenticator key & QR code URI to display on the form
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = await _userManager.GetEmailAsync(user);
        return new
        {
            SharedKey = FormatKey(unformattedKey!),
            AuthenticatorUri = GenerateQrCodeUri(email!, unformattedKey!)
        };
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        return string.Format(
            authenticatorUriFormat,
            _urlEncoder.Encode("Bacera.Identity"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }
}