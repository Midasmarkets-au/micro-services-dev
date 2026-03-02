using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5NetworkBackupServer
{
    public ulong Login { get; set; }

    public ulong PairServer { get; set; }

    public ulong BackupFlags { get; set; }

    public string BackupPath { get; set; } = null!;

    public uint BackupPeriod { get; set; }

    public uint BackupTtl { get; set; }

    public uint BackupTimeFull { get; set; }

    public DateTime BackupLastStartup { get; set; }

    public DateTime BackupLastFull { get; set; }

    public DateTime BackupLastArchive { get; set; }

    public DateTime BackupLastSync { get; set; }

    public DateTime BackupLastSyncSql { get; set; }

    public uint SqlMode { get; set; }

    public string SqlServer { get; set; } = null!;

    public string SqlFolder { get; set; } = null!;

    public ulong SqlFlags { get; set; }

    public uint SqlPeriod { get; set; }
}
