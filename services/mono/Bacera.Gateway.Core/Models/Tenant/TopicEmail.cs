using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TopicEmail
{
    public long Id { get; set; }
    public int TopicId { get; set; }
    public string Email { get; set; } = null!;
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}