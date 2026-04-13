
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[Area("Tenant")]
[Tags("Tenant/Payment Method")]
[Route("api/" + VersionTypes.V2 + "/[Area]/payment-method")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class PaymentMethodController(
    PaymentMethodService paymentMethodSvc,
    AccountManageService accManageSvc,
    TenantDbContext tenantCtx,
    WalletService walletSvc)
    : TenantBaseControllerV2
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("cache/reload")]
    public async Task<IActionResult> ReloadCache()
    {
        await paymentMethodSvc.ReloadCacheAsync();
        return Ok();
    }

    /// <summary>
    /// Get deposit payment methods with sorting support
    /// </summary>
    /// <param name="criteria">Filter criteria</param>
    /// <param name="includeDeleted">Include soft-deleted payment methods (default: false)</param>
    /// <returns></returns>
    [HttpGet("deposit")]
    public async Task<IActionResult> GetDeposit([FromQuery] PaymentMethod.Criteria? criteria, [FromQuery] bool includeDeleted = false)
    {
        criteria ??= new PaymentMethod.Criteria();
        var result = await GetTenantPaymentMethods(PaymentMethodTypes.Deposit, criteria, includeDeleted);

        // 根据Sort字段排序，默认0数据, 放>0的后面
        result = result
            .OrderByDescending(x => x.Sort > 0)  // Sort > 0 items first
            .ThenBy(x => x.Sort > 0 ? x.Sort : int.MaxValue)  // Among Sort > 0, order by Sort ASC
            .ThenBy(x => x.Id)  // For Sort <= 0, order by Id
            .ToList();
        
        return Ok(result);
    }
    
    /// <summary>
    /// Get withdrawal payment methods
    /// </summary>
    /// <param name="criteria">Filter criteria</param>
    /// <param name="includeDeleted">Include soft-deleted payment methods (default: false)</param>
    /// <returns></returns>
    [HttpGet("withdrawal")]
    public async Task<IActionResult> GetWithdraw([FromQuery] PaymentMethod.Criteria? criteria, [FromQuery] bool includeDeleted = false)
    {
        criteria ??= new PaymentMethod.Criteria();
        var result = await GetTenantPaymentMethods(PaymentMethodTypes.Withdrawal, criteria, includeDeleted);   
        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPaymentMethodById(long id)
    {
        var result = await paymentMethodSvc.GetMethodByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result.ToTenantDetailModel());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await paymentMethodSvc.SoftDeletePaymentMethodAsync(id);
        return result ? Ok() : BadRequest(ToErrorResult("Failed to delete payment method."));
    }

    /// <summary>
    /// Restore a soft-deleted payment method (set IsDeleted = false)
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns></returns>
    [HttpPut("{id:long}/restore")]
    public async Task<IActionResult> Restore(long id)
    {
        var result = await paymentMethodSvc.RestorePaymentMethodAsync(id);
        return result ? Ok() : BadRequest(ToErrorResult("Failed to restore payment method."));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentMethod.CreateSpec spec)
    {
        var result = await paymentMethodSvc.CreateMethodAsync(spec.ToEntity());
        if (!result) return BadRequest(ToErrorResult("Failed to create payment method."));
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PaymentMethod.UpdateSpec spec)
    {
        var partyId = GetPartyId();
        var result = await paymentMethodSvc.UpdateMethodAsync(id, x => spec.ApplyToEntity(ref x, partyId));
        if (!result) return BadRequest(ToErrorResult("Failed to update payment method."));
        return Ok();
    }

    /// <summary>
    /// 批量更新 payment method sort order
    /// </summary>
    /// <param name="spec">List of payment method IDs with their new sort values</param>
    /// <returns></returns>
    [HttpPut("batch-update-sort")]
    public async Task<IActionResult> BatchUpdateSort([FromBody] PaymentMethod.BatchUpdateSortSpec spec)
    {
        if (spec.Items == null || spec.Items.Count == 0)
            return BadRequest(ToErrorResult("No items provided for update"));

        var (success, message) = await paymentMethodSvc.BatchUpdateSortAsync(spec.Items);

        if (!success)
            return BadRequest(ToErrorResult(message));
        
        return Ok(new { message });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/instruction")]
    public async Task<IActionResult> GetPaymentMethodInstruction(long id)
    {
        var method = await paymentMethodSvc.GetMethodByIdAsync(id);
        if (method == null) return BadRequest(ToErrorResult("Payment method not found."));

        var result = await paymentMethodSvc.GetInstructionByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/policy")]
    public async Task<IActionResult> GetPaymentMethodPolicy(long id)
    {
        var method = await paymentMethodSvc.GetMethodByIdAsync(id);
        if (method == null) return BadRequest(ToErrorResult("Payment method not found."));

        var result = method.Group == "Union Pay"
            ? await paymentMethodSvc.GetUnionPayPolicyAsync()
            : await paymentMethodSvc.GetPolicyByIdAsync(id);

        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/instruction")]
    public async Task<IActionResult> UpdateInstruction(long id, [FromBody] Dictionary<string, string> dictionary)
    {
        var (result, msg) = await paymentMethodSvc.UpdateInstructionAsync(id, dictionary);
        return result ? Ok(dictionary) : BadRequest(ToErrorResult(msg));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/policy")]
    public async Task<IActionResult> UpdatePolicy(long id, [FromBody] Dictionary<string, string> dictionary)
    {
        var (result, msg) = await paymentMethodSvc.UpdatePolicyAsync(id, dictionary);
        return result ? Ok(dictionary) : BadRequest(ToErrorResult(msg));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpGet("account/{accountId:long}/access")]
    public async Task<IActionResult> GetAccountPaymentMethodAccess(long accountId)
    {
        var result = await GetPaymentMethodAccess(nameof(Account), accountId);
        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="walletId"></param>
    /// <returns></returns>
    [HttpGet("wallet/{walletId:long}/access")]
    public async Task<IActionResult> GetWalletPaymentMethodAccess(long walletId)
    {
        var result = await GetPaymentMethodAccess(nameof(Wallet), walletId);
        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/account-enable/{accountId:long}")]
    public async Task<IActionResult> EnableAccount(long id, long accountId)
    {
        var methodExists = await paymentMethodSvc.MethodExistByIdAsync(id);
        if (!methodExists) return BadRequest(ToErrorResult("Payment method not found."));

        var accountExists = await accManageSvc.AccountExistByIdAsync(accountId);
        if (!accountExists) return BadRequest(ToErrorResult("Account not found."));

        var result = await paymentMethodSvc.EnableAccountAccessByMethodIdAsync(accountId, id);
        if (!result) return BadRequest(ToErrorResult("Failed to enable the payment method for account."));

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/account-disable/{accountId:long}")]
    public async Task<IActionResult> DisableAccount(long id, long accountId)
    {
        var methodExists = await paymentMethodSvc.MethodExistByIdAsync(id);
        if (!methodExists) return BadRequest(ToErrorResult("Payment method not found."));

        var accountExists = await accManageSvc.AccountExistByIdAsync(accountId);
        if (!accountExists) return BadRequest(ToErrorResult("Account not found."));

        var result = await paymentMethodSvc.DisableAccountAccessByMethodIdAsync(accountId, id);
        if (!result) return BadRequest(ToErrorResult("Failed to disable the payment method for account."));

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("account-group-enable/{accountId:long}")]
    public async Task<IActionResult> EnableAccountByGroup(long accountId,
        [FromBody] PaymentMethod.UpdateAccessGroupSpec spec)
    {
        var accountExists = await accManageSvc.AccountExistByIdAsync(accountId);
        if (!accountExists) return BadRequest(ToErrorResult("Account not found."));

        var partyId = GetPartyId();
        var result = await paymentMethodSvc.EnableAccountAccessByGroupAsync(accountId, spec.Group, partyId);
        if (!result) return BadRequest(ToErrorResult("Failed to enable the payment method for account."));
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("account-group-disable/{accountId:long}")]
    public async Task<IActionResult> DisableAccountByGroup(long accountId,
        [FromBody] PaymentMethod.UpdateAccessGroupSpec spec)
    {
        var accountExists = await accManageSvc.AccountExistByIdAsync(accountId);
        if (!accountExists) return BadRequest(ToErrorResult("Account not found."));

        var partyId = GetPartyId();
        var result = await paymentMethodSvc.DisableAccountAccessByGroupAsync(accountId, spec.Group, partyId);
        if (!result) return BadRequest(ToErrorResult("Failed to enable the payment method for account."));
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="walletId"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/wallet-enable/{walletId:long}")]
    public async Task<IActionResult> EnableWallet(long id, long walletId)
    {
        var methodExists = await paymentMethodSvc.MethodExistByIdAsync(id);
        if (!methodExists) return BadRequest(ToErrorResult("Payment method not found."));

        var walletExists = await walletSvc.WalletExistByIdAsync(walletId);
        if (!walletExists) return BadRequest(ToErrorResult("Wallet not found."));

        var result = await paymentMethodSvc.EnableWalletAccessByMethodIdAsync(walletId, id);
        if (!result) return BadRequest(ToErrorResult("Failed to enable the payment method for wallet."));

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="walletId"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/wallet-disable/{walletId:long}")]
    public async Task<IActionResult> DisableWallet(long id, long walletId)
    {
        var methodExists = await paymentMethodSvc.MethodExistByIdAsync(id);
        if (!methodExists) return BadRequest(ToErrorResult("Payment method not found."));

        var accountExists = await walletSvc.WalletExistByIdAsync(walletId);
        if (!accountExists) return BadRequest(ToErrorResult("Wallet not found."));

        var result = await paymentMethodSvc.DisableWalletAccessByMethodIdAsync(walletId, id);
        if (!result) return BadRequest(ToErrorResult("Failed to disable the payment method for wallet."));

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="walletId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("wallet-group-enable/{walletId:long}")]
    public async Task<IActionResult> EnableWalletByGroup(long walletId,
        [FromBody] PaymentMethod.UpdateAccessGroupSpec spec)
    {
        var walletExists = await walletSvc.WalletExistByIdAsync(walletId);
        if (!walletExists) return BadRequest(ToErrorResult("Wallet not found."));

        var result = await paymentMethodSvc.EnableWalletAccessByGroupAsync(walletId, spec.Group);
        if (!result) return BadRequest(ToErrorResult("Failed to enable the payment method for wallet."));

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="walletId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("wallet-group-disable/{walletId:long}")]
    public async Task<IActionResult> DisableWalletByGroup(long walletId,
        [FromBody] PaymentMethod.UpdateAccessGroupSpec spec)
    {
        var walletExists = await walletSvc.WalletExistByIdAsync(walletId);
        if (!walletExists) return BadRequest(ToErrorResult("Wallet not found."));

        var result = await paymentMethodSvc.DisableWalletAccessByGroupAsync(walletId, spec.Group);
        if (!result) return BadRequest(ToErrorResult("Failed to enable the payment method for wallet."));
        return Ok();
    }

    [HttpPut("account/open-by-existing")]
    public async Task<IActionResult> OpenByExisting([FromBody] PaymentMethod.GroupEnableByExisting spec)
    {
        var opened = spec.OpenedMethodIds;
        var newOpens = spec.NewOpenMethodIds;

        var sql = $"""
                   DELETE
                   FROM acct."_AccountPaymentMethodAccess"
                   WHERE "AccountId" IN
                         (SELECT DISTINCT "AccountId" FROM acct."_AccountPaymentMethodAccess" WHERE "PaymentMethodId" IN ({string.Join(",", opened)}))
                     AND "PaymentMethodId" IN ({string.Join(",", newOpens)});

                   DO $$
                   DECLARE
                       payment_method_id integer;
                       payment_method_ids integer[] := ARRAY[{string.Join(",", newOpens)}];
                   BEGIN
                       FOREACH payment_method_id IN ARRAY payment_method_ids
                       LOOP
                           INSERT INTO acct."_AccountPaymentMethodAccess" ("AccountId", "PaymentMethodId", "Status", "OperatedPartyId")
                           SELECT DISTINCT "AccountId", payment_method_id, 10, 1
                           FROM acct."_AccountPaymentMethodAccess"
                           WHERE "PaymentMethodId" IN ({string.Join(",", opened)});
                   
                           RAISE NOTICE 'Processed PaymentMethodId: %', payment_method_id;
                       END LOOP;
                   END;
                   $$;
                   """;

        await tenantCtx.Database.ExecuteSqlRawAsync(sql);
        return Ok();
    }

    private async Task<PaymentMethodDTO.TenantAccessManagement> GetPaymentMethodAccess(string model, long id)
    {
        var methods = await paymentMethodSvc.GetMethodsAsync();
        var items = model switch
        {
            nameof(Account) => await paymentMethodSvc.GetAccountAccessQuery(id)
                .Select(x => new { x.PaymentMethodId, x.Status })
                .ToListAsync(),
            nameof(Wallet) => await paymentMethodSvc.GetWalletAccessQuery(id)
                .Select(x => new { x.PaymentMethodId, x.Status })
                .ToListAsync(),
            _ => throw new NotSupportedException()
        };

        var dictionary = methods.OrderBy(x => x.Name)
            .GroupBy(x => x.MethodType)
            .ToDictionary(x => x.Key, x => x.Select(y =>
            {
                var access = items.FirstOrDefault(z => z.PaymentMethodId == y.Id);
                return new PaymentMethodDTO.AccessManagement
                {
                    Id = y.Id,
                    Name = y.Name,
                    Group = y.Group,
                    Status = (PaymentMethodStatusTypes)y.Status,
                    CurrencyId = (CurrencyTypes)y.CurrencyId,
                    AccessStatus = access?.Status == null
                        ? PaymentMethodAccessStatusTypes.Inactive
                        : (PaymentMethodAccessStatusTypes)access.Status
                };
            }).ToList());

        var result = new PaymentMethodDTO.TenantAccessManagement
        {
            Deposit = dictionary.GetValueOrDefault(PaymentMethodTypes.Deposit, []),
            Withdrawal = dictionary.GetValueOrDefault(PaymentMethodTypes.Withdrawal, []),
        };

        return result;
    }

    private async Task<List<PaymentMethod.TenantPageModel>> GetTenantPaymentMethods(string methodType,
        PaymentMethod.Criteria criteria, bool includeDeleted = false)
    {
        var currencies = criteria.AvailableCurrencies?
            .Select(x => (int)x)
            .ToHashSet();
        
        var items = await paymentMethodSvc.GetMethodsAsync();
        items = items
            .Where(x => x.MethodType == methodType)
            .Where(x => currencies == null || currencies.Contains(x.CurrencyId))
            .Where(x => includeDeleted || !x.IsDeleted)  // Filter out deleted items by default
            .ToList();

        var partyIds = items.Select(x => x.OperatorPartyId).Distinct().ToList();
        var names = await tenantCtx.Parties
            .Where(x => partyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        var result = items.ToTenantPageModel().ToList();
        result.ForEach(x => x.OperatorName = names.GetValueOrDefault(x.OperatorPartyId, ""));
        return result;
    }
}