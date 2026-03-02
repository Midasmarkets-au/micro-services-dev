using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bacera.Gateway.Web.Services;
using System.Text.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[Area("Tenant")]
[Tags("Tenant/Documents")]
[Route("api/" + VersionTypes.V2 + "/[Area]/documents")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class DocumentsController(TenantDbContext tenantCtx, IStorageService storage, ILogger<DocumentsController> logger)
    : TenantBaseControllerV2
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int size = 20,
        [FromQuery] string? site = null)
    {
        if (string.IsNullOrWhiteSpace(site))
            return BadRequest(new { message = "Site is required" });

        if (page < 1) page = 1;
        if (size < 1) size = 20;
        if (size > 200) size = 200;

        var q = tenantCtx.Documents.AsNoTracking().Where(x => x.Site == site).OrderBy(x => x.Id);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Site,
                x.Link,
                x.Languages,
                x.OperatorInfo,
                x.Comment,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        var languages = new Dictionary<string, string>
        {
            ["en-us"] = "English",
            ["jp-jp"] = "Japanese",
            ["ms-my"] = "Malay",
            ["th-th"] = "Thai",
            ["vi-vn"] = "Vietnamese",
            ["zh-cn"] = "Simplified Chinese",
            ["zh-hk"] = "Traditional Chinese",
            ["ko-kr"] = "Korean",
            ["es-es"] = "Spanish",
            ["km-kh"] = "Cambodian",
        };

        var result = new
        {
            data = items,
            criteria = new { size, page, total, site },
            languages
        };
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> History(long id, [FromQuery] int page = 1, [FromQuery] int size = 20,
        [FromQuery] string? language = null)
    {
        if (page < 1) page = 1;
        if (size < 1) size = 20;
        if (size > 200) size = 200;

        var q = tenantCtx.HistoricalDocuments.AsNoTracking().Where(x => x.DocumentId == id);
        if (!string.IsNullOrWhiteSpace(language)) q = q.Where(x => x.Language == language);
        q = q.OrderByDescending(x => x.CreatedAt);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(x => new
            {
                x.Id,
                x.DocumentId,
                x.Link,
                x.Language,
                x.Site,
                x.OperatorInfo,
                x.Comment,
                x.CreatedAt,
                x.UpdatedAt,
                reference = x.DocumentId + "-" + x.CreatedAt.ToString("yyyyMMdd") + "-" + x.Language
            })
            .ToListAsync();

        var result = new
        {
            data = items,
            criteria = new { size, page, total, language }
        };
        return Ok(result);
    }

    public sealed class DocumentsUploadForm
    {
        public List<IFormFile> Files { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public string Operator_Info { get; set; } = string.Empty;
        public string Type { get; set; } = ""; // "pdf" for PDFs
    }

    [HttpPost("{id:long}/upload")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(long id, [FromForm] DocumentsUploadForm form)
    {
        var document = await tenantCtx.Documents.SingleOrDefaultAsync(x => x.Id == id);
        if (document == null) return NotFound(new { message = "Document not found" });
        if (form.Files.Count == 0) return BadRequest(new { message = "No files" });
        if (form.Languages.Count != form.Files.Count) return BadRequest(new { message = "Languages count mismatch" });

        var fileName = document.Link ?? "document";
        var site = document.Site ?? "";
        var isPdf = string.Equals(form.Type, "pdf", StringComparison.OrdinalIgnoreCase);

        var links = new Dictionary<string, string>();
        var operatorInfo = form.Operator_Info;

        for (var i = 0; i < form.Files.Count; i++)
        {
            var file = form.Files[i];
            var lang = form.Languages[i];
            var ext = Path.GetExtension(file.FileName);

            if (isPdf)
            {
                var dir = $"docs/{site}/{lang}/";
                var name = $"{fileName}{ext}";
                await using var stream = file.OpenReadStream();
                var (ok, key) = await storage.UploadPublicFileAsync(stream, dir, name, ext, file.ContentType,
                    GetTenantId(), GetPartyId(), changeFileName: false);
                if (!ok) return Problem($"Upload failed for {lang}");
                links[lang] = $"https://{Environment.GetEnvironmentVariable("AWS_PUBLIC_BUCKET")}.s3.amazonaws.com/{key}";
                continue;
            }

            // Ensure languages json contains this lang
            try
            {
                var langsDict = string.IsNullOrWhiteSpace(document.Languages)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(document.Languages!)
                      ?? new Dictionary<string, string>();
                if (!langsDict.ContainsKey(lang))
                {
                    langsDict[lang] = lang;
                    document.Languages = JsonSerializer.Serialize(langsDict);
                    await tenantCtx.SaveChangesAsync();
                }
            }
            catch { }

            var history = new Core.Models.Tenant.HistoricalDocument
            {
                DocumentId = id,
                Language = lang,
                Site = site,
                OperatorInfo = operatorInfo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            tenantCtx.HistoricalDocuments.Add(history);
            await tenantCtx.SaveChangesAsync();

            var dir2 = $"docs/word-files/{site}/{history.Id}/";
            var name2 = $"{fileName}-{document.Id}-{DateTime.UtcNow:yyyyMMdd}-{lang}{ext}";
            await using var stream2 = file.OpenReadStream();
            var (ok2, key2) = await storage.UploadPublicFileAsync(stream2, dir2, name2, ext, file.ContentType,
                GetTenantId(), GetPartyId(), changeFileName: false);
            if (!ok2) return Problem($"Upload failed for {lang}");

            history.Link = key2;
            await tenantCtx.SaveChangesAsync();

            links[lang] = $"https://{Environment.GetEnvironmentVariable("AWS_PUBLIC_BUCKET")}.s3.amazonaws.com/{key2}";
        }

        return Ok(links);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Core.Models.Tenant.Document model)
    {
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;
        tenantCtx.Documents.Add(model);
        await tenantCtx.SaveChangesAsync();
        return Ok(model);
    }
}


