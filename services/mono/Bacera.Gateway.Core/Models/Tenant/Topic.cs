using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Topic
{
    public int Id { get; set; }

    public short Type { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime EffectiveTo { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Title { get; set; } = null!;

    public string AdditionalInformation { get; set; } = null!;

    public short Category { get; set; }

    public virtual ICollection<TopicContent> TopicContents { get; set; } = new List<TopicContent>();
}
