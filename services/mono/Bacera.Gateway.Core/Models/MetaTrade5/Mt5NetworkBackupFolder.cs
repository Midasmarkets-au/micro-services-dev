using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5NetworkBackupFolder
{
    public ulong FolderId { get; set; }

    public ulong Login { get; set; }

    public string Folder { get; set; } = null!;

    public string Masks { get; set; } = null!;

    public string Filter { get; set; } = null!;

    public uint Flags { get; set; }
}
