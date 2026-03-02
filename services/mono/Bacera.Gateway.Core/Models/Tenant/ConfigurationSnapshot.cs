using System;

namespace Bacera.Gateway;

/// <summary>
/// Configuration snapshot entity for storing user's updated configuration data
/// </summary>
public partial class ConfigurationSnapshot
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string ConfigName { get; set; } = null!;
    public long RowId { get; set; }
    public string SnapshotId { get; set; } = null!;
    public DateTime SnapshotVersion { get; set; } // Version timestamp when snapshot was created
    public string SnapshotJson { get; set; } = null!; // The JSON data that was saved by the user
    public DateTime CreatedOn { get; set; }
    public DateTime LastActivity { get; set; }
}

