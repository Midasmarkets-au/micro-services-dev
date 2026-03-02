using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.ViewModels.Tenant;

public class ApplicationTenantViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public short Type { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long? ApprovedBy { get; set; }
    public string? OperatorName { get; set; }

    public DateTime? RejectedOn { get; set; }

    public long? RejectedBy { get; set; }

    public string? RejectedReason { get; set; }

    public long? ReferenceId { get; set; }
    public short? Status { get; set; }

    public long? CompletedBy { get; set; }

    public DateTime? CompletedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public object Supplement => Utils.JsonDeserializeDynamic(SupplementData ?? "{}");

    [JsonIgnore] public string? SupplementData { get; set; }
    public SiteTypes? SiteId { get; set; }


    // public dynamic User => new
    // {
    //     UserRaw.Id,
    //     UserRaw.PartyId,
    //     UserRaw.Email,
    //     UserRaw.Avatar,
    //     UserRaw.FirstName,
    //     UserRaw.LastName,
    //     UserRaw.NativeName,
    //     UserRaw.PartyTags,
    //     HasComment,
    // };

    public TenantUserBasicModel User { get; set; } = null!;

    public bool HasComment { get; set; }
}

public static class ApplicationTenantViewModelExt
{
    public static IQueryable<ApplicationTenantViewModel> ToResponseModels(this IQueryable<Application> queryable,
        bool hideEmail = false)
        => queryable
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Select(x => new ApplicationTenantViewModel
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Type = x.Type,
                ApprovedOn = x.ApprovedOn,
                ApprovedBy = x.ApprovedBy,
                RejectedOn = x.RejectedOn,
                RejectedBy = x.RejectedBy,
                RejectedReason = x.RejectedReason,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                CompletedBy = x.CompletedBy,
                CompletedOn = x.CompletedOn,
                Status = x.Status,
                SiteId = (SiteTypes?)x.Party.SiteId,
                ReferenceId = x.ReferenceId,
                SupplementData = x.Supplement,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}