
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.ContactRequest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Contact")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]

public class ContactController : TenantBaseController
{
    private readonly TenantDbContext _ctx;

    public ContactController(TenantDbContext ctx
    )
    {
        _ctx = ctx;
    }

    /// <summary>
    /// Pagination
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var roles = await _ctx.ContactRequests
            .PagedFilterBy(criteria)
            .Select(x => new M
            {
                Id = x.Id,
                Ip = x.Ip,
                Name = x.Name,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                PartyId = x.PartyId,
                CreatedOn = x.CreatedOn,
                IsArchived = x.IsArchived,
            })
            .ToListAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Get role detail with claims
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Detail(long id)
    {
        var item = await _ctx.ContactRequests
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    /// <summary>
    /// Archive
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/archive")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(long id)
    {
        var item = await _ctx.ContactRequests
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        item.IsArchived = 1;
        _ctx.ContactRequests.Update(item);
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Unarchive
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/unarchive")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(long id)
    {
        var item = await _ctx.ContactRequests
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        item.IsArchived = 0;
        _ctx.ContactRequests.Update(item);
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }
}