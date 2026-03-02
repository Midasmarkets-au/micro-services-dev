using System.Text.Json.Serialization;

namespace Bacera.Gateway.Services;

/// <summary>
/// Sales Statistics Models
/// </summary>
public class SalesStatistics
{
    /// <summary>
    /// Request criteria for sales statistics
    /// </summary>
    public class Criteria
    {
        /// <summary>
        /// Sales UID (required)
        /// </summary>
        public long SalesUid { get; set; }

        /// <summary>
        /// Start date (default: 30 days ago)
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// End date (default: today)
        /// </summary>
        public DateTime? To { get; set; }

        public Criteria()
        {
            From = DateTime.UtcNow.AddDays(-30).Date;
            To = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1); // End of today
        }
    }

    /// <summary>
    /// Complete response model containing all statistics
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// Hierarchical tree data for table display
        /// </summary>
        public List<HierarchyNode> HierarchyData { get; set; } = new();

        /// <summary>
        /// Time series data for charts (daily breakdown)
        /// </summary>
        public List<TimeSeriesData> TimeSeriesData { get; set; } = new();

        /// <summary>
        /// Summary statistics for cards
        /// </summary>
        public SummaryStats SummaryStats { get; set; } = new();

        /// <summary>
        /// Product distribution for pie chart
        /// </summary>
        public List<ProductDistribution> ProductDistribution { get; set; } = new();
    }

    /// <summary>
    /// Hierarchical node representing a user (Sales/Agent/Client) in the tree
    /// </summary>
    public class HierarchyNode
    {
        /// <summary>
        /// User UID
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User type: 100/300/400
        /// </summary>
        public long Type { get; set; }

        /// <summary>
        /// User type: "Sales"/"Agent"/"Client"
        /// </summary>
        public string TypeName { get; set; } = null!;

        /// <summary>
        /// Group code (e.g., "MBJ001")
        /// </summary>
        public string? GroupCode { get; set; }

        /// <summary>
        /// Trade count (recursive - includes all descendants)
        /// </summary>
        public int Trades { get; set; }

        /// <summary>
        /// Net deposit in 0.01 cent (recursive - includes all descendants)
        /// </summary>
        public long NetDeposit { get; set; }

        /// <summary>
        /// Total deposit in 0.01 cent (own only)
        /// </summary>
        public long Deposit { get; set; }

        /// <summary>
        /// Total withdrawal in 0.01 cent (own only)
        /// </summary>
        public long Withdrawal { get; set; }

        /// <summary>
        /// Rebate amount in 0.01 cent (recursive - includes all descendants, excludes SalesRebate)
        /// </summary>
        public long Rebate { get; set; }

        /// <summary>
        /// Trading lots (recursive - includes all descendants)
        /// </summary>
        public double Lots { get; set; }

        /// <summary>
        /// Trading products/symbols (recursive - includes all descendants)
        /// </summary>
        public List<string> Products { get; set; } = new();

        /// <summary>
        /// Parent node UID (null for root)
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// Child nodes
        /// </summary>
        public List<HierarchyNode> Children { get; set; } = new();
    }

    /// <summary>
    /// Time series data for trend charts
    /// </summary>
    public class TimeSeriesData
    {
        /// <summary>
        /// Date in "MM-DD" format
        /// </summary>
        public string Date { get; set; } = null!;

        /// <summary>
        /// Daily trade count
        /// </summary>
        public int Trades { get; set; }

        /// <summary>
        /// Daily deposit in 0.01 cent
        /// </summary>
        public long Deposit { get; set; }

        /// <summary>
        /// Daily withdrawal in 0.01 cent
        /// </summary>
        public long Withdrawal { get; set; }

        /// <summary>
        /// Daily net deposit in 0.01 cent (deposit - withdrawal)
        /// </summary>
        public long NetDeposit { get; set; }

        /// <summary>
        /// Daily rebate in 0.01 cent
        /// </summary>
        public long Rebate { get; set; }

        /// <summary>
        /// Daily trading lots
        /// </summary>
        public double Lots { get; set; }
    }

    /// <summary>
    /// Summary statistics for display cards
    /// </summary>
    public class SummaryStats
    {
        /// <summary>
        /// Total trade count
        /// </summary>
        public int TotalTrades { get; set; }

        /// <summary>
        /// Total net deposit in 0.01 cent
        /// </summary>
        public long TotalNetDeposit { get; set; }

        /// <summary>
        /// Total rebate in 0.01 cent
        /// </summary>
        public long TotalRebate { get; set; }

        /// <summary>
        /// Total deposit in 0.01 cent
        /// </summary>
        public long TotalDeposit { get; set; }

        /// <summary>
        /// Total withdrawal in 0.01 cent
        /// </summary>
        public long TotalWithdrawal { get; set; }

        /// <summary>
        /// Total trading lots
        /// </summary>
        public double TotalLots { get; set; }
    }

    /// <summary>
    /// Product distribution for pie chart
    /// </summary>
    public class ProductDistribution
    {
        /// <summary>
        /// Symbol/Product code
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Trade count for this symbol
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Percentage (1 decimal place)
        /// </summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Internal model for aggregating account data
    /// </summary>
    internal class AccountAggregateData
    {
        public long Uid { get; set; }
        public long PartyId { get; set; }
        public string Name { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string? GroupCode { get; set; }
        public string ReferPath { get; set; } = null!;
        public int Level { get; set; }
        
        // Own data (not recursive)
        public long OwnDeposit { get; set; }
        public long OwnWithdrawal { get; set; }
        
        // Recursive data (includes descendants)
        public int TotalTrades { get; set; }
        public long TotalRebate { get; set; }
        public double TotalLots { get; set; }
        public HashSet<string> AllProducts { get; set; } = new();
    }
}

