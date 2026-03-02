using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TopicContent
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Language { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Subtitle { get; set; } = null!;

    public virtual Topic Topic { get; set; } = null!;
}
