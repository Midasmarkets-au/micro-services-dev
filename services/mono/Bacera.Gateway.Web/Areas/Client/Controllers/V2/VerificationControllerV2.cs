
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Response;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Verification")]
[Area("Client")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/[Area]/verification")]
public class VerificationControllerV2(
    TenantDbContext tenantDbContext,
    UserService userService,
    ConfigurationService configurationService,
    IStorageService storageSvc,
    ITenantGetter tenantGetter,
    IMediator mediator,
    IMyCache myCache)
    : ClientBaseControllerV2
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    /// <summary>
    /// Get User Verification
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(VerificationSettingV2))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(Result))]
    public async Task<IActionResult> Index()
    {
        var partyId = GetPartyId();
        if (await userService.IsBannedVerificationQuizAsync(partyId))
            return BadRequest(Result.Error(ResultMessage.Verification.VerificationDisabled));

        var party = await userService.GetPartyAsync(partyId);
        var language = party.Language;
        if (language == LanguageTypes.Indonesian) language = LanguageTypes.English;

        var docRaw = await tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.BcrLegalDocument)
            .Select(x => x.Data)
            .FirstAsync();

        var docData = BCRLegalDocumentData.Parse(docRaw);

        var documents = docData.Documents
            .Select(x => new BCRLegalDocument
            {
                Title = x.Title,
                Src = x.Title != "Contract Specifications"
                    ? $"{docData.BaseUrl}/{language}/{x.Src}"
                    : $"{x.Src}{LanguageTypes.ParseCrmLanguage(language)}/contract-specifications"
            })
            .ToList();
        
        var quizEnabled = await configurationService.GetVerificationQuizToggleSwitchAsync();
        var settings = quizEnabled ? VerificationCategoryTypes.All : VerificationCategoryTypes.NoQuiz;

        var verification = await GetVerificationAsync();
        var items = await tenantDbContext.VerificationItems
            .Where(x => x.VerificationId == verification.Id)
            .Where(x => settings.Contains(x.Category))
            .Select(x => new { x.Category, x.Content })
            .ToDictionaryAsync(x => x.Category, x => x.Content);

        if (_tenantId == 10005)
        {
            var jpRes = new VerificationJpSettingV2
            {
                Documents = documents,
                Settings = settings,
                Data = VerificationDTOV2.FromDictionary(items)
            };

            jpRes.Data.Status = (VerificationStatusTypes)verification.Status;
            jpRes.Data.CreatedOn = verification.CreatedOn;
            jpRes.Data.UpdatedOn = verification.UpdatedOn;
            return Ok(jpRes);
        }
            

        var result = new VerificationSettingV2
        {
            Documents = documents,
            Settings = settings,
            Data = VerificationDTOV2.FromDictionary(items)
        };

        result.Data.Status = (VerificationStatusTypes)verification.Status;
        result.Data.CreatedOn = verification.CreatedOn;
        result.Data.UpdatedOn = verification.UpdatedOn;
        return Ok(result);
    }

    /// <summary>
    /// Save Started
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Started)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> SaveStarted([FromBody] object spec)
    {
        var json = JsonConvert.SerializeObject(spec);
        var parseResult = VerificationStartedDTO.TryParse(json, out var dto);
        if (GetTenantId() == 10005 && !parseResult)
        {
            return BadRequest(ToErrorResult("Invalid data"));
        }
        var verification = await GetVerificationAsync();
        if (verification.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(ResultMessage.Verification.CannotUpdateAtThisStatus));

        if (GetTenantId() == 1 && !dto.VerificationQuestionsDTO.IsValid())
        {
            await userService.BanVerificationQuizAsync(verification.PartyId);
            return BadRequest(ToErrorResult("Quiz failed, locked out for 1 day."));
        }

        if (string.IsNullOrEmpty(dto.Referral))
        {
            var partyId = GetPartyId();
            var party = await userService.GetPartyAsync(partyId);
            dto.Referral = party.ReferCode;
            spec = dto;
        }
        await SaveItem(verification, VerificationCategoryTypes.Started, JsonConvert.SerializeObject(spec));
        return NoContent();
    }

    /// <summary>
    /// Save Started
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Info)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> SaveInfo([FromBody] object spec)
    {
        
        var json = JsonConvert.SerializeObject(spec);
        var parseResult = VerificationInfoDTO.TryParse(json, out var dto);
        if (GetTenantId() == 10005 && !parseResult)
        {
            return BadRequest(ToErrorResult("Invalid data"));
        }
        
        var verification = await GetVerificationAsync();
        if (verification.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(ResultMessage.Verification.CannotUpdateAtThisStatus));

        await SaveItem(verification, VerificationCategoryTypes.Info, JsonConvert.SerializeObject(spec));
        var socialData = JsonConvert.SerializeObject(dto.SocialMedium);
        var supplement = await tenantDbContext.Supplements.FirstOrDefaultAsync(x =>
            x.RowId == verification.PartyId && x.Type == (int)SupplementTypes.SocialMediaRecord);
        if (supplement == null)
        {
            supplement = Supplement.Build(SupplementTypes.SocialMediaRecord, verification.PartyId, socialData);
            tenantDbContext.Supplements.Add(supplement);
        }
        else
        {
            supplement.Data = socialData;
            tenantDbContext.Supplements.Update(supplement);
        }

        await tenantDbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Save Financial
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Financial)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> SaveFinancial([FromBody] object spec)
    {
        var json = JsonConvert.SerializeObject(spec);
        var parseResult = VerificationFinancialDTO.TryParse(json, out var dto);
        if (GetTenantId() == 10005 && !parseResult)
        {
            return BadRequest(ToErrorResult("Invalid data"));
        }
        
        var verification = await GetVerificationAsync();
        if (verification.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(ResultMessage.Verification.CannotUpdateAtThisStatus));

        if (GetTenantId() == 1 && !dto.IsValid())
        {
            await userService.BanVerificationQuizAsync(verification.PartyId);
            return BadRequest(ToErrorResult("Quiz failed, locked out for 1 day."));
        }

        await SaveItem(verification, VerificationCategoryTypes.Financial, JsonConvert.SerializeObject(spec));
        return NoContent();
    }

    /// <summary>
    /// Save Agreement
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Agreement)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> SaveAgreement([FromBody] object spec)
    {
        var json = JsonConvert.SerializeObject(spec);
        var parseResult = VerificationAgreementDTO.TryParse(json, out var dto);
        if (GetTenantId() == 10005 && !parseResult)
        {
            return BadRequest(ToErrorResult("Invalid data"));
        }
        
        var verification = await GetVerificationAsync();
        if (verification.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(ResultMessage.Verification.CannotUpdateAtThisStatus));

        await SaveItem(verification, VerificationCategoryTypes.Agreement, JsonConvert.SerializeObject(spec));
        return NoContent();
    }

    /// <summary>
    /// Get Verification Quiz
    /// </summary>
    /// <returns></returns>
    [HttpGet(VerificationCategoryTypes.Quiz)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Quiz()
    {
        var questions = await QueryQuizQuestions();
        var result = questions.OrderBy(_ => Guid.NewGuid()).Take(10).ToList();
        var key = UserVerificationQuizKey(GetTenantId(), GetPartyId());
        var sentQuestions = result.ToDictionary(x => x.Id, x => x.Answer);
        await myCache.HSetStringAsync(key, "questions", JsonConvert.SerializeObject(sentQuestions));
        await myCache.HSetStringAsync(key, "count", "0", TimeSpan.FromDays(1));
        return Ok(result);
    }

    /// <summary>
    /// Save Quiz
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost(VerificationCategoryTypes.Quiz)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> SaveQuiz([FromBody] VerificationAnswerDTO[] spec)
    {
        var verification = await GetVerificationAsync();
        if (verification.Status != (int)VerificationStatusTypes.Incomplete)
            return BadRequest(ToErrorResult(ResultMessage.Verification.CannotUpdateAtThisStatus));

        var key = UserVerificationQuizKey(GetTenantId(), GetPartyId());
        var questionJson = await myCache.HGetStringAsync(key, "questions");
        var countString = await myCache.HGetStringAsync(key, "count");

        // check answer in spec
        if (questionJson == null || countString == null || !VerificationQuizQuestion.TryParseAnswer(questionJson, out var sentQuestions))
        {
            var questions = await QueryQuizQuestions();
            var correctCount = spec.Count(x => questions.Any(q => q.Id == x.Id && q.Answer == x.Answer));
            if (correctCount < 9)
            {
                return BadRequest(ToErrorResult("Quiz failed, please try again."));
            }
        }
        else if (AppEnvironment.IsProduction())
        {
            var count = int.Parse(countString);
            var correctCount = spec.Count(x => sentQuestions.TryGetValue(x.Id, out var answer) && answer == x.Answer);
            if (correctCount < 9)
            {
                if (count >= 2)
                {
                    await userService.BanVerificationQuizAsync(verification.PartyId);
                    return BadRequest(Result.Error("Quiz failed, locked out for 1 day.", new { round = count }));
                }

                count++;
                await myCache.HSetStringAsync(key, "count", count.ToString(), TimeSpan.FromDays(1));
                return BadRequest(Result.Error("Quiz failed, please try again.", new { round = count }));
            }
        }

        await SaveItem(verification, VerificationCategoryTypes.Quiz, JsonConvert.SerializeObject(spec));
        return NoContent();
    }

    private static string UserVerificationQuizKey(long tenantId, long partyId) =>
        $"user_verification_quiz_tid:{tenantId}_pid:{partyId}";

    /// <summary>
    /// Save Document
    /// </summary>
    /// <returns></returns>
    [HttpPut("submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit()
    {
        var verification = await GetVerificationAsync();

        if (!verification.TryChangeToAwaitingReview())
            return BadRequest(Result.Error(ResultMessage.Verification.CannotUpdateAtThisStatus));

        tenantDbContext.Verifications.Update(verification);
        await tenantDbContext.SaveChangesAsync();
        await mediator.Publish(new VerificationSubmittedEvent(verification));
        return NoContent();
    }

    [HttpPost("document/submit")]
    public async Task<IActionResult> DocumentSubmit()
    {
        var files = Request.Form.Files.GetFiles("files[]").ToArray();
        var types = Request.Form["types[]"].ToArray();
        var verification = await GetVerificationAsync();
        var docItem = await tenantDbContext.VerificationItems.FirstOrDefaultAsync(x =>
            x.VerificationId == verification.Id && x.Category == VerificationCategoryTypes.Document);

        if (docItem == null)
        {
            docItem = new VerificationItem
            {
                VerificationId = verification.Id,
                Category = VerificationCategoryTypes.Document,
                Content = "[]"
            };
            tenantDbContext.VerificationItems.Add(docItem);
            await tenantDbContext.SaveChangesAsync();
        }

        var docs = VerificationDocumentMedium.FromJson(docItem.Content);
        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var type = types[i];
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var medium = await storageSvc.UploadFileAndSaveMediaAsync(
                memoryStream
                , Guid.NewGuid().ToString()
                , Path.GetExtension(file.FileName).ToLower()
                , VerificationCategoryTypes.Document
                , verification.Id
                , file.ContentType
                , GetTenantId()
                , verification.PartyId);

            if (type != VerificationDocumentTypes.Other)
            {
                foreach (var item in docs.Where(item => item.DocumentType == type))
                {
                    item.FileName = VerificationDocumentTypes.Other;
                }
            }

            var doc = new VerificationDocumentMedium
            {
                Id = medium.Id,
                ContentType = medium.ContentType,
                FileName = medium.FileName,
                Guid = medium.Guid,
                Url = medium.Url,
                Type = medium.Type,
                Status = 0,
                DocumentType = type,
                RejectedOn = null,
                RejectedReason = "",
            };

            docs.Add(doc);
        }

        docItem.Content = JsonConvert.SerializeObject(docs);
        docItem.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.VerificationItems.Update(docItem);

        verification.Status = (int)VerificationStatusTypes.AwaitingReview;
        verification.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.Verifications.Update(verification);
        await tenantDbContext.SaveChangesAsync();
        await mediator.Publish(new VerificationSubmittedEvent(verification));
        return NoContent();
    }

    [HttpPost("document/media/submit")]
    public async Task<IActionResult> DocumentMediaSubmit([FromBody] VerificationMediaSubmitModel spec)
    {
        var verification = await GetVerificationAsync();
        var docItem = await tenantDbContext.VerificationItems.FirstOrDefaultAsync(x =>
            x.VerificationId == verification.Id && x.Category == VerificationCategoryTypes.Document);

        if (docItem == null)
        {
            docItem = new VerificationItem
            {
                VerificationId = verification.Id,
                Category = VerificationCategoryTypes.Document,
                Content = "[]"
            };
            tenantDbContext.VerificationItems.Add(docItem);
            await tenantDbContext.SaveChangesAsync();
        }

        var docs = VerificationDocumentMedium.FromJson(docItem.Content);
        foreach (var medium in spec.Media.Select(model => model.ToModel()))
        {
            if (medium.Type != VerificationDocumentTypes.Other)
            {
                foreach (var item in docs.Where(item => item.DocumentType == medium.Type))
                {
                    item.FileName = VerificationDocumentTypes.Other;
                }
            }

            var doc = new VerificationDocumentMedium
            {
                Id = medium.Id,
                ContentType = medium.ContentType,
                FileName = medium.FileName,
                Guid = medium.Guid,
                Url = medium.Url,
                Type = medium.Type,
                Status = 0,
                DocumentType = medium.Type,
                RejectedOn = null,
                RejectedReason = "",
            };

            docs.Add(doc);
        }

        docItem.Content = JsonConvert.SerializeObject(docs);
        docItem.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.VerificationItems.Update(docItem);

        verification.Status = (int)VerificationStatusTypes.AwaitingReview;
        verification.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.Verifications.Update(verification);
        await tenantDbContext.SaveChangesAsync();
        await mediator.Publish(new VerificationSubmittedEvent(verification));
        return NoContent();
    }

    [HttpPost("document/chunk/submit")]
    public async Task<IActionResult> DocumentChunkSubmit()
    {
        var files = Request.Form.Files.GetFiles("files[]").ToArray();
        var types = Request.Form["types[]"].ToArray();
        var contentTypes = Request.Form["contentTypes[]"].ToArray();
        
        var verification = await GetVerificationAsync();
        var docItem = await tenantDbContext.VerificationItems.FirstOrDefaultAsync(x =>
            x.VerificationId == verification.Id && x.Category == VerificationCategoryTypes.Document);

        if (docItem == null)
        {
            docItem = new VerificationItem
            {
                VerificationId = verification.Id,
                Category = VerificationCategoryTypes.Document,
                Content = "[]"
            };
            tenantDbContext.VerificationItems.Add(docItem);
            await tenantDbContext.SaveChangesAsync();
        }

        var docs = VerificationDocumentMedium.FromJson(docItem.Content);
        var fileDict = new Dictionary<string, (string, List<IFormFile>)>();
        for (var i = 0; i < files.Length; i++)
        {
            var type = types[i];
            var contentType = contentTypes[i]!;
            if (string.IsNullOrWhiteSpace(type))
            {
                type = VerificationDocumentTypes.Other;
            }

            if (!fileDict.TryGetValue(type, out var value))
            {
                value = (contentType, []);
                fileDict[type] = value;
            }

            value.Item2.Add(files[i]);
        }

        foreach (var type in fileDict.Keys)
        {
            var (contentType, filesOfType) = fileDict[type];
            if (filesOfType.Count == 0)
                continue;
            var fileName = filesOfType[0].FileName;
            using var memoryStream = new MemoryStream();
            foreach (var fileChunk in filesOfType)
            {
                await fileChunk.CopyToAsync(memoryStream);
            }

            var medium = await storageSvc.UploadFileAndSaveMediaAsync(
                memoryStream
                , Guid.NewGuid().ToString()
                , Path.GetExtension(fileName).ToLower()
                , VerificationCategoryTypes.Document
                , verification.Id
                , contentType
                , GetTenantId()
                , verification.PartyId);

            if (type != VerificationDocumentTypes.Other)
            {
                foreach (var item in docs.Where(item => item.DocumentType == type))
                {
                    item.FileName = VerificationDocumentTypes.Other;
                }
            }

            var doc = new VerificationDocumentMedium
            {
                Id = medium.Id,
                ContentType = medium.ContentType,
                FileName = medium.FileName,
                Guid = medium.Guid,
                Url = medium.Url,
                Type = medium.Type,
                Status = 0,
                DocumentType = type,
                RejectedOn = null,
                RejectedReason = "",
            };

            docs.Add(doc);
        }

        docItem.Content = JsonConvert.SerializeObject(docs);
        docItem.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.VerificationItems.Update(docItem);

        verification.Status = (int)VerificationStatusTypes.AwaitingReview;
        verification.UpdatedOn = DateTime.UtcNow;
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
    [RequestSizeLimit(20_000_000)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, string? type = null)
    {
        var verification = await GetVerificationAsync();
        var docItem = await tenantDbContext.VerificationItems.FirstOrDefaultAsync(x =>
            x.VerificationId == verification.Id && x.Category == VerificationCategoryTypes.Document);

        if (docItem == null)
        {
            docItem = new VerificationItem
            {
                VerificationId = verification.Id,
                Category = VerificationCategoryTypes.Document,
                Content = "[]"
            };
            tenantDbContext.VerificationItems.Add(docItem);
            await tenantDbContext.SaveChangesAsync();
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var medium = await storageSvc.UploadFileAndSaveMediaAsync(
            memoryStream
            , Guid.NewGuid().ToString()
            , Path.GetExtension(file.FileName).ToLower()
            , VerificationCategoryTypes.Document
            , verification.Id
            , file.ContentType
            , GetTenantId()
            , verification.PartyId);

        type ??= VerificationDocumentTypes.All.Select(x => x.ToLower()).Contains(type?.Trim().ToLower() ?? "")
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
            DocumentType = type,
            RejectedOn = null,
            RejectedReason = "",
        };

        var docs = VerificationDocumentMedium.FromJson(docItem.Content);

        if (type != VerificationDocumentTypes.Other)
        {
            foreach (var item in docs.Where(item => item.DocumentType == type))
            {
                item.FileName = VerificationDocumentTypes.Other;
            }
        }
        else
        {
            verification.TryChangeToAwaitingReview();
        }

        docs.Add(doc);
        docItem.Content = JsonConvert.SerializeObject(docs);
        docItem.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.VerificationItems.Update(docItem);

        verification.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.Verifications.Update(verification);
        await tenantDbContext.SaveChangesAsync();

        return Ok(medium.Guid);
    }

    private async Task<Verification> GetVerificationAsync()
    {
        var partyId = GetPartyId();
        var item = await tenantDbContext.Verifications
            .Where(x => x.PartyId == partyId && x.Type == (int)VerificationTypes.Verification)
            .FirstOrDefaultAsync();
        if (item != null) return item;

        var verification = new Verification
        {
            PartyId = partyId,
            Type = (int)VerificationTypes.Verification,
            Status = (int)VerificationStatusTypes.Incomplete,
            Note = "",
        };
        await tenantDbContext.Verifications.AddAsync(verification);
        await tenantDbContext.SaveChangesAsync();
        return verification;
    }

    private async Task SaveItem(Verification verification, string category, string json)
    {
        var item = await tenantDbContext.VerificationItems
            .Where(x => x.VerificationId == verification.Id && x.Category == category)
            .SingleOrDefaultAsync();
        if (item == null)
        {
            item = new VerificationItem
            {
                Category = category, Content = json,
                CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow
            };
            verification.VerificationItems.Add(item);
        }
        else
        {
            item.Content = json;
            item.UpdatedOn = DateTime.UtcNow;
            tenantDbContext.VerificationItems.Update(item);
        }

        verification.UpdatedOn = DateTime.UtcNow;
        await tenantDbContext.SaveChangesAsync();
    }

    private async Task<List<VerificationQuizQuestion>> QueryQuizQuestions()
    {
        var partyId = GetPartyId();
        var lang = await tenantDbContext.Parties
            .Where(x => x.Id == partyId)
            .Select(x => x.Language)
            .SingleOrDefaultAsync() ?? LanguageTypes.English;

        var quizJson = await tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.VerificationQuiz && x.RowId == LanguageTypes.LangToId[lang])
            .Select(x => x.Data)
            .FirstOrDefaultAsync();
        return quizJson == null ? [] : VerificationQuizQuestion.FromJsonArrayToList(quizJson);
    }
}