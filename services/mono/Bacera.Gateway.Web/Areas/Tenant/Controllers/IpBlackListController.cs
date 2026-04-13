using System.Net;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Areas.Tenant.Helpers;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.IpBlackList;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Tags("Tenant/IP Black List")]
[Route("api/" + VersionTypes.V1 + "/[Area]/ip-black-list")]
public class IpBlackListController(CentralDbContext centralDbContext, AuthDbContext authDbContext, IMyCache myCache)
    : TenantBaseController
{
    private const int UploadBatchSize = 200;
    private const long MaxUploadBytes = 25 * 1024 * 1024;

    private readonly string _ipKey = CacheKeys.GetBlackedIpHashKey();


    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await centralDbContext.IpBlackLists
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<IpBlackList.TenantPageModel>, M.Criteria>.Of(items, criteria));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        if (!IPAddress.TryParse(spec.Ip, out _)) return BadRequest("__INVALID_IP_ADDRESS__");
        var exists = await centralDbContext.IpBlackLists.AnyAsync(x => x.Ip == spec.Ip);
        if (exists) return BadRequest("__IP_ALREADY_EXISTS__");
        var entity = spec.ToEntity(await GetOperatorNameAsync());
        centralDbContext.IpBlackLists.Add(entity);
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity.Ip);
        return Ok(entity);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await centralDbContext.IpBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        centralDbContext.IpBlackLists.Remove(entity);
        await centralDbContext.SaveChangesAsync();
        await RemoveCacheAsync(entity.Ip);
        return Ok();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var entity = await centralDbContext.IpBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        await RemoveCacheAsync(entity.Ip);
        spec.ApplyTo(entity, await GetOperatorNameAsync());
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity.Ip);
        return Ok(entity);
    }

    [HttpPut("reload-cache")]
    public async Task<IActionResult> ReloadCache() => Ok(await ReloadCacheAsync());

    [HttpPost("upload-ips")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26214400)]
    public async Task<IActionResult> UploadIps(IFormFile file, [FromQuery] int sheetIndex = 0,
        [FromQuery] string? sheetName = null)
    {
        if (file == null || file.Length == 0) return BadRequest("__FILE_REQUIRED__");
        if (file.Length > MaxUploadBytes) return BadRequest("__FILE_TOO_LARGE__");
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("__INVALID_FILE_EXTENSION__");

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        using var workbook = new XLWorkbook(ms);
        if (!BlacklistExcelImportHelper.TryGetWorksheet(workbook, sheetName, sheetIndex, out var ws, out var sheetErr))
            return BadRequest(sheetErr);

        var headerRow = BlacklistExcelImportHelper.FindHeaderRow(ws);
        if (headerRow == 0) return BadRequest("__EMPTY_SHEET__");

        var map = BlacklistExcelImportHelper.ResolveColumns(ws, headerRow);
        if (map.IpCol == null || map.ReasonCol == null)
            return BadRequest("__MISSING_COLUMNS__");

        var tracked = await centralDbContext.IpBlackLists.ToListAsync();
        var byCanonical = new Dictionary<string, IpBlackList>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in tracked)
        {
            var key = BlacklistExcelImportHelper.TryCanonicalIp(e.Ip, out var c) ? c : e.Ip.Trim();
            if (!byCanonical.ContainsKey(key))
                byCanonical[key] = e;
        }

        var result = new BlacklistUploadResult();
        var op = await GetOperatorNameAsync();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        var pendingSinceLastSave = 0;
        var ipColLetter = ws.Column(map.IpCol!.Value).ColumnLetter();

        string Cell(int row, int col) => BlacklistExcelImportHelper.CellText(ws.Cell(row, col));

        for (var r = headerRow + 1; r <= lastRow; r++)
        {
            var reasonRaw = Cell(r, map.ReasonCol!.Value);
            if (string.IsNullOrWhiteSpace(reasonRaw))
            {
                result.SkippedEmptyOrInvalid++;
                continue;
            }

            var ipCell = Cell(r, map.IpCol!.Value);
            if (string.IsNullOrWhiteSpace(ipCell))
                continue;

            var ips = BlacklistExcelImportHelper.ExtractIps(ipCell);
            if (ips.Count == 0)
            {
                result.SkippedParseError++;
                var preview = BlacklistExcelImportHelper.FormatCellForError(ipCell);
                BlacklistExcelImportHelper.AddError(result,
                    $"Row {r} col {ipColLetter}: no parseable IP; value={preview}");
                continue;
            }

            var note = BlacklistExcelImportHelper.TruncateNote(reasonRaw.Trim());

            foreach (var ip in ips)
            {
                if (ip.Length > 64 || !IPAddress.TryParse(ip, out _))
                {
                    result.SkippedParseError++;
                    BlacklistExcelImportHelper.AddError(result,
                        $"Row {r} col {ipColLetter}: rejected '{BlacklistExcelImportHelper.FormatCellForError(ip, 120)}' (length>64 or invalid); cell={BlacklistExcelImportHelper.FormatCellForError(ipCell, 160)}");
                    continue;
                }

                if (byCanonical.TryGetValue(ip, out var existing))
                {
                    if (existing.Enabled)
                    {
                        result.SkippedDuplicate++;
                        continue;
                    }

                    existing.Enabled = true;
                    existing.Note = note;
                    existing.OperatorName = op;
                    existing.UpdatedOn = DateTime.UtcNow;
                    result.Updated++;
                }
                else
                {
                    var entity = new IpBlackList
                    {
                        Ip = ip,
                        Note = note,
                        Enabled = true,
                        OperatorName = op,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    };
                    centralDbContext.IpBlackLists.Add(entity);
                    byCanonical[ip] = entity;
                    result.Inserted++;
                }

                pendingSinceLastSave++;
                if (pendingSinceLastSave >= UploadBatchSize)
                {
                    await centralDbContext.SaveChangesAsync();
                    pendingSinceLastSave = 0;
                }
            }
        }

        if (pendingSinceLastSave > 0)
            await centralDbContext.SaveChangesAsync();

        await ReloadCacheAsync();
        return Ok(result);
    }

    private Task RemoveCacheAsync(string ip) => myCache.HSetDeleteByKeyFieldAsync(_ipKey, ip);

    private Task AddCacheAsync(string ip) => myCache.HSetStringAsync(_ipKey, ip, "1");

    private async Task<long> ReloadCacheAsync()
    {
        await myCache.HSetDeleteByKeyAsync(_ipKey);

        var ips = await centralDbContext.IpBlackLists
            .Select(x => x.Ip)
            .ToListAsync();

        foreach (var ip in ips)
        {
            await myCache.HSetStringAsync(_ipKey, ip, "1");
        }

        return ips.Count;
    }

    private async Task<string> GetOperatorNameAsync()
    {
        long tenantId = GetTenantId(), partyId = GetPartyId();
        var user = await authDbContext.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .ToUserNameModel()
            .SingleAsync();
        return user.GuessNativeName();
    }
}