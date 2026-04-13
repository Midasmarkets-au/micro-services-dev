
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Message;

[Tags("Client/Message")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class MessageController : ClientBaseController
{
    private readonly TenantDbContext _tenantDbContext;

    public MessageController(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Message pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await _tenantDbContext.Messages
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result<List<M>, M.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Message by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Details(long id)
    {
        var item = await _tenantDbContext.Messages
            .Where(x => x.PartyId == GetPartyId())
            .SingleOrDefaultAsync(x => x.Id == id);

        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var item = await _tenantDbContext.Messages
            .Where(x => x.PartyId == GetPartyId())
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        item.ReadOn = DateTime.UtcNow;
        _tenantDbContext.Messages.Update(item);
        await _tenantDbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Mark messages as read by batch
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPut("read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BatchMarkAsRead([FromBody] List<long> ids)
    {
        var items = await _tenantDbContext.Messages
                .Where(x => x.PartyId == GetPartyId())
                .Where(x => ids.Contains(x.Id))
                .Where(x => x.ReadOn == null)
                .ToListAsync()
            ;

        if (!items.Any())
            return NoContent();

        foreach (var item in items)
        {
            item.ReadOn = DateTime.UtcNow;
        }

        _tenantDbContext.Messages.UpdateRange(items);
        var read = await _tenantDbContext.SaveChangesAsync();
        return Ok(new { read });
    }
}