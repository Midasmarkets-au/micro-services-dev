using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Bacera.Gateway.Web.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web;

public class VerificationDTO
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public int Status { get; set; } = (int)VerificationStatusTypes.Incomplete;
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public UserInfo? Operator => JsonConvert.DeserializeObject<VerificationNoteResponseModel>(OperatorRaw)?.Operator;
    [Newtonsoft.Json.JsonIgnore] public string OperatorRaw { get; set; } = string.Empty;
    public VerificationStartedDTO? Started => ItemsRaw.ToStartedDTO();
    public VerificationInfoDTO? Info => ItemsRaw.ToInfoDTO();
    public VerificationFinancialDTO? Financial => ItemsRaw.ToFinancialDTO();
    public VerificationAnswerDTO[]? Quiz => ItemsRaw.ToQuizDTO();
    public VerificationAgreementDTO? Agreement => ItemsRaw.ToAgreementDTO();
    public IList<VerificationDocumentMedium>? Document => ItemsRaw.ToDocumentDTO();
    public SiteTypes SiteId { get; set; }
    public TenantUserBasicModel User { get; set; } = new();

    [Newtonsoft.Json.JsonIgnore] public List<VerificationItem> ItemsRaw { get; set; } = [];

    public List<Comment.TenantDTO> Comments { get; set; } = [];
    public bool HasComment { get; set; }

    public bool IsValidated() => Started != null &&
                                 Info != null &&
                                 Financial != null &&
                                 // Quiz != null &&
                                 Agreement != null &&
                                 Document != null;

    public VerificationDTO SetComments(List<Comment.TenantDTO> comments)
    {
        Comments = comments;
        HasComment = comments.Count > 0;
        return this;
    }
}

public class VerificationJpDTO
{
    public long Id { get; set; }

    public string HashId => Verification.HashEncode(Id);

    public long PartyId { get; set; }
    public int Status { get; set; } = (int)VerificationStatusTypes.Incomplete;
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public UserInfo? Operator => JsonConvert.DeserializeObject<VerificationNoteResponseModel>(OperatorRaw)?.Operator;
    [Newtonsoft.Json.JsonIgnore] public string OperatorRaw { get; set; } = string.Empty;
    public dynamic? Started => GetContent(VerificationCategoryTypes.Started);
    public dynamic? Info => GetContent(VerificationCategoryTypes.Info);
    public dynamic? Financial => GetContent(VerificationCategoryTypes.Financial);
    public VerificationAnswerDTO[]? Quiz => ItemsRaw.ToQuizDTO();
    public dynamic? Agreement => GetContent(VerificationCategoryTypes.Agreement);
    public IList<VerificationDocumentMedium>? Document => ItemsRaw.ToDocumentDTO();
    public SiteTypes SiteId { get; set; }
    public TenantUserBasicModel User { get; set; } = new();

    [Newtonsoft.Json.JsonIgnore] public List<VerificationItem> ItemsRaw { get; set; } = [];

    public List<Comment.TenantDTO> Comments { get; set; } = [];
    public bool HasComment { get; set; }

    public dynamic? GetContent(string type)
    {
        var content = ItemsRaw.FirstOrDefault(x => x.Category == type)?.Content;
        return content == null ? null : Utils.JsonDeserializeDynamic(content);
    }

    public VerificationJpDTO SetComments(List<Comment.TenantDTO> comments)
    {
        Comments = comments;
        HasComment = comments.Count > 0;
        return this;
    }
}

public class VerificationJpClientDTO
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long Id { get; set; }

    public string HashId => Verification.HashEncode(Id);

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long PartyId { get; set; }

    public int Status { get; set; } = (int)VerificationStatusTypes.Incomplete;
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public UserInfo? Operator => JsonConvert.DeserializeObject<VerificationNoteResponseModel>(OperatorRaw)?.Operator;
    [Newtonsoft.Json.JsonIgnore] public string OperatorRaw { get; set; } = string.Empty;
    public dynamic? Started => GetContent(VerificationCategoryTypes.Started);
    public dynamic? Info => GetContent(VerificationCategoryTypes.Info);
    public dynamic? Financial => GetContent(VerificationCategoryTypes.Financial);
    public VerificationAnswerDTO[]? Quiz => ItemsRaw.ToQuizDTO();
    public dynamic? Agreement => GetContent(VerificationCategoryTypes.Agreement);
    public IList<VerificationDocumentMedium>? Document => ItemsRaw.ToDocumentDTO();
    public SiteTypes SiteId { get; set; }
    public TenantUserBasicModel User { get; set; } = new();

    [Newtonsoft.Json.JsonIgnore] public List<VerificationItem> ItemsRaw { get; set; } = [];

    public List<Comment.TenantDTO> Comments { get; set; } = [];
    public bool HasComment { get; set; }

    public bool IsValidated() => Started != null &&
                                 Info != null &&
                                 Financial != null &&
                                 // Quiz != null &&
                                 Agreement != null &&
                                 Document != null;

    public dynamic? GetContent(string type)
    {
        var content = ItemsRaw.FirstOrDefault(x => x.Category == type)?.Content;
        return content == null ? null : Utils.JsonDeserializeDynamic(content);
    }

    public VerificationJpClientDTO SetComments(List<Comment.TenantDTO> comments)
    {
        Comments = comments;
        HasComment = comments.Count > 0;
        return this;
    }
}

public class VerificationDTOV2
{
    public VerificationStatusTypes Status { get; set; } = VerificationStatusTypes.Incomplete;
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public VerificationStartedDTO? Started { get; set; }
    public VerificationInfoDTO? Info { get; set; }
    public VerificationFinancialDTO? Financial { get; set; }
    public VerificationAnswerDTO[]? Quiz { get; set; }
    public VerificationAgreementDTO? Agreement { get; set; }
    public IList<VerificationDocumentMedium>? Document { get; set; }

    public static VerificationDTOV2 FromDictionary(Dictionary<string, string> items)
    {
        var dto = new VerificationDTOV2();
        foreach (var (k, v) in items)
        {
            if (k == VerificationCategoryTypes.Started)
                dto.Started = JsonConvert.DeserializeObject<VerificationStartedDTO>(v);
            else if (k == VerificationCategoryTypes.Info)
                dto.Info = JsonConvert.DeserializeObject<VerificationInfoDTO>(v);
            else if (k == VerificationCategoryTypes.Financial)
                dto.Financial = JsonConvert.DeserializeObject<VerificationFinancialDTO>(v);
            else if (k == VerificationCategoryTypes.Quiz)
                dto.Quiz = JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(v);
            else if (k == VerificationCategoryTypes.Agreement)
                dto.Agreement = JsonConvert.DeserializeObject<VerificationAgreementDTO>(v);
            else if (k == VerificationCategoryTypes.Document)
                dto.Document = JsonConvert.DeserializeObject<List<VerificationDocumentMedium>>(v);
        }

        return dto;
    }
}

public class VerificationJpDTOV2
{
    public VerificationStatusTypes Status { get; set; } = VerificationStatusTypes.Incomplete;
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public dynamic? Started { get; set; }
    public dynamic? Info { get; set; }
    public dynamic? Financial { get; set; }
    public VerificationAnswerDTO[]? Quiz { get; set; }
    public dynamic? Agreement { get; set; }
    public IList<VerificationDocumentMedium>? Document { get; set; }

    public static VerificationJpDTOV2 FromDictionary(Dictionary<string, string> items)
    {
        var dto = new VerificationJpDTOV2();
        foreach (var (k, v) in items)
        {
            if (k == VerificationCategoryTypes.Started)
                dto.Started = Utils.JsonDeserializeDynamic(v);
            else if (k == VerificationCategoryTypes.Info)
                dto.Info = Utils.JsonDeserializeDynamic(v);
            else if (k == VerificationCategoryTypes.Financial)
                dto.Financial = Utils.JsonDeserializeDynamic(v);
            else if (k == VerificationCategoryTypes.Quiz)
                dto.Quiz = JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(v);
            else if (k == VerificationCategoryTypes.Agreement)
                dto.Agreement = Utils.JsonDeserializeDynamic(v);
            else if (k == VerificationCategoryTypes.Document)
                dto.Document = JsonConvert.DeserializeObject<List<VerificationDocumentMedium>>(v);
        }

        return dto;
    }
}

[Serializable]
public class VerificationStartedDTO : IVerificationRequest
{
    [Required]
    [JsonProperty("currency")]
    [JsonPropertyName("currency")]
    public int Currency { get; set; }

    [Required]
    [JsonProperty("accountType")]
    [JsonPropertyName("accountType")]
    public int AccountType { get; set; }

    [Required]
    [JsonProperty("platform")]
    [JsonPropertyName("platform")]
    public int Platform { get; set; }

    [Required]
    [JsonProperty("serviceId")]
    [JsonPropertyName("serviceId")]
    public int ServiceId { get; set; }

    [Required]
    [JsonProperty("leverage")]
    [JsonPropertyName("leverage")]
    public int Leverage { get; set; }

    [JsonProperty("referral")]
    [JsonPropertyName("referral")]
    public string Referral { get; set; } = string.Empty;

    [Required]
    [JsonProperty("questions")]
    [JsonPropertyName("questions")]
    public VerificationQuestionsDTO VerificationQuestionsDTO { get; set; } = null!;

    public static bool TryParse(string json, out VerificationStartedDTO result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<VerificationStartedDTO>(json)!;
            return true;
        }
        catch
        {
            result = new VerificationStartedDTO();
            return false;
        }
    } 
}

[Serializable]
public class VerificationFinancialDTO : IVerificationRequest
{
    [JsonProperty("industry")]
    [JsonPropertyName("industry")]
    [Required]
    public string Industry { get; set; } = null!;

    [JsonProperty("position")]
    [JsonPropertyName("position")]
    [Required]
    public string Position { get; set; } = null!;

    [JsonProperty("income")]
    [JsonPropertyName("income")]
    [Required]
    public string Income { get; set; } = null!;

    [JsonProperty("investment")]
    [JsonPropertyName("investment")]
    [Required]
    public string Investment { get; set; } = null!;

    [JsonProperty("fund")]
    [JsonPropertyName("fund")]
    [Required]
    public List<string> Fund { get; set; } = new();

    [JsonProperty("bg1")]
    [JsonPropertyName("bg1")]
    [Required]
    public string Bg1 { get; set; } = null!;

    [JsonProperty("bg2")]
    [JsonPropertyName("bg2")]
    [Required]
    public string Bg2 { get; set; } = null!;

    [JsonProperty("bg2_more")]
    [JsonPropertyName("bg2_more")]
    public string? Bg2More { get; set; }

    [JsonProperty("exp1")]
    [JsonPropertyName("exp1")]
    [Required]
    public string Exp1 { get; set; } = null!;

    [JsonProperty("exp2")]
    [JsonPropertyName("exp2")]
    [Required]
    public string Exp2 { get; set; } = null!;

    [JsonProperty("exp3")]
    [JsonPropertyName("exp3")]
    [Required]
    public string Exp3 { get; set; } = null!;

    [JsonProperty("exp4")]
    [JsonPropertyName("exp4")]
    [Required]
    public string Exp4 { get; set; } = null!;

    [JsonProperty("exp5")]
    [JsonPropertyName("exp5")]
    [Required]
    public string Exp5 { get; set; } = null!;

    [JsonProperty("exp1_employer")]
    [JsonPropertyName("exp1_employer")]
    public string? Exp1Employer { get; set; }

    [JsonProperty("exp1_position")]
    [JsonPropertyName("exp1_position")]
    public string? Exp1Position { get; set; }

    [JsonProperty("exp1_remuneration")]
    [JsonPropertyName("exp1_remuneration")]
    public string? Exp1Remuneration { get; set; }

    [JsonProperty("exp2_more")]
    [JsonPropertyName("exp2_more")]
    public string? Exp2More { get; set; }

    [JsonProperty("exp3_more")]
    [JsonPropertyName("exp3_more")]
    public string? Exp3More { get; set; }

    [JsonProperty("exp4_more")]
    [JsonPropertyName("exp4_more")]
    public string? Exp4More { get; set; }

    [JsonProperty("exp5_more")]
    [JsonPropertyName("exp5_more")]
    public string? Exp5More { get; set; }

    public bool IsValid() => Bg1 == "0" && Bg2 == "0";

    public static bool TryParse(string json, out VerificationFinancialDTO result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<VerificationFinancialDTO>(json)!;
            return true;
        }
        catch
        {
            result = new VerificationFinancialDTO();
            return false;
        }
    }
}

[Serializable]
public class VerificationInfoDTO : IVerificationRequest
{
    [JsonProperty("firstName")]
    [JsonPropertyName("firstName")]
    [Required]
    public string FirstName { get; set; } = null!;

    [JsonProperty("lastName")]
    [JsonPropertyName("lastName")]
    [Required]
    public string LastName { get; set; } = null!;

    [JsonProperty("priorName")]
    [JsonPropertyName("priorName")]
    public string? PriorName { get; set; }

    [JsonProperty("birthday")]
    [JsonPropertyName("birthday")]
    [Required]
    public DateOnly Birthday { get; set; }

    [JsonProperty("gender")]
    [JsonPropertyName("gender")]
    [Required]
    public string Gender { get; set; } = null!;

    [JsonProperty("citizen")]
    [JsonPropertyName("citizen")]
    [Required]
    public string Citizen { get; set; } = null!;

    [JsonProperty("ccc")]
    [JsonPropertyName("ccc")]
    [Required]
    public string Ccc { get; set; } = null!;

    [JsonProperty("phone")]
    [JsonPropertyName("phone")]
    [Required]
    public string Phone { get; set; } = null!;

    [JsonProperty("email")]
    [JsonPropertyName("email")]
    [Required]
    public string Email { get; set; } = null!;

    [JsonProperty("address")]
    [JsonPropertyName("address")]
    [Required]
    public string Address { get; set; } = null!;

    [JsonProperty("idType")]
    [JsonPropertyName("idType")]
    [Required]
    public int IdType { get; set; }

    [JsonProperty("idNumber")]
    [JsonPropertyName("idNumber")]
    [Required]
    public string IdNumber { get; set; } = null!;

    [JsonProperty("idIssuer")]
    [JsonPropertyName("idIssuer")]
    [Required]
    public string IdIssuer { get; set; } = null!;

    [JsonProperty("idIssuedOn")]
    [JsonPropertyName("idIssuedOn")]
    public DateOnly? IdIssuedOn { get; set; }

    [JsonProperty("idExpireOn")]
    [JsonPropertyName("idExpireOn")]
    public DateOnly? IdExpireOn { get; set; }

    [JsonProperty("socialMedium")]
    [JsonPropertyName("socialMedium")]
    public List<VerificationSocialMedia> SocialMedium { get; set; } = [];

    public static bool TryParse(string json, out VerificationInfoDTO result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<VerificationInfoDTO>(json)!;
            return true;
        }
        catch
        {
            result = new VerificationInfoDTO();
            return false;
        }
    }
}

[Serializable]
public class VerificationQuestionsDTO
{
    [Required]
    [JsonProperty("q1")]
    [JsonPropertyName("q1")]
    public bool Q1 { get; set; }

    [Required]
    [JsonProperty("q2")]
    [JsonPropertyName("q2")]
    public bool Q2 { get; set; }

    [Required]
    [JsonProperty("q3")]
    [JsonPropertyName("q3")]
    public bool Q3 { get; set; }

    [Required]
    [JsonProperty("q4")]
    [JsonPropertyName("q4")]
    public bool Q4 { get; set; }

    public bool IsValid() => Q1 && Q2 && Q3 && Q4;

    public static bool TryParse(string json, out VerificationQuestionsDTO result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<VerificationQuestionsDTO>(json)!;
            return true;
        }
        catch
        {
            result = new VerificationQuestionsDTO();
            return false;
        }
    }
}

public class VerificationAnswerDTO
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    [Required]
    public long Id { get; set; }

    [JsonProperty("answer")]
    [JsonPropertyName("answer")]
    [Required]
    public string Answer { get; set; } = null!;

    public static bool TryParse(string json, out VerificationAnswerDTO[] result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(json)!;
            return true;
        }
        catch
        {
            result = [];
            return false;
        }
    } 
}

public class VerificationAgreementDTO
{
    [JsonProperty("consent_1")]
    [JsonPropertyName("consent_1")]
    [Required]
    public bool Consent1 { get; set; }

    [JsonProperty("consent_2")]
    [JsonPropertyName("consent_2")]
    [Required]
    public bool Consent2 { get; set; }

    [JsonProperty("consent_3")]
    [JsonPropertyName("consent_3")]
    [Required]
    public bool Consent3 { get; set; }

    public static bool TryParse(string json, out VerificationAgreementDTO result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<VerificationAgreementDTO>(json)!;
            return true;
        }
        catch
        {
            result = new VerificationAgreementDTO();
            return false;
        }
    }
}

public class VerificationSocialMedia
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("account")]
    [JsonPropertyName("account")]
    public string Account { get; set; } = string.Empty;
}

public class VerificationQuizQuestion
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonProperty("answer")]
    [JsonPropertyName("answer")]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string Answer { get; set; } = string.Empty;

    [JsonProperty("options")]
    [JsonPropertyName("options")]
    public OptionsObject Options { get; set; } = new();

    [JsonProperty("question")]
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;


    public class OptionsObject
    {
        [JsonProperty("a")]
        [JsonPropertyName("a")]
        public string A { get; set; } = string.Empty;

        [JsonProperty("b")]
        [JsonPropertyName("b")]
        public string B { get; set; } = string.Empty;

        [JsonProperty("c")]
        [JsonPropertyName("c")]
        public string C { get; set; } = string.Empty;

        [JsonProperty("d")]
        [JsonPropertyName("d")]
        public string D { get; set; } = string.Empty;
    }
    public static List<VerificationQuizQuestion> FromJsonArrayToList(string json)
        => Utils.JsonDeserializeObjectWithDefault<List<VerificationQuizQuestion>>(json);

    public static bool TryParseAnswer(string questionJson, out Dictionary<long, string> result)
    {
        result = new Dictionary<long, string>();
        try
        {
            result = JsonConvert.DeserializeObject<Dictionary<long, string>>(questionJson)!;
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class VerificationAuthCode
{
    public string HashId { get; set; } = null!;
    public string Code { get; set; } = null!;
}

public class VerificationDocumentMedium : Medium.ResponseModel
{
    public VerificationDocumentStatusTypes Status { get; set; }

    public string DocumentType { get; set; } = VerificationDocumentTypes.Other;
    public DateTime? ApprovedOn { get; set; }
    public DateTime? RejectedOn { get; set; }
    public string? RejectedReason { get; set; }

    public static List<VerificationDocumentMedium> FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<List<VerificationDocumentMedium>>(json);
}

public sealed class VerificationMediaSubmitModel
{
    public List<Medium.ClientPageModel> Media { get; set; } = [];
}

public static class VerificationDocumentTypes
{
    public const string IdFront = "id_front";
    public const string IdBack = "id_back";
    public const string Address = "address";
    public const string Other = "other";

    public static readonly List<string> All = [IdFront, IdBack, Address, Other];
    public static readonly List<string> ValidForAutoCreate = [IdFront, IdBack];
    public static readonly List<string> ValidForCNAutoCreate = [IdFront, IdBack];
}

public static class VerificationDTOExtensions
{
    public static IQueryable<VerificationDTO> ToTenantPageModel(this IQueryable<Verification> query,
        bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.PartyTags)
            .Select(x => new VerificationDTO
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Status = x.Status,
                SiteId = (SiteTypes)x.Party.SiteId,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                ItemsRaw = x.VerificationItems.ToList(),
                OperatorRaw = x.Note,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });

    public static VerificationDTO ToDTO(this Verification verification)
    {
        return new VerificationDTO
        {
            Id = verification.Id,
            PartyId = verification.PartyId,
            SiteId = (SiteTypes)verification.Party.SiteId,
            Status = verification.Status,
            CreatedOn = verification.CreatedOn,
            UpdatedOn = verification.UpdatedOn,
            ItemsRaw = verification.VerificationItems.ToList(),
            // Info = verification.VerificationItems.ToInfoDTO(),
            // Started = verification.VerificationItems.ToStartedDTO(),
            // Financial = verification.VerificationItems.ToFinancialDTO(),
            // Quiz = verification.VerificationItems.ToQuizDTO(),
            // Agreement = verification.VerificationItems.ToAgreementDTO(),
            // Document = verification.VerificationItems.ToDocumentDTO(),
            // Operator =
            //     JsonConvert.DeserializeObject<VerificationNoteResponseModel>(verification.Note)?.Operator ?? null,
        };
    }

    public static IQueryable<VerificationJpDTO> ToJpDTO(this IQueryable<Verification> query, bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.PartyTags)
            .Select(x => new VerificationJpDTO
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Status = x.Status,
                SiteId = (SiteTypes)x.Party.SiteId,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                ItemsRaw = x.VerificationItems.ToList(),
                OperatorRaw = x.Note,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });

    public static IQueryable<VerificationJpClientDTO> ToClientJpDTO(this IQueryable<Verification> query,
        bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.PartyTags)
            .Select(x => new VerificationJpClientDTO
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Status = x.Status,
                SiteId = (SiteTypes)x.Party.SiteId,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                ItemsRaw = x.VerificationItems.ToList(),
                OperatorRaw = x.Note,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });


    public static VerificationJpClientDTO ToClientJpDTO(this Verification verification)
    {
        return new VerificationJpClientDTO
        {
            Id = verification.Id,
            PartyId = verification.PartyId,
            SiteId = (SiteTypes)verification.Party.SiteId,
            Status = verification.Status,
            CreatedOn = verification.CreatedOn,
            UpdatedOn = verification.UpdatedOn,
            ItemsRaw = verification.VerificationItems.ToList(),
        };
    }
                    
    public static VerificationJpDTO ToJpDTO(this Verification verification)
    {
        return new VerificationJpDTO
        {
            Id = verification.Id,
            PartyId = verification.PartyId,
            SiteId = (SiteTypes)verification.Party.SiteId,
            Status = verification.Status,
            CreatedOn = verification.CreatedOn,
            UpdatedOn = verification.UpdatedOn,
            ItemsRaw = verification.VerificationItems.ToList(),
            // Info = verification.VerificationItems.ToInfoDTO(),
            // Started = verification.VerificationItems.ToStartedDTO(),
            // Financial = verification.VerificationItems.ToFinancialDTO(),
            // Quiz = verification.VerificationItems.ToQuizDTO(),
            // Agreement = verification.VerificationItems.ToAgreementDTO(),
            // Document = verification.VerificationItems.ToDocumentDTO(),
            // Operator =
            //     JsonConvert.DeserializeObject<VerificationNoteResponseModel>(verification.Note)?.Operator ?? null,
        };
    }

    public static VerificationInfoDTO? ToInfoDTO(this ICollection<VerificationItem> items)
    {
        var item = items.FirstOrDefault(x => x.Category == VerificationCategoryTypes.Info);
        try
        {
            return item == null ? null : JsonConvert.DeserializeObject<VerificationInfoDTO>(item.Content);
        }
        catch (Exception)
        {
            return new VerificationInfoDTO();
        }
    }

    public static VerificationFinancialDTO? ToFinancialDTO(this ICollection<VerificationItem> items)
    {
        var item = items.FirstOrDefault(x => x.Category == VerificationCategoryTypes.Financial);
        try
        {
            return item == null ? null : JsonConvert.DeserializeObject<VerificationFinancialDTO>(item.Content);
        }
        catch (Exception)
        {
            return new VerificationFinancialDTO();
        }
    }

    public static VerificationStartedDTO? ToStartedDTO(this ICollection<VerificationItem> items)
    {
        var item = items.FirstOrDefault(x => x.Category == VerificationCategoryTypes.Started);
        try
        {
            return item == null ? null : JsonConvert.DeserializeObject<VerificationStartedDTO>(item.Content);
        }
        catch (Exception)
        {
            return new VerificationStartedDTO();
        }
    }

    public static VerificationAnswerDTO[]? ToQuizDTO(this ICollection<VerificationItem> items)
    {
        var item = items.FirstOrDefault(x => x.Category == VerificationCategoryTypes.Quiz);
        try
        {
            return item == null ? null : JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(item.Content);
        }
        catch (Exception)
        {
            return Array.Empty<VerificationAnswerDTO>();
        }
    }

    public static VerificationAgreementDTO? ToAgreementDTO(this ICollection<VerificationItem> items)
    {
        var item = items.FirstOrDefault(x => x.Category == VerificationCategoryTypes.Agreement);
        try
        {
            return item == null ? null : JsonConvert.DeserializeObject<VerificationAgreementDTO>(item.Content);
        }
        catch (Exception)
        {
            return new VerificationAgreementDTO();
        }
    }

    public static IList<VerificationDocumentMedium>? ToDocumentDTO(this ICollection<VerificationItem> items)
    {
        var item = items.FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        try
        {
            return item == null
                ? null
                : JsonConvert.DeserializeObject<List<VerificationDocumentMedium>?>(item.Content);
        }
        catch (Exception)
        {
            return new List<VerificationDocumentMedium>();
        }
    }

    public static object? ToItemDTO(this VerificationItem item)
    {
        if (string.IsNullOrEmpty(item.Content)) return default;
        return item.Category switch
        {
            VerificationCategoryTypes.Agreement =>
                JsonConvert.DeserializeObject<VerificationAgreementDTO>(item.Content) ?? default,
            VerificationCategoryTypes.Financial =>
                JsonConvert.DeserializeObject<VerificationFinancialDTO>(item.Content) ?? default,
            VerificationCategoryTypes.Info => JsonConvert.DeserializeObject<VerificationInfoDTO>(
                                                  item.Content) ??
                                              default,
            VerificationCategoryTypes.Quiz =>
                JsonConvert.DeserializeObject<VerificationAnswerDTO[]>(item.Content) ?? default,
            VerificationCategoryTypes.Started =>
                JsonConvert.DeserializeObject<VerificationStartedDTO>(item.Content) ?? default,
            VerificationCategoryTypes.Document =>
                JsonConvert.DeserializeObject<List<VerificationDocumentMedium>>(item.Content) ?? default,
            _ => default,
        };
    }

    public static bool CanAwaitingApprove(this IEnumerable<VerificationDocumentMedium> list)
        => list.All(x => x.Status != (int)VerificationDocumentStatusTypes.AwaitingReview);
}