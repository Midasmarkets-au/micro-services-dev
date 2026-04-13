
﻿using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor.Magick;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Request;
using Bacera.Gateway.Web.Response;
using ImageMagick;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MSG = Bacera.Gateway.ResultMessage.Verification;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Verification")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class VerificationController(
    IMediator mediator,
    TenantDbContext tenantDbContext,
    ITenantGetter tenantGetter,
    ConfigurationService configurationService,
    UserService userService,
    IStorageService storageService)
    : ClientBaseController
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    
    /// <summary>
    /// Get Verification Setting. Verification Category Types and existing data
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Index()
    {
        if (await userService.IsBannedVerificationQuizAsync(GetPartyId()))
            return BadRequest(Result.Error(MSG.VerificationDisabled));

        var quizEnabled = await configurationService.GetVerificationQuizToggleSwitchAsync();
        var item = await GetPartyVerification(GetPartyId());
        if (_tenantId == 10005)
        {
            return Ok(new VerificationJpSetting
            {
                Settings = quizEnabled ? VerificationCategoryTypes.All : VerificationCategoryTypes.NoQuiz,
                Data = item.ToClientJpDTO(),
            });
        }

        return Ok(new
        {
            Settings = quizEnabled ? VerificationCategoryTypes.All : VerificationCategoryTypes.NoQuiz,
            Data = item.ToDTO(),
        });
    }

    [HttpPost("new")]
    public async Task<ActionResult<VerificationStartedDTO>> NewVerification()
    {
        var item = new Verification
        {
            Note = "",
            PartyId = GetPartyId(),
            Type = (short)VerificationTypes.Verification,
            Status = (int)VerificationStatusTypes.Incomplete,
        };
        await tenantDbContext.Verifications.AddAsync(item);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("existing")]
    public async Task<IActionResult> ExistingVerification()
    {
        if (_tenantId != 10005)
            return BadRequest("Action not allow for this tenant");
        var partyId = GetPartyId();
        var hideEmail = ShouldHideEmail();
        var items = await tenantDbContext.Verifications
            .OrderByDescending(x => x.Id)
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Type == (short)VerificationTypes.Verification)
            .ToJpDTO(hideEmail)
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Save Started
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Started)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(VerificationStartedDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificationStartedDTO>> SaveStarted([FromBody] object spec)
    {
        var json = JsonConvert.SerializeObject(spec);
        if (_tenantId != 10005 && !VerificationInfoDTO.TryParse(json, out _))
        {
            return BadRequest(Result.Error("Invalid data"));
        }
        
        
        var item = await GetPartyVerification(GetPartyId());
        if (item.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(MSG.CannotUpdateAtThisStatus));

        var result = await SaveItem(VerificationCategoryTypes.Started, json);

        await mediator.Publish(new VerificationStartedEvent(item));
        return Ok(JsonConvert.DeserializeObject<VerificationStartedDTO>(result.Content));
    }

    /// <summary>
    /// Save Info
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Info)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(VerificationInfoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificationInfoDTO>> SaveInfo([FromBody] object spec)
    {
        //VerificationInfoDTO

        var json = JsonConvert.SerializeObject(spec);

        var parseResult = VerificationInfoDTO.TryParse(json, out var dto);
        if (_tenantId != 10005 && !parseResult)
        {
            return BadRequest(Result.Error("Invalid data"));
        }
        
        var partyId = GetPartyId();
        var item = await GetPartyVerification(partyId);
        if (item.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(MSG.CannotUpdateAtThisStatus));

        var result = await SaveItem(VerificationCategoryTypes.Info, JsonConvert.SerializeObject(spec));

        //Save Social Media Info to Supplement
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.RowId == partyId)
            .Where(x => x.Type == (short)SupplementTypes.SocialMediaRecord)
            .FirstOrDefaultAsync();

        if (supplement != null)
        {
            supplement.Data = JsonConvert.SerializeObject(dto.SocialMedium);
        }
        else
        {
            tenantDbContext.Supplements.Add(
                Supplement.Build(SupplementTypes.SocialMediaRecord, GetPartyId(), JsonConvert.SerializeObject(dto.SocialMedium))
            );
        }

        await tenantDbContext.SaveChangesAsync();

        return Ok(JsonConvert.DeserializeObject(result.Content));
    }

    /// <summary>
    /// Save Financial
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Financial)]
    [ProducesResponseType(typeof(VerificationFinancialDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationFinancialDTO>> SaveFinancial([FromBody] object spec)
    {
        // VerificationFinancialDTO
        var json = JsonConvert.SerializeObject(spec);
        if (_tenantId != 10005 && !VerificationFinancialDTO.TryParse(json, out _))
        {
            return BadRequest(Result.Error("Invalid data"));
        }
        var item = await GetPartyVerification(GetPartyId());
        if (item.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(MSG.CannotUpdateAtThisStatus));

        var result = await SaveItem(VerificationCategoryTypes.Financial, JsonConvert.SerializeObject(spec));
        return Ok(JsonConvert.DeserializeObject<VerificationFinancialDTO>(result.Content));
    }

    /// <summary>
    /// Save Agreement
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Agreement)]
    [ProducesResponseType(typeof(VerificationAgreementDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationAgreementDTO>> SaveAgreement([FromBody] object spec)
    {
        var json = JsonConvert.SerializeObject(spec);
        if (_tenantId != 10005 && !VerificationAgreementDTO.TryParse(json, out _))
        {
            return BadRequest(Result.Error("Invalid data"));
        }
        var item = await GetPartyVerification(GetPartyId());
        if (item.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(MSG.CannotUpdateAtThisStatus));

        var result = await SaveItem(VerificationCategoryTypes.Agreement, JsonConvert.SerializeObject(spec));
        return Ok(JsonConvert.DeserializeObject<VerificationAgreementDTO>(result.Content));
    }

    /// <summary>
    /// Save Quiz
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("quiz")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(VerificationAnswerDTO[]), StatusCodes.Status200OK)]
    // Specially handling the quiz, avoid the wrong request to be saved, using merging instead of replacing
    public async Task<ActionResult<VerificationAnswerDTO[]>> SaveAnswers([FromBody] VerificationAnswerDTO[] spec)
    {
        var partyId = GetPartyId();
        var answer = await tenantDbContext.VerificationItems
            .Where(x => x.Category == VerificationCategoryTypes.Quiz)
            .Where(x => x.Verification.PartyId == partyId)
            .FirstOrDefaultAsync();
        if (answer == null)
        {
            var result = await SaveItem(VerificationCategoryTypes.Quiz, JsonConvert.SerializeObject(spec));
            return Ok(JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(result.Content));
        }

        var answers = JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(answer.Content) ?? [];

        var newAnswers = new List<VerificationAnswerDTO>(answers);
        foreach (var item in spec)
        {
            var exists = newAnswers.FirstOrDefault(x => x.Id.Equals(item.Id));
            if (exists == null) newAnswers.Add(item);
            else exists.Answer = item.Answer;
        }

        var resultMerged = await SaveItem(VerificationCategoryTypes.Quiz, JsonConvert.SerializeObject(newAnswers.ToArray()));
        return Ok(JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(resultMerged.Content));
    }

    /// <summary>
    /// Get Quizzes for verification
    /// </summary>
    /// <returns></returns>
    [HttpGet("quiz")]
    [ProducesResponseType(typeof(List<VerificationQuizDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<VerificationQuizDTO>>> GetQuizzes()
    {
        var items = await tenantDbContext.TopicContents
            .Where(x => x.Topic.Type == (short)TopicTypes.VerificationQuiz)
            .Where(x => x.Topic.EffectiveTo > DateTime.UtcNow)
            //.Where(x => x.Language == (language ?? "en-US"))
            .ToListAsync();
        return Ok(items.Select(x => x.ToDTO()).ToList());
    }

    /// <summary>
    /// Save Document
    /// </summary>
    /// <returns></returns>
    [HttpPut("submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit()
    {
        var verification = await GetPartyVerification(GetPartyId());
        if (verification.Id == 0)
            return NotFound();

        if (!verification.TryChangeToAwaitingReview())
            return BadRequest(Result.Error(MSG.CannotUpdateAtThisStatus));

        tenantDbContext.Verifications.Update(verification);
        await tenantDbContext.SaveChangesAsync();

        await mediator.Publish(new VerificationSubmittedEvent(verification));
        return NoContent();
    }

    /// <summary>
    /// Upload document
    /// </summary>
    /// <returns></returns>


    [HttpPost("document/upload")]
    [ProducesResponseType(typeof(Medium.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, string? type = null)
    {
        // Validate that we have a file
        if (file.Length == 0)
            return BadRequest("No file uploaded");

        // Process image if the file is an image type
        // if (IsImage(file.ContentType))
        // {
        //     file = await MagickService.CompressImageAsync(file);
        // }

        var partyId = GetPartyId();
        var verification = await tenantDbContext.Verifications
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Type == (int)VerificationTypes.Verification)
            .SingleOrDefaultAsync();

        if (verification == null)
            return NotFound();

        var supplement = tenantDbContext.VerificationItems
            .Where(x => x.Verification.PartyId == partyId)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);

        if (supplement == null)
        {
            supplement = new VerificationItem
                { VerificationId = verification.Id, Category = VerificationCategoryTypes.Document, Content = "[]" };
            tenantDbContext.VerificationItems.Add(supplement);
            await tenantDbContext.SaveChangesAsync();
        }

        var medium = await UploadForParty(file, VerificationCategoryTypes.Document, supplement.VerificationId);
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

        List<VerificationDocumentMedium> docs;
        try
        {
            docs = JsonConvert.DeserializeObject<List<VerificationDocumentMedium>>(supplement.Content)
                   ?? new List<VerificationDocumentMedium>();
        }
        catch (Exception)
        {
            docs = new List<VerificationDocumentMedium>();
        }

        if (!string.Equals(documentType, VerificationDocumentTypes.Other,
                StringComparison.CurrentCultureIgnoreCase))
        {
            foreach (var item in docs.Where(item => item.DocumentType == documentType))
            {
                item.FileName = VerificationDocumentTypes.Other;
            }
        }

        docs.Add(doc);

        supplement.Content = Utils.JsonSerializeObject(docs);
        supplement.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.VerificationItems.Update(supplement);

        if (doc.DocumentType == VerificationDocumentTypes.Other
            && verification.TryChangeToAwaitingReview())
            tenantDbContext.Verifications.Update(verification);

        await tenantDbContext.SaveChangesAsync();

        return Ok(medium);
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerificationAuthCode spec)
    {
        var partyId = GetPartyId();
        var verification = await tenantDbContext.Verifications
            .Where(x => x.PartyId == partyId && x.Type == (short)VerificationTypes.Verification)
            .Where(x => x.Id == Verification.HashDecode(spec.HashId))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (verification == null)
            return NotFound();

        if (verification.Status != (int)VerificationStatusTypes.AwaitingCodeVerify)
            return BadRequest("Verification status is not awaiting code verify");

        var item = await tenantDbContext.AuthCodes
            .Where(x => x.PartyId == verification.PartyId)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.PaperMail)
            .Where(x => x.MethodValue == verification.Id.ToString())
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound("No paper mail code found");

        if (item.Code != spec.Code)
            return BadRequest("Invalid code");

        item.Status = (short)AuthCodeStatusTypes.Invalid;
        verification.Status = (int)VerificationStatusTypes.CodeVerified;
        tenantDbContext.AuthCodes.Update(item);
        tenantDbContext.Verifications.Update(verification);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }

    private async Task<Medium> UploadForParty(IFormFile file, string type, long rowId = 0)
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
       
        return result;
    }

    //private async Task<bool> canUpdateAsync(long partyId)
    //{
    //    return await _tenantDbContext.Verifications
    //        .Where(x => x.PartyId == partyId)
    //        .Where(x => x.Type == VerificationTypes.Verification)
    //        .Select(x => x.Status == (int)VerificationStatusTypes.Incomplete)
    //        .AnyAsync();
    //}

    private async Task<VerificationItem> SaveItem(string category, string json)
    {
        var verify = await GetPartyVerification(GetPartyId());
        var item = verify.VerificationItems.FirstOrDefault(x => x.Category == category);
        if (item == null)
        {
            item = new VerificationItem
            {
                Category = category,
                Content = json,
                VerificationId = verify.Id,
            };
            verify.VerificationItems.Add(item);
        }
        else
        {
            item.Content = json;
            item.UpdatedOn = DateTime.UtcNow;
        }

        await tenantDbContext.SaveChangesAsync();
        return item;
    }

    private async Task<Verification> GetPartyVerification(long partyId)
    {
        var isVerificationExist = await tenantDbContext.Verifications
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Type == (short)VerificationTypes.Verification)
            .AnyAsync();
        if (!isVerificationExist)
        {
            var item = new Verification
            {
                Note = "",
                PartyId = partyId,
                Type = (short)VerificationTypes.Verification,
                Status = (int)VerificationStatusTypes.Incomplete,
            };
            await tenantDbContext.Verifications.AddAsync(item);
            await tenantDbContext.SaveChangesAsync();
        }

        var verification = await tenantDbContext.Verifications
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Type == (short)VerificationTypes.Verification)
            .OrderByDescending(x => x.Id)
            .Include(x => x.Party)
            .Include(x => x.VerificationItems)
            .FirstAsync();

        return verification;
    }
}