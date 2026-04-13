namespace Bacera.Gateway.Services.Report.Models;

/// <summary>
/// Daily Equity Report 模型（无对应数据库表，仅用于报告生成）
/// </summary>
public class DailyEquity
{
    /// <summary>
    /// Daily Equity Report 查询条件
    /// 注意：DailyEquity 没有对应的数据库表，因此不继承 Criteria<T>
    /// </summary>
    public class Criteria
    {
        /// <summary>
        /// 开始日期（GMT+x 时区，用户输入）
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// 结束日期（GMT+x 时区，用户输入）
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// 是否使用 MT5 ClosingTime 逻辑
        /// - true: 根据 MT5 交易关闭时间 + Master状态时间双重过滤（Job入口默认）
        /// - false/null: 仅根据 Master状态时间过滤（API入口默认）
        /// </summary>
        public bool? UseClosingTime { get; set; }

        /// <summary>
        /// TradeRebate.ClosedOn 查询的开始时间（原始用户输入时间，未调整时区）
        /// 仅当 UseClosingTime = true 时使用
        /// </summary>
        public DateTime? ClosedOnFrom { get; set; }

        /// <summary>
        /// TradeRebate.ClosedOn 查询的结束时间（原始用户输入时间，未调整时区）
        /// 仅当 UseClosingTime = true 时使用
        /// </summary>
        public DateTime? ClosedOnTo { get; set; }

        /// <summary>
        /// Whether to aggregate results by Office (one row per Office, all currencies summed)
        /// </summary>
        public bool? AggregateByOffice { get; set; }

        /// <summary>
        /// 配对报告的文件名（用于 UI 显示下载链接）
        /// </summary>
        public string? PairFileName { get; set; }

        /// <summary>
        /// 配对报告的名称
        /// </summary>
        public string? PairReportName { get; set; }

        /// <summary>
        /// 页码（默认 1）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页大小（默认 500000，用于 CSV 导出）
        /// </summary>
        public int Size { get; set; } = 500000;

        /// <summary>
        /// 排序字段（默认 "Id"）
        /// </summary>
        public string SortField { get; set; } = "Id";

        /// <summary>
        /// 排序方向（默认 false = 降序）
        /// </summary>
        public bool SortFlag { get; set; } = false;

        /// <summary>
        /// 总记录数（用于分页）
        /// </summary>
        public int Total { get; set; }
    }
}
