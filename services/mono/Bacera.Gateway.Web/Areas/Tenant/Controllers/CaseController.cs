
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using HashidsNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;


[Tags("Tenant/Case")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class CaseController : TenantBaseController
{
    private readonly TenantDbContext _ctx;
    private readonly GptService _gptService;

    public CaseController(TenantDbContext ctx, GptService gptService)
    {
        _ctx = ctx;
        _gptService = gptService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Case.Criteria? criteria)
    {
        criteria ??= new Case.Criteria();
        var items = await _ctx.Cases
            .PagedFilterBy(criteria)
            .ToTenantBasicModel()
            .ToListAsync();
        return Ok(Result<List<Case.TenantBasicViewModel>, Case.Criteria>.Of(items, criteria));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var item = await _ctx.Cases
            .Where(x => x.Id == id)
            .Include(x => x.InverseReply)
            .ThenInclude(x => x.Category)
            .Include(x => x.InverseReply)
            .ThenInclude(x => x.CaseLanguages)
            .ToTenantResponseModel(true)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        item.InverseReply = item.InverseReply.OrderBy(x => x.CreatedOn).ToList();
        return Ok(item);
    }

    [HttpPost("{id:long}/reply")]
    public async Task<IActionResult> Reply(long id, [FromBody] Case.ReplySpec spec)
    {
        var item = await _ctx.Cases.FindAsync(id);
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        if (!item.CanReply()) return BadRequest("__CASE_CANNOT_REPLY__");
        var operatorPartyId = GetPartyId();
        var reply = spec.ToEntity(operatorPartyId).ReplyTo(item);
        if (spec is { SourceLanguage: not null, TargetLanguage: not null })
        {
            var (result, translatedContent) = await _gptService.TranslateTextFromSourceToTargetAsync(
                spec.SourceLanguage, spec.TargetLanguage, spec.Content);
            if (!result) return BadRequest("__TRANSLATION_FAILED__");
            reply.Content = translatedContent;
            reply.CaseLanguages.Add(CaseLanguage.Build(0, spec.SourceLanguage, spec.Content));
        }

        _ctx.Cases.Add(reply);
        _ctx.Cases.Update(item);
        await _ctx.SaveChangesAsync();
        return Ok(reply);
    }

    [HttpPost("{id:long}/claim")]
    public async Task<IActionResult> Claim(long id)
    {
        var operatorPartyId = GetPartyId();
        var item = await _ctx.Cases.FindAsync(id);
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        if (item.IsClaimed()) return BadRequest("__CASE_ALREADY_CLAIMED__");
        item.AssignTo(operatorPartyId);
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:long}/close")]
    public async Task<IActionResult> Close(long id)
    {
        var item = await _ctx.Cases.FindAsync(id);
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        item.Status = (short)CaseStatusTypes.Closed;
        item.UpdatedOn = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:long}/reopen")]
    public async Task<IActionResult> Reopen(long id)
    {
        var item = await _ctx.Cases
            .Where(x => x.Id == id && x.Status == (short)CaseStatusTypes.Closed)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        item.Status = (short)CaseStatusTypes.Processing;
        item.UpdatedOn = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:long}/translate")]
    public async Task<IActionResult> Translate(long id, Case.TenantTranslateSpec spec)
    {
        var item = await _ctx.Cases
            .Where(x => x.Id == id)
            .Include(x => x.CaseLanguages)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound("__CASE_NOT_FOUND__");

        var languageExisted = item.CaseLanguages.FirstOrDefault(x => x.Language == spec.TargetLanguage);
        if (languageExisted != null) return Ok(languageExisted.Content);

        var (result, translatedContent) =
            await _gptService.TranslateTextToTargetAsync(spec.TargetLanguage, item.Content);
        if (!result) return BadRequest("__TRANSLATION_FAILED__");

        var caseLanguage = CaseLanguage.Build(item.Id, spec.TargetLanguage, translatedContent);
        _ctx.CaseLanguages.Add(caseLanguage);
        await _ctx.SaveChangesAsync();
        return Ok(translatedContent);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Case.TenantCreateSpec spec)
    {
        var item = spec.ToEntity();
        if (spec is { SourceLanguage: not null, TargetLanguage: not null })
        {
            var (result, translatedContent) =
                await _gptService.TranslateTextFromSourceToTargetAsync(spec.SourceLanguage, spec.TargetLanguage,
                    spec.Content);
            if (!result) return BadRequest("__TRANSLATION_FAILED__");

            item.Content = translatedContent;
            item.CaseLanguages.Add(CaseLanguage.Build(0, spec.SourceLanguage, spec.Content));
        }

        item.IsAdmin = true;
        item.PartyId = GetPartyId();
        item.CreatedOn = DateTime.UtcNow;
        item.UpdatedOn = DateTime.UtcNow;
        _ctx.Cases.Add(item);
        await _ctx.SaveChangesAsync();

        var hashIds = new Hashids("BCRCaseId", 6, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        item.CaseId = hashIds.Encode((int)item.Id);
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }

    [HttpGet("category")]
    public async Task<IActionResult> GetCategories([FromQuery] long? parentCategoryId)
        => Ok(await _ctx.CaseCategories
            .Where(x => x.ParentId == parentCategoryId)
            .ToCategoryResponseModel()
            .ToListAsync());
}