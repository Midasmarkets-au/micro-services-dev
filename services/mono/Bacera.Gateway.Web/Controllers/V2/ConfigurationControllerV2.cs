
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers.V2;

[ApiController]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/config")]
[Tags("Config")]
public class ConfigurationControllerV2(
    TenantDbContext tenantDbContext,
    ReferralCodeService referralCodeService,
    ConfigurationService configurationService)
    : BaseControllerV2
{
    [HttpGet("public")]
    public async Task<IActionResult> Public()
    {
        var partyId = GetPartyId();
        var party = await tenantDbContext.Parties
            .Where(x => x.Id == partyId)
            .Select(x => new { x.SiteId, x.ReferCode })
            .FirstOrDefaultAsync();

        if (party == null) return Unauthorized();

        var accountTypesAvailable = await referralCodeService.GetAccountTypesInReferralCodeAsync(party.ReferCode);
        if (accountTypesAvailable == null)
        {
            var items = await configurationService.GetAccountTypeAvailableAsync(party.SiteId);
            accountTypesAvailable = items.Select(x => (AccountTypes)x).ToList();
        }
        var info = await configurationService.GetContactInfoAsync(party.SiteId);
        var contactInfo = Utils.JsonDeserializeDynamic(info.GetValueOrDefault("value", "{}"));
        var result = new ApplicationConfigure.PublicSettingV2
        {
            SiteId = party.SiteId,
            ContactInfo = contactInfo,
            AccountTypeAvailable = accountTypesAvailable,
            CurrencyAvailable = await configurationService.GetCurrencyAvailableAsync(party.SiteId),
            DefaultTradeService = await configurationService.GetDefaultTradeServiceAsync(party.SiteId),
            DemoTradingPlatformAvailable =
                await configurationService.GetDemoTradingPlatformAvailableAsync(party.SiteId),
            LeverageAvailable = await configurationService.GetLeverageAvailableAsync(party.SiteId),
            VerificationQuizEnabled = await configurationService.GetVerificationQuizToggleSwitchAsync(party.SiteId),
            SmsValidationEnabled = await configurationService.GetSmsVerificationToggleSwitchAsync(party.SiteId)
        };
        return Ok(result);
    }
}