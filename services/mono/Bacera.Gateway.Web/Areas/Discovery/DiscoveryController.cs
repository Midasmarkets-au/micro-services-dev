using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Controllers.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Discovery;

[Tags("Public")]
[AllowAnonymous]
public class DiscoveryController(WebsiteDbContext websiteDbContext, IMyCache myCache) : BaseControllerV2
{
    private Task<List<string?>> GetNewsLanguages()
        => myCache.GetOrSetAsync("website_public_news_languages"
            , () => websiteDbContext.News.Select(e => e.Language).Distinct().ToListAsync()
            , TimeSpan.FromDays(7));


    [HttpGet("news/languages")]
    public async Task<IActionResult> NewsLanguages() => Ok(await GetNewsLanguages());

    [HttpGet("news")]
    public async Task<IActionResult> News([FromQuery] string language = LanguageTypes.English, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        language = LanguageTypes.ParseCrmLanguage(language);

        var query = websiteDbContext.News
            .Where(e => e.Language == language)
            .OrderByDescending(e => e.Id)
            .ToPublicModel();

        var count = await query.CountAsync();
        if (count > 100) count = 100;
        
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var result = Result.Of(items, new { language, page, size, count });
        return Ok(result);
    }


    [HttpGet("post")]
    public async Task<IActionResult> Post([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] string language = LanguageTypes.English)
    {
        language = LanguageTypes.ParseCrmLanguage(language);
        var query = websiteDbContext.Posts
#pragma warning disable EF1002
            .FromSqlRaw($"SELECT * FROM posts WHERE JSON_CONTAINS(Languages, '\"{language}\"')")
#pragma warning restore EF1002
            .OrderByDescending(e => e.Id)
            .ToPublicPageModel();

        var count = await query.CountAsync();
        if (count > 100) count = 100;
        
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var result = Result.Of(items, new { page, size, count });
        return Ok(result);
    }

    [HttpGet("post/{hashId}")]
    public async Task<IActionResult> PostDetail(string hashId)
    {
        var id = Gateway.Post.HashDecode(hashId);
        var post = await websiteDbContext.Posts
            .Where(e => e.Id == id)
            .ToPublicDetailModel()
            .SingleOrDefaultAsync();

        if (post == null) return NotFound(Result.Error("Post not found"));
        return Ok(post);
    }


    private Task<List<string?>> GetEconomicCalendarLanguages()
        => myCache.GetOrSetAsync("website_public_economic_calendar_languages"
            , () => websiteDbContext.EconomicCalendars.Select(e => e.Language).Distinct().ToListAsync()
            , TimeSpan.FromDays(7));


    [HttpGet("economic-calendar/languages")]
    public async Task<IActionResult> EconomicCalendarLanguages() => Ok(await GetEconomicCalendarLanguages());


    [HttpGet("economic-calendar")]
    public async Task<IActionResult> EconomicCalendar([FromQuery] string language = LanguageTypes.English,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        // language = LanguageTypes.ParseCrmLanguage(language);
        language = "en";
        var query = websiteDbContext.EconomicCalendars
            .Where(e => e.Language == language)
            .OrderByDescending(e => e.Id)
            .ToPublicModel();

        var count = await query.CountAsync();
        if (count > 100) count = 100;
        
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var result = Result.Of(items, new { language, page, size, count });
        return Ok(result);
    }
}