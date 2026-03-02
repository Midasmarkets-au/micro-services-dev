using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.ViewModels.Tenant;

public class VerificationViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }

    public VerificationTypes Type { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Note { get; set; } = null!;

    public object? NoteObject
    {
        get
        {
            try
            {
                return JsonConvert.DeserializeObject(Note);
            }
            catch
            {
                return null;
            }
        }
    }

    public VerificationStatusTypes Status { get; set; }

    // public string Content { get; set; }
    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();
}

public static class VerificationViewModelExtension
{
    public static IQueryable<VerificationViewModel> ToTenantViewModel(this IQueryable<Verification> query,
        bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.PartyTags)
            .Select(x => new VerificationViewModel
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Type = (VerificationTypes)x.Type,
                Status = (VerificationStatusTypes)x.Status,
                Note = x.Note,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}