using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Application;

public class ApplicationService(
    TenantDbContext dbContext,
    ITenantGetter tenancyResolver,
    UserService userSvc,
    AccountManageService accountManageService,
    ILogger<ApplicationService>? logger = null)
{
    private readonly ILogger<ApplicationService> _logger = logger ?? new NullLogger<ApplicationService>();

    private readonly long _tenantId = tenancyResolver.GetTenantId();

    public async Task<M> CreateApplication(long partyId, ApplicationTypes type, IApplicationSupplement supplement, long referenceId = 0,
        ApplicationStatusTypes status = ApplicationStatusTypes.AwaitingApproval)
    {
        var currencyId = ((Bacera.Gateway.ApplicationSupplement)supplement).CurrencyId;
        // Validate USC Account: Only one USC account per user email
        if (currencyId == (int)CurrencyTypes.USC)
        {
            var party = await userSvc.GetPartyAsync(partyId);
            if (string.IsNullOrWhiteSpace(party.Email))
            {
                // Return empty Application with error message in RejectedReason
                var errorModel = new M { Id = 0, RejectedReason = "User email is required for USC account creation" };
                return errorModel;
            }

            // Check if user already has a USC account (by email), one user can only have one USC account
            var hasUscAccount = await accountManageService.HasUscAccountByEmailAsync(party.Email);
            if (hasUscAccount)
            {
                // Return empty Application with error message in RejectedReason
                var errorModel = new M { Id = 0, RejectedReason = $"You already have a USC account. Only one USC account per user is allowed." };
                return errorModel;
            }
        }

        var model = M.Build(partyId, type, referenceId, status);
        model.Supplement = Utils.JsonSerializeObject(supplement);
        await dbContext.Applications.AddAsync(model);
        await dbContext.SaveChangesAsync();
        return model;
    }

    public async Task<M> GetAsync(long id)
    {
        var item = await dbContext.Applications
            .SingleOrDefaultAsync(x => x.Id == id) ?? new M();
        return item;
    }

    public async Task<M> UpdateSupplementAsync(long id, ApplicationSupplement request)
    {
        var item = await GetAsync(id);
        if (item.IsEmpty())
            return item;

        var supplement = JsonConvert
            .DeserializeObject<ApplicationSupplement>(item.Supplement!)!
            .Merge(request);

        item.Supplement = Utils.JsonSerializeObject(supplement);
        dbContext.Applications.Update(item);
        await dbContext.SaveChangesAsync();
        return item;
    }

    public async Task<Result<List<ApplicationTenantViewModel>, M.Criteria>> QueryAsync(M.Criteria criteria,
        bool hideEmail = false)
    {
        var items = await dbContext.Applications
            .PagedFilterBy(criteria)
            .ToResponseModels(hideEmail)
            .ToListAsync();
        return Result<List<ApplicationTenantViewModel>, M.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<ApplicationTenantViewModel>, M.Criteria>> TenantQueryAsync(M.Criteria criteria,
        bool hideEmail = false)
    {
        var items = await dbContext.Applications
            .PagedFilterBy(criteria)
            .ToResponseModels(hideEmail)
            .ToListAsync();

        var comments = await dbContext.Comments
            .Where(x => items.Select(a => a.Id).Contains(x.RowId) && x.Type == (int)CommentTypes.Application)
            .Select(x => x.RowId)
            .Distinct()
            .ToListAsync();

        var operatorPartyIds = items.Select(x => new List<long?>
            {
                x.ApprovedBy,
                x.RejectedBy,
                x.CompletedBy,
            })
            .SelectMany(x => x)
            .Where(x => x != null)
            .Select(x => x!)
            .Distinct()
            .ToList();

        var names = await dbContext.Parties
            .Where(x => operatorPartyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.NativeName);

        // Get unique emails from items to check for USC accounts
        var emails = items.Select(x => x.User.EmailRaw.ToLower()).Distinct().ToList();
        var uscAccountEmails = new HashSet<string>();
        
        foreach (var email in emails)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var hasUscAccount = await accountManageService.HasUscAccountByEmailAsync(email);
                if (hasUscAccount)
                {
                    uscAccountEmails.Add(email.ToLower());
                }
            }
        }

        foreach (var item in items)
        {
            var approveName = names.GetValueOrDefault(item.ApprovedBy ?? 0, "None");
            var rejectName = names.GetValueOrDefault(item.RejectedBy ?? 0, "None");
            var completeName = names.GetValueOrDefault(item.CompletedBy ?? 0, "None");
            item.OperatorName = $"{approveName},{rejectName},{completeName}";
            item.HasComment = comments.Any(c => c == item.Id);
            /* v1/tenant/application 需放开唯一USC Account验证, 这里直接返回false */
            item.User.HasUSCAccount = false; //uscAccountEmails.Contains(item.User.EmailRaw.ToLower());
        }
        

        return Result<List<ApplicationTenantViewModel>, M.Criteria>.Of(items, criteria);
    }

    public async Task<bool> ApproveAsync(long id, long operatorPartyId, ApplicationTypes type, long referenceId = 0)
    {
        var model = await dbContext.Applications.FindAsync(id);
        if (model == null)
        {
            _logger.LogWarning("Application {Id} not found", id);
            return false;
        }

        if (!model.Approve(operatorPartyId, type, referenceId)) return false;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Application {Id} approved by {PartyId}", id, operatorPartyId);
        return true;
    }

    public async Task<bool> CompleteAsync(long id, long operatorPartyId)
    {
        var model = await dbContext.Applications.FindAsync(id);
        if (model == null)
        {
            _logger.LogWarning("Application {Id} not found", id);
            return false;
        }

        if (!model.Complete(operatorPartyId)) return false;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Application {Id} completed by {PartyId}", id, operatorPartyId);
        return true;
    }

    public async Task<bool> RejectAsync(long id, long operatorPartyId, string rejectReason = "")
    {
        var model = await dbContext.Applications.FindAsync(id);
        if (model == null)
        {
            _logger.LogWarning("Application {Id} not found", id);
            return false;
        }

        if (!model.Reject(operatorPartyId, rejectReason)) return false;
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Application {Id} rejected by {PartyId}", id, operatorPartyId);

        return true;
    }

    public async Task<bool> ReverseRejectAsync(long id, long operatorPartyId, string rejectReason = "")
    {
        var model = await dbContext.Applications.FindAsync(id);
        if (model == null)
        {
            _logger.LogWarning("Application {Id} not found", id);
            return false;
        }

        if (!model.ReverseReject(operatorPartyId, rejectReason)) return false;
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Application {Id} rejected by {PartyId}", id, operatorPartyId);

        return true;
    }
}