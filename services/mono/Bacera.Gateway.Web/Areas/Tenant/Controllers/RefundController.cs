using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSGCommon = Bacera.Gateway.ResultMessage.Common;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Refund;
using MSG = ResultMessage.Refund;

[Tags("Tenant/Refund")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RefundController : TenantBaseController
{
    private readonly AccountingService _accountingSvc;

    public RefundController(AccountingService accountingSvc)
    {
        _accountingSvc = accountingSvc;
    }

    /// <summary>
    /// Refund pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var hideEmail = ShouldHideEmail();
        return Ok(await _accountingSvc.RefundQueryAsync(criteria, hideEmail));
    }

    /// <summary>
    /// Refund Create and Complete
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        // Validate spec.Amount != 0
        if (spec.Amount == 0)
        {
            return BadRequest(ToErrorResult(MSGCommon.ZeroAmountNotAllowed));
        }
        // Validate that the target wallet is the first created wallet (USD wallet) if targeting a wallet
        if (spec.TargetType == RefundTargetTypes.Wallet)
        {
            var isPrimaryWallet = await _accountingSvc.IsPrimaryWallet(spec.TargetId);
            if (!isPrimaryWallet)
            {
                return BadRequest(ToErrorResult(MSG.OnlyPrimaryWalletAllowed));
            }
        }

        var refund = await _accountingSvc.RefundCreateAsync(spec, GetPartyId());
        if (refund.IsEmpty()) return BadRequest(ToErrorResult(MSG.CreateFailed));

        var result = await _accountingSvc.RefundCompleteAsync(refund.Id, GetPartyId());
        if (result == false) return BadRequest(ToErrorResult(MSG.CompleteFailed));
        return Ok(result);
    }
}