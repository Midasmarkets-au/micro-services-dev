using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Context;

/// <summary>
/// Mirrors every Matter create/state-transition and every new Activity into the k8s-native
/// core.matter_k8s / core.activity_k8s tables, regardless of which of the many scattered
/// AccountingService/AcctService call sites (Deposit/Withdrawal/Transaction/Refund/Rebate)
/// performed the write. Runs on the ChangeTracker rather than patching individual call
/// sites, since state transitions go through two parallel mechanisms (TransitAsync and
/// AcctService's private TransitRaw) and creation is spread across 10+ locations.
///
/// The k8s mirror write is best-effort: the old core."_Matter"/core."_Activity" tables
/// remain the authoritative source of truth (same as every other k8s read-model in this
/// migration). A mirror-write failure is logged and swallowed, never surfaced to the
/// caller — a deposit/withdrawal/transfer completing correctly always matters more than
/// the k8s mirror being perfectly in sync.
/// </summary>
public sealed class MatterK8sSyncInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<MatterK8sSyncInterceptor> _logger;
    private readonly ConditionalWeakTable<DbContext, PendingChanges> _pending = new();

    public MatterK8sSyncInterceptor(ILogger<MatterK8sSyncInterceptor> logger)
    {
        _logger = logger;
    }

    private sealed class PendingChanges
    {
        public List<MatterSnapshot> TouchedMatters { get; } = [];
        public List<ActivitySnapshot> NewActivities { get; } = [];
    }

    private sealed record MatterSnapshot(long Id, long? Pid, int Type, int StateId, DateTime PostedOn, DateTime StatedOn);

    private sealed record ActivitySnapshot(long MatterId, long PartyId, DateTime PerformedOn, int OnStateId,
        int ActionId, int ToStateId, string Data);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is TenantDbContext ctx)
        {
            // Never let a bug in this bookkeeping step block or fail the real save — the
            // deposit/withdrawal/transfer/refund actually completing matters far more than
            // the k8s mirror. Everything here operates on already-in-memory data and isn't
            // expected to throw, but this is money-adjacent code, so be defensive anyway.
            try
            {
                // Always clear first: if a previous save on this pooled context captured
                // pending changes here but then failed (SavedChangesAsync never fires on
                // failure), a stale entry must not survive to be incorrectly replayed
                // against k8s on a later, unrelated successful save on the same reused
                // context instance.
                _pending.Remove(ctx);

                var pending = new PendingChanges();

                foreach (var entry in ctx.ChangeTracker.Entries<Matter>())
                {
                    if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
                    var m = entry.Entity;
                    pending.TouchedMatters.Add(new MatterSnapshot(m.Id, m.Pid, m.Type, m.StateId, m.PostedOn, m.StatedOn));
                }

                foreach (var entry in ctx.ChangeTracker.Entries<Activity>())
                {
                    if (entry.State != EntityState.Added) continue;
                    var a = entry.Entity;
                    pending.NewActivities.Add(new ActivitySnapshot(a.MatterId, a.PartyId, a.PerformedOn, a.OnStateId,
                        a.ActionId, a.ToStateId, a.Data ?? string.Empty));
                }

                if (pending.TouchedMatters.Count > 0 || pending.NewActivities.Count > 0)
                    _pending.Add(ctx, pending);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "MatterK8sSync: failed to snapshot pending Matter/Activity changes");
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is TenantDbContext ctx && _pending.TryGetValue(ctx, out var pending))
        {
            _pending.Remove(ctx);
            try
            {
                await SyncAsync(ctx, pending, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "MatterK8sSync: failed to mirror Matter/Activity changes to k8s tables");
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task SyncAsync(TenantDbContext ctx, PendingChanges pending, CancellationToken ct)
    {
        // Activities reference a matter_id FK, so every matter they touch must be upserted
        // first even if that matter itself wasn't in the Added/Modified set for this save
        // (shouldn't normally happen since TransitAsync/TransitRaw always touch both, but
        // this keeps the FK constraint safe regardless).
        var matterIds = new HashSet<long>(pending.TouchedMatters.Select(m => m.Id));
        var missingMatterIds = pending.NewActivities
            .Select(a => a.MatterId)
            .Where(id => !matterIds.Contains(id))
            .Distinct()
            .ToList();

        var matters = new List<MatterSnapshot>(pending.TouchedMatters);
        if (missingMatterIds.Count > 0)
        {
            var fetched = await ctx.Matters
                .Where(m => missingMatterIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Pid, m.Type, m.StateId, m.PostedOn, m.StatedOn })
                .ToListAsync(ct);
            matters.AddRange(fetched.Select(m => new MatterSnapshot(m.Id, m.Pid, m.Type, m.StateId, m.PostedOn, m.StatedOn)));
        }

        foreach (var m in matters)
        {
            await ctx.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO core.matter_k8s (id, pid, type, state_id, posted_on, stated_on)
                VALUES ({m.Id}, {m.Pid}, {m.Type}, {m.StateId}, {m.PostedOn}, {m.StatedOn})
                ON CONFLICT (id) DO UPDATE SET state_id = EXCLUDED.state_id, stated_on = EXCLUDED.stated_on
                """, ct);
        }

        foreach (var a in pending.NewActivities)
        {
            await ctx.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO core.activity_k8s (matter_id, party_id, performed_on, on_state_id, action_id, to_state_id, data)
                VALUES ({a.MatterId}, {a.PartyId}, {a.PerformedOn}, {a.OnStateId}, {a.ActionId}, {a.ToStateId}, {a.Data})
                """, ct);
        }
    }
}
