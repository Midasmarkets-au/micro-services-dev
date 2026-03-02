using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using M = Bacera.Gateway.PaymentService;
using MSG = Bacera.Gateway.ResultMessage.Common;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Area("Client")]
[Tags("Client/Payment Service")]
[Route("api/" + VersionTypes.V1 + "/[Area]/payment-service")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class PaymentServiceController : ClientBaseController
{
    private readonly AccountingService _accountingSvc;
    private readonly TenantDbContext _tenantDbContext;
    private readonly ConfigurationService _configurationSvc;
    private readonly AuthDbContext _authDbContext;

    public PaymentServiceController(
        TenantDbContext tenantDbContext
        , AccountingService accountingService
        , ConfigurationService configurationSvc, AuthDbContext authDbContext)
    {
        _tenantDbContext = tenantDbContext;
        _accountingSvc = accountingService;
        _configurationSvc = configurationSvc;
        _authDbContext = authDbContext;
    }
    
    /// <summary>
    /// Get activated payment services for current User
    /// </summary>
    /// <param name="fundType"></param>
    /// <param name="currencyId"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaymentService.AccessResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentService.AccessResponseModel>> GetPaymentService(
        [FromQuery] FundTypes? fundType = null, [FromQuery] CurrencyTypes? currencyId = null)
    {
        var services = await _accountingSvc.GetPaymentServiceAccessForClientAsync(GetPartyId(), fundType, currencyId);
        return Ok(services);
    }
    
    /// <summary>
    /// Get payment services by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PaymentService), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentServiceById(long id)
    {
        var paymentService = await _tenantDbContext.PaymentServiceAccesses
            .Where(x => x.PaymentService.IsActivated == 1)
            .Where(x => x.PaymentService.CanDeposit == 1 || x.PaymentService.CanWithdraw == 1)
            .Where(x => x.PartyId == GetPartyId())
            .Where(x => x.PaymentServiceId == id)
            .Where(x => x.PaymentService.Id > 10)
            .Where(x => !string.IsNullOrEmpty(x.PaymentService.CategoryName))
            .Select(x => new
            {
                x.PaymentService.Id,
                x.PaymentService.Name,
                x.PaymentService.Description,
                x.PaymentService.CategoryName,
                x.PaymentService.InitialValue,
                x.PaymentService.MinValue,
                x.PaymentService.MaxValue,
                x.PaymentService.CurrencyId,
                x.PaymentService.Platform,
                x.PaymentService.Sequence,
            })
            .FirstOrDefaultAsync();
        return Ok(paymentService);
    }
    
    /// <summary>
    /// Get account is initial deposit
    /// </summary>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [HttpGet("deposit/initial/{accountUid:long}")]
    [ProducesResponseType(typeof(PaymentService), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsInitialForAccount(long accountUid)
        => Ok(!await AccountHasDepositAsync(accountUid));
    
    /// <summary>
    /// Get activated payment services for current User by Category
    /// </summary>
    /// <param name="categoryName"></param>
    /// <param name="amount"></param>
    /// <param name="fundType"></param>
    /// <param name="currencyId"></param>
    /// <returns></returns>
    [HttpGet("deposit/category/{categoryName}")]
    [ProducesResponseType(typeof(PaymentService), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentServiceByCategoryName(
        [FromRoute] string categoryName,
        [FromQuery] FundTypes fundType,
        [FromQuery] CurrencyTypes currencyId,
        [FromQuery] decimal? amount)
    {
        var query = _tenantDbContext.PaymentServiceAccesses
                .Where(x => x.PartyId == GetPartyId())
                .Where(x => x.CanDeposit == 1)
                .Where(x => x.FundType == (int)fundType)
                .Where(x => x.CurrencyId == (int)currencyId)
                .Where(x => x.PaymentService.CategoryName == categoryName)
                .Where(x => !string.IsNullOrEmpty(x.PaymentService.CategoryName))
                .Where(x => x.PaymentService.IsActivated == 1)
                .Where(x => x.PaymentService.CanDeposit == 1)
                .Where(x => x.PaymentService.Id > 10)
            ;
    
        if (amount is > 0)
        {
            query = query.Where(x => x.PaymentService.MinValue <= amount && x.PaymentService.MaxValue >= amount);
        }
    
        var paymentServices = await query
            .Select(x => new PaymentService
            {
                Id = x.PaymentService.Id,
                Name = x.PaymentService.Name,
                Description = x.PaymentService.Description,
                CategoryName = x.PaymentService.CategoryName,
                InitialValue = x.PaymentService.InitialValue,
                MinValue = x.PaymentService.MinValue,
                MaxValue = x.PaymentService.MaxValue,
                CurrencyId = x.PaymentService.CurrencyId,
                Platform = x.PaymentService.Platform,
                Sequence = x.PaymentService.Sequence,
                IsHighDollarEnabled = x.PaymentService.IsHighDollarEnabled,
            })
            .ToListAsync();
        if (!paymentServices.Any())
            return NotFound();
    
    
        var rand = new Random();
        var paymentService = paymentServices.ElementAt(rand.Next(paymentServices.Count));
    
        if (categoryName.ToLower().Contains("union")
            && paymentServices.Any(x => x.IsHighDollarEnabled == 1)
            && amount >= await _configurationSvc.GetHighDollarValueAsync())
            paymentService = GetRandomRecordBySequence(paymentServices.Where(x => x.IsHighDollarEnabled == 1).ToList());
    
        var exchangeRate = 1m;
        if (currencyId != (CurrencyTypes)paymentService.CurrencyId)
        {
            exchangeRate = GetExchangeRateAsync(currencyId, (CurrencyTypes)paymentService.CurrencyId).Result;
            if (exchangeRate == -1)
            {
                return BadRequest(Result.Error(ResultMessage.Deposit.ExchangeRateNotExists,
                    new { From = currencyId, To = paymentService.CurrencyId }));
            }
        }
    
        return Ok(new
        {
            paymentService.Id,
            paymentService.Name,
            paymentService.Description,
            paymentService.CategoryName,
            paymentService.InitialValue,
            paymentService.MinValue,
            paymentService.MaxValue,
            paymentService.CurrencyId,
            paymentService.Platform,
            paymentService.Sequence,
            exchangeRate,
        });
    }
    
    /// <summary>
    /// Get all categories
    /// </summary>
    /// <param name="fundType"></param>
    /// <param name="currencyId"></param>
    /// <returns></returns>
    [HttpGet("deposit/category")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentServiceCategory([FromQuery] FundTypes fundType,
        [FromQuery] CurrencyTypes currencyId)
    {
        var categories = await _tenantDbContext.PaymentServiceAccesses
            .Where(x => x.PartyId == GetPartyId())
            .Where(x => x.CanDeposit == 1)
            .Where(x => x.FundType == (int)fundType)
            .Where(x => x.CurrencyId == (int)currencyId)
            .Where(x => x.PaymentService.IsActivated == 1)
            .Where(x => x.PaymentService.CanDeposit == 1)
            .Where(x => x.PaymentService.Id > 10)
            .Where(x => !string.IsNullOrEmpty(x.PaymentService.CategoryName))
            .GroupBy(x => x.PaymentService.CategoryName)
            .Select(x => x.Key)
            .ToListAsync();
        return Ok(categories);
    }
    
    /// <summary>
    /// Get payment service instructions
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/instruction")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstruction(long id, string? language)
    {
        var svc = await _tenantDbContext.PaymentServices
            .Where(x => x.IsActivated == 1).SingleOrDefaultAsync(x => x.Id == id);
        if (svc == null)
            return NotFound();
    
        var item = await _tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync() ?? Supplement.Build(SupplementTypes.PaymentServiceInstruction, id);
    
        var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Data) ??
                    new Dictionary<string, string>();
        if (LanguageTypes.All.Contains(language ?? string.Empty))
            return Ok(items.Where(x => x.Key == language).ToDictionary(p => p.Key, p => p.Value));
    
        return Ok(items);
    }
    
    [HttpGet("union-pay/policy")]
    public async Task<IActionResult> GetUnionPayAmountRange([FromQuery] long accountUid)
    {
        if (accountUid <= 0) return BadRequest(Result.Error("__ACCOUNT_UID_REQUIRED__"));
        var firstDeposit = !await AccountHasDepositAsync(accountUid);
    
        var item = await _tenantDbContext.PaymentServices
            .AsNoTracking()
            .Where(x => x.CategoryName == "Union Pay")
            .Where(x => x.IsActivated == 1 && x.CanDeposit == 1)
            .GroupBy(x => x.CategoryName)
            .Select(x => new
            {
                Ids = x.Select(y => y.Id).ToList(),
                MinInitialValue = x.Min(y => y.InitialValue),
                MaxInitialValue = x.Max(y => y.InitialValue),
                MinMinValue = x.Min(y => y.MinValue),
                MaxMaxValue = x.Any(y => y.IsHighDollarEnabled == 1)
                    ? x.Where(y => y.IsHighDollarEnabled == 1).Max(y => y.MaxValue)
                    : x.Max(y => y.MaxValue),
                // HighDollarEnabled = x.Any(y => y.IsHighDollarEnabled == 1)
            })
            .FirstOrDefaultAsync();
        if (item == null) return NotFound();
        var range = new List<long> { firstDeposit ? item.MinInitialValue : item.MinMinValue, item.MaxMaxValue };
    
        var supplement = await _tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
            .Where(x => item.Ids.Contains(x.RowId))
            .FirstOrDefaultAsync();
    
        var policy = JsonConvert.DeserializeObject<Dictionary<string, string>>(supplement?.Data ?? "{}") ??
                     new Dictionary<string, string>();
        var exchangeRate = await GetExchangeRateAsync(CurrencyTypes.USD, CurrencyTypes.CNY);
        if (exchangeRate == -1)
            return BadRequest(Result.Error(ResultMessage.Deposit.ExchangeRateNotExists,
                new { From = CurrencyTypes.CNY, To = CurrencyTypes.USD }));
    
        return Ok(new { policy, range, exchangeRate });
    }
    
    
    [HttpGet("union-pay/service-id")]
    public async Task<IActionResult> GetUnionPayPaymentServiceByAmount([FromQuery] long amount,
        [FromQuery] long accountUid)
    {
        var account = await _tenantDbContext.Accounts
            .Where(x => x.Uid == accountUid)
            .Select(x => new { x.CurrencyId, x.FundType, x.PartyId })
            .FirstOrDefaultAsync();
    
        if (account == null) return BadRequest(Result.Error("Account Not Found"));
    
        var amountInDecimal = amount / 100m;
        var firstDeposit = !await AccountHasDepositAsync(accountUid);
        var highDollarValue = await _configurationSvc.GetHighDollarValueAsync();
    
        var paymentServices = await _tenantDbContext.PaymentServiceAccesses
            .Where(x => x.PartyId == account.PartyId)
            .Where(x => x.CanDeposit == 1)
            .Where(x => x.CurrencyId == account.CurrencyId && x.FundType == account.FundType)
            .Select(x => x.PaymentService)
            .Where(x => x.CategoryName == "Union Pay")
            .Where(x => x.IsActivated == 1 && x.CanDeposit == 1)
            .Where(x => x.MaxValue >= amountInDecimal)
            .Where(x => firstDeposit ? amountInDecimal >= x.InitialValue : amountInDecimal >= x.MinValue)
            .Select(x => new { x.Id, x.Sequence, x.IsHighDollarEnabled, x.MaxValue, x.Name, x.Platform, x.CategoryName, x.Description })
            .ToListAsync();
    
        if (amountInDecimal >= highDollarValue &&
            paymentServices.Any(x => x.IsHighDollarEnabled == 1 && x.MaxValue >= amountInDecimal))
        {
            paymentServices = paymentServices.Where(x => x.IsHighDollarEnabled == 1).ToList();
        }
        else
        {
            paymentServices = paymentServices.Where(x => x.MaxValue >= amountInDecimal).ToList();
        }
    
        if (!paymentServices.Any()) return Ok(new { id = (long)-1, name = "", platform = -1 });
        var sum = paymentServices.Sum(x => x.Sequence);
        var randomValue = new Random().Next(0, sum);
        foreach (var record in paymentServices)
        {
            randomValue -= record.Sequence;
            if (randomValue <= 0)
                return Ok(new { id = record.Id, name = record.Name, platform = record.Platform, record.CategoryName, record.Description });
        }
    
        var paymentService = paymentServices.First();
        return Ok(new
        {
            id = paymentService.Id, name = paymentService.Name, platform = paymentService.Platform, paymentService.CategoryName,
            paymentService.Description
        });
    }
    
    /// <summary>
    /// Get payment service policies
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/policy")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPolicy(long id, string? language)
    {
        var svc = await _tenantDbContext.PaymentServices
            .Where(x => x.IsActivated == 1).SingleOrDefaultAsync(x => x.Id == id);
        if (svc == null)
            return NotFound();
    
        var item = await _tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync() ?? Supplement.Build(SupplementTypes.PaymentServicePolicy, id);
        var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Data) ??
                    new Dictionary<string, string>();
        if (LanguageTypes.All.Contains(language ?? string.Empty))
            return Ok(items.Where(x => x.Key == language).ToDictionary(p => p.Key, p => p.Value));
    
        return Ok(items);
    }
    
    private Task<bool> AccountHasDepositAsync(long uid) => _tenantDbContext.Deposits
        .Where(x => x.TargetAccount != null && x.TargetAccount.Uid == uid)
        .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCompleted)
        .AnyAsync();
    
    private static PaymentService GetRandomRecordBySequence(List<PaymentService> records)
    {
        var totalWeight = records.Sum(r => r.Sequence);
        var randomValue = new Random().Next(0, totalWeight);
    
        foreach (var record in records)
        {
            randomValue -= record.Sequence;
            if (randomValue <= 0)
            {
                return record;
            }
        }
    
        return records.First();
    }
    
    
    private async Task<decimal> GetExchangeRateAsync(CurrencyTypes from, CurrencyTypes to)
    {
        var exchangeRateEntity = await _accountingSvc.GetExchangeRateAsync(from, to);
        if (exchangeRateEntity == null) return -1;
    
        return Math.Ceiling(
            exchangeRateEntity.SellingRate *
            (1 + exchangeRateEntity.AdjustRate / 100) *
            1000
        ) / 1000;
    }
}