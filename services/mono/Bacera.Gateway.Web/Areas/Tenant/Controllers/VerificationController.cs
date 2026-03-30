using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using M = Bacera.Gateway.Verification;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Verification")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class VerificationController(
    TenantDbContext tenantCtx,
    UserService userSvc)
    : TenantBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<VerificationDTO>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria = null)
    {
        criteria ??= new M.Criteria();
        criteria.Type ??= VerificationTypes.Verification;

        var hideEmail = ShouldHideEmail();
        var items = await tenantCtx.Verifications
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();

        var comments = await tenantCtx.Comments
            .Where(x => x.Type == (int)CommentTypes.Verification)
            .Where(x => items.Select(y => y.Id).Contains(x.RowId))
            .Select(x => x.RowId)
            .ToListAsync();

        foreach (var item in items)
            item.HasComment = comments.Contains(item.Id);

        await userSvc.ApplyUserBlackListInfo(items.Select(x => x.User).ToList());
        return Ok(Result<List<VerificationDTO>, M.Criteria>.Of(items, criteria));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(VerificationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tenantCtx.Verifications
            .Include(x => x.VerificationItems)
            .Include(x => x.Party)
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();

        var comments = await tenantCtx.Comments
            .Where(x => x.Type == (int)CommentTypes.Verification && x.RowId == id)
            .Select(x => new Comment.TenantDTO
            {
                Content   = x.Content,
                CreatedOn = x.CreatedOn,
            })
            .ToListAsync();

        return Ok(item.ToDTO().SetComments(comments));
    }
}
