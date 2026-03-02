using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.ExchangeRate;
using MSG = Bacera.Gateway.ResultMessage.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Exchange Rate")]
[Route("api/" + VersionTypes.V1 + "/[Area]/exchange-rate")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]

public class ExchangeRateController : TenantBaseController
{
    private readonly TradingService _tradeService;
    private readonly TenantDbContext _tenantDbContext;

    public ExchangeRateController(TradingService tradeService
        , TenantDbContext tenantDbContext
    )
    {
        _tradeService = tradeService;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Pagination
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var roles = await _tradeService.ExchangeRateQueryAsync(criteria);
        return Ok(roles);
    }

    /// <summary>
    /// Details
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Detail(long id)
    {
        var item = await _tradeService.ExchangeRateGetResponseModelAsync(id);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// History
    /// </summary>
    /// <param name="id"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/history")]
    [ProducesResponseType(typeof(Result<Audit.Criteria, List<Audit>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> History(long id, [FromQuery] Audit.Criteria? criteria)
    {
        criteria ??= new Audit.Criteria();
        criteria.RowId = id;
        criteria.Type = AuditTypes.ExchangeRate;
        var items = await _tenantDbContext.Audits
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .Join(_tenantDbContext.Parties, x => x.PartyId, x => x.Id, (a, p) => new { a, p })
            .Select(x => new
            {
                Audit = x.a,
                OperatorName = x.p.NativeName
            })
            .ToListAsync();
        var result = items.Select(x => x.Audit.ApplyOperatorName(x.OperatorName)).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(M.CreateSpec spec)
    {
        var exists = await _tenantDbContext.ExchangeRates
            .AnyAsync(x => x.FromCurrencyId == (int)spec.FromCurrencyId
                           && x.ToCurrencyId == (int)spec.ToCurrencyId);
        if (exists)
            return BadRequest(Result.Error(ResultMessage.Common.RecordExists));

        var item = await _tradeService.ExchangeRateCreateAsync(spec);
        return item.IsEmpty() ? BadRequest() : Ok(item);
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, M.UpdateSpec spec)
    {
        if (id != spec.Id)
            return BadRequest();

        var existItem = await _tradeService.ExchangeRateGetResponseModelAsync(id);
        if (existItem.IsEmpty())
            return NotFound();

        // if (!isRateWithinLimit(spec.SellingRate, existItem.SellingRate) ||
        //     !isRateWithinLimit(spec.BuyingRate, existItem.BuyingRate))
        //     return BadRequest(Result.Error(MSG.OutOfRange));

        var item = await _tradeService.ExchangeRateUpdateAsync(spec, GetPartyId());
        return item.IsEmpty() ? BadRequest() : Ok(item);
    }

    private static bool IsRateWithinLimit(decimal rate, decimal existingRate)
        => !(rate > existingRate * 1.02m || rate < existingRate * 0.98m);
}