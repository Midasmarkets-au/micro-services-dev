using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

/// <summary>
/// Trade Account Password主表 - 存储MT5交易账户的初始密码（加密）
/// 管理员永远只能查看初始密码，客户修改后的当前密码不存储
/// </summary>
[Table("_TradeAccountPassword", Schema = "trd")]
public partial class TradeAccountPassword
{
    public long Id { get; set; }
    
    public long AccountId { get; set; }
    
    public long AccountNumber { get; set; }
    
    public int ServiceId { get; set; }
    
    /// <summary>
    /// 加密的初始交易密码（Main Password）- 永久保留，管理员可查看
    /// 使用 ASP.NET Core Data Protection API 加密（AES-256-GCM）
    /// </summary>
    public string? InitialMainPassword { get; set; }
    
    /// <summary>
    /// 加密的初始观摩密码（Investor Password）- 永久保留，管理员可查看
    /// 使用 ASP.NET Core Data Protection API 加密（AES-256-GCM）
    /// </summary>
    public string? InitialInvestorPassword { get; set; }
    
    /// <summary>
    /// 加密的初始手机密码（Phone Password）- 永久保留，管理员可查看
    /// 使用 ASP.NET Core Data Protection API 加密（AES-256-GCM）
    /// </summary>
    public string? InitialPhonePassword { get; set; }
    
    /// <summary>
    /// 交易密码修改次数
    /// </summary>
    public int MainPasswordChangedCount { get; set; }
    
    /// <summary>
    /// 观摩密码修改次数
    /// </summary>
    public int InvestorPasswordChangedCount { get; set; }
    
    /// <summary>
    /// 最后一次交易密码修改时间
    /// </summary>
    public DateTime? LastMainPasswordChangedOn { get; set; }
    
    /// <summary>
    /// 最后一次观摩密码修改时间
    /// </summary>
    public DateTime? LastInvestorPasswordChangedOn { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime UpdatedOn { get; set; }
    
    // Navigation properties
    public virtual Account Account { get; set; } = null!;
    
    public virtual TradeService TradeService { get; set; } = null!;
    
    public virtual ICollection<TradeAccountPasswordHistory> PasswordHistories { get; set; } = new List<TradeAccountPasswordHistory>();
}

