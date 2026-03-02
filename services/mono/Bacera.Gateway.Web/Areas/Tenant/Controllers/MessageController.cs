using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Context;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Bacera.Gateway.Message;

[Tags("Tenant/Message")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class MessageController(TenantDbContext tenantCtx, MyDbContextPool pool, IServiceProvider provider)
    : TenantBaseController
{
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
        var items = await tenantCtx.Messages
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result.Of(items, criteria));
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
        var item = await tenantCtx.Messages
            .SingleOrDefaultAsync(x => x.Id == id);

        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await tenantCtx.Messages
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        tenantCtx.Messages.Remove(item);
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Create Message
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        try
        {
            var item = new M
            {
                Title = spec.Title,
                Content = spec.Content,
                Type = (int)spec.Type,
                PartyId = spec.PartyId,
                SenderType = (int)spec.SenderType,
                SenderId = spec.SenderId ?? 0,
                ReferenceType = (int)spec.ReferenceType,
                ReferenceId = spec.ReferenceId ?? 0,
            };
            await tenantCtx.Messages.AddAsync(item);
            await tenantCtx.SaveChangesAsync();
            return Ok(item);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    /// <summary>
    /// Update Message
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] M.CreateSpec spec)
    {
        var item = await tenantCtx.Messages
            .SingleOrDefaultAsync(x => x.Id == id);
        if (item == null || item.IsEmpty())
            return NotFound();

        try
        {
            item.Title = spec.Title;
            item.Content = spec.Content;
            item.Type = (int)spec.Type;
            item.PartyId = spec.PartyId;
            item.SenderType = (int)spec.SenderType;
            item.SenderId = spec.SenderId ?? 0;
            item.ReferenceType = (int)spec.ReferenceType;
            item.ReferenceId = spec.ReferenceId ?? 0;

            item.UpdatedOn = DateTime.UtcNow;

            tenantCtx.Messages.Update(item);
            await tenantCtx.SaveChangesAsync();
            return Ok(item);
        }
        catch (Exception e)
        {
            return BadRequest(Result.Error(e.Message));
        }
    }

    /// <summary>
    /// Send Popup Notification
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("send-popup")]
    public async Task<IActionResult> SendPopNotification([FromBody] CreateMessagePopupSpec spec)
    {
        if (!spec.IsValid()) return BadRequest("Invalid request");
        if (!pool.GetTenantIds().Contains(spec.TenantId))
            return BadRequest("Tenant not found");
        
        if (SendMsgKey != spec.Key)
            return BadRequest();

        using var scope = provider.CreateTenantScope(spec.TenantId);
        var sendMsgSvc = scope.ServiceProvider.GetRequiredService<ISendMessageService>();

        var dto = spec.ToMessagePopupDTO();
        if (spec.PartyId != 0)
        {
            await sendMsgSvc.SendPopupToPartyAsync(spec.TenantId, spec.PartyId, dto);
        }
        else
        {
            await sendMsgSvc.SendPopupToRoleAsync(spec.TenantId, spec.Role, dto);
        }
        return Ok();
    }

    private const string SendMsgKey = "send-msg-12345";
}