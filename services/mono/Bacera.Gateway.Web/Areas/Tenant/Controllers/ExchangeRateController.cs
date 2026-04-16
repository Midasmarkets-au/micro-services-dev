using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = ExchangeRate;

[Tags("Tenant/Exchange Rate")]
[Route("api/" + VersionTypes.V1 + "/[Area]/exchange-rate")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ExchangeRateController(TradingService tradingSvc, TenantDbContext db)
    : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        return Ok(await tradingSvc.ExchangeRateQueryAsync(criteria));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id)
    {
        var item = await tradingSvc.ExchangeRateGetResponseModelAsync(id);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    [HttpGet("{id:long}/history")]
    public async Task<IActionResult> History(long id, [FromQuery] Audit.Criteria? criteria)
    {
        criteria ??= new Audit.Criteria();
        criteria.RowId = id;
        criteria.Type = AuditTypes.ExchangeRate;
        var items = await db.Audits
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .Join(db.Parties, x => x.PartyId, x => x.Id, (a, p) => new { a, p })
            .Select(x => new { Audit = x.a, OperatorName = x.p.NativeName })
            .ToListAsync();
        return Ok(items.Select(x => x.Audit.ApplyOperatorName(x.OperatorName)).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create(M.CreateSpec spec)
    {
        var exists = await db.ExchangeRates
            .AnyAsync(x => x.FromCurrencyId == (int)spec.FromCurrencyId
                        && x.ToCurrencyId == (int)spec.ToCurrencyId);
        if (exists)
            return BadRequest(Result.Error(ResultMessage.Common.RecordExists));
        var item = await tradingSvc.ExchangeRateCreateAsync(spec);
        return item.IsEmpty() ? BadRequest() : Ok(item);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, M.UpdateSpec spec)
    {
        if (id != spec.Id) return BadRequest();
        var existItem = await tradingSvc.ExchangeRateGetResponseModelAsync(id);
        if (existItem.IsEmpty()) return NotFound();
        var item = await tradingSvc.ExchangeRateUpdateAsync(spec, GetPartyId());
        return item.IsEmpty() ? BadRequest() : Ok(item);
    }
}
