using Bacera.Gateway.Web.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = MaterialRequest;

[Tags("Client/MaterialRequest")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class MaterialRequestController(TenantDbContext tenantDbContext, IStorageService storageSvc)
    : ClientBaseController
{
    /// <summary>
    /// List my material requests
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<MaterialRequestDTO>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        var partyId = GetPartyId();
        criteria ??= new M.Criteria();
        criteria.PartyId = partyId;

        var items = await tenantDbContext.MaterialRequests
            .PagedFilterBy(criteria)
            .ToListAsync();

        var data = items.Select(MaterialRequestDTO.FromEntity).ToList();
        return Ok(Result<List<MaterialRequestDTO>, M.Criteria>.Of(data, criteria));
    }

    /// <summary>
    /// Get one of my material requests
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(MaterialRequestDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await GetOwnMaterialRequest(id);
        if (item == null) return NotFound();
        return Ok(MaterialRequestDTO.FromEntity(item));
    }

    /// <summary>
    /// Submit a new material request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MaterialRequestDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MaterialRequestCreateModel spec)
    {
        var partyId = GetPartyId();
        var request = new MaterialRequest
        {
            PartyId = partyId,
            Status = (int)VerificationStatusTypes.AwaitingReview,
            MaterialType = spec.MaterialType,
            Description = spec.Description,
            Quantity = spec.Quantity,
            Note = string.Empty,
            Attachments = "[]",
        };
        await tenantDbContext.MaterialRequests.AddAsync(request);
        await tenantDbContext.SaveChangesAsync();

        return Ok(MaterialRequestDTO.FromEntity(request));
    }

    /// <summary>
    /// Attach a reference file / artwork to a material request
    /// </summary>
    [HttpPost("{id:long}/attachment")]
    [RequestSizeLimit(20_000_000)]
    [ProducesResponseType(typeof(MaterialRequestDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAttachment(long id, IFormFile file)
    {
        var request = await GetOwnMaterialRequest(id);
        if (request == null) return NotFound();

        List<MaterialAttachmentDTO> attachments;
        try
        {
            attachments = JsonConvert.DeserializeObject<List<MaterialAttachmentDTO>>(request.Attachments) ?? [];
        }
        catch
        {
            attachments = [];
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var medium = await storageSvc.UploadFileAndSaveMediaAsync(
            memoryStream
            , Guid.NewGuid().ToString()
            , Path.GetExtension(file.FileName).ToLower()
            , VerificationCategoryTypes.MaterialRequest
            , request.Id
            , file.ContentType
            , GetTenantId()
            , request.PartyId);

        attachments.Add(new MaterialAttachmentDTO
        {
            Guid = medium.Guid,
            Url = medium.Url,
            FileName = medium.FileName,
        });

        request.Attachments = JsonConvert.SerializeObject(attachments);
        request.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.MaterialRequests.Update(request);
        await tenantDbContext.SaveChangesAsync();

        return Ok(MaterialRequestDTO.FromEntity(request));
    }

    /// <summary>
    /// Withdraw a material request that hasn't been reviewed yet
    /// </summary>
    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(long id)
    {
        var request = await GetOwnMaterialRequest(id);
        if (request == null) return NotFound();

        if (request.Status != (int)VerificationStatusTypes.AwaitingReview)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        // No dedicated "Canceled" status exists on VerificationStatusTypes; Rejected is reused
        // for a client-initiated withdrawal, distinguished from a tenant rejection via Note.
        request.Status = (int)VerificationStatusTypes.Rejected;
        request.Note = "Cancelled by requester";
        request.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.MaterialRequests.Update(request);
        await tenantDbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<MaterialRequest?> GetOwnMaterialRequest(long id)
    {
        var partyId = GetPartyId();
        return await tenantDbContext.MaterialRequests
            .SingleOrDefaultAsync(x => x.Id == id && x.PartyId == partyId);
    }
}
