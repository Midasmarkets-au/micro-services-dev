using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Comment;

[Tags("Sales/Comment")]
public class CommentController : SalesBaseController
{
    private readonly TenantDbContext _tenantDbContext;

    public CommentController(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

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
        var items = await _tenantDbContext.Comments
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Get comment for type and id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="commentId"></param>
    /// <returns></returns>
    [HttpGet("type-{type:int}/{commentId:long}")]
    [ProducesResponseType(typeof(List<M>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRow(int type, long commentId)
    {
        var item = await _tenantDbContext.Comments
            .Where(x => x.Type == type)
            .Where(x => x.RowId == commentId)
            .ToListAsync();
        return Ok(item);
    }

    /// <summary>
    /// Get comment
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns></returns>
    [HttpGet("{commentId:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Details(long commentId)
    {
        var item = await _tenantDbContext.Comments
            .SingleOrDefaultAsync(x => x.Id == commentId);

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
        var item = await _tenantDbContext.Comments
            .Where(x => x.PartyId == GetPartyId())
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        _tenantDbContext.Comments.Remove(item);
        await _tenantDbContext.SaveChangesAsync();
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
        var item = new M
        {
            Content = spec.Content,
            Type = (short)spec.Type,
            RowId = spec.RowId,
            PartyId = GetPartyId()
        };
        await _tenantDbContext.Comments.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
        return Ok(item);
    }
}