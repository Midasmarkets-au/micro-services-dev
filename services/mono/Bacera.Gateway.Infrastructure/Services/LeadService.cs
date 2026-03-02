using System.Diagnostics;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

using M = Lead;

public class LeadService(TenantDbContext tenantCtx, ConfigService cfgSvc) : ILeadService
{
    public async Task<Result<List<LeadBasicViewModel>, M.Criteria>> QueryViewModelAsync(M.Criteria criteria)
    {
        var items = await tenantCtx.Leads
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        return Result<List<LeadBasicViewModel>, M.Criteria>.Of(items, criteria);
    }

    public async Task<LeadDetailViewModel> GetAsync(long id)
    {
        var item = await tenantCtx.Leads
            .Where(x => x.Id == id)
            .ToRepDetailResponseModel()
            .SingleOrDefaultAsync();
        if (item == null) return new LeadDetailViewModel();

        var comments = await tenantCtx.Comments
            .Where(x => x.RowId == id)
            .Where(x => x.Type == (int)CommentTypes.Lead)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
        item.UpdateLogs = comments;
        return item;
    }

    public async Task<LeadDetailViewModel> LookUpUnderAssignedAccountUid(long accountUid, long id)
    {
        var item = await tenantCtx.Leads
            .Where(x => x.Accounts.Any(a => a.Uid == accountUid))
            .Where(x => x.Id == id)
            .ToRepDetailResponseModel()
            .SingleOrDefaultAsync();
        if (item == null) return new LeadDetailViewModel();

        var comments = await tenantCtx.Comments
            .Where(x => x.RowId == id)
            .Where(x => x.Type == (int)CommentTypes.Lead)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
        item.UpdateLogs = comments;
        return item;
    }

    public async Task<bool> TryReferenceTo(long partyId, string email = "", string phoneNumber = "", string name = "",
        LeadStatusTypes status = LeadStatusTypes.UserRegistered, string? sourceComment = null, string? utm = null)
    {
        if (email == "" && phoneNumber == "") return false;
        var lead = await tenantCtx.Leads
            .Where(x => x.IsArchived == (int)LeadIsArchivedTypes.Unarchived)
            .Where(x => x.Email == email)
            .FirstOrDefaultAsync();

        if (lead == null)
        {
            var spec = new M.CreateSpec
            {
                PartyId = partyId,
                Name = name,
                Email = email,
                PhoneNumber = phoneNumber,
                Status = status,
                SourceType = LeadSourceTypes.UserRegister,
            };
            if (!string.IsNullOrEmpty(utm))
            {
                spec.Supplement = new Dictionary<string, LeadItem> { { "utm", new LeadItem { Data = utm } } };
            }

            lead = await CreateAsync(spec);
        }
        else
        {
            lead.PartyId = partyId;
            lead.Status = (int)status;
            lead.UpdatedOn = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(utm))
            {
                var supplement = lead.GetSupplement();
                lead.Supplement = Utils.JsonSerializeObject(supplement);
            }
            
            tenantCtx.Leads.Update(lead);
        }

        await tenantCtx.SaveChangesAsync();
        await AddCommentWithPartyId(lead.Id, partyId, LeadStatusDescriptions.Dictionary[status]);
        if (!string.IsNullOrEmpty(sourceComment))
        {
            await AddCommentWithPartyId(lead.Id, partyId, sourceComment);
        }

        return true;
    }

    public async Task<M> CreateAsync(M.CreateSpec spec)
    {
        var item = M.Build(
            spec.PartyId
            , name: spec.Name
            , email: spec.Email
            , phoneNumber: spec.PhoneNumber
            , status: spec.Status
            , sourceType: spec.SourceType
            , supplement: spec.Supplement
        );
        tenantCtx.Leads.Add(item);
        await tenantCtx.SaveChangesAsync();
        await AddCommentWithPartyId(item.Id, 1, LeadStatusDescriptions.Dictionary[spec.Status]);

        if (!string.IsNullOrEmpty(spec.SourceComment))
        {
            await AddCommentWithPartyId(item.Id, 1, spec.SourceComment);
        }

        var autoAssignInfo = await cfgSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo);
        if (autoAssignInfo?.Enabled == true)
        {
            await AssignOwnerAccount(item.Id, autoAssignInfo.AutoAssignAccountUid, 1);
        }

        return item;
    }

    public async Task<bool> AppendEvent<T, TC>(long partyId, ILeadable<T, TC> entity, LeadStatusTypes status)
        where T : struct, IConvertible where TC : struct, IConvertible
    {
        var entityModelName = entity.GetType().Name;

        var lead = await GetLeadByStatus(partyId, status);
        var comment = LeadStatusDescriptions.Dictionary[status];
        if (lead == null) return false;

        await AddCommentWithPartyId(lead.Id, partyId, comment);
        var supplement = lead.GetSupplement();
        
        supplement[entityModelName] = entity.ToLeadItem();

        lead.Status = (int)status;
        lead.UpdatedOn = DateTime.UtcNow;
        lead.Supplement = Utils.JsonSerializeObject(supplement);

        tenantCtx.Leads.Update(lead);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatus(long id, LeadStatusTypes status)
    {
        var item = await tenantCtx.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return false;
        item.Status = (int)status;
        item.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Leads.Update(item);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignOwnerAccount(long leadId, long assignedAccountUid, long operatorPartyId)
    {
        var lead = await tenantCtx.Leads
            .Where(x => x.Id == leadId)
            .Where(x => x.Accounts.All(a => a.Uid != assignedAccountUid))
            .SingleOrDefaultAsync();
        if (lead == null)
            return false;

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == assignedAccountUid)
            .FirstOrDefaultAsync();
        if (account == null)
            return false;

        lead.Accounts.Add(account!);
        lead.UpdatedOn = DateTime.UtcNow;

        var comment = Comment.Build(
            lead.Id
            , operatorPartyId
            , CommentTypes.Lead
            , $"Assigned To: {account!.Name} ({account.Code}) ({Enum.GetName((AccountRoleTypes)account.Role) ?? string.Empty})"
        );
        tenantCtx.Comments.Add(comment);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnAssignOwnerAccount(long leadId, long assignedAccountUid, long operatorPartyId)
    {
        var lead = await tenantCtx.Leads
            .Where(x => x.Id == leadId)
            .Include(x => x.Accounts.Where(a => a.Uid == assignedAccountUid))
            .SingleOrDefaultAsync();

        if (lead == null || lead.Accounts.All(a => a.Uid != assignedAccountUid))
            return false;

        var account = lead.Accounts.First();

        lead.Accounts.Remove(account);
        lead.UpdatedOn = DateTime.UtcNow;

        var comment = Comment.Build(
            lead.Id
            , operatorPartyId
            , CommentTypes.Lead
            , $"Removed From: {account!.Name} ({account.Code}) ({Enum.GetName((AccountRoleTypes)account.Role) ?? string.Empty})"
        );
        tenantCtx.Comments.Add(comment);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Archive(long id, LeadIsArchivedTypes leadIsArchivedType)
    {
        var item = await tenantCtx.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return false;
        item.IsArchived = (int)leadIsArchivedType;
        item.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Leads.Update(item);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddComment(long id, string content, long operatorPartyId)
    {
        if (!await tenantCtx.Leads.AnyAsync(x => x.Id == id))
            return false;

        await AddCommentWithPartyId(id, operatorPartyId, content);
        return true;
    }

    private async Task<Lead?> GetLeadByStatus(long partyId, LeadStatusTypes status)
    {
        var userLeadsQuery = tenantCtx.Leads
            .Where(x => x.IsArchived == (int)LeadIsArchivedTypes.Unarchived)
            .Where(x => x.PartyId == partyId);

        var lead = status switch
        {
            LeadStatusTypes.UserRegistered => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.UserNotRegistered),

            LeadStatusTypes.UserVerifying => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.UserRegistered),

            LeadStatusTypes.UserVerificationUnderReview => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.UserVerifying),

            LeadStatusTypes.UserVerificationApproved => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.UserVerificationUnderReview),

            LeadStatusTypes.AccountApplicationCreated => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.UserVerificationApproved),

            LeadStatusTypes.AccountApplicationRejected => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.AccountApplicationCreated),

            LeadStatusTypes.AccountApplicationApproved => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.AccountApplicationCreated),

            LeadStatusTypes.AgentAccountCreated => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.AccountApplicationCreated),

            LeadStatusTypes.TradeAccountCreated => await userLeadsQuery.FirstOrDefaultAsync(x =>
                x.Status == (int)LeadStatusTypes.AccountApplicationApproved),

            LeadStatusTypes.UserNotRegistered or LeadStatusTypes.UserVerificationRejected or _ => null,
        };

        return lead;
    }

    private async Task AddCommentWithPartyId(long id, long operatorPartyId, string content)
    {
        var comment = Comment.Build(
            id
            , operatorPartyId
            , CommentTypes.Lead
            , content
        );
        tenantCtx.Comments.Add(comment);
        await tenantCtx.SaveChangesAsync();
    }
}