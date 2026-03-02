using Bacera.Gateway.Context;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bacera.Gateway;

public sealed partial class AccountingService
{
    private readonly AuthDbContext _authDbContext;
    private readonly TenantDbContext _tenantDbContext;
    private readonly ILogger<AccountingService> _logger;
    private readonly MyDbContextPool _myDbContextPool;
    private readonly long _tenantId;

    public AccountingService(
        TenantDbContext tenantDbContext
        , AuthDbContext authDbContext
        , ITenantGetter tenancyResolver
        , MyDbContextPool myDbContextPool, ILogger<AccountingService>? logger = null
    )
    {
        _authDbContext = authDbContext;
        _myDbContextPool = myDbContextPool;
        _tenantId = tenancyResolver.GetTenantId();
        _tenantDbContext = tenantDbContext;
        _logger = logger ?? new NullLogger<AccountingService>();
    }

    public async Task<bool> CanTransitToStateAsync(IHasMatter matter, ActionTypes action, StateTypes toState,
        long operatorPartyId)
    {
        await Task.Delay(0);
        return true;
        // var result = await _tenantDbContext.Transitions.AnyAsync(x =>
        //     x.ActionId == (int)action
        //     && x.OnStateId == matter.IdNavigation.StateId
        //     && x.ToStateId == (int)toState);
        // return result;
    }

    public async Task<Tuple<bool, Matter>> TransitAsync(IHasMatter matter, ActionTypes action, long operatorPartyId)
    {
        var query = from m in _tenantDbContext.Matters
            join t in _tenantDbContext.Transitions on
                new { OnStateId = m.StateId, ActionId = (int)action } equals
                new { t.OnStateId, t.ActionId }
            where m.Id == matter.Id
            select new
            {
                Matter = m,
                t.ToStateId,
            };

        var transition = await query.FirstOrDefaultAsync();
        if (transition == null)
        {
            return Tuple.Create(false, new Matter());
        }

        var now = DateTime.UtcNow;
        var activity = new Activity
        {
            PartyId = operatorPartyId,
            MatterId = matter.Id,
            PerformedOn = now,
            OnStateId = transition.Matter.StateId,
            ActionId = (int)action,
            ToStateId = transition.ToStateId,
        };
        transition.Matter.StateId = transition.ToStateId;
        transition.Matter.StatedOn = now;
        transition.Matter.Activities.Add(activity);

        _tenantDbContext.Matters.Update(transition.Matter);
        await _tenantDbContext.SaveChangesAsync();
        return Tuple.Create(true, transition.Matter);
    }

    // private async Task FulfillUserForViewModel<T>(List<T> iHasUserItems)
    //     where T : class, ICanFulfillUserBasicInfo
    // {
    //     var partyIds = iHasUserItems
    //         .Select(x => x.PartyId)
    //         .Where(x => x > 0)
    //         .Distinct()
    //         .ToList();
    //
    //     var users = await _authDbContext.Users
    //         .Where(x => partyIds.Contains(x.PartyId) && x.TenantId == _tenantId)
    //         .ToTenantBasicViewModel()
    //         .ToListAsync();
    //
    //     foreach (var item in iHasUserItems)
    //     {
    //         var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
    //         if (user != null) item.User = user;
    //     }
    // }
    //
    // private async Task FulfillUserWithHasComment<T>(List<T> iHasUserItems)
    //     where T : class, ICanFulfillUserBasicInfo
    // {
    //     var partyIds = iHasUserItems
    //         .Select(x => x.PartyId)
    //         .Where(x => x > 0)
    //         .Distinct()
    //         .ToList();
    //
    //     var users = await _authDbContext.Users
    //         .Where(x => partyIds.Contains(x.PartyId) && x.TenantId == _tenantId)
    //         .ToTenantBasicViewModel()
    //         .ToListAsync();
    //
    //     var userIds = users.Select(x => x.Id).ToList();
    //
    //     var comments = await _tenantDbContext.Comments
    //         .Where(x => (CommentTypes)x.Type == CommentTypes.User && userIds.Contains(x.RowId))
    //         .GroupBy(x => new { x.RowId, x.Type })
    //         .Select(g => new { g.Key.RowId, g.Key.Type })
    //         .ToListAsync();
    //
    //     foreach (var item in iHasUserItems)
    //     {
    //         var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
    //         if (user == null) continue;
    //         user.HasComment = comments.Any(x => x.RowId == user.Id && (CommentTypes)x.Type == CommentTypes.User);
    //         item.User = user;
    //     }
    // }
}