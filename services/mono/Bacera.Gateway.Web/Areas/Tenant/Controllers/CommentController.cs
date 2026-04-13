
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Comment;

[Tags("Tenant/Comment")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class CommentController(TenantDbContext tenantDbContext) : TenantBaseController
{
    /// <summary>
    /// Comments pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        if (criteria.Type == CommentTypes.User)
        {
            var query = tenantDbContext.PartyComments.Where(x => x.PartyId == criteria.RowId);
            var items = await query
                .Where(x => x.PartyId == criteria.RowId)
                .Skip((criteria.Page - 1) * criteria.Size)
                .OrderByDescending(x => x.Id)
                .ToTenantUserPageModel()
                .ToListAsync();
            criteria.Total = await query.CountAsync();
            return Ok(Result.Of(items, criteria));
        }

        if (criteria.Type == CommentTypes.Account)
        {
            var query = tenantDbContext.AccountComments.Where(x => x.AccountId == criteria.RowId);
            var items = await query
                .Where(x => x.AccountId == criteria.RowId)
                .Skip((criteria.Page - 1) * criteria.Size)
                .OrderByDescending(x => x.Id)
                .ToTenantAccountPageModel()
                .ToListAsync();
            criteria.Total = await query.CountAsync();
            return Ok(Result.Of(items, criteria));
        }

        var comments = await tenantDbContext.Comments
            .PagedFilterBy(criteria)
            .ToTenantGeneralPageModel()
            .ToListAsync();

        return Ok(Result.Of(comments, criteria));
    }

    /// <summary>
    /// Get comment for type and id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("type-{type:int}/{id:long}")]
    [ProducesResponseType(typeof(List<M>), StatusCodes.Status200OK)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRow(int type, long id)
    {
        var item = await tenantDbContext.Comments
            .Where(x => x.Type == type)
            .Where(x => x.RowId == id)
            .ToListAsync();
        return Ok(item);
    }

    /// <summary>
    /// Get comment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Details(long id)
    {
        var item = await tenantDbContext.Comments
            .SingleOrDefaultAsync(x => x.Id == id);

        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(long id, [FromQuery] CommentTypes type)
    {
        if (type == CommentTypes.User)
        {
            var item0 = await tenantDbContext.PartyComments
                .SingleOrDefaultAsync(x => x.Id == id);

            if (item0 == null)
                return NotFound();

            tenantDbContext.PartyComments.Remove(item0);
            await tenantDbContext.SaveChangesAsync();
            return NoContent();
        }

        if (type == CommentTypes.Account)
        {
            var item1 = await tenantDbContext.AccountComments
                .SingleOrDefaultAsync(x => x.Id == id);

            if (item1 == null)
                return NotFound();

            tenantDbContext.AccountComments.Remove(item1);
            await tenantDbContext.SaveChangesAsync();
            return NoContent();
        }


        var item = await tenantDbContext.Comments
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        tenantDbContext.Comments.Remove(item);
        await tenantDbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Create comment
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] M.RequestModel spec)
    {
        if (spec.Type == CommentTypes.User)
        {
            if (await tenantDbContext.Parties.AllAsync(x => x.Id != spec.RowId))
                return NotFound("User not found");
                
            tenantDbContext.PartyComments.Add(new PartyComment
            {
                PartyId = spec.RowId,
                OperatorPartyId = GetPartyId(),
                Content = spec.Content,
                CreatedOn = DateTime.UtcNow
            });
            await tenantDbContext.SaveChangesAsync();
            return Ok();
        }

        if (spec.Type == CommentTypes.Account)
        {
            if (await tenantDbContext.Accounts.AllAsync(x => x.Id != spec.RowId))
                return NotFound("Account not found");
            
            tenantDbContext.AccountComments.Add(new AccountComment
            {
                AccountId = spec.RowId,
                OperatorPartyId = GetPartyId(),
                Content = spec.Content,
                CreatedOn = DateTime.UtcNow
            });
            await tenantDbContext.SaveChangesAsync();
            return Ok();
        }

        var item = new M
        {
            Content = spec.Content,
            Type = (short)spec.Type,
            RowId = spec.RowId,
            PartyId = GetPartyId()
        };
        await tenantDbContext.Comments.AddAsync(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }
}