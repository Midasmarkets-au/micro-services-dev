using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor.Magick;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Response;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using M = Bacera.Gateway.Verification;
using MSG = Bacera.Gateway.ResultMessage.Verification;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/Verification")]
public class VerificationController(
    TenantDbContext tenantCtx,
    IStorageService storageService,
    IMediator mediator,
    ITenantGetter tenantGetter,
    UserService userSvc,
    TagService tagSvc)
    : TenantBaseController
{
    private readonly long _tenantId = tenantGetter.GetTenantId();

    /// <summary>
    /// Verification pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<VerificationDTO>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria = null)
    {
        criteria ??= new M.Criteria();
        criteria.Type ??= VerificationTypes.Verification;
        return await Query(criteria);
    }

    /// <summary>
    /// Get verification
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(VerificationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tenantCtx.Verifications
            .Include(x => x.VerificationItems)
            .Include(x => x.Party)
            .Where(x => x.Id.Equals(id))
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();

        var comments = await tenantCtx.Comments
            .Where(x => x.Type == (int)CommentTypes.Verification && x.RowId == id)
            .Select(x => new Comment.TenantDTO
            {
                Content = x.Content,
                CreatedOn = x.CreatedOn,
            })
            .ToListAsync();

        return Ok(_tenantId == 10005 ? item.ToJpDTO().SetComments(comments) : item.ToDTO().SetComments(comments));
    }

    /// <summary>
    /// Get Verification Item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/item/{itemId:long}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItem(long id, long itemId)
    {
        var item = await tenantCtx.VerificationItems
            .Where(x => x.VerificationId.Equals(id))
            .Where(x => x.Id.Equals(itemId))
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        return Ok(item.ToItemDTO());
    }

    /// <summary>
    /// Get Verification Items List
    /// </summary>
    /// <param name="id"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/item-list")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemList(long id, long itemId)
    {
        var items = await tenantCtx.VerificationItems
            .Where(x => x.VerificationId.Equals(id))
            .OrderBy(x => x.Id)
            .ToListAsync();
        var result = items
            .Select(x => new
            {
                x.Id,
                x.UpdatedOn,
                x.CreatedOn,
                x.Category,
                x.Content,
                Object = FormatContent(x.Content)
            }).ToList();

        return Ok(result);
    }

    private static object? FormatContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return null;
        try
        {
            return JsonConvert.DeserializeObject<object>(content);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set verification status to UnderReview
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/under-review")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnderReview(long id)
    {
        var item = await tenantCtx.Verifications
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();

        if (item == null)
            return NotFound();

        await UpdateStatus(item, VerificationStatusTypes.UnderReview);
        return NoContent();
    }

    /// <summary>
    /// Set verification status to AwaitingApproval
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/awaiting-approve")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AwaitingApprove(long id)
    {
        var item = await tenantCtx.Verifications
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        await UpdateStatus(item, VerificationStatusTypes.AwaitingApprove);
        return NoContent();
    }

    /// <summary>
    /// Set verification status to Approved
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(long id) => await ApproveWithTag(id);


    /// <summary>
    /// Set verification status to Approved, Add DelayedReview tag into Party
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/delayed-approve")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DelayedApprove(long id) => await ApproveWithTag(id, true);

    /// <summary>
    /// Set verification status to AwaitApproval
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/awaiting-review")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AwaitingReview(long id)
    {
        var item = await tenantCtx.Verifications
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        await UpdateStatus(item, VerificationStatusTypes.AwaitingReview);
        return NoContent();
    }

    /// <summary>
    /// Set verification status to Rejected
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(long id)
    {
        var item = await tenantCtx.Verifications
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();

        if (item == null)
            return NotFound();

        await UpdateStatus(item, VerificationStatusTypes.Rejected);
        await mediator.Publish(new VerificationRejectedEvent(item));
        return NoContent();
    }


    /// <summary>
    /// Upload document
    /// </summary>
    /// <returns></returns>
    ///
    [HttpPost("{id:long}/document/upload")]
    [RequestSizeLimit(30_000_000)]
    [ProducesResponseType(typeof(Medium.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(long id, IFormFile file, string? type = null)
    {
        var verification = await tenantCtx.Verifications
            .Include(x => x.VerificationItems.Where(v => v.Category == VerificationCategoryTypes.Document))
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();

        if (verification == null) return NotFound();
        if (verification.Status == (int)VerificationStatusTypes.Incomplete)
            return BadRequest(Result.Error("Verification is incomplete"));

        var supplement = verification.VerificationItems.SingleOrDefault() ?? new VerificationItem
            { VerificationId = id, Category = VerificationCategoryTypes.Document, Content = "[]" };

        var medium = await UploadForParty(file, verification.PartyId, VerificationCategoryTypes.Document,
            verification.Id);

        var documentType = VerificationDocumentTypes.All.Select(x => x.ToLower())
            .Contains(type?.Trim().ToLower() ?? "")
            ? type!.ToLower()
            : VerificationDocumentTypes.Other;

        var doc = new VerificationDocumentMedium
        {
            Id = medium.Id,
            ContentType = medium.ContentType,
            FileName = medium.FileName,
            Guid = medium.Guid,
            Url = medium.Url,
            Type = medium.Type,
            Status = 0,
            DocumentType = documentType,
            RejectedOn = null,
            RejectedReason = "",
        };

        var docs = Utils.JsonDeserializeObjectWithDefault<List<VerificationDocumentMedium>>(supplement.Content);

        if (!string.Equals(documentType, VerificationDocumentTypes.Other, StringComparison.CurrentCultureIgnoreCase))
        {
            foreach (var item in docs.Where(item => item.DocumentType == documentType))
            {
                item.FileName = VerificationDocumentTypes.Other;
            }
        }

        docs.Add(doc);
        supplement.Content = Utils.JsonSerializeObject(docs);
        supplement.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(supplement);

        if (supplement.Verification.TryChangeToUnderReview())
            tenantCtx.Verifications.Update(supplement.Verification);

        await tenantCtx.SaveChangesAsync();
        return Ok(medium);
    }

    /// <summary>
    /// Reject document
    /// </summary>
    /// <returns></returns>
    ///
    [HttpPut("{id:long}/document/{mediumId:long}/reject")]
    [ProducesResponseType(typeof(Medium.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectDocument(long id, long mediumId, [FromBody] RejectSpec spec)
    {
        var verificationItem = tenantCtx.VerificationItems
            .Include(x => x.Verification)
            .Where(x => x.VerificationId == id)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        if (verificationItem == null)
            return NotFound();

        var docs = JsonConvert.DeserializeObject<List<VerificationDocumentMedium>>(verificationItem.Content)
                   ?? new List<VerificationDocumentMedium>();
        var rejectItem = docs.SingleOrDefault(x => x.Id == mediumId);
        if (rejectItem == null)
            return BadRequest(Result.Error(ResultMessage.Verification.UploadedDocumentNotExists));
        if (rejectItem.Status == VerificationDocumentStatusTypes.Rejected)
            return BadRequest(Result.Error(ResultMessage.Verification.UploadedDocumentAlreadyRejected));

        rejectItem.RejectedOn = DateTime.UtcNow;
        rejectItem.RejectedReason = spec.Reason;
        rejectItem.Status = VerificationDocumentStatusTypes.Rejected;

        verificationItem.Content = Utils.JsonSerializeObject(docs);
        verificationItem.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(verificationItem);
        await tenantCtx.SaveChangesAsync();

        var result = false;
        if (docs.CanAwaitingApprove())
            result = verificationItem.Verification.TryChangeToAwaitingApprove();
        else if ((int)VerificationStatusTypes.AwaitingReview == verificationItem.Verification.Status)
            result = verificationItem.Verification.TryChangeToUnderReview();

        if (!result) return NoContent();

        tenantCtx.Verifications.Update(verificationItem.Verification);
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Approve document
    /// </summary>
    /// <returns></returns>
    ///
    [HttpPut("{id:long}/document/{mediumId:long}/approve")]
    [ProducesResponseType(typeof(Medium.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveDocument(long id, long mediumId)
    {
        var supplement = tenantCtx.VerificationItems
            .Include(x => x.Verification)
            .Where(x => x.VerificationId == id)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        if (supplement == null)
            return NotFound();

        var docs = VerificationDocumentMedium.FromJson(supplement.Content);
        var approveItem = docs.SingleOrDefault(x => x.Id == mediumId);
        if (approveItem == null)
            return BadRequest(Result.Error(ResultMessage.Verification.UploadedDocumentNotExists));
        if (approveItem.Status == VerificationDocumentStatusTypes.Approved)
            return BadRequest(Result.Error(ResultMessage.Verification.UploadedDocumentAlreadyApproved));

        approveItem.RejectedOn = DateTime.UtcNow;
        approveItem.Status = VerificationDocumentStatusTypes.Approved;

        supplement.Content = Utils.JsonSerializeObject(docs);
        supplement.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(supplement);
        await tenantCtx.SaveChangesAsync();

        var result = false;
        if (docs.CanAwaitingApprove())
            result = supplement.Verification.TryChangeToAwaitingApprove();
        else if ((int)VerificationStatusTypes.AwaitingReview == supplement.Verification.Status)
            result = supplement.Verification.TryChangeToUnderReview();

        if (!result) return NoContent();

        tenantCtx.Verifications.Update(supplement.Verification);
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete document
    /// </summary>
    /// <returns></returns>
    ///
    [HttpDelete("{id:long}/document/{mediumId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteDocument(long id, long mediumId)
    {
        var supplement = tenantCtx.VerificationItems
            .Include(x => x.Verification)
            .Where(x => x.VerificationId == id)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        if (supplement == null)
            return NotFound();

        var docs = VerificationDocumentMedium.FromJson(supplement.Content);
        var deleteItem = docs.SingleOrDefault(x => x.Id == mediumId);
        if (deleteItem == null)
            return BadRequest(Result.Error(ResultMessage.Verification.UploadedDocumentNotExists));

        var medium = await tenantCtx.Media.FindAsync(mediumId);
        if (medium != null)
        {
            medium.DeletedOn = DateTime.UtcNow;
            tenantCtx.Media.Update(medium);
            await tenantCtx.SaveChangesAsync();
        }

        docs.Remove(deleteItem);
        supplement.Content = Utils.JsonSerializeObject(docs);
        supplement.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(supplement);
        await tenantCtx.SaveChangesAsync();

        if (supplement.Verification.Status != (int)VerificationStatusTypes.UnderReview
            || !docs.CanAwaitingApprove())
            return NoContent();

        supplement.Verification.Status = (int)VerificationStatusTypes.AwaitingApprove;
        supplement.Verification.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Verifications.Update(supplement.Verification);
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    public class RejectSpec
    {
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reject document
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id:long}/document/reject-notice")]
    [ProducesResponseType(typeof(Medium.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOutDocumentRejectNotice(long id)
    {
        var partyId = await tenantCtx.Verifications
            .Where(x => x.Id == id)
            .Select(x => x.PartyId)
            .SingleAsync();

        await mediator.Publish(new VerificationDocumentRejectedEvent(partyId));
        return NoContent();
    }

    [HttpPut("{id:long}/awaiting-address-verify")]
    public async Task<IActionResult> AwaitingAddressVerify(long id)
    {
        var item = await tenantCtx.Verifications
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        await UpdateStatus(item, VerificationStatusTypes.AwaitingAddressVerify);
        return NoContent();
    }

    [HttpPut("{id:long}/awaiting-code-verify")]
    public async Task<IActionResult> AwaitingCodeVerify(long id)
    {
        var item = await tenantCtx.Verifications
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        await UpdateStatus(item, VerificationStatusTypes.AwaitingCodeVerify);
        return NoContent();
    }

    // get paper mail code
    [HttpGet("{id:long}/mail-code")]
    public async Task<IActionResult> GetPaperMailCode(long id)
    {
        var verification = await tenantCtx.Verifications
            .Select(x => new { x.Id, x.PartyId })
            .SingleOrDefaultAsync(x => x.Id == id);

        if (verification == null)
            return NotFound();

        var item = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == verification.PartyId)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.PaperMail)
            .Where(x => x.MethodValue == verification.Id.ToString())
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound("No paper mail code found");

        return Ok(item.Code);
    }


    [HttpPost("{id:long}/mail-code")]
    public async Task<IActionResult> CreatePaperMailCode(long id)
    {
        var verification = await tenantCtx.Verifications.SingleOrDefaultAsync(x => x.Id == id);
        if (verification == null)
            return NotFound();

        var item = AuthCode.Build(verification.PartyId, AuthCode.EventLabel.PaperVerification,
            AuthCodeMethodTypes.PaperMail, verification.Id.ToString());

        tenantCtx.AuthCodes.Add(item);
        await tenantCtx.SaveChangesAsync();
        return Ok(item.Code);
    }

    private async Task<Medium> UploadForParty(IFormFile file, long partyId, string type, long rowId = 0)
    {
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
            type, rowId, file.ContentType,
            GetTenantId(), partyId);

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
        return result;
    }

    private async Task UpdateStatus(M item, VerificationStatusTypes status)
    {
        var user = await userSvc.GetPartyAsync(GetPartyId());
        item.Note = Utils.JsonSerializeObject(new { @operator = user });
        item.Status = (int)status;
        item.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Verifications.Update(item);
        await tenantCtx.SaveChangesAsync();
    }

    private async Task<IActionResult> Query(M.Criteria criteria)
    {
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
        {
            item.HasComment = comments.Contains(item.Id);
        }

        await userSvc.ApplyUserBlackListInfo(items.Select(x => x.User).ToList());
        return Ok(Result<List<VerificationDTO>, M.Criteria>.Of(items, criteria));
    }

    private async Task<IActionResult> ApproveWithTag(long id, bool delayedReview = false)
    {
        var item = await tenantCtx.Verifications
            .Include(x => x.Party)
            .Include(x => x.VerificationItems)
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();

        if (item == null)
            return NotFound();

        if (delayedReview)
            await tagSvc.AddPartyTagAsync(item.PartyId, "DelayedReview");

        await UpdateStatus(item, VerificationStatusTypes.Approved);

        // if any application exists, meaning user already has an account, then skip
        if (GetTenantId() == 10005)
            return NoContent();
        
        if (await tenantCtx.Applications.AnyAsync(x => x.PartyId == item.PartyId))
            return NoContent();

        // assign FundType to application
        var verificationDTO = item.ToDTO();
        var accountType = (AccountTypes)(verificationDTO.Started?.AccountType ?? 0);

        var serviceId = verificationDTO.Started?.ServiceId ?? 0;
        var platform = (PlatformTypes)(verificationDTO.Started?.Platform ?? 0);
        var currency = (CurrencyTypes)(verificationDTO.Started?.Currency ?? 0);

        var supplement = ApplicationSupplement.Build(AccountRoleTypes.Client, FundTypes.Wire,
            accountType: accountType, leverage: verificationDTO.Started?.Leverage ?? 0,
            currency: currency, platform: platform, serviceId: serviceId);
        supplement.ReferCode = verificationDTO.Started?.Referral ?? "";

        var application = new Application
        {
            PartyId = item.PartyId,
            Type = (short)ApplicationTypes.TradeAccount,
            Status = (short)ApplicationStatusTypes.AwaitingApproval,
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            Supplement = supplement.ToJson(),
        };
        var entity = await tenantCtx.Applications.AddAsync(application);
        await tenantCtx.SaveChangesAsync();

        await mediator.Publish(new VerificationApprovedEvent(item, entity.Entity));
        return NoContent();
    }
}