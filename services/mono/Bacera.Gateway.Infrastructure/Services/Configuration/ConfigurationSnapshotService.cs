using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

/// <summary>
/// Service for managing configuration snapshots for optimistic concurrency control
/// </summary>
public class ConfigurationSnapshotService
{
    private readonly TenantDbContext _dbContext;

    public ConfigurationSnapshotService(TenantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Create a snapshot of user's updated configuration data
    /// </summary>
    public async Task<string> CreateSnapshotAsync(long userId, string configName, long rowId, 
        DateTime snapshotVersion, string snapshotJson)
    {
        var snapshotId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        var snapshot = new ConfigurationSnapshot
        {
            PartyId = userId,
            ConfigName = configName,
            RowId = rowId,
            SnapshotId = snapshotId,
            SnapshotVersion = snapshotVersion,
            SnapshotJson = snapshotJson,
            CreatedOn = now,
            LastActivity = now
        };

        _dbContext.ConfigurationSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync();

        return snapshotId;
    }

    /// <summary>
    /// Get snapshot data by snapshot ID
    /// </summary>
    public async Task<ConfigurationSnapshot?> GetSnapshotAsync(long userId, string snapshotId)
    {
        var snapshot = await _dbContext.ConfigurationSnapshots
            .Where(s => s.SnapshotId == snapshotId && s.PartyId == userId)
            .FirstOrDefaultAsync();

        return snapshot;
    }

    /// <summary>
    /// Update last activity timestamp for a snapshot
    /// </summary>
    public async Task UpdateLastActivityAsync(long userId, string snapshotId)
    {
        var snapshot = await GetSnapshotAsync(userId, snapshotId);
        if (snapshot == null) return;

        snapshot.LastActivity = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Delete a snapshot
    /// </summary>
    public async Task DeleteSnapshotAsync(long userId, string snapshotId)
    {
        var snapshot = await GetSnapshotAsync(userId, snapshotId);
        if (snapshot == null) return;

        _dbContext.ConfigurationSnapshots.Remove(snapshot);
        await _dbContext.SaveChangesAsync();
    }
}

