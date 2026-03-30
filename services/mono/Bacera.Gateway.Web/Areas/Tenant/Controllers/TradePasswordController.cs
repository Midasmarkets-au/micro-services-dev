using Bacera.Gateway.Auth;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Controllers;
using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AuthUser = Bacera.Gateway.Auth.User;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Route("api/" + VersionTypes.V1 + "/[Area]/tradepassword")]
[Tags("Tenant/Update passwords")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TradePasswordController : BaseController
{
    private readonly Tenancy _tenancy;
    private readonly TenantDbContext _tenantCtx;
    private readonly UserManager<AuthUser> _userManager;
    private readonly ITradePasswordEncryptionService _encryptionService;
    private readonly ITradePasswordValidationService _validationService;
    private readonly ITradingApiService _tradingApiService;
    private readonly ILogger<TradePasswordController> _logger;
    
    public TradePasswordController(
        Tenancy tenancy,
        TenantDbContext tenantCtx,
        UserManager<AuthUser> userManager,
        ITradePasswordEncryptionService encryptionService,
        ITradePasswordValidationService validationService,
        ITradingApiService tradingApiService,
        ILogger<TradePasswordController> logger)
    {
        _tenancy = tenancy;
        _tenantCtx = tenantCtx;
        _userManager = userManager;
        _encryptionService = encryptionService;
        _validationService = validationService;
        _tradingApiService = tradingApiService;
        _logger = logger;
    }
    
    /// <summary>
    /// API 1: 管理员重置CRM密码
    /// </summary>
    [HttpPut("account/{accountId:long}/crm-password/reset")]
    [Authorize(Roles = $"{UserRoleTypesString.TenantAdmin},{UserRoleTypesString.AccountAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetCrmPassword(long accountId, [FromBody] ResetCrmPasswordByAdminRequest request)
    {
        var account = await _tenantCtx.Accounts
            .Include(x => x.Party)
            .FirstOrDefaultAsync(x => x.Id == accountId);
            
        if (account == null)
            return NotFound(Result.Error("Account not found"));
            
        // 获取用户
        var user = await _userManager.Users
            .Where(x => x.PartyId == account.PartyId && x.TenantId == _tenancy.GetTenantId())
            .FirstOrDefaultAsync();
            
        if (user == null)
            return BadRequest(Result.Error("User not found"));
            
        // 验证密码复杂度（复用Identity的验证器）
        var passwordValidator = HttpContext.RequestServices.GetRequiredService<IPasswordValidator<AuthUser>>();
        var validationResult = await passwordValidator.ValidateAsync(_userManager, user, request.NewPassword);
        
        if (!validationResult.Succeeded)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.Description));
            return BadRequest(Result.Error($"Password complexity validation failed: {errors}"));
        }
        
        // 重置密码（使用token方式，不需要旧密码）
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to reset CRM password for user {UserId}: {Errors}", user.Id, errors);
            return BadRequest(Result.Error($"Failed to reset CRM password: {errors}"));
        }

        // 记录到历史表
        await RecordPasswordHistoryAsync(account.Id, account.AccountNumber, PasswordTypes.CRM,
            PasswordOperationTypeConstants.AdminChange, true, null, request.Reason);

        // 记录操作日志
        var operatorPartyId = User.Identity?.IsAuthenticated == true ? GetPartyId() : 1;
        account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "CrmPasswordResetByAdmin", "********", "********"));
        account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "CrmPasswordResetByAdmin:Reason", "", request.Reason ?? "No reason provided"));
         
        await _tenantCtx.SaveChangesAsync();

        _logger.LogInformation("CRM password reset by admin for account {AccountId}, user {UserId}, by party {PartyId}", 
            accountId, user.Id, operatorPartyId);
            
        // TODO: 如果需要发送邮件，在这里实现
        if (request.SendEmail && !string.IsNullOrEmpty(account.Party?.Email))
        {
            // backgroundJobClient.Enqueue<IGeneralJob>(x => 
            //     x.CrmPasswordResetByAdminAsync(_tenancy.GetTenantId(), emailModel, account.Party.Language));
        }
        
        return Ok(Result.Success("CRM password has been reset"));
    }
    
    /// <summary>
    /// API 2: 获取交易账户初始密码
    /// </summary>
    [HttpGet("account/{accountId:long}/trade-password/initial")]
    [Authorize(Roles = $"{UserRoleTypesString.TenantAdmin},{UserRoleTypesString.AccountAdmin}")]
    [ProducesResponseType(typeof(TradeAccountInitialPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInitialPassword(long accountId)
    {
        var passwordRecord = await _tenantCtx.TradeAccountPasswords
            .FirstOrDefaultAsync(x => x.AccountId == accountId);
            
        if (passwordRecord == null)
            return NotFound(Result.Error(ResultMessage.TradePassword.InitialPasswordNotFound));
            
        // 解密初始密码
        string? initialMainPassword = null;
        string? initialInvestorPassword = null;
        string? initialPhonePassword = null;
        
        try
        {
            if (!string.IsNullOrEmpty(passwordRecord.InitialMainPassword))
                initialMainPassword = _encryptionService.Decrypt(passwordRecord.InitialMainPassword);
                
            if (!string.IsNullOrEmpty(passwordRecord.InitialInvestorPassword))
                initialInvestorPassword = _encryptionService.Decrypt(passwordRecord.InitialInvestorPassword);
                
            if (!string.IsNullOrEmpty(passwordRecord.InitialPhonePassword))
                initialPhonePassword = _encryptionService.Decrypt(passwordRecord.InitialPhonePassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt initial passwords for account {AccountId}", accountId);
            return BadRequest(Result.Error("Failed to decrypt passwords"));
        }
        
        // 记录查询操作（审计需要）
        var account = await _tenantCtx.Accounts.FindAsync(accountId);
        if (account != null)
        {
            var operatorPartyId = User.Identity?.IsAuthenticated == true ? GetPartyId() : 1;
            account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "InitialPasswordViewed", "", ""));
            await _tenantCtx.SaveChangesAsync();
        }
        
        var response = new TradeAccountInitialPasswordResponse
        {
            AccountNumber = passwordRecord.AccountNumber,
            InitialMainPassword = initialMainPassword,
            InitialInvestorPassword = initialInvestorPassword,
            InitialPhonePassword = initialPhonePassword,
            MainPasswordChanged = passwordRecord.MainPasswordChangedCount > 0,
            InvestorPasswordChanged = passwordRecord.InvestorPasswordChangedCount > 0,
            MainPasswordChangedCount = passwordRecord.MainPasswordChangedCount,
            LastMainPasswordChangedOn = passwordRecord.LastMainPasswordChangedOn
        };
        
        var operatorPartyId2 = User.Identity?.IsAuthenticated == true ? GetPartyId() : 1;
        _logger.LogInformation("Initial password viewed for account {AccountId} by party {PartyId}", 
            accountId, operatorPartyId2);
            
        return Ok(response);
    }
    
    /// <summary>
    /// API 3: 管理员修改交易密码（Main Password）
    /// </summary>
    [HttpPut("account/{accountId:long}/trade-password/main")]
    [Authorize(Roles = $"{UserRoleTypesString.TenantAdmin},{UserRoleTypesString.AccountAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeMainPassword(long accountId, [FromBody] ChangeTradePasswordByAdminRequest request)
    {
        return await ChangeTradePasswordInternal(accountId, request, PasswordTypeConstants.Main);
    }
    
    /// <summary>
    /// API 4: 管理员修改观摩密码（Investor Password）
    /// </summary>
    [HttpPut("account/{accountId:long}/trade-password/investor")]
    [Authorize(Roles = $"{UserRoleTypesString.TenantAdmin},{UserRoleTypesString.AccountAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeInvestorPassword(long accountId, [FromBody] ChangeTradePasswordByAdminRequest request)
    {
        return await ChangeTradePasswordInternal(accountId, request, PasswordTypeConstants.Investor);
    }
    
    /// <summary>
    /// 修改交易密码的内部实现（Main和Investor共用）
    /// </summary>
    private async Task<IActionResult> ChangeTradePasswordInternal(
        long accountId, 
        ChangeTradePasswordByAdminRequest request, 
        string passwordType)
    {
        // 验证密码复杂度
        var (isValid, errorMessage) = _validationService.ValidatePasswordComplexity(request.NewPassword);
        if (!isValid)
            return BadRequest(Result.Error(errorMessage!));
            
        // 检查修改频率限制
        var (isAllowed, rateLimitError) = await _validationService.CheckPasswordChangeRateLimitAsync(
            _tenancy.GetTenantId(), accountId, passwordType);
        if (!isAllowed)
            return BadRequest(Result.Error(rateLimitError!));
            
        // 获取账户信息
        var account = await _tenantCtx.Accounts
            .Include(x => x.Party)
            .FirstOrDefaultAsync(x => x.Id == accountId);
            
        if (account == null)
            return NotFound(Result.Error("Account not found"));
            
        // 调用MT5 API修改密码
        var forInvestor = passwordType == PasswordTypeConstants.Investor;
        var success = await _tradingApiService.ChangePasswordAsync(
            account.ServiceId, 
            account.AccountNumber, 
            request.NewPassword, 
            passwordType);
            
        if (!success)
        {
            _logger.LogWarning("Failed to change {PasswordType} password via MT5 API for account {AccountId}", 
                passwordType, accountId);
                
            // 记录失败到历史表
            await RecordPasswordHistoryAsync(accountId, account.AccountNumber, passwordType, 
                PasswordOperationTypeConstants.AdminChange, false, "MT5 API call failed", request.Reason);
                
            return BadRequest(Result.Error(ResultMessage.TradePassword.Mt5PasswordChangeFailed));
        }
        
        // 更新或创建密码记录表
        var passwordRecord = await _tenantCtx.TradeAccountPasswords
            .FirstOrDefaultAsync(x => x.AccountId == accountId);
            
        if (passwordRecord == null)
        {
            // 如果记录不存在，创建新记录（用于老账户或开发环境）
            passwordRecord = new TradeAccountPassword
            {
                AccountId = accountId,
                AccountNumber = account.AccountNumber,
                ServiceId = account.ServiceId,
                MainPasswordChangedCount = 0,
                InvestorPasswordChangedCount = 0,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            _tenantCtx.TradeAccountPasswords.Add(passwordRecord);
            
            _logger.LogInformation("Created TradeAccountPassword record for existing account {AccountId}", accountId);
        }
        
        // 更新修改次数和时间
        if (passwordType == PasswordTypeConstants.Main)
        {
            passwordRecord.MainPasswordChangedCount++;
            passwordRecord.LastMainPasswordChangedOn = DateTime.UtcNow;
        }
        else if (passwordType == PasswordTypeConstants.Investor)
        {
            passwordRecord.InvestorPasswordChangedCount++;
            passwordRecord.LastInvestorPasswordChangedOn = DateTime.UtcNow;
        }
        passwordRecord.UpdatedOn = DateTime.UtcNow;
        
        // 记录操作日志到AccountLog
        var logAction = passwordType == PasswordTypeConstants.Main 
            ? "TradeAccountMainPasswordChanged" 
            : "TradeAccountInvestorPasswordChanged";
        account.AccountLogs.Add(Account.BuildLog(GetPartyId(), logAction, "********", "********"));
        account.AccountLogs.Add(Account.BuildLog(GetPartyId(), $"{logAction}:Reason", "", request.Reason ?? ""));
        
        // 记录到历史表
        await RecordPasswordHistoryAsync(accountId, account.AccountNumber, passwordType, 
            PasswordOperationTypeConstants.AdminChange, true, null, request.Reason);
            
        await _tenantCtx.SaveChangesAsync();
        
        _logger.LogInformation("Successfully changed {PasswordType} password for account {AccountId} by party {PartyId}", 
            passwordType, accountId, GetPartyId());
            
        // TODO: 如果需要发送邮件
        if (request.SendEmail && !string.IsNullOrEmpty(account.Party?.Email))
        {
            // backgroundJobClient.Enqueue<IGeneralJob>(x => 
            //     x.TradePasswordChangedByAdminAsync(_tenancy.GetTenantId(), emailModel, account.Party.Language));
        }
        
        return Ok(Result.Success($"{passwordType} password has been changed"));
    }
    
    /// <summary>
    /// API 5: 重置为初始密码
    /// </summary>
    [HttpPost("account/{accountId:long}/trade-password/reset-to-initial")]
    [Authorize(Roles = $"{UserRoleTypesString.TenantAdmin},{UserRoleTypesString.AccountAdmin}")]
    [ProducesResponseType(typeof(ResetToInitialPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetToInitialPassword(long accountId, [FromBody] ResetToInitialPasswordRequest request)
    {
        // 验证密码类型
        if (request.PasswordType != PasswordTypeConstants.Main && request.PasswordType != PasswordTypeConstants.Investor)
            return BadRequest(Result.Error(ResultMessage.TradePassword.InvalidPasswordType));
            
        // 检查修改频率限制
        var (isAllowed, rateLimitError) = await _validationService.CheckPasswordChangeRateLimitAsync(
            _tenancy.GetTenantId(), accountId, request.PasswordType);
        if (!isAllowed)
            return BadRequest(Result.Error(rateLimitError!));
            
        // 获取初始密码
        var passwordRecord = await _tenantCtx.TradeAccountPasswords
            .FirstOrDefaultAsync(x => x.AccountId == accountId);
            
        if (passwordRecord == null)
            return NotFound(Result.Error(ResultMessage.TradePassword.InitialPasswordNotFound));
            
        string? initialPassword = null;
        try
        {
            initialPassword = request.PasswordType == PasswordTypeConstants.Main
                ? _encryptionService.Decrypt(passwordRecord.InitialMainPassword!)
                : _encryptionService.Decrypt(passwordRecord.InitialInvestorPassword!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt initial password for account {AccountId}", accountId);
            return BadRequest(Result.Error("Failed to decrypt initial password"));
        }
        
        if (string.IsNullOrEmpty(initialPassword))
            return NotFound(Result.Error(ResultMessage.TradePassword.InitialPasswordNotFound));
            
        // 获取账户信息
        var account = await _tenantCtx.Accounts
            .Include(x => x.Party)
            .FirstOrDefaultAsync(x => x.Id == accountId);
            
        if (account == null)
            return NotFound(Result.Error("Account not found"));
            
        // 调用MT5 API重置为初始密码
        var success = await _tradingApiService.ChangePasswordAsync(
            account.ServiceId, 
            account.AccountNumber, 
            initialPassword, 
            request.PasswordType);
            
        if (!success)
        {
            _logger.LogWarning("Failed to reset password to initial for account {AccountId}, type {PasswordType}", 
                accountId, request.PasswordType);
                
            await RecordPasswordHistoryAsync(accountId, account.AccountNumber, request.PasswordType, 
                PasswordOperationTypeConstants.ResetToInitial, false, "MT5 API call failed", request.Reason);
                
            return BadRequest(Result.Error(ResultMessage.TradePassword.Mt5PasswordChangeFailed));
        }
        
        // 更新密码记录表
        if (request.PasswordType == PasswordTypeConstants.Main)
        {
            passwordRecord.MainPasswordChangedCount++;
            passwordRecord.LastMainPasswordChangedOn = DateTime.UtcNow;
        }
        else
        {
            passwordRecord.InvestorPasswordChangedCount++;
            passwordRecord.LastInvestorPasswordChangedOn = DateTime.UtcNow;
        }
        passwordRecord.UpdatedOn = DateTime.UtcNow;
        _tenantCtx.TradeAccountPasswords.Update(passwordRecord);
        
        // 记录操作日志
        account.AccountLogs.Add(Account.BuildLog(GetPartyId(), "PasswordResetToInitial", request.PasswordType, ""));
        account.AccountLogs.Add(Account.BuildLog(GetPartyId(), "PasswordResetToInitial:Reason", "", request.Reason ?? ""));
        
        // 记录到历史表
        await RecordPasswordHistoryAsync(accountId, account.AccountNumber, request.PasswordType, 
            PasswordOperationTypeConstants.ResetToInitial, true, null, request.Reason);
            
        await _tenantCtx.SaveChangesAsync();
        
        _logger.LogInformation("Successfully reset password to initial for account {AccountId}, type {PasswordType}, by party {PartyId}", 
            accountId, request.PasswordType, GetPartyId());
            
        // TODO: 如果需要发送邮件
        if (request.SendEmail && !string.IsNullOrEmpty(account.Party?.Email))
        {
            // backgroundJobClient.Enqueue<IGeneralJob>(x => 
            //     x.PasswordResetToInitialAsync(_tenancy.GetTenantId(), emailModel, account.Party.Language));
        }
        
        return Ok(new ResetToInitialPasswordResponse
        {
            Success = true,
            Message = "Password has been reset to initial password",
            InitialPassword = initialPassword
        });
    }
    
    /// <summary>
    /// API 6: 获取密码修改历史
    /// </summary>
    [HttpGet("account/{accountId:long}/trade-password/history")]
    [Authorize(Roles = $"{UserRoleTypesString.TenantAdmin},{UserRoleTypesString.AccountAdmin}")]
    [ProducesResponseType(typeof(PasswordHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPasswordHistory(
        long accountId,
        [FromQuery] string? passwordType = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        // 限制limit最大值
        limit = Math.Min(limit, 100);
        
        var query = _tenantCtx.TradeAccountPasswordHistories
            .Where(x => x.AccountId == accountId);
            
        // 按密码类型过滤
        if (!string.IsNullOrEmpty(passwordType))
            query = query.Where(x => x.PasswordType == passwordType);
            
        var total = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(x => x.ChangedOn)
            .Skip(offset)
            .Take(limit)
            .Select(x => new PasswordHistoryItem
            {
                Id = x.Id,
                PasswordType = x.PasswordType,
                OperationType = x.OperationType,
                OperatorPartyId = x.OperatorPartyId,
                OperatorRole = x.OperatorRole,
                OperatorName = x.OperatorParty != null ? x.OperatorParty.NativeName : null,
                Reason = x.Reason,
                Success = x.Success,
                ChangedOn = x.ChangedOn,
                IpAddress = x.IpAddress
            })
            .ToListAsync();
            
        // 记录查询操作
        var account = await _tenantCtx.Accounts.FindAsync(accountId);
        if (account != null)
        {
            var operatorPartyId = User.Identity?.IsAuthenticated == true ? GetPartyId() : 1;
            account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "PasswordHistoryViewed", "", ""));
            await _tenantCtx.SaveChangesAsync();
        }
        
        return Ok(new PasswordHistoryResponse
        {
            Total = total,
            Items = items
        });
    }

    /// <summary>
    /// 记录密码修改历史的辅助方法
    /// </summary>
    private async Task RecordPasswordHistoryAsync(
        long accountId,
        long accountNumber,
        string passwordType,
        string operationType,
        bool success,
        string? errorMessage,
        string? reason)
    {
        var ipAddress = GetRemoteIpAddress();
        var userAgent = Request.Headers["User-Agent"].ToString();

        // 获取当前用户角色
        var operatorPartyId = User.Identity?.IsAuthenticated == true ? GetPartyId() : 1;
        var operatorRole = User.IsInRole("TenantAdmin") ? "TenantAdmin" : "AccountAdmin";

        var history = new TradeAccountPasswordHistory
        {
            AccountId = accountId,
            AccountNumber = accountNumber,
            PasswordType = passwordType,
            OperationType = operationType,
            OperatorPartyId = operatorPartyId,
            OperatorRole = operatorRole,
            Reason = reason,
            Success = success,
            ErrorMessage = errorMessage,
            ChangedOn = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _tenantCtx.TradeAccountPasswordHistories.Add(history);
    }
}

