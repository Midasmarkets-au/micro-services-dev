using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Post
{
    public uint Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Subtitle { get; set; }

    public string Slug { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;

    public string? Type { get; set; }

    public string Tag { get; set; } = null!;

    public string? Image { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishTime { get; set; }

    public string? Category { get; set; }

    public string? Languages { get; set; }
}
