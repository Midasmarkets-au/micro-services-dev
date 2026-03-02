using Bacera.Gateway.Context;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers;

[Tags("Public/Referral Code")]
public class ReferralCodeController(
    CentralDbContext centralDbContext,
    ILogger<ReferralCodeController> logger,
    IServiceProvider serviceProvider)
    : BaseController
{
    /// <summary>
    /// Get referral code whit supplement
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReferralCode.ClientResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string code)
    {
        var centralReferralCode = await centralDbContext.CentralReferralCodes
            .Where(x => x.Code == code.Trim())
            .FirstOrDefaultAsync();
        if (centralReferralCode == null)
            return NotFound();

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(centralReferralCode.TenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        try
        {
            var conn = ctx.Database.GetConnectionString() ?? string.Empty;
            var connection = Utils.PasswordHideRegex.Replace(conn, "Password=***");
            logger.LogInformation("Auth_Register_TenantDb_Connection: {Connection}", connection);
            if (!ctx.IsConnectionValid())
            {
                return BadRequest("__INVALID_CONNECTION__");
            }
        }
        catch (Exception e)
        {
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connection = pool.GetConnectionStringByTenantId(centralReferralCode.TenantId);
            connection = Utils.PasswordHideRegex.Replace(connection, "Password=***");
            BcrLog.Slack(
                $"error_checking_tenant_db_connection: {e.Message}, tenantId: {centralReferralCode.TenantId}, Connection:{connection}");
            return BadRequest();
        }

        // await using var ctx = pool.CreateTenantDbContext(centralReferralCode.TenantId);
        try
        {
            var referralCode = await ctx.ReferralCodes
                .Where(x => x.Code == code.Trim())
                .FirstAsync();

            var supplement = await ctx.Supplements
                .Where(x => x.Type == (int)SupplementTypes.ReferralCode)
                .Where(x => x.RowId == referralCode.Id)
                .FirstOrDefaultAsync();

            var response = referralCode.ToClientResponse();
            if (supplement != null)
                response.SupplementJson = supplement.Data;

            return Ok(response);
        }
        catch (Exception e)
        {
            BcrLog.Slack($"ReferralController_Get_Error: {e.Message}");
            return NotFound();
        }
    }
}