using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Services.ChunkStorage;
using Bacera.Gateway.Vendor.Magick;
using Bacera.Gateway.Web.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Media")]
[Area("Client")]
[Route("api/" + VersionTypes.V2 + "/[Area]/media")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class MediumControllerV2(
    IStorageService storageService,
    IChunkStorageService chunkStorageSvc,
    AuthDbContext authDbContext,
    TenantDbContext tenantDbContext)
    : BaseController
{
    /// <summary>
    /// Upload avatar for current user
    /// </summary>
    /// <param name="avatar"></param>
    /// <returns></returns>
    [HttpPost("user/profile/avatar")]
    [Authorize]
    [RequestSizeLimit(2_000_000)]
    [ProducesResponseType(typeof(Medium), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadAvatarForParty(IFormFile avatar)
    {
        if (avatar.Length < 1) return BadRequest();
        var partyId = GetPartyId();
        var tenantId = GetTenantId();
        using var memoryStream = new MemoryStream();
        await avatar.CopyToAsync(memoryStream);
        if (Utils.IsImage(avatar.ContentType))
        {
            memoryStream.Position = 0;
            await MagickService.CompressImageAsync(memoryStream);
        }
        
        var result = await storageService.UploadPublicFileAndSaveMediaAsync(memoryStream,
            avatar.FileName, Path.GetExtension(avatar.FileName).ToLower(),
            "avatar", partyId, avatar.ContentType,
            GetTenantId(), partyId);

        using var originalStream = new MemoryStream();
        await avatar.CopyToAsync(originalStream);
        await storageService.UploadFileAsync(originalStream
            , $""
            , result.FileName + "_original"
            , Path.GetExtension(avatar.FileName).ToLower()
            , avatar.ContentType
            , tenantId
            , partyId
        );

        if (result.Id == 0)
            return Problem("__UPLOAD_AVATAR_FAILED__");

        var task1 = Task.Run(async () =>
        {
            var user = await authDbContext.Users.FirstAsync(x => x.PartyId == partyId && x.TenantId == tenantId);
            user.Avatar = result.Url;
            authDbContext.Users.Update(user);
            await authDbContext.SaveChangesAsync();
        });

        var task2 = Task.Run(async () =>
        {
            var party = await tenantDbContext.Parties.FirstAsync(x => x.Id == partyId);
            party.Avatar = result.Url;
            tenantDbContext.Parties.Update(party);
            await tenantDbContext.SaveChangesAsync();
        });

        await Task.WhenAll(task1, task2);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> ClientMediaPagination([FromQuery] Medium.ClientCriteria? criteria)
    {
        criteria ??= new Medium.ClientCriteria();
        criteria.PartyId = GetPartyId();
        var items = await tenantDbContext.Media
            .Where(x => x.DeletedOn == null)
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return Ok(Result<List<Medium.ClientPageModel>, Medium.ClientCriteria>.Of(items, criteria));
    }

    [HttpGet("{guid}")]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 600)]
    public async Task<IActionResult> Image(string guid)
    {
        var result = await storageService.GetObjectByGuidAndPartyIdAsync(GetPartyId(), guid);
        return result.IsEmpty() ? NotFound() : File(result.Stream, result.Medium.ContentType, result.Medium.FileName);
    }

    [HttpDelete("{guid}")]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 600)]
    public async Task<IActionResult> MarkAsDeleteForParty(string guid)
    {
        var result = await storageService.MarkAsDeletedByGuidAndPartyIdAsync(GetPartyId(), guid);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadForParty([FromQuery] string type, IFormFile file)
    {
        type = string.IsNullOrEmpty(type) ? "unknown" : type.Trim().ToLower();
        type = type[..Math.Min(30, type.Length)];
        if (file.Length < 1) return BadRequest();
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        if (Utils.IsImage(file.ContentType))
        {
            memoryStream.Position = 0;
            await MagickService.CompressImageAsync(memoryStream);
        }
        
        var result = await storageService.UploadFileAndSaveMediaAsync(
            memoryStream, Guid.NewGuid().ToString(),
            Path.GetExtension(file.FileName).ToLower(),
            type, 0, file.ContentType,
            GetTenantId(), GetPartyId());

        using var originalStream = new MemoryStream();
        await file.CopyToAsync(originalStream);
        await storageService.UploadFileAsync(originalStream
            , $""
            , result.FileName + "_original"
            , Path.GetExtension(file.FileName).ToLower()
            , file.ContentType
            , GetTenantId()
            , GetPartyId()
        );
       
        return Ok(result.Guid);
    }

    [HttpPost("upload/chunk")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadChunk([FromForm] Medium.ChunkInfo chunk)
    {
        if (chunk.File == null || chunk.File.Length < 1)
            return BadRequest("Invalid file");

        using var memoryStream = new MemoryStream();
        await chunk.File.CopyToAsync(memoryStream);
        var fieldId =
            await chunkStorageSvc.SaveChunkAsync(chunk.FieldId ?? string.Empty, chunk.ChunkIndex,
                memoryStream.ToArray());
        return Ok(fieldId);
    }

    [HttpPost("upload/merge")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> MergeChunk([FromForm] Medium.ChunkInfo chunk)
    {
        if (chunk.FieldId == null || chunk.TotalChunks < 1)
            return BadRequest("Invalid fieldId or totalChunks");

        var chunks = await chunkStorageSvc.GetUploadedChunksAsync(chunk.FieldId ?? string.Empty);
        if (chunks.Count != chunk.TotalChunks)
            return BadRequest("Chunks are not completed");

        using var memoryStream = new MemoryStream();
        foreach (var index in chunks.OrderBy(x => x))
        {
            var data = await chunkStorageSvc.GetChunkAsync(chunk.FieldId!, index);
            await memoryStream.WriteAsync(data);
        }

        var result = await storageService.UploadFileAndSaveMediaAsync(
            memoryStream, Guid.NewGuid().ToString(),
            Path.GetExtension(chunk.FileName).ToLower(),
            chunk.Type, 0, chunk.ContentType,
            GetTenantId(), GetPartyId());

        await chunkStorageSvc.DeleteChunksAsync(chunk.FieldId!);
        return Ok(result.ToClientPageModel());
    }
}