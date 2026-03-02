using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class News
{
    public ulong Id { get; set; }

    /// <summary>
    /// 新闻类型
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 新闻标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime? PublishedDate { get; set; }

    /// <summary>
    /// 整个api的数据
    /// </summary>
    public string? Data { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// parent id
    /// </summary>
    public uint? Pid { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    public string? Intro { get; set; }
}
