using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Vendor.Magick;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Bacera.Gateway.Web.Controllers;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class MediumController(AuthDbContext authDbContext, IStorageService storageService, TenantDbContext tenantDbContext)
    : BaseController
{
    private const string RoutePrefix = "/api/v1";

    /// <summary>
    /// Upload avatar for current user
    /// </summary>
    /// <param name="avatar"></param>
    /// <returns></returns>
    [HttpPost(RoutePrefix + "/user/profile/avatar")]
    [Authorize]
    [SwaggerOperation(Tags = new[] { "User" })]
    [RequestSizeLimit(20_000_000)]
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
            await MagickService.CompressImageAsync(memoryStream, 400, 400);
        }

        using var originalStream = new MemoryStream();
        await avatar.CopyToAsync(originalStream);
        
        var result = await storageService.UploadPublicFileAndSaveMediaAsync(memoryStream,
            avatar.FileName, Path.GetExtension(avatar.FileName).ToLower(),
            "avatar", partyId, avatar.ContentType,
            GetTenantId(), partyId);

        await storageService.UploadFileAsync(originalStream
            , ""
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

    /// <summary>
    /// Get media file for client by guid
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Client/Media" })]
    [HttpGet(RoutePrefix + "/client/media/{guid}")]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 600)]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Image(string guid, [FromQuery] string type = "public")
    {
        var result = await storageService.GetObjectByGuidAndPartyIdAsync(GetPartyId(), type, guid);
        return result.IsEmpty() ? NotFound() : File(result.Stream, result.Medium.ContentType, result.Medium.FileName);
    }

    /// <summary>
    /// Media pagination for client
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Client/Media/" })]
    [HttpGet(RoutePrefix + "/client/media/list")]
    [ProducesResponseType(200, Type = typeof(Result<List<Medium.ResponseModel>, Medium.Criteria>))]
    public async Task<IActionResult> ClientMediaPagination([FromQuery] Medium.Criteria? criteria)
    {
        criteria ??= new Medium.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await tenantDbContext.Media
                .Where(x => x.DeletedOn == null)
                .PagedFilterBy(criteria)
                .ToResponseModel()
                .ToListAsync()
            ;
        return Ok(Result<List<Medium.ResponseModel>, Medium.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Mark media file as deleted for client by guid
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Client/Media" })]
    [HttpDelete(RoutePrefix + "/client/media/{guid}")]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 600)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsDeleteForParty(string guid)
    {
        var result = await storageService.MarkAsDeletedByGuidAndPartyIdAsync(GetPartyId(), guid);
        return result ? NoContent() : NotFound();
    }


    /// <summary>
    /// Upload File for client
    /// </summary>
    /// <param name="type"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Client/Upload" })]
    [HttpPost(RoutePrefix + "/client/upload")]
    [Authorize]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(Medium), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadForParty([FromQuery] string type, IFormFile file)
    {
        type = string.IsNullOrEmpty(type) ? "unknown" : type.Trim().ToLower();
        type = type[..Math.Min(30, type.Length)];
        return await UploadForParty(file, type);
    }

    /// <summary>
    /// Get media file for tenant by guid
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Tenant/Media" })]
    [HttpGet(RoutePrefix + "/tenant/media/{guid}")]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 600)]
    [Authorize(Roles = UserRoleTypesString.TenantAdmin + ", AccountAdmin, EventAdmin")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> TenantImage(string guid)
    {
        var result = await storageService.GetObjectByGuidAsync(guid);
        return result.IsEmpty() ? NotFound() : File(result.Stream, result.Medium.ContentType, result.Medium.FileName);
    }

    /// <summary>
    /// Tenant Media pagination (undeleted)
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Tenant/Media" })]
    [HttpGet(RoutePrefix + "/tenant/media/list")]
    [ProducesResponseType(200, Type = typeof(Result<List<Medium.ResponseModel>, Medium.Criteria>))]
    public async Task<IActionResult> TenantMediaPagination([FromQuery] Medium.Criteria? criteria)
    {
        criteria ??= new Medium.Criteria();
        var items = await tenantDbContext.Media
            .Where(x => x.DeletedOn == null)
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        return Ok(Result<List<Medium.ResponseModel>, Medium.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Mark media file as deleted for tenant by guid
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Tenant/Media" })]
    [HttpDelete(RoutePrefix + "/tenant/media/{guid}")]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 600)]
    [Authorize(Roles = UserRoleTypesString.TenantAdmin + ", AccountAdmin, EventAdmin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsDeletedForTenant(string guid)
    {
        var result = await storageService.MarkAsDeletedAsync(guid);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Upload File for tenant
    /// </summary>
    /// <param name="type"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Tenant/Upload" })]
    [HttpPost(RoutePrefix + "/tenant/upload")]
    [Authorize(Roles = UserRoleTypesString.TenantAdmin + ", EventAdmin")]
    [ProducesResponseType(typeof(Medium), 200)]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadForTenant([FromQuery] string type, IFormFile file)
    {
        type = string.IsNullOrEmpty(type) ? "unknown" : type.Trim().ToLower();
        type = type[..Math.Min(30, type.Length)];
        return await UploadForTenant(file, type);
    }

    /// <summary>
    /// Tenant upload a file for a party
    /// </summary>
    /// <param name="type"></param>
    /// <param name="file"></param>
    /// <param name="partyId"></param>
    /// <returns></returns>
    [RequestSizeLimit(100_000_000)]
    [SwaggerOperation(Tags = new[] { "Tenant/Upload" })]
    [HttpPost(RoutePrefix + "/tenant/upload/party/{partyId}")]
    [Authorize(Roles = UserRoleTypesString.TenantAdmin)]
    [ProducesResponseType(typeof(Medium), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadForTenantWithPartyId([FromQuery] string type, IFormFile file, long partyId)
    {
        type = string.IsNullOrEmpty(type) ? "unknown" : type.Trim().ToLower();
        type = type[..Math.Min(30, type.Length)];
        return await UploadForTenant(file, type, partyId);
    }

    /// <summary>
    /// Tenant upload a public file
    /// </summary>
    /// <param name="type"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [RequestSizeLimit(100_000_000)]
    [SwaggerOperation(Tags = new[] { "Tenant/Upload" })]
    [HttpPost(RoutePrefix + "/tenant/upload/public")]
    [Authorize(Roles = UserRoleTypesString.TenantAdmin + ", EventAdmin")]
    [ProducesResponseType(typeof(Medium), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPublicFileForTenant([FromQuery] string type, IFormFile file)
    {
        type = string.IsNullOrEmpty(type) ? "public" : type.Trim().ToLower();
        type = type[..Math.Min(30, type.Length)];

        if (file.Length < 1) return BadRequest();

        var partyId = GetPartyId();
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        if (Utils.IsImage(file.ContentType))
        {
            memoryStream.Position = 0;
            await MagickService.CompressImageAsync(memoryStream);
        }
        var result = await storageService.UploadPublicFileAndSaveMediaAsync(memoryStream,
            file.FileName, Path.GetExtension(file.FileName).ToLower(),
            type, partyId, file.ContentType,
            GetTenantId(), partyId);


        using var originalStream = new MemoryStream();
        await file.CopyToAsync(originalStream);
        await storageService.UploadFileAsync(originalStream
            , ""
            , result.FileName + "_original"
            , Path.GetExtension(file.FileName).ToLower()
            , file.ContentType
            , GetTenantId()
            , partyId
        );

        return result.Id == 0
            ? Problem("__UPLOAD_FILE_FAILED__")
            : Ok(result);
    }

    private async Task<IActionResult> UploadForParty(IFormFile file, string type)
    {
        if (file.Length < 1)
            return BadRequest();

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
            , ""
            , result.FileName + "_original"
            , Path.GetExtension(file.FileName).ToLower()
            , file.ContentType
            , GetTenantId()
            , GetPartyId()
        );
       
        return Ok(result);
    }

    private async Task<IActionResult> UploadForTenant(IFormFile file, string type, long? partyId = null)
    {
        if (file.Length < 1)
            return BadRequest();

        var pid = partyId ?? GetPartyId();
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
            GetTenantId(), pid);

        using var originalStream = new MemoryStream();
        await file.CopyToAsync(originalStream);
        await storageService.UploadFileAsync(originalStream
            , ""
            , result.FileName + "_original"
            , Path.GetExtension(file.FileName).ToLower()
            , file.ContentType
            , GetTenantId()
            , pid
        );
        return Ok(result);
    }
}