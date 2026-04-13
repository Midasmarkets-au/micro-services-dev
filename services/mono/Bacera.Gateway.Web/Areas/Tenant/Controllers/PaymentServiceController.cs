
﻿using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor;
using Bacera.Gateway.Vendor.Bakong;
using Bacera.Gateway.Vendor.BipiPay;
using Bacera.Gateway.Vendor.ExLink.Models;
using Bacera.Gateway.Vendor.GPay;
using Bacera.Gateway.Vendor.Help2Pay;
using Bacera.Gateway.Vendor.Monetix;
using Bacera.Gateway.Vendor.OFAPay;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Vendor.PayPal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using M = Bacera.Gateway.PaymentService;
using MSG = Bacera.Gateway.ResultMessage.Common;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Payment Service")]
[Route("api/" + VersionTypes.V1 + "/[Area]/payment-service")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class PaymentServiceController(
    TenantDbContext tenantCtx,
    PaymentMethodService paymentMethodSvc,
    ILogger<PaymentServiceController> logger,
    ConfigService cfgSvc
) : TenantBaseController
{
    /// <summary>
    /// Payment services pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await tenantCtx.PaymentServices
            .PagedFilterBy(criteria)
            .SelectBaseInfo()
            .ToListAsync();
        return Ok(Result<List<M>, M.Criteria>.Of(items, criteria));
    }
    
    
    /// <summary>
    /// Get payment service
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await paymentMethodSvc.GetMethodByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Test payment service
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/test")]
    public async Task<IActionResult> Test(long id)
    {
        // var svc = await tenantCtx.PaymentServices
        //     .Where(x => x.Id == id)
        //     .Select(x => new { x.Platform, x.Configuration, })
        //     .FirstOrDefaultAsync();
        // if (svc == null) return NotFound();
        //
        // Func<string, ILogger, Task<object>> handler = (PaymentPlatformTypes)svc.Platform switch
        // {
        //     PaymentPlatformTypes.GPay => GPay.TestAsync,
        //     PaymentPlatformTypes.UEnjoy => UEnjoy.TestAsync,
        //     PaymentPlatformTypes.ExLink => ExLink.TestAsync,
        //     PaymentPlatformTypes.Help2Pay => Help2Pay.TestAsync,
        //     PaymentPlatformTypes.DragonPay => DragonPayPHP.TestAsync,
        //     PaymentPlatformTypes.PaymentAsiaRMB => PaymentAsiaRMB.TestAsync,
        //     PaymentPlatformTypes.OFAPay => OFAPay.TestAsync,
        //     PaymentPlatformTypes.PayPal => PayPal.TestAsync,
        //     PaymentPlatformTypes.BipiPay => BipiPay.TestAsync,
        //     PaymentPlatformTypes.Bakong => Bakong.TestAsync,
        //     PaymentPlatformTypes.Monetix => Monetix.TestAsync,
        //     _ => (_, _) => Task.Run(() => new object())
        // };
        // var response = await TestHandler(svc.Configuration, logger, handler);

        return Ok();
    }

    private static Task<object> TestHandler(string config, ILogger logger, Func<string, ILogger, Task<object>> func)
        => func(config, logger);

    /// <summary>
    /// Create a wire payment serviced
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create([FromBody] M.UpdateSpec spec)
    {
        var item = new PaymentService
        {
            Platform = (int)PaymentPlatformTypes.Wire,
            CurrencyId = (int)spec.CurrencyId,
            MinValue = spec.MinValue,
            MaxValue = spec.MaxValue,
            InitialValue = spec.InitialValue,
            CanWithdraw = ToShort(spec.CanWithdraw),
            CanDeposit = ToShort(spec.CanDeposit),
            IsActivated = ToShort(spec.IsActivated),
            Sequence = spec.Sequence,
            Name = Truncate(spec.Name, 32),
            CommentCode = Truncate(spec.CommentCode, 6),
            CategoryName = Truncate(spec.CategoryName, 32),
            Description = Truncate(spec.Description, 1024),
            IsHighDollarEnabled = ToShort(spec.IsHighDollarEnabled),
            IsAutoDepositEnabled = ToShort(spec.IsAutoDepositEnabled),
        };

        tenantCtx.PaymentServices.Add(item);

        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());

        tenantCtx.PaymentMethods.Add(new PaymentMethod
        {
            Id = item.Id,
            CommentCode = item.CommentCode,
            Configuration = "{}",
            CreatedOn = DateTime.UtcNow,
            CurrencyId = (int)spec.CurrencyId,
            Group = spec.CategoryName,
            InitialValue = spec.InitialValue,
            IsAutoDepositEnabled = ToShort(spec.IsAutoDepositEnabled),
            IsHighDollarEnabled = ToShort(spec.IsHighDollarEnabled),
            Logo = "",
            MaxValue = spec.MaxValue,
            MinValue = spec.MinValue,
            Name = spec.Name,
            Status = (short)PaymentMethodStatusTypes.Inactive,
            UpdatedOn = DateTime.UtcNow,
        });
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Update payment service
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var item = await tenantCtx.PaymentServices
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound();

        UpdateItem(item, spec);

        tenantCtx.PaymentServices.Update(item);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Update Fund Type of payment service
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/fund-type")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateFundType(long id, [FromBody] List<FundType> spec)
    {
        var item = await tenantCtx.PaymentServices
            .Include(x => x.FundTypes)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound();

        var fundTypes = await tenantCtx.FundTypes
            .Where(x => spec.Select(y => y.Id).Contains(x.Id))
            .ToListAsync();
        item.FundTypes = fundTypes;

        tenantCtx.PaymentServices.Update(item);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Get Fund Type of payment service
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/fund-type")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(List<FundTypes>))]
    public async Task<IActionResult> GetFundType(long id, [FromBody] List<FundType> spec)
    {
        var item = await tenantCtx.PaymentServices
            .Include(x => x.FundTypes)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound();

        return Ok(item.FundTypes);
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
        if (!await paymentMethodSvc.MethodExistByIdAsync(id))
            return NotFound();

        var item = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync() ?? Supplement.Build(SupplementTypes.PaymentServiceInstruction, id);

        var items = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, string>>(item.Data);

        if (LanguageTypes.All.Contains(language ?? string.Empty))
            return Ok(items.Where(x => x.Key == language).ToDictionary(p => p.Key, p => p.Value));

        return Ok(items);
    }

    /// <summary>
    /// Update payment service instructions
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/instruction")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInstruction(long id, [FromBody] Dictionary<string, string> dictionary)
    {
        if (!await paymentMethodSvc.MethodExistByIdAsync(id))
            return NotFound();

        if (dictionary.Any(keyValuePair => !LanguageTypes.All.Contains(keyValuePair.Key)))
            return BadRequest(Result.Error(MSG.LanguageNotSupport));

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServiceInstruction)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync() ?? Supplement.Build(SupplementTypes.PaymentServiceInstruction, id);

        var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(supplement.Data) ??
                    new Dictionary<string, string>();

        foreach (var item in dictionary)
        {
            if (items.Any(x => x.Key == item.Key))
                items[item.Key] = item.Value;
            else
                items.Add(item.Key, item.Value);
        }

        supplement.Data = JsonConvert.SerializeObject(items);
        supplement.UpdatedOn = DateTime.UtcNow;

        if (supplement.IsEmpty())
            await tenantCtx.Supplements.AddAsync(supplement);
        else
            tenantCtx.Supplements.Update(supplement);

        await tenantCtx.SaveChangesAsync();

        return Ok(items);
    }

    /// <summary>
    /// Update payment service policy
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/policy")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePolicy(long id, [FromBody] Dictionary<string, string> dictionary)
    {
        if (!await paymentMethodSvc.MethodExistByIdAsync(id))
            return NotFound();

        if (dictionary.Any(keyValuePair => !LanguageTypes.All.Contains(keyValuePair.Key)))
            return BadRequest(Result.Error(MSG.LanguageNotSupport));

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync() ?? Supplement.Build(SupplementTypes.PaymentServicePolicy, id);

        var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(supplement.Data) ??
                    new Dictionary<string, string>();

        foreach (var item in dictionary)
        {
            if (items.Any(x => x.Key == item.Key))
                items[item.Key] = item.Value;
            else
                items.Add(item.Key, item.Value);
        }

        supplement.Data = JsonConvert.SerializeObject(items);
        supplement.UpdatedOn = DateTime.UtcNow;

        if (supplement.IsEmpty())
            await tenantCtx.Supplements.AddAsync(supplement);
        else
            tenantCtx.Supplements.Update(supplement);

        await tenantCtx.SaveChangesAsync();

        return Ok(items);
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
        if (!await paymentMethodSvc.MethodExistByIdAsync(id))
            return NotFound();

        var item = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.PaymentServicePolicy)
            .Where(x => x.RowId == id)
            .SingleOrDefaultAsync() ?? Supplement.Build(SupplementTypes.PaymentServicePolicy, id);
        var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Data) ??
                    new Dictionary<string, string>();
        if (LanguageTypes.All.Contains(language ?? string.Empty))
            return Ok(items.Where(x => x.Key == language).ToDictionary(p => p.Key, p => p.Value));

        return Ok(items);
    }

    [HttpPut("union-pay/{partyId:long}/enable-all")]
    public async Task<IActionResult> EnableAllUnionPayForUser(long partyId, [FromBody] M.BatchSwitchSpec spec)
    {
        var existing = await tenantCtx.PaymentServiceAccesses
            .Where(x => x.PartyId == partyId)
            .Where(x => x.PaymentService.CategoryName == "Union Pay")
            .Where(x => spec.IncludeInactivated || x.PaymentService.IsActivated == 1)
            .Where(x => x.FundType == (int)spec.FundType && x.CurrencyId == (int)spec.CurrencyId)
            .ToListAsync();
        tenantCtx.PaymentServiceAccesses.RemoveRange(existing);

        var items = await tenantCtx.PaymentServices
            .Where(x => x.CategoryName == "Union Pay")
            .Where(x => x.CurrencyId == (int)spec.CurrencyId)
            .Where(x => spec.IncludeInactivated || x.IsActivated == 1)
            .ToListAsync();

        var accesses = items.Select(x => new PaymentServiceAccess
        {
            PartyId = partyId,
            FundType = (int)spec.FundType,
            CurrencyId = (int)spec.CurrencyId,
            PaymentServiceId = x.Id,
            CanDeposit = 1,
            CanWithdraw = 0,
        });
        tenantCtx.PaymentServiceAccesses.AddRange(accesses);
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("union-pay/{partyId:long}/disable-all")]
    public async Task<IActionResult> DisableAllUnionPayForUser(long partyId, [FromBody] M.BatchSwitchSpec spec)
    {
        var existing = await tenantCtx.PaymentServiceAccesses
            .Where(x => x.PartyId == partyId)
            .Where(x => x.PaymentService.CategoryName == "Union Pay")
            .Where(x => spec.IncludeInactivated || x.PaymentService.IsActivated == 1)
            .Where(x => x.FundType == (int)spec.FundType && x.CurrencyId == (int)spec.CurrencyId)
            .ToListAsync();
        tenantCtx.PaymentServiceAccesses.RemoveRange(existing);
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("callback-setting")]
    public async Task<ActionResult> CallbackSetting(PaymentService.CallbackSetting spec)
    {
        await cfgSvc.SetAsync(ConfigCategoryTypes.Public, 0, ConfigKeys.PaymentServiceCallbackSetting, spec,
            GetPartyId());
        return NoContent();
    }

    private static void UpdateItem(M item, M.UpdateSpec spec)
    {
        item.CurrencyId = (int)spec.CurrencyId;
        item.MinValue = spec.MinValue;
        item.MaxValue = spec.MaxValue;
        item.InitialValue = spec.InitialValue;
        item.CanWithdraw = ToShort(spec.CanWithdraw);
        item.CanDeposit = ToShort(spec.CanDeposit);
        item.IsActivated = ToShort(spec.IsActivated);
        item.Sequence = spec.Sequence;
        item.Name = Truncate(spec.Name, 32);
        item.CommentCode = Truncate(spec.CommentCode, 6);
        item.CategoryName = Truncate(spec.CategoryName, 32);
        item.Description = Truncate(spec.Description, 1024);
        item.IsHighDollarEnabled = ToShort(spec.IsHighDollarEnabled);
        item.IsAutoDepositEnabled = ToShort(spec.IsAutoDepositEnabled);
    }

    private static short ToShort(bool value) => value ? (short)1 : (short)0;

    private static string Truncate(string value, int maxLength) => value[..Math.Min(value.Length, maxLength)];
}