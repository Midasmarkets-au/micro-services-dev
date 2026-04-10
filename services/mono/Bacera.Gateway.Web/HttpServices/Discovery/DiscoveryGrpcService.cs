using Bacera.Gateway.Services;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using ProtoPost = Http.V1.Post;

namespace Bacera.Gateway.Web.HttpServices.Discovery;

/// <summary>
/// gRPC JSON Transcoding implementation of DiscoveryService.
/// Replaces Areas/Discovery/DiscoveryController.cs.
/// Endpoints are public (AllowAnonymous) — no authentication required.
/// Routes are defined via google.api.http annotations in discovery.proto.
/// </summary>
public class DiscoveryGrpcService(WebsiteDbContext db, IMyCache cache)
    : DiscoveryService.DiscoveryServiceBase
{
    // ─── News ─────────────────────────────────────────────────────────────────

    public override async Task<GetNewsLanguagesResponse> GetNewsLanguages(
        GetNewsLanguagesRequest request, ServerCallContext context)
    {
        var languages = await cache.GetOrSetAsync(
            "website_public_news_languages",
            () => db.News.Select(e => e.Language).Distinct().ToListAsync(),
            TimeSpan.FromDays(7));

        var inner = new LanguagesResponse();
        inner.Languages.AddRange(languages.Where(l => l != null).Cast<string>());
        return new GetNewsLanguagesResponse { Data = inner };
    }

    public override async Task<ListNewsResponse> ListNews(
        ListNewsRequest request, ServerCallContext context)
    {
        var language = LanguageTypes.ParseCrmLanguage(
            string.IsNullOrEmpty(request.Language) ? LanguageTypes.English : request.Language);

        var page = request.Page > 0 ? request.Page : 1;
        var size = request.Size > 0 ? request.Size : 20;

        var query = db.News
            .Where(e => e.Language == language)
            .OrderByDescending(e => e.Id)
            .ToPublicModel();

        var total = await query.CountAsync();
        if (total > 100) total = 100;

        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        var response = new ListNewsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = page,
                Size      = size,
                Total     = total,
                PageCount = (int)Math.Ceiling((double)total / size),
                HasMore   = page * size < total,
            }
        };

        response.Data.AddRange(items.Select(n => new NewsItem
        {
            HashId      = n.Pid?.ToString() ?? "",
            Title       = n.Title ?? "",
            Content     = n.Intro ?? "",
            Language    = n.Language,
            PublishedAt = n.PublishedDate?.ToString("O") ?? "",
        }));

        return response;
    }

    // ─── Posts ────────────────────────────────────────────────────────────────

    public override async Task<ListPostsResponse> ListPosts(
        ListPostsRequest request, ServerCallContext context)
    {
        var language = LanguageTypes.ParseCrmLanguage(
            string.IsNullOrEmpty(request.Language) ? LanguageTypes.English : request.Language);

        var page = request.Page > 0 ? request.Page : 1;
        var size = request.Size > 0 ? request.Size : 20;

#pragma warning disable EF1002
        var query = db.Posts
            .FromSqlRaw($"SELECT * FROM posts WHERE JSON_CONTAINS(Languages, '\"{language}\"')")
            .OrderByDescending(e => e.Id)
            .ToPublicPageModel();
#pragma warning restore EF1002

        var total = await query.CountAsync();
        if (total > 100) total = 100;

        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        var response = new ListPostsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = page,
                Size      = size,
                Total     = total,
                PageCount = (int)Math.Ceiling((double)total / size),
                HasMore   = page * size < total,
            }
        };

        response.Data.AddRange(items.Select(p => new ProtoPost
        {
            HashId      = p.HashId,
            Title       = p.Title ?? "",
            Content     = "",           // list view — body not returned
            Language    = p.LanguageCode ?? "",
            PublishedAt = p.PublishTime?.ToString("O") ?? "",
        }));

        return response;
    }

    public override async Task<GetPostResponse> GetPost(GetPostRequest request, ServerCallContext context)
    {
        var id = Gateway.Post.HashDecode(request.HashId);
        var post = await db.Posts
            .Where(e => e.Id == id)
            .ToPublicDetailModel()
            .SingleOrDefaultAsync();

        if (post == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Post not found"));

        return new GetPostResponse
        {
            Data = new ProtoPost
            {
                HashId      = post.HashId,
                Title       = post.Title ?? "",
                Content     = post.Body ?? "",
                Language    = post.LanguageCode ?? "",
                PublishedAt = post.PublishTime?.ToString("O") ?? "",
            }
        };
    }

    // ─── Economic Calendar ────────────────────────────────────────────────────

    public override async Task<GetEconomicCalendarLanguagesResponse> GetEconomicCalendarLanguages(
        GetEconomicCalendarLanguagesRequest request, ServerCallContext context)
    {
        var languages = await cache.GetOrSetAsync(
            "website_public_economic_calendar_languages",
            () => db.EconomicCalendars.Select(e => e.Language).Distinct().ToListAsync(),
            TimeSpan.FromDays(7));

        var inner = new LanguagesResponse();
        inner.Languages.AddRange(languages.Where(l => l != null).Cast<string>());
        return new GetEconomicCalendarLanguagesResponse { Data = inner };
    }

    public override async Task<ListEconomicCalendarResponse> ListEconomicCalendar(
        ListEconomicCalendarRequest request, ServerCallContext context)
    {
        // Language is pinned to "en" matching original controller behavior
        const string language = "en";

        var page = request.Page > 0 ? request.Page : 1;
        var size = request.Size > 0 ? request.Size : 20;

        var query = db.EconomicCalendars
            .Where(e => e.Language == language)
            .OrderByDescending(e => e.Id)
            .ToPublicModel();

        var total = await query.CountAsync();
        if (total > 100) total = 100;

        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        var response = new ListEconomicCalendarResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = page,
                Size      = size,
                Total     = total,
                PageCount = (int)Math.Ceiling((double)total / size),
                HasMore   = page * size < total,
            }
        };

        response.Data.AddRange(items.Select(e => new EconomicCalendarItem
        {
            Date      = e.Date.ToString("O"),
            EventName = e.Event ?? "",
            Impact    = e.Impact ?? "",
            Language  = e.Language,
        }));

        return response;
    }
}
