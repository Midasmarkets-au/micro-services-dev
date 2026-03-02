using System.Text.RegularExpressions;
using Bacera.Gateway.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services;

/// <summary>
/// 交易密码验证服务
/// 包含密码复杂度验证和修改频率限制
/// </summary>
public interface ITradePasswordValidationService
{
    /// <summary>
    /// 验证密码复杂度
    /// 要求：至少8字符，包含大小写字母和特殊符号
    /// </summary>
    (bool isValid, string? errorMessage) ValidatePasswordComplexity(string password);
    
    /// <summary>
    /// 检查密码修改频率是否超限
    /// 限制：同一账户的同一密码类型，24小时内最多修改3次
    /// </summary>
    Task<(bool isAllowed, string? errorMessage)> CheckPasswordChangeRateLimitAsync(
        long tenantId, long accountId, string passwordType);
}

public class TradePasswordValidationService : ITradePasswordValidationService
{
    // 密码复杂度正则：至少8字符，包含大小写字母和特殊符号
    private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$";
    
    private readonly MyDbContextPool _myDbContextPool;
    private readonly ILogger<TradePasswordValidationService> _logger;
    
    public TradePasswordValidationService(
        MyDbContextPool myDbContextPool,
        ILogger<TradePasswordValidationService> logger)
    {
        _myDbContextPool = myDbContextPool;
        _logger = logger;
    }
    
    /// <summary>
    /// 验证密码复杂度
    /// </summary>
    /// <param name="password">待验证的密码</param>
    /// <returns>(是否有效, 错误消息)</returns>
    public (bool isValid, string? errorMessage) ValidatePasswordComplexity(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return (false, "Password cannot be empty");
        }
        
        if (password.Length < 8)
        {
            return (false, "Password must be at least 8 characters long");
        }
        
        if (password.Length > 16)
        {
            return (false, "Password cannot exceed 16 characters");
        }
        
        if (!Regex.IsMatch(password, PasswordRegex))
        {
            return (false, "Password must contain uppercase letters, lowercase letters, and special characters (e.g., @#$%^&*!)");
        }
        
        return (true, null);
    }
    
    /// <summary>
    /// 检查密码修改频率限制
    /// 规则：同一账户的同一密码类型，24小时内最多修改3次
    /// </summary>
    /// <param name="tenantId">租户ID</param>
    /// <param name="accountId">账户ID</param>
    /// <param name="passwordType">密码类型：main/investor/phone</param>
    /// <returns>(是否允许修改, 错误消息)</returns>
    public async Task<(bool isAllowed, string? errorMessage)> CheckPasswordChangeRateLimitAsync(
        long tenantId, long accountId, string passwordType)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            // 查询最近24小时的修改次数
            var last24Hours = DateTime.UtcNow.AddHours(-24);
            var recentChanges = await ctx.TradeAccountPasswordHistories
                .Where(x => x.AccountId == accountId)
                .Where(x => x.PasswordType == passwordType)
                .Where(x => x.ChangedOn >= last24Hours)
                .Where(x => x.Success == true)
                .CountAsync();
            
            if (recentChanges >= 3)
            {
                _logger.LogWarning(
                    "Password change rate limit exceeded for account {AccountId}, type {PasswordType}: {Count} changes in last 24 hours",
                    accountId, passwordType, recentChanges);
                
                return (false, "Password has been changed too frequently. Please try again after 24 hours");
            }
            
            return (true, null);
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }
}

