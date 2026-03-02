using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Services.Acct;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[Area("Tenant")]
[Tags("Tenant/Finance Summary")]
[Route("api/" + VersionTypes.V2 + "/[Area]/finance")] 
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class FinanceSummaryController(AcctService acctService, ILogger<FinanceSummaryController> logger)
    : TenantBaseControllerV2
{
    [HttpGet("deposit-withdraw-summary/{partyId:long?}")]
    public async Task<IActionResult> GetSummary(
        long? partyId)
    {
        var result = await acctService.GetDepositWithdrawSummaryAsync(partyId);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated deposit and withdrawal details combined in a single array
    /// </summary>
    /// <param name="partyId">Party ID</param>
    /// <param name="groupId">Customized group id in configuration PaymentMethodGroups</param>
    /// <param name="criteria">Pagination criteria</param>
    /// <returns>Paginated list of deposits and withdrawals combined</returns>
    [HttpGet("deposit-withdraw-details/{partyId:long}/{groupId:int}")]
    [ProducesResponseType(typeof(Result<List<AcctService.FinanceDetailRow>, AcctService.FinanceDetailCriteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailsPaginated(
        long partyId,
        int groupId,
        [FromQuery] AcctService.FinanceDetailCriteria? criteria)
    {
        criteria ??= new AcctService.FinanceDetailCriteria();
        criteria.PartyId = partyId;
        criteria.GroupId = groupId;
        
        var result = await acctService.GetDepositWithdrawDetailsPaginatedAsync(criteria);
        return Ok(result);
    }
}


