using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

public abstract class BaseDbContext
{
    public static async Task<int> SaveChangesWithAuditAsync<T>(DbContext dbContext, long partyId, Func<Task<int>> func)
        where T : class, IEntityAudit, new()
    {
        var modifiedEntities = dbContext.ChangeTracker.Entries()
            .Where(p => p.State == EntityState.Modified).ToList();

        var entity = modifiedEntities.FirstOrDefault();
        IEntity? myTest = entity?.Entity as IEntity;

        if (myTest == null) return await func();

        var auditType = myTest.ToAuditType();
        if (auditType == AuditTypes.Unknown)
        {
            return await func();
        }

        foreach (var change in modifiedEntities)
        {
            _ = long.TryParse(change.OriginalValues["Id"]?.ToString(), out var id);
            var audit = new T
            {
                PartyId = partyId,
                RowId = id,
                Type = (int)auditType,
                Action = (int)AuditActionTypes.Update,
                CreatedOn = DateTime.UtcNow,
                Environment = System.Net.Dns.GetHostName(),
                Data = ""
            };
            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();

            foreach (var prop in change.OriginalValues.Properties.Select(p => p.Name))
            {
                var originalValue = change.OriginalValues[prop]?.ToString();
                var currentValue = change.CurrentValues[prop]?.ToString();
                if (originalValue == currentValue) continue;
                if (originalValue != null) oldValues.Add(prop, originalValue);
                if (currentValue != null) newValues.Add(prop, currentValue);
            }

            var data = Audit.EntityChanges.Create(oldValues, newValues);
            audit.Data = data.ToJson();
            dbContext.Set<T>().Add(audit);
        }

        return await func();
    }
}

public static class AuditExt
{
    //define which Type of entity should be audited
    public static AuditTypes ToAuditType(this IEntity entity)
    {
        var type = entity.GetType();
        if (type == typeof(Auth.User)) return AuditTypes.User;
        if (type == typeof(Account)) return AuditTypes.Account;
        if (type == typeof(TradeAccount)) return AuditTypes.TradeAccount;
        if (type == typeof(Wallet)) return AuditTypes.Wallet;
        if (type == typeof(PaymentService)) return AuditTypes.PaymentService;
        if (type == typeof(ExchangeRate)) return AuditTypes.ExchangeRate;

        if (type == typeof(RebateAgentRule)) return AuditTypes.RebateAgentRule;
        if (type == typeof(RebateClientRule)) return AuditTypes.RebateClientRule;
        if (type == typeof(RebateDirectRule)) return AuditTypes.RebateDirectRule;
        if (type == typeof(RebateDirectSchema)) return AuditTypes.RebateDirectSchema;
        if (type == typeof(RebateDirectSchemaItem)) return AuditTypes.RebateDirectSchemaItem;

        if (type == typeof(Configuration)) return AuditTypes.Configuration;

        return AuditTypes.Unknown;
    }
}