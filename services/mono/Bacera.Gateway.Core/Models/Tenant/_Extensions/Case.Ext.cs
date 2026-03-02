using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Case
{
    public bool IsEmpty() => Id == 0;
    public bool CanReply() => ReplyId == null;
    public bool IsClaimed() => AdminPartyId != null;

    public Case ReplyTo(Case caseToReply, CaseStatusTypes status = CaseStatusTypes.Processing)
    {
        caseToReply.Status = (short)status;
        caseToReply.UpdatedOn = DateTime.UtcNow;
        ReplyId = caseToReply.Id;
        CaseId = caseToReply.CaseId;
        CategoryId = caseToReply.CategoryId;
        Status = (short)CaseStatusTypes.Processing;
        CreatedOn = DateTime.UtcNow;
        UpdatedOn = DateTime.UtcNow;
        return this;
    }

    public Case AssignTo(long adminPartyId)
    {
        AdminPartyId = adminPartyId;
        Status = (short)CaseStatusTypes.Processing;
        UpdatedOn = DateTime.UtcNow;
        return this;
    }

    public class CategoryResponseModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool HasChild { get; set; }
    }

    public class TenantBasicViewModel
    {
        public long Id { get; set; }
        public long? ReplyId { get; set; }
        public long PartyId { get; set; }
        public string CaseId { get; set; } = "";
        public long CategoryId { get; set; }
        public bool IsAdmin { get; set; }
        public long? AdminPartyId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string DataJson { get; set; } = "{}";

        public string Content { get; set; } = "";
        public short Status { get; set; }
        public string CategoryName { get; set; } = "";
        public string ClaimedAdminName { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string FilesJson { get; set; } = "[]";

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool ContainFiles { get; set; } = true;

        public List<Dictionary<string, string>> Languages { get; set; } = new();

        public List<string> Files
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(FilesJson) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        public bool ShouldSerializeFiles() => ContainFiles;
    }


    public class TenantResponseModel : TenantBasicViewModel
    {
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Case> InverseReply { get; set; } = new List<Case>();


        public object Data
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject(DataJson) ?? new { };
                }
                catch
                {
                    return new { };
                }
            }
        }

        public List<TenantBasicViewModel> Replies => InverseReply.AsQueryable().TenantBasicViewModel().ToList();
    }


    public class ClientBasicModel
    {
        public string CaseId { get; set; } = "";
        public bool IsAdmin { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string DataJson { get; set; } = "{}";

        public string Content { get; set; } = "";
        public short Status { get; set; }
        public string CategoryName { get; set; } = "";
        public string ClaimedAdminName { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string FilesJson { get; set; } = "[]";

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool ContainFiles { get; set; } = true;

        public List<string> Files
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(FilesJson) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        public bool ShouldSerializeFiles() => ContainFiles;
    }

    public class ClientResponseModel : ClientBasicModel
    {
        public object Data
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject(DataJson) ?? new { };
                }
                catch
                {
                    return new { };
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Case> InverseReply { get; set; } = new List<Case>();

        public List<ClientBasicModel> Replies => InverseReply.AsQueryable().ToClientBasicModel().ToList();
    }
}

public static class CaseExt
{
    public static IQueryable<Case.TenantBasicViewModel> ToTenantBasicModel(this IQueryable<Case> query)
        => query.Select(x => new Case.TenantBasicViewModel
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            PartyId = x.PartyId,
            CaseId = x.CaseId,
            CategoryId = x.CategoryId,
            IsAdmin = x.IsAdmin,
            AdminPartyId = x.AdminPartyId,
            DataJson = x.Data,
            Content = x.Content,
            Status = x.Status,
            CategoryName = x.Category.Name,
            ClaimedAdminName = x.AdminParty != null ? x.AdminParty!.NativeName : "",
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
        });

    public static IQueryable<Case.TenantResponseModel> ToTenantResponseModel(this IQueryable<Case> query,
        bool includeReplies = false)
        => query.Select(x => new Case.TenantResponseModel
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            PartyId = x.PartyId,
            CaseId = x.CaseId,
            CategoryId = x.CategoryId,
            IsAdmin = x.IsAdmin,
            AdminPartyId = x.AdminPartyId,
            DataJson = x.Data,
            Content = x.Content,
            Status = x.Status,
            CategoryName = x.Category.Name,
            ClaimedAdminName = x.AdminParty != null ? x.AdminParty!.NativeName : "",
            Languages = x.CaseLanguages
                .Select(y => new Dictionary<string, string> { { y.Language, y.Content } })
                .ToList(),
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            InverseReply = x.InverseReply,
            FilesJson = x.Files,
        });

    public static IQueryable<Case.TenantBasicViewModel> TenantBasicViewModel(this IQueryable<Case> query)
        => query.Select(x => new Case.TenantBasicViewModel
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            PartyId = x.PartyId,
            CaseId = x.CaseId,
            CategoryId = x.CategoryId,
            IsAdmin = x.IsAdmin,
            AdminPartyId = x.AdminPartyId,
            DataJson = x.Data,
            Content = x.Content,
            Status = x.Status,
            CategoryName = x.Category.Name,
            ClaimedAdminName = x.AdminParty != null ? x.AdminParty!.NativeName : "",
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            FilesJson = x.Files,
            Languages = x.CaseLanguages
                .Select(y => new Dictionary<string, string> { { y.Language, y.Content } })
                .ToList(),
        });

    public static IQueryable<Case.ClientBasicModel> ToClientBasicModel(this IQueryable<Case> query)
        => query.Select(x => new Case.ClientBasicModel
        {
            CaseId = x.CaseId,
            IsAdmin = x.IsAdmin,
            DataJson = x.Data,
            Content = x.Content,
            Status = x.Status,
            CategoryName = x.Category.Name,
            ClaimedAdminName = x.AdminParty != null ? x.AdminParty!.NativeName : "",
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
        });

    public static IQueryable<Case.ClientResponseModel> ToClientResponseModel(this IQueryable<Case> query)
        => query.Select(x => new Case.ClientResponseModel
        {
            CaseId = x.CaseId,
            IsAdmin = x.IsAdmin,
            DataJson = x.Data,
            Content = x.Content,
            Status = x.Status,
            CategoryName = x.Category.Name,
            ClaimedAdminName = x.AdminParty != null ? x.AdminParty!.NativeName : "",
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            InverseReply = x.InverseReply,
            FilesJson = x.Files,
        });

    // public static IEnumerable<Case.ClientBasicModel> ToClientBasicModel(this IEnumerable<Case> query)
    //     => query.Select(x => new Case.ClientBasicModel
    //     {
    //         CaseId = x.CaseId,
    //         IsAdmin = x.IsAdmin,
    //         DataJson = x.Data,
    //         Content = x.Content,
    //         Status = x.Status,
    //         CategoryName = x.Category.Name,
    //         ClaimedAdminName = x.AdminParty != null ? x.AdminParty!.NativeName : "",
    //         CreatedOn = x.CreatedOn,
    //         UpdatedOn = x.UpdatedOn,
    //         FilesJson = x.Files,
    //     });

    public static IQueryable<Case.CategoryResponseModel> ToCategoryResponseModel(this IQueryable<CaseCategory> query)
        => query.Select(x => new Case.CategoryResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            HasChild = x.ChildCaseCategories.Any(),
        });
}