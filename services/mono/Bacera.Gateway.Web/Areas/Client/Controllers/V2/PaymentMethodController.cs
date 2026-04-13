using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Vendor.EuPayment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MSG = Bacera.Gateway.ResultMessage.Transaction;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Payment Method")]
[Area("Client")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/[Area]/payment-method")]
public class PaymentMethodController(
    PaymentMethodService paymentMethodSvc,
    AccountManageService accManageSvc,
    TenantDbContext tenantCtx,
    ConfigService cfgSvc,
    UserService userService,
    ILogger<PaymentMethodController> logger)
    : ClientBaseControllerV2
{
    /// <summary>
    /// Get payment method instruction
    /// </summary>
    /// <param name="hashId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("{hashId}/instruction")]
    public async Task<IActionResult> GetInstruction(string hashId)
    {
        var id = PaymentMethod.HashDecode(hashId);
        var party = await userService.GetPartyAsync(GetPartyId());
        var method = await paymentMethodSvc.GetMethodByIdAsync(id);
        if (method == null) return NotFound();
        var instruction = await paymentMethodSvc.GetInstructionAsync(method, party.Language);
        return Ok(instruction);
    }

    /// <summary>
    /// Get account's available currency for deposit.
    /// </summary>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("account/{accountUid:long}/available-currency")]
    public async Task<IActionResult> AvailableCurrency(long accountUid)
    {
        var accountId = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var accountCurrency = await accManageSvc.GetAccountCurrencyIdAsync(accountId);
        var paymentMethodIds = await paymentMethodSvc.GetAccountAccessIdsAsync(accountId
            , PaymentMethodTypes.Deposit
            , PaymentMethodStatusTypes.Active
            , PaymentMethodAccessStatusTypes.Active);

        var result = new HashSet<CurrencyTypes>();
        foreach (var methodId in paymentMethodIds)
        {
            var method = await paymentMethodSvc.GetMethodByIdAsync(methodId);
            if (method == null) continue;
            var currencies = method.GetAvailableCurrencies(accountCurrency);
            foreach (var currency in currencies)
            {
                result.Add(currency);
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// Get active deposit access group by account uid.
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="currencyId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("account/{accountUid:long}/deposit-group")]
    public async Task<IActionResult> GetActiveAccountDepositAccessGrouped(long accountUid,
        [FromQuery] CurrencyTypes? currencyId = null)
    {
        var accountId = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var activeMethodIds = (await paymentMethodSvc.GetAccountAccessIdsAsync(accountId
            , PaymentMethodTypes.Deposit
            , PaymentMethodStatusTypes.Active
            , PaymentMethodAccessStatusTypes.Active))
            .ToHashSet();

        var allMethods = await paymentMethodSvc.GetMethodsAsync();
        var depositMethods = allMethods
            .Where(x => x.MethodType == PaymentMethodTypes.Deposit && x.Status == (int)PaymentMethodStatusTypes.Active)
            .ToList();

        var dict = new Dictionary<string, PaymentMethod.ClientGroupModel>();
        foreach (var method in depositMethods)
        {
            var currencies = method.GetAvailableCurrencies();
            if (currencyId != null && !currencies.Contains(currencyId.Value)) continue;
            var isActive = activeMethodIds.Contains(method.Id);

            if (!dict.TryGetValue(method.Group, out var value))
            {
                var model = method.ToClientGroupModel();
                model.IsActive = isActive;
                dict[method.Group] = model;
            }
            else
            {
                value.Range[0] = Math.Min(value.Range[0], method.MinValue);
                value.Range[1] = Math.Max(value.Range[1], method.MaxValue);
                value.InitialValue = method.InitialValue;
                if (!string.IsNullOrEmpty(method.Logo))
                    value.Logo = method.Logo;
                if (isActive)
                    value.IsActive = true;

                foreach (var currency in currencies)
                    value.AvailableCurrencies.Add(currency);
            }
        }

        var result = dict.Values
            .OrderByDescending(x => x.IsActive)
            .ToList();
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(item.Logo)) item.Logo = $"/images/wallet/{item.Group}.png";
            if (currencyId != null) item.AvailableCurrencies = [currencyId.Value];
        }
        return Ok(result);
    }

    /// <summary>
    /// Get and select a random active deposit method by group.
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("account/{accountUid:long}/deposit-group-info")]
    public async Task<IActionResult> GetMethodWithInfoByGroup(long accountUid, [FromQuery] string group)
    {
        if (accountUid <= 0) return BadRequest(Result.Error("__ACCOUNT_UID_REQUIRED__"));
        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == accountUid)
            .Select(x => new { x.Id, x.CurrencyId })
            .SingleAsync();

        var method = await paymentMethodSvc.GetActiveDepositMethodRandomlyAsync(group, account.Id);
        if (method == null) return NotFound();

        var party = await userService.GetPartyAsync(GetPartyId());
        var instruction = await paymentMethodSvc.GetInstructionAsync(method, party.Language);
        var policy = await paymentMethodSvc.GetPolicyAsync(method, party.Language);

        var hashId = PaymentMethod.HashEncode(method.Id);
        var range = await paymentMethodSvc.GetRangeByGroupAsync(group, account.Id);
        var toCurrency = await accManageSvc.GetAccountCurrencyIdAsync(account.Id);
        // var exchangeRate = await paymentMethodSvc.GetSellingExchangeRateAsync(toCurrency, (CurrencyTypes)method.CurrencyId);

        var currencies = method.GetAvailableCurrencies((CurrencyTypes)account.CurrencyId);
        var exchangeRates = await paymentMethodSvc.GetSellingExchangeRatesAsync(toCurrency, currencies);
        var result = PaymentMethodDTO.GroupInfo.Build(hashId, (PlatformTypes)method.Platform, policy, instruction, range, exchangeRates,
            method.GetRequestKeys());
        if (method.Platform == (int)PaymentPlatformTypes.EuPay)
        {
            var obj = await cfgSvc.GetRawValueAsync(nameof(Party), party.Id,
                ConfigKeys.EuPayDesensitizedRequestData) ?? "{}";
            result.RequestValues = Utils.JsonDeserializeDynamic(obj);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get withdrawal info
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{hashId}/account/{accountUid:long}/withdrawal-info")]
    public async Task<IActionResult> GetWithdrawalMethodInfo(string hashId, long accountUid)
    {
        if (accountUid <= 0) return BadRequest(Result.Error("__ACCOUNT_UID_REQUIRED__"));
        var accountId = await accManageSvc.GetAccountIdByUidAsync(accountUid);

        var id = PaymentMethod.HashDecode(hashId);
        if (!await paymentMethodSvc.IsAccountAccessEnabledAsync(accountId, id))
            return BadRequest(Result.Error("Payment_method_not_enabled"));

        var method = await paymentMethodSvc.GetMethodByIdAsync(id);
        if (method == null) return NotFound();

        var party = await userService.GetPartyAsync(GetPartyId());

        var policy = await paymentMethodSvc.GetPolicyAsync(method, party.Language);
        var instruction = await paymentMethodSvc.GetInstructionAsync(method, party.Language);

        var range = new[] { method.MinValue, method.MaxValue };
        var fromCurrency = await accManageSvc.GetAccountCurrencyIdAsync(accountId);
        var exchangeRates = await paymentMethodSvc.GetBuyingExchangeRatesAsync(fromCurrency, method.GetAvailableCurrencies());
        var result = PaymentMethodDTO.GroupInfo.Build(hashId, (PlatformTypes)method.Platform, policy, instruction, range, exchangeRates,
            method.GetRequestKeys());
        return Ok(result);
    }

    /// <summary>
    /// Get active withdrawal access group by account uid.
    /// </summary>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("account/{accountUid:long}/withdrawal")]
    public async Task<IActionResult> GetActiveAccountWithdrawAccessGrouped(long accountUid)
    {
        var accountId = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var activeMethodIds = (await paymentMethodSvc.GetAccountAccessIdsAsync(accountId, PaymentMethodTypes.Withdrawal,
            PaymentMethodStatusTypes.Active, PaymentMethodAccessStatusTypes.Active))
            .ToHashSet();

        var paymentMethods = await paymentMethodSvc.GetMethodsAsync();
        var result = paymentMethods
            .Where(x => x is { MethodType: PaymentMethodTypes.Withdrawal, Status: (int)PaymentMethodStatusTypes.Active })
            .Select(x => new PaymentMethod.ClientNameModel
            {
                Id = x.Id,
                Name = x.Name,
                Logo = x.Logo,
                Range = new[] { x.MinValue, x.MaxValue },
                InitialValue = x.InitialValue,
                IsActive = activeMethodIds.Contains(x.Id)
            })
            .OrderByDescending(x => x.IsActive)
            .ToList();

        foreach (var item in result.Where(x => string.IsNullOrEmpty(x.Logo)))
        {
            item.Logo = "/images/wallet/" + item.Name + ".png";
        }
            
        return Ok(result);
    }

    /// <summary>
    ///  Get active deposit access group by wallet hash id.
    /// </summary>
    /// <param name="walletHashId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("wallet/{walletHashId}/deposit-group")]
    public async Task<IActionResult> GetActiveWalletDepositAccessGrouped(string walletHashId)
    {
        var walletId = Wallet.HashDecode(walletHashId);
        var activeMethodIds = (await paymentMethodSvc.GetWalletAccessIdsAsync(walletId, PaymentMethodTypes.Deposit,
            PaymentMethodStatusTypes.Active, PaymentMethodAccessStatusTypes.Active))
            .ToHashSet();

        var paymentMethods = await paymentMethodSvc.GetMethodsAsync();
        var depositMethods = paymentMethods
            .Where(x => x is { MethodType: PaymentMethodTypes.Deposit, Status: (int)PaymentMethodStatusTypes.Active })
            .ToList();

        var dict = new Dictionary<string, PaymentMethod.ClientGroupModel>();
        foreach (var method in depositMethods)
        {
            var isActive = activeMethodIds.Contains(method.Id);
            if (!dict.TryGetValue(method.Group, out var value))
            {
                dict[method.Group] = new PaymentMethod.ClientGroupModel
                {
                    Group = method.Group,
                    Logo = "/images/wallet/" + method.Group + ".png",
                    Range = [method.MinValue, method.MaxValue],
                    AvailableCurrencies = method.GetAvailableCurrencies(),
                    IsActive = isActive
                };
            }
            else
            {
                value.Range[0] = Math.Min(value.Range[0], method.MinValue);
                value.Range[1] = Math.Max(value.Range[1], method.MaxValue);
                if (isActive)
                    value.IsActive = true;

                foreach (var currency in method.GetAvailableCurrencies())
                    value.AvailableCurrencies.Add(currency);
            }
        }

        return Ok(dict.Values.ToList());
    }

    /// <summary>
    /// Get active withdrawal access group by wallet hash id. 
    /// </summary>
    /// <param name="walletHashId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("wallet/{walletHashId}/withdrawal")]
    public async Task<IActionResult> GetActiveWalletWithdrawAccessGrouped(string walletHashId)
    {
        var walletId = Wallet.HashDecode(walletHashId);
        var activeMethodIds = (await paymentMethodSvc.GetWalletAccessIdsAsync(walletId, PaymentMethodTypes.Withdrawal,
            PaymentMethodStatusTypes.Active, PaymentMethodAccessStatusTypes.Active))
            .ToHashSet();

        var paymentMethods = await paymentMethodSvc.GetMethodsAsync();
        var result = paymentMethods
            .Where(x => x is { MethodType: PaymentMethodTypes.Withdrawal, Status: (int)PaymentMethodStatusTypes.Active })
            .Select(x => new PaymentMethod.ClientNameModel
            {
                Id = x.Id,
                Name = x.Name,
                Logo = x.Logo,
                IsActive = activeMethodIds.Contains(x.Id),
                Range = new[] { x.MinValue, x.MaxValue }
            })
            .OrderByDescending(x => x.IsActive)
            .ToList();
        
        foreach (var item in result)
        {
            if (string.IsNullOrEmpty(item.Logo)) item.Logo = $"/images/wallet/{item.Name}.png";
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Get and select a random active withdrawal method by group.
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="walletHashId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{hashId}/wallet/{walletHashId}/withdrawal-info")]
    public async Task<IActionResult> GetWalletWithdrawalMethodInfo(string hashId, string walletHashId)
    {
        if (string.IsNullOrEmpty(hashId)) return BadRequest(Result.Error("__WALLET_HASHID_REQUIRED__"));
        var walletId = Wallet.HashDecode(walletHashId);

        var id = PaymentMethod.HashDecode(hashId);
        if (!await paymentMethodSvc.IsWalletAccessEnabledAsync(walletId, id))
            return NotFound();

        var method = await paymentMethodSvc.GetMethodByIdAsync(id);
        if (method == null) return NotFound();

        var party = await userService.GetPartyAsync(GetPartyId());

        var policy = await paymentMethodSvc.GetPolicyAsync(method, party.Language);
        var instruction = await paymentMethodSvc.GetInstructionAsync(method, party.Language);

        var range = new[] { method.MinValue, method.MaxValue };

        var fromCurrency = await tenantCtx.Wallets.Where(x => x.Id == walletId).Select(x => (CurrencyTypes)x.CurrencyId).SingleAsync();
        var exchangeRates = await paymentMethodSvc.GetBuyingExchangeRatesAsync(fromCurrency, method.GetAvailableCurrencies());
        var result = PaymentMethodDTO.GroupInfo.Build(hashId, (PlatformTypes)method.Platform, policy, instruction, range, exchangeRates,
            method.GetRequestKeys());
        return Ok(result);
    }

    /// <summary>
    /// Query Payment Bank Info
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("bank-info")]
    public async Task<IActionResult> GetPaymentBankInfo([FromQuery] PaymentInfo.ClientCriteria? criteria = null)
    {
        criteria ??= new PaymentInfo.ClientCriteria();
        criteria.PartyId = GetPartyId();
        var items = await paymentMethodSvc.QueryPaymentBankInfoForClientAsync(criteria);
        return Ok(Result<List<PaymentInfo.ClientPageModel>, PaymentInfo.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Create Payment Bank Info
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpPost("bank-info")]
    public async Task<IActionResult> CreatePaymentBankInfo(PaymentInfo.CreateAndUpdateSpec spec)
    {
        if (!spec.IsValid()) return BadRequest(Result.Error("__INVALID_REQUEST__"));
        string infoJson = JsonConvert.SerializeObject(spec.Info);
        var result = await paymentMethodSvc.CreatePaymentBankInfoAsync(GetPartyId(), spec.PaymentPlatform, spec.Name, infoJson);
        if (!result) return BadRequest(Result.Error("__CREATE_FAILED__"));
        return NoContent();
    }


    /// <summary>
    /// Update Payment Bank Info
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpPut("bank-info/{hashId}")]
    public async Task<IActionResult> UpdatePaymentBankInfo(string hashId, [FromBody] PaymentInfo.CreateAndUpdateSpec spec)
    {
        if (!spec.IsValid()) return BadRequest(Result.Error("__INVALID_REQUEST__"));
        var id = PaymentInfo.HashDecode(hashId);
        string infoJson = JsonConvert.SerializeObject(spec.Info);
        var result = await paymentMethodSvc.UpdatePaymentBankInfoAsync(id, spec.PaymentPlatform, spec.Name, infoJson);
        if (!result) return BadRequest(Result.Error("__UPDATE_FAILED__"));
        return NoContent();
    }

    /// <summary>
    /// Delete Payment Bank Info
    /// </summary>
    /// <param name="hashId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("bank-info/{hashId}")]
    public async Task<IActionResult> DeletePaymentBankInfo(string hashId)
    {
        var id = PaymentInfo.HashDecode(hashId);
        await paymentMethodSvc.DeletePaymentBankInfoAsync(id);
        return NoContent();
    }
}