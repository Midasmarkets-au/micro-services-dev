using System;

namespace Bacera.Gateway;

/// <summary>
/// 交易账户密码修改历史记录表
/// 记录所有密码修改操作（管理员修改、客户自己修改、重置为初始密码）
/// 不存储密码明文，仅记录操作信息和结果
/// </summary>
public partial class TradeAccountPasswordHistory
{
    public long Id { get; set; }
    
    public long AccountId { get; set; }
    
    public long AccountNumber { get; set; }
    
    /// <summary>
    /// 密码类型: "main"（交易密码）, "investor"（观摩密码）, "phone"（手机密码）
    /// </summary>
    public string PasswordType { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作类型:
    /// - "admin_change": 管理员修改密码
    /// - "client_change": 客户自己修改密码
    /// - "reset_to_initial": 重置为初始密码
    /// </summary>
    public string OperationType { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作人的PartyId（管理员或客户的PartyId）
    /// </summary>
    public long? OperatorPartyId { get; set; }
    
    /// <summary>
    /// 操作人角色: "TenantAdmin", "AccountAdmin", "Client" 等
    /// </summary>
    public string? OperatorRole { get; set; }
    
    /// <summary>
    /// 修改原因（可选，通常由管理员填写）
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 操作是否成功（MT5 API调用结果）
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 错误消息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime ChangedOn { get; set; }
    
    /// <summary>
    /// 操作者IP地址
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// 操作者User Agent
    /// </summary>
    public string? UserAgent { get; set; }
    
    // Navigation properties
    public virtual Account Account { get; set; } = null!;
    
    public virtual Party? OperatorParty { get; set; }
}

