using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Document
{
    public ulong DocumentId { get; set; }

    public long Timestamp { get; set; }

    public ulong RelatedClient { get; set; }

    public DateTime ApprovedDate { get; set; }

    public ulong ApprovedBy { get; set; }

    public DateTime DateIssue { get; set; }

    public DateTime DateExpiration { get; set; }

    public uint DocumentType { get; set; }

    public string DocumentName { get; set; } = null!;

    public string DocumentComment { get; set; } = null!;

    public uint DocumentStatus { get; set; }
}
