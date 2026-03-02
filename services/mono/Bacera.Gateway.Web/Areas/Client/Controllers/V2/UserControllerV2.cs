using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/User")]
[Area("Client")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/[Area]/user")]
public class UserControllerV2(
    AuthDbContext authCtx,
    TenantDbContext tenantCtx,
    UserService userService
) : ClientBaseControllerV2
{
    /// <summary>
    /// Update User Phone Number
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("change-phone-number")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeUserPhoneNumber([FromBody] ChangePhoneNumberRequestModel spec)
    {
        if (!PhoneNumberRegionCodeTypes.All.Contains(spec.ccc))
            return BadRequest(Result.Error(ResultMessage.Verification.RegionCodeInvalid));

        var (tenantId, partyId) = (GetTenantId(), GetPartyId());
        var user = await authCtx.Users.SingleOrDefaultAsync(x =>
            x.PartyId == partyId && x.TenantId == tenantId);
        if (user == null) return BadRequest(Result.Error(ResultMessage.Common.UserNotFound));

        user.CCC = spec.ccc;
        user.PhoneNumber = spec.phoneNumber;
        user.PhoneNumberConfirmed = true;
        user.UpdatedOn = DateTime.UtcNow;

        await authCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Update User language
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("profile/language")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetLanguage(SetLanguageRequestModel spec)
    {
        long tenantId = GetTenantId(), partyId = GetPartyId();
        var task1 = Task.Run(async () =>
        {
            var email = await authCtx.Users
                .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync();

            var users = await authCtx.Users
                .Where(x => x.Email == email)
                .ToListAsync();

            users.ForEach(x =>
            {
                x.Language = spec.Language;
                x.UpdatedOn = DateTime.UtcNow;
            });

            await authCtx.SaveChangesAsync();
        });

        var task2 = Task.Run(async () =>
        {
            var party = await tenantCtx.Parties
                .Where(x => x.Id == partyId)
                .SingleAsync();

            party.Language = spec.Language;
            party.UpdatedOn = DateTime.UtcNow;
            await tenantCtx.SaveChangesAsync();
        });

        await Task.WhenAll(task1, task2);
        await userService.UpdateSearchAsync(new User.Criteria { TenantId = tenantId, PartyId = partyId });
        return Ok();
    }
}