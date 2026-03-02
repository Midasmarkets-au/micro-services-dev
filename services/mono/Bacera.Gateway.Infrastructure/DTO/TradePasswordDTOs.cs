namespace Bacera.Gateway.DTO;

/// <summary>
/// 管理员重置CRM密码请求
/// API 1: PUT /api/v1/Tenant/account/{uid}/crm-password/reset
/// </summary>
public class ResetCrmPasswordByAdminRequest
{
    /// <summary>
    /// 新密码（需符合复杂度要求）
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// 重置原因
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 是否发送邮件通知客户
    /// </summary>
    public bool SendEmail { get; set; }
}

/// <summary>
/// 管理员修改交易密码请求（Main/Investor）
/// API 3: PUT /api/v1/Tenant/account/{accountId}/trade-password/main
/// API 4: PUT /api/v1/Tenant/account/{accountId}/trade-password/investor
/// </summary>
public class ChangeTradePasswordByAdminRequest
{
    /// <summary>
    /// 新密码（需符合MT5复杂度要求：8字符+大小写+特殊符号）
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// 修改原因
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 是否发送邮件通知客户
    /// </summary>
    public bool SendEmail { get; set; }
}

/// <summary>
/// 重置为初始密码请求
/// API 5: POST /api/v1/Tenant/account/{accountId}/trade-password/reset-to-initial
/// </summary>
public class ResetToInitialPasswordRequest
{
    /// <summary>
    /// 密码类型："main" 或 "investor"
    /// </summary>
    public string PasswordType { get; set; } = string.Empty;
    
    /// <summary>
    /// 重置原因
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 是否发送邮件通知客户
    /// </summary>
    public bool SendEmail { get; set; }
}

/// <summary>
/// 获取初始密码响应
/// API 2: GET /api/v1/Tenant/account/{accountId}/trade-password/initial
/// </summary>
public class TradeAccountInitialPasswordResponse
{
    public long AccountNumber { get; set; }
    
    /// <summary>
    /// 初始交易密码（明文）
    /// </summary>
    public string? InitialMainPassword { get; set; }
    
    /// <summary>
    /// 初始观摩密码（明文）
    /// </summary>
    public string? InitialInvestorPassword { get; set; }
    
    /// <summary>
    /// 初始手机密码（明文）
    /// </summary>
    public string? InitialPhonePassword { get; set; }
    
    /// <summary>
    /// 交易密码是否已被修改
    /// </summary>
    public bool MainPasswordChanged { get; set; }
    
    /// <summary>
    /// 观摩密码是否已被修改
    /// </summary>
    public bool InvestorPasswordChanged { get; set; }
    
    /// <summary>
    /// 交易密码修改次数
    /// </summary>
    public int MainPasswordChangedCount { get; set; }
    
    /// <summary>
    /// 最后一次交易密码修改时间
    /// </summary>
    public DateTime? LastMainPasswordChangedOn { get; set; }
}

/// <summary>
/// 重置为初始密码响应
/// API 5: POST /api/v1/Tenant/account/{accountId}/trade-password/reset-to-initial
/// </summary>
public class ResetToInitialPasswordResponse
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 初始密码（明文，方便管理员告知客户）
    /// </summary>
    public string InitialPassword { get; set; } = string.Empty;
}

/// <summary>
/// 密码修改历史查询响应
/// API 6: GET /api/v1/Tenant/account/{accountId}/trade-password/history
/// </summary>
public class PasswordHistoryResponse
{
    public int Total { get; set; }
    
    public List<PasswordHistoryItem> Items { get; set; } = new();
}

/// <summary>
/// 密码修改历史项
/// </summary>
public class PasswordHistoryItem
{
    public long Id { get; set; }
    
    /// <summary>
    /// 密码类型："main", "investor", "phone"
    /// </summary>
    public string PasswordType { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作类型："admin_change", "client_change", "reset_to_initial"
    /// </summary>
    public string OperationType { get; set; } = string.Empty;
    
    public long? OperatorPartyId { get; set; }
    
    public string? OperatorRole { get; set; }
    
    public string? OperatorName { get; set; }
    
    public string? Reason { get; set; }
    
    public bool Success { get; set; }
    
    public DateTime ChangedOn { get; set; }
    
    public string? IpAddress { get; set; }
}

/// <summary>
/// 密码类型常量
/// </summary>
public static class PasswordTypeConstants
{
    public const string Main = "main";
    public const string Investor = "investor";
    public const string Phone = "phone";
    public const string CRM = "crm";
}

/// <summary>
/// 操作类型常量
/// </summary>
public static class PasswordOperationTypeConstants
{
    public const string AdminChange = "admin_change";
    public const string ClientChange = "client_change";
    public const string ResetToInitial = "reset_to_initial";
}

