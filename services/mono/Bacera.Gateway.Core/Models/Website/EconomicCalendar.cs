using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class EconomicCalendar
{
    public ulong Id { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string Event { get; set; } = null!;

    /// <summary>
    /// 事件日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 事件国家
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 事件数值
    /// </summary>
    public decimal? Actual { get; set; }

    /// <summary>
    /// 事件数值
    /// </summary>
    public decimal? Previous { get; set; }

    /// <summary>
    /// 事件数值
    /// </summary>
    public decimal? Change { get; set; }

    /// <summary>
    /// 事件数值
    /// </summary>
    public decimal? ChangePercentage { get; set; }

    /// <summary>
    /// 事件预期或预算
    /// </summary>
    public decimal? Estimate { get; set; }

    /// <summary>
    /// 事件影响力,高,中,低,3个值
    /// </summary>
    public string? Impact { get; set; }

    /// <summary>
    /// api的整个数据整合的json
    /// </summary>
    public string? Data { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }
}
