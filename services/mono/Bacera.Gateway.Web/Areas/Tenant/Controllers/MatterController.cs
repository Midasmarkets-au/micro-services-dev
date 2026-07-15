
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Matter")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class MatterController(TenantDbContext tenantCtx) : TenantBaseController
{
    [HttpGet("{id:long}/state-detail")]
    public async Task<IActionResult> GetStateDetails(long id)
    {
        var k8sItem = await tenantCtx.MatterK8s
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Type,
                StateChanges = x.Activities
                    .OrderBy(a => a.PerformedOn)
                    .Select(a => new Matter.StateChangeModel
                    {
                        StateRaw = a.ToStateId,
                        Info = a.Data,
                        PerformedOn = a.PerformedOn,
                        Operator = a.Party.Email,
                    })
                    .ToList(),
            })
            .SingleOrDefaultAsync();

        if (k8sItem != null)
        {
            // Deposit/Payment were never migrated to k8s tables — join back to the legacy
            // tables by Id (same shared-PK value the dual-write interceptor mirrors).
            var callbackBody = k8sItem.Type == (int)MatterTypes.Deposit
                ? await tenantCtx.Deposits
                    .Where(d => d.Id == id)
                    .Select(d => d.Payment.CallbackBody)
                    .SingleOrDefaultAsync() ?? "{}"
                : "{}";

            return Ok(new Matter.StateDetailModel
            {
                StateChanges = k8sItem.StateChanges,
                CallbackBodyRaw = callbackBody,
            });
        }

        // Fallback for matters created before the k8s dual-write went live (no mirror row yet).
        var item = await tenantCtx.Matters
            .Where(x => x.Id == id)
            .ToStateDetailModel()
            .SingleOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }
}