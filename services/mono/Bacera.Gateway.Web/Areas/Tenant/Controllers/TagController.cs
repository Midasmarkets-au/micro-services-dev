using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Tag")]
[Area("Tenant")]
[Route("api/" + VersionTypes.V1 + "/[Area]/[controller]")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TagController(TagService tagService, TenantDbContext tenantCtx, IMyCache cache) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Tag.QuerySpec spec)
    {
        var query = spec.Type switch
        {
            "account" => tenantCtx.Accounts.SelectMany(x => x.Tags.Select(y => new { x.Id, y.Name, spec.Type })),
            "party" => tenantCtx.Parties.SelectMany(x => x.Tags.Select(y => new { x.Id, y.Name, spec.Type })),
            _ => null
        };

        if (query == null) return BadRequest();
        var items = await query.Where(x => x.Id == spec.RowId).ToListAsync();
        var allTags = await tagService.GetAllTags();
        var result = allTags
            .Where(x => x.Type == spec.Type)
            .Select(x => new Tag.TypeBasicModel
            {
                Name = x.Name,
                Enabled = items.Any(y => y.Name == x.Name)
            })
            .ToList();
        return Ok(result);
    }


    [HttpGet("all")]
    public async Task<IActionResult> IndexAll() => Ok(await tagService.GetAllTags());

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] Tag.CreateOrUpdateSpec spec)
    {
        Func<string, string, long, Task<bool>> handler = spec.Enabled
            ? tagService.AddTagForTypeAsync
            : tagService.RemoveTagForTypeAsync;

        var result = await handler(spec.Type, spec.Name, spec.RowId);
        return result ? Ok() : BadRequest();
    }

    [HttpPut("refresh")]
    public IActionResult Refresh()
    {
        Task.Run(() => cache.KeyDeleteAsync(CacheKeys.GetAllTagKey(GetTenantId())));
        return Ok();
    }

    // [HttpPut("{name}")]
    // public async Task<IActionResult> Add(string type, long id, string name) =>
    //     Ok(await tagService.AddTagAsync(type, name, id));
    //
    // [HttpDelete("{name}")]
    // public async Task<IActionResult> Delete(string type, long id, string name) =>
    //     Ok(await tagService.DeleteTagAsync(type, name, id));


    // /// <summary>
    // /// Pagination
    // /// </summary>
    // /// <returns></returns>
    // [HttpGet]
    // [ProducesResponseType(typeof(Result<List<M>, Core.Models.Tenant._Extensions.Tag.Criteria>),
    //     StatusCodes.Status200OK)]
    // public async Task<IActionResult> Index([FromQuery] Core.Models.Tenant._Extensions.Tag.Criteria? criteria)
    // {
    //     criteria ??= new Core.Models.Tenant._Extensions.Tag.Criteria();
    //     var roles = await _userService.QueryTagsAsync(criteria);
    //     return Ok(roles);
    // }

    // /// <summary>
    // /// Tag details
    // /// </summary>
    // /// <param name="id"></param>
    // /// <returns></returns>
    // [HttpGet("{id:long}")]
    // [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> Detail(long id)
    // {
    //     var item = await _userService.GetTagAsync(id);
    //
    //     return Ok(item);
    // }
    //
    // /// <summary>
    // /// Delete
    // /// </summary>
    // /// <param name="id"></param>
    // /// <returns></returns>
    // [HttpDelete("{id:long}")]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // public async Task<IActionResult> Delete(long id)
    // {
    //     var item = await _userService.DeleteTagAsync(id);
    //     return item ? NoContent() : BadRequest();
    // }
}