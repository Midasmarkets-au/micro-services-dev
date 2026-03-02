using OpenIddict.Validation.AspNetCore;
using System;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Tenant;
using HashidsNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Referral History")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ReferralController(
    TenantDbContext tenantCtx,
    ReferralCodeService referralCodeSvc,
    CentralDbContext centralDbContext,
    ITenantGetter getter,
    ConfigService configSvc) : TenantBaseController
{
    private readonly long _tenantId = getter.GetTenantId();
    /// <summary>
    /// Referral Code pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<ReferralCode.WithDefaultPaymentMethodResponseModel>, ReferralCode.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] ReferralCode.Criteria? criteria = null)
    {
        criteria ??= new ReferralCode.Criteria();
        
        // Load referral codes with navigation properties (Party, Account, Referrals)
        var referralCodes = await tenantCtx.ReferralCodes
            .PagedFilterBy(criteria)
            .ToListAsync();
        
        // Get referral code IDs
        var referralCodeIds = referralCodes.Select(r => r.Id).ToList();
        
        // Load configurations for all referral codes in one query (both Deposit and Withdrawal)
        var depositConfigurations = await tenantCtx.Configurations
            .Where(c => c.Category == ConfigCategoryTypes.Public 
                        && referralCodeIds.Contains(c.RowId) 
                        && c.Key == ConfigKeys.DefaultAutoCreatePaymentMethod)
            .GroupBy(c => c.RowId)
            .Select(g => new
            {
                ReferralCodeId = g.Key,
                ConfigValue = g.OrderByDescending(c => c.Id).Select(c => c.Value).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ReferralCodeId, x => x.ConfigValue);
        
        var withdrawalConfigurations = await tenantCtx.Configurations
            .Where(c => c.Category == ConfigCategoryTypes.Public 
                        && referralCodeIds.Contains(c.RowId) 
                        && c.Key == ConfigKeys.DefaultAutoCreateWithdrawalPaymentMethod)
            .GroupBy(c => c.RowId)
            .Select(g => new
            {
                ReferralCodeId = g.Key,
                ConfigValue = g.OrderByDescending(c => c.Id).Select(c => c.Value).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ReferralCodeId, x => x.ConfigValue);
        
        // Map to response model with all fields including navigation properties and configuration
        var items = referralCodes.Select(r =>
        {
            var model = new ReferralCode.WithDefaultPaymentMethodResponseModel
            {
                // All ReferralCode properties
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                PartyId = r.PartyId,
                AccountId = r.AccountId,
                ServiceType = (ReferralServiceTypes)r.ServiceType,
                Status = (ReferralCodeStatusTypes)r.Status,
                IsDefault = r.IsDefault,
                Summary = r.Summary,
                CreatedOn = r.CreatedOn,
                UpdatedOn = r.UpdatedOn,
                IsAutoCreatePaymentMethod = r.IsAutoCreatePaymentMethod,
                
                // Navigation properties
                Party = r.Party,
                Account = r.Account,
                Referrals = r.Referrals.ToList(),
                
                // Computed property
                DisplaySummary = r.DisplaySummary
            };
            
            // Parse deposit configuration value if exists
            if (depositConfigurations.TryGetValue(r.Id, out var depositConfigValue) && !string.IsNullOrEmpty(depositConfigValue))
            {
                try
                {
                    model.DefaultAutoCreatePaymentMethod = JsonConvert.DeserializeObject<List<long>>(depositConfigValue) ?? new List<long>();
                }
                catch
                {
                    model.DefaultAutoCreatePaymentMethod = new List<long>();
                }
            }
            else
            {
                model.DefaultAutoCreatePaymentMethod = new List<long>();
            }
            
            // Parse withdrawal configuration value if exists
            if (withdrawalConfigurations.TryGetValue(r.Id, out var withdrawalConfigValue) && !string.IsNullOrEmpty(withdrawalConfigValue))
            {
                try
                {
                    model.DefaultAutoCreateWithdrawalPaymentMethod = JsonConvert.DeserializeObject<List<long>>(withdrawalConfigValue) ?? new List<long>();
                }
                catch
                {
                    model.DefaultAutoCreateWithdrawalPaymentMethod = new List<long>();
                }
            }
            else
            {
                model.DefaultAutoCreateWithdrawalPaymentMethod = new List<long>();
            }
            
            return model;
        }).ToList();
        
        return Ok(Result<List<ReferralCode.WithDefaultPaymentMethodResponseModel>, ReferralCode.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Referral History pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(Result<List<Referral>, Referral.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> History([FromQuery] Referral.Criteria? criteria = null)
    {
        criteria ??= new Referral.Criteria();
        var items = await tenantCtx.Referrals
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result<List<Referral>, Referral.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Create Referral Code
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] ReferralCode.CreateSpec spec)
    {
        var operatorPartyId = GetPartyId();
        var targetAccount = await tenantCtx.Accounts
            .Where(x => x.Id == spec.AccountId && x.PartyId == spec.PartyId)
            .Select(x => new { x.Id, x.PartyId, x.Role, })
            .FirstOrDefaultAsync();

        if (targetAccount == null) return BadRequest("Account not found");
        
        var item = new ReferralCode
        {
            Name = spec.Name ?? "Tenant Generated",
            Code = Guid.NewGuid().ToString(),
            PartyId = targetAccount.PartyId,
            AccountId = targetAccount.Id,
            ServiceType = (int)spec.ServiceType,
            Summary = spec.Summary ?? "{}",
        };

        tenantCtx.ReferralCodes.Add(item);
        await tenantCtx.SaveChangesWithAuditAsync(operatorPartyId);

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);

        var codePrefix = (AccountRoleTypes)targetAccount.Role switch
        {
            AccountRoleTypes.Sales => "RSA",
            AccountRoleTypes.Agent => "RAA",
            _ => throw new ArgumentOutOfRangeException(nameof(targetAccount.Role), targetAccount.Role, null)
        };
        item.Code = codePrefix + code + code.Length + Tenancy.GetTenancyInReferCode(_tenantId);
        
        tenantCtx.ReferralCodes.Update(item);
        await tenantCtx.SaveChangesWithAuditAsync(operatorPartyId);

        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(_tenantId));
        await centralDbContext.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>
    /// Get Account infos by Referral Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet("referred-by-account/{code}")]
    [ProducesResponseType(typeof(ReferralCode.TenantWithAccountInfoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReferredByAccount(string code)
    {
        var item = await tenantCtx.ReferralCodes
            .Where(x => x.Code == code.Trim())
            .ToTenantResponseModel()
            .FirstOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Set Agent Default Client Referral Code
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPut("code/{code}/account/{accountId:long}/default-client")]
    public async Task<IActionResult> SetDefaultClient(long accountId, string code)
    {
        var result = await referralCodeSvc.SetAgentDefaultClientReferralCodeAsync(accountId, code);
        return result ? Ok() : BadRequest();
    }


    /// <summary>
    /// Get Account infos by Referral Code
    /// </summary>
    /// <returns></returns>
    [HttpGet("fix-fail-referral-code-by-it")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FixReferralCodeByIt()
    {
        var referCodes = (await tenantCtx.ReferralCodes
                .Where(x => x.Name == "Default Client Code")
                .ToListAsync())
            .Where(x =>
            {
                dynamic? summary = JsonConvert.DeserializeObject<object>(x.Summary);
                if (summary == null)
                {
                    return false;
                }

                var allowAccountTypes = summary.allowAccountTypes;
                return allowAccountTypes != null && allowAccountTypes?.Count == 0;
            })
            .ToList();

        var codeIds = referCodes.Select(x => x.AccountId)
            .ToList();

        var rebateRules = await tenantCtx.RebateAgentRules
            .Where(x => codeIds.Contains(x.AgentAccountId))
            .ToListAsync();

        // foreach (var rule in rebateRules)
        // {
        //     var schema = JsonConvert.DeserializeObject<List<dynamic>>(rule.Schema);
        //     if (schema == null) continue;
        //     
        //     List<object> allowAccountType = new List<object>();
        //     
        //     foreach (var item in schema)
        //     {
        //         var accountType = new
        //         {
        //             accountType = item.accountType,
        //             pips = item.pips,
        //             commission = item.commission
        //         };
        //         
        //         allowAccountType.Add(accountType);
        //     }
        //     
        //     var referralCode = referCodes.Find(x => x.AccountId == rule.AgentAccountId);
        //     dynamic summary = JsonConvert.DeserializeObject<object>(referralCode.Summary);
        //     summary.allowAccountTypes = JArray.FromObject(allowAccountType);
        //     
        //     referralCode.Summary = JsonConvert.SerializeObject(summary);
        //     referralCode.UpdatedOn = DateTime.UtcNow;
        //     _tenantDbContext.ReferralCodes.Update(referralCode);
        //     await _tenantDbContext.SaveChangesAsync();
        // }

        return Ok(referCodes.Select(x => x.Id));
    }

    /// <summary>
    /// Update DefaultAutoCreatePaymentMethod configuration for a referral code
    /// </summary>
    /// <param name="referralCodeId">Referral Code ID</param>
    /// <param name="spec">Configuration update spec with payment method IDs and type (Deposit or Withdrawal)</param>
    /// <returns></returns>
    [HttpPut("{referralCodeId:long}/default-payment-method")]
    [ProducesResponseType(typeof(List<long>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDefaultPaymentMethod(long referralCodeId, [FromBody] ReferralCode.UpdateDefaultPaymentMethodSpec spec)
    {
        // Verify referral code exists
        var referralCode = await tenantCtx.ReferralCodes
            .Where(x => x.Id == referralCodeId)
            .FirstOrDefaultAsync();
        
        if (referralCode == null)
            return NotFound(new { error = "Referral code not found", referralCodeId });
        
        // Determine config key based on type
        var configKey = spec.Type?.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase) == true
            ? ConfigKeys.DefaultAutoCreateWithdrawalPaymentMethod
            : ConfigKeys.DefaultAutoCreatePaymentMethod;
        
        // Update configuration
        await configSvc.SetAsync<List<long>>(
            ConfigCategoryTypes.Public,
            referralCodeId,
            configKey,
            spec.PaymentMethodIds,
            partyId: GetPartyId());
        
        // Return updated value
        var updatedValue = await configSvc.GetAsync<List<long>>(
            ConfigCategoryTypes.Public,
            referralCodeId,
            configKey);
        
        return Ok(updatedValue ?? new List<long>());
    }
}