using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Areas.Tenant.Helpers;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = UserBlackList;

[Area("Tenant")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Tags("Tenant/User Black List")]
[Route("api/" + VersionTypes.V1 + "/[Area]/user-black-list")]
public class UserBlackListController(
    CentralDbContext centralDbContext,
    AuthDbContext authDbContext,
    IMyCache myCache)
    : TenantBaseController
{
    private const int UploadBatchSize = 200;
    private const long MaxUploadBytes = 25 * 1024 * 1024;

    private readonly string _nameKey = CacheKeys.GetBlackedUserNameHashKey();
    private readonly string _phoneKey = CacheKeys.GetBlackedUserPhoneHashKey();
    private readonly string _emailKey = CacheKeys.GetBlackedUserEmailHashKey();
    private readonly string _idNumberKey = CacheKeys.GetBlackedUserIdNumberHashKey();

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await centralDbContext.UserBlackLists
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<M.TenantPageModel>, M.Criteria>.Of(items, criteria));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var exists = await centralDbContext.UserBlackLists.AnyAsync(x =>
            x.Phone == spec.Phone || x.Email == spec.Email || x.IdNumber == spec.IdNumber);
        if (exists) return BadRequest("__USER_ALREADY_EXISTS__");
        var entity = spec.ToEntity(await GetOperatorNameAsync());
        centralDbContext.UserBlackLists.Add(entity);
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity);
        return Ok(entity);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await centralDbContext.UserBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        centralDbContext.UserBlackLists.Remove(entity);
        await centralDbContext.SaveChangesAsync();
        await RemoveCacheAsync(entity);
        return Ok();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var entity = await centralDbContext.UserBlackLists.FindAsync(id);
        if (entity == null) return NotFound();
        await RemoveCacheAsync(entity);
        spec.ApplyTo(entity, await GetOperatorNameAsync());
        await centralDbContext.SaveChangesAsync();
        await AddCacheAsync(entity);
        return Ok(entity);
    }


    [HttpPut("reload-cache")]
    public async Task<IActionResult> ReloadCache() => Ok(await ReloadCacheAsync());

    [HttpPost("upload-users")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26214400)]
    public async Task<IActionResult> UploadUsers(IFormFile file, [FromQuery] int sheetIndex = 0,
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
        if (map.NameCol == null || map.PhoneCol == null || map.IdNumberCol == null || map.EmailCol == null)
            return BadRequest("__MISSING_COLUMNS__");

        var existingRows = await centralDbContext.UserBlackLists
            .Select(x => new { x.Email, x.Phone, x.IdNumber })
            .ToListAsync();
        var emailKeySet = new HashSet<string>(StringComparer.Ordinal);
        var phoneKeySet = new HashSet<string>(StringComparer.Ordinal);
        var idKeySet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var x in existingRows)
        {
            if (!string.IsNullOrWhiteSpace(x.Email))
                emailKeySet.Add(x.Email.Trim().ToLowerInvariant());
            var p = BlacklistExcelImportHelper.NormalizePhone(x.Phone ?? "");
            p = BlacklistExcelImportHelper.TruncateField(p, 64);
            if (!string.IsNullOrWhiteSpace(p))
                phoneKeySet.Add(p);
            if (!string.IsNullOrWhiteSpace(x.IdNumber))
                idKeySet.Add(x.IdNumber.Trim());
        }

        var batchEmailKeys = new HashSet<string>(StringComparer.Ordinal);
        var batchPhoneKeys = new HashSet<string>(StringComparer.Ordinal);
        var batchIdKeys = new HashSet<string>(StringComparer.Ordinal);
        var result = new BlacklistUploadResult();
        var op = await GetOperatorNameAsync();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        var pendingSinceLastSave = 0;

        string Cell(int row, int col) => BlacklistExcelImportHelper.CellText(ws.Cell(row, col));

        for (var r = headerRow + 1; r <= lastRow; r++)
        {
            var name = Cell(r, map.NameCol!.Value);
            var phoneRaw = Cell(r, map.PhoneCol!.Value);
            var idNumberRaw = Cell(r, map.IdNumberCol!.Value);
            var emailRaw = Cell(r, map.EmailCol!.Value);

            if (string.IsNullOrWhiteSpace(name))
            {
                result.SkippedEmptyOrInvalid++;
                continue;
            }

            var emailTrimmed = string.IsNullOrWhiteSpace(emailRaw) ? "" : emailRaw.Trim();
            var emailKeyOpt = string.IsNullOrWhiteSpace(emailTrimmed)
                ? null
                : emailTrimmed.ToLowerInvariant();
            var phoneNorm = BlacklistExcelImportHelper.NormalizePhone(phoneRaw);
            phoneNorm = BlacklistExcelImportHelper.TruncateField(phoneNorm, 64);
            var phoneKeyOpt = string.IsNullOrWhiteSpace(phoneNorm) ? null : phoneNorm;
            var idTrim = string.IsNullOrWhiteSpace(idNumberRaw) ? "" : idNumberRaw.Trim();
            var idKeyOpt = string.IsNullOrWhiteSpace(idTrim) ? null : idTrim;

            if (emailKeyOpt == null && phoneKeyOpt == null && idKeyOpt == null)
            {
                result.SkippedEmptyOrInvalid++;
                continue;
            }

            if (emailKeyOpt != null)
            {
                if (emailKeySet.Contains(emailKeyOpt) || batchEmailKeys.Contains(emailKeyOpt))
                {
                    result.SkippedDuplicate++;
                    continue;
                }
            }
            else if (phoneKeyOpt != null)
            {
                if (phoneKeySet.Contains(phoneKeyOpt) || batchPhoneKeys.Contains(phoneKeyOpt))
                {
                    result.SkippedDuplicate++;
                    continue;
                }
            }
            else if (idKeyOpt != null)
            {
                if (idKeySet.Contains(idKeyOpt) || batchIdKeys.Contains(idKeyOpt))
                {
                    result.SkippedDuplicate++;
                    continue;
                }
            }

            var entity = new M
            {
                Name = BlacklistExcelImportHelper.TruncateField(name.Trim(), 64),
                Phone = string.IsNullOrWhiteSpace(phoneNorm) ? "" : phoneNorm,
                Email = string.IsNullOrWhiteSpace(emailTrimmed) ? "" : BlacklistExcelImportHelper.TruncateField(emailTrimmed, 64),
                IdNumber = idTrim,
                OperatorName = op,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            centralDbContext.UserBlackLists.Add(entity);

            if (emailKeyOpt != null)
            {
                batchEmailKeys.Add(emailKeyOpt);
                emailKeySet.Add(emailKeyOpt);
            }
            else if (phoneKeyOpt != null)
            {
                batchPhoneKeys.Add(phoneKeyOpt);
                phoneKeySet.Add(phoneKeyOpt);
            }
            else if (idKeyOpt != null)
            {
                batchIdKeys.Add(idKeyOpt);
                idKeySet.Add(idKeyOpt);
            }

            result.Inserted++;
            pendingSinceLastSave++;
            if (pendingSinceLastSave >= UploadBatchSize)
            {
                await centralDbContext.SaveChangesAsync();
                pendingSinceLastSave = 0;
            }
        }

        if (pendingSinceLastSave > 0)
            await centralDbContext.SaveChangesAsync();

        await ReloadCacheAsync();
        return Ok(result);
    }

    private async Task RemoveCacheAsync(M entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
            await myCache.HSetDeleteByKeyFieldAsync(_nameKey, entity.Name);
        if (!string.IsNullOrEmpty(entity.Phone))
            await myCache.HSetDeleteByKeyFieldAsync(_phoneKey, entity.Phone);
        if (!string.IsNullOrEmpty(entity.Email))
            await myCache.HSetDeleteByKeyFieldAsync(_emailKey, entity.Email);
        if (!string.IsNullOrEmpty(entity.IdNumber))
            await myCache.HSetDeleteByKeyFieldAsync(_idNumberKey, entity.IdNumber);
    }

    private async Task AddCacheAsync(M entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
            await myCache.HSetStringAsync(_nameKey, entity.Name, "1");
        if (!string.IsNullOrEmpty(entity.Phone))
            await myCache.HSetStringAsync(_phoneKey, entity.Phone, "1");
        if (!string.IsNullOrEmpty(entity.Email))
            await myCache.HSetStringAsync(_emailKey, entity.Email, "1");
        if (!string.IsNullOrEmpty(entity.IdNumber))
            await myCache.HSetStringAsync(_idNumberKey, entity.IdNumber, "1");
    }

    private async Task<long> ReloadCacheAsync()
    {
        var nameKey = CacheKeys.GetBlackedUserNameHashKey();
        var phoneKey = CacheKeys.GetBlackedUserPhoneHashKey();
        var emailKey = CacheKeys.GetBlackedUserEmailHashKey();
        var idNumberKey = CacheKeys.GetBlackedUserIdNumberHashKey();
        await myCache.HSetDeleteByKeyAsync(nameKey);
        await myCache.HSetDeleteByKeyAsync(phoneKey);
        await myCache.HSetDeleteByKeyAsync(emailKey);
        await myCache.HSetDeleteByKeyAsync(idNumberKey);

        var items = await centralDbContext.UserBlackLists
            .Select(x => new { x.Name, x.Phone, x.Email, x.IdNumber })
            .ToListAsync();

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Name))
                await myCache.HSetStringAsync(nameKey, item.Name, "1");
            if (!string.IsNullOrEmpty(item.Phone))
                await myCache.HSetStringAsync(phoneKey, item.Phone, "1");
            if (!string.IsNullOrEmpty(item.Email))
                await myCache.HSetStringAsync(emailKey, item.Email, "1");
            if (!string.IsNullOrEmpty(item.IdNumber))
                await myCache.HSetStringAsync(idNumberKey, item.IdNumber, "1");
        }

        return items.Count;
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