using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

/// <summary>
/// Default Rebate Level Setting Models (客户基础返佣表)
/// </summary>
public class DefaultRebateLevelSetting
{
    public class RebateCategoryInfo
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public double Value { get; set; }
    }

    public class AllowPipOrCommissionItem
    {
        public string Name { get; set; } = null!;
        public int Value { get; set; }
        public Dictionary<string, double> Items { get; set; } = new();
    }

    public class RebateOption
    {
        public string OptionName { get; set; } = null!;
        public Dictionary<string, double> Category { get; set; } = new();
        public List<int> AllowPipOptions { get; set; } = new();
        public Dictionary<string, AllowPipOrCommissionItem> AllowPipSetting { get; set; } = new();
        public List<int> AllowCommissionOptions { get; set; } = new();
        public Dictionary<string, AllowPipOrCommissionItem> AllowCommissionSetting { get; set; } = new();
    }

    public class AccountTypeRebate
    {
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; } = null!;
        public List<RebateOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Response model with enriched data
    /// </summary>
    public class ResponseModel
    {
        public List<AccountTypeRebate> AccountTypes { get; set; } = new();
    }

    /// <summary>
    /// Detailed response for a specific account type and option
    /// </summary>
    public class DetailResponseModel
    {
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; } = null!;
        public string OptionName { get; set; } = null!;
        public List<RebateCategoryInfo> Categories { get; set; } = new();
        public List<int> AllowPipOptions { get; set; } = new();
        public Dictionary<string, AllowPipOrCommissionItem> AllowPipSetting { get; set; } = new();
        public List<int> AllowCommissionOptions { get; set; } = new();
        public Dictionary<string, AllowPipOrCommissionItem> AllowCommissionSetting { get; set; } = new();
    }

    /// <summary>
    /// Update model - uses same structure as ResponseModel
    /// </summary>
    public class UpdateModel
    {
        public List<AccountTypeRebate> AccountTypes { get; set; } = new();
    }
}

