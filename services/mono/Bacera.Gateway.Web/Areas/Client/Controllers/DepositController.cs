using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Request;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Deposit;
using MSG = ResultMessage.Deposit;

[Tags("Client/Deposit")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class DepositController(
    IMediator mediator,
    IStorageService storageService,
    AcctService acctSvc,
    TenantDbContext tenantCtx,
    AccountingService accountingService,
    ILogger<DepositController> logger)
    : ClientBaseController
{
    /// <summary>
    /// Deposit Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>, M.Criteria>>> Index(
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        return Ok(await accountingService.DepositQueryForClientAsync(GetPartyId(), criteria));
    }

    /// <summary>
    /// Get Deposit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> Get(long id)
    {
        var result = await accountingService.DepositGetForPartyAsync(id, GetPartyId());
        var supplement = await tenantCtx.Supplements.Where(x => x.Type == (int)SupplementTypes.Deposit)
            .Where(x => x.Id == result.Id)
            .FirstOrDefaultAsync();
        if (supplement != null)
            result.Supplement = JsonConvert.DeserializeObject<object>(supplement.Data);

        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get receipt for deposit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/receipt")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> GetReceipt(long id)
    {
        var exists = await tenantCtx.Deposits
            .Where(x => x.PartyId == GetPartyId())
            .AnyAsync(x => x.Id == id);
        if (!exists)
            return NotFound();

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReceipt)
            .Where(x => x.RowId == id)
            .FirstOrDefaultAsync();
        var items = JsonConvert.DeserializeObject<List<string>>(supplement?.Data ?? "[]");
        return Ok(items);
    }

    /// <summary>
    /// Get Deposit reference
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/reference")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> GetReference(long id)
    {
        var exists = await tenantCtx.Deposits
            .Where(x => x.PartyId == GetPartyId())
            .AnyAsync(x => x.Id == id);
        if (!exists)
            return NotFound();

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReference)
            .Where(x => x.RowId == id)
            .FirstOrDefaultAsync();
        var items = JsonConvert.DeserializeObject<object>(supplement?.Data ?? "{}");
        return Ok(items);
    }

    /// <summary>
    /// Upload receipts
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/receipt")]
    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M.ClientResponseModel>> Upload(long id, IFormFile file)
    {
        if (file.Length < 1)
            return BadRequest();
        var partyId = GetPartyId();
        var exists = await tenantCtx.Deposits
            .Where(x => x.PartyId == partyId)
            .AnyAsync(x => x.Id == id);
        if (!exists)
            return NotFound();

        const string type = "DepositReceipt";

        Medium result;
        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            result = await storageService.UploadFileAndSaveMediaAsync(
                memoryStream, Guid.NewGuid().ToString(),
                Path.GetExtension(file.FileName).ToLower(),
                type, id, file.ContentType,
                GetTenantId(), GetPartyId());
        }
        catch (Exception e)
        {
            logger.LogError("Upload deposit receipt fail. {Message}", e.Message);
            return Problem();
        }

        await TryExecutePayment(id);

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReceipt)
            .Where(x => x.RowId == id)
            .FirstOrDefaultAsync();

        if (supplement == null)
        {
            var data = JsonConvert.SerializeObject(new List<string> { result.Guid });
            supplement = Supplement.Build(SupplementTypes.DepositReceipt, id, data);

            await tenantCtx.Supplements.AddAsync(supplement);
            await tenantCtx.SaveChangesAsync();
            return Ok(new List<string> { result.Guid });
        }

        var receipts = JsonConvert.DeserializeObject<List<string>>(supplement.Data) ?? new List<string>();
        receipts.Add(result.Guid);

        supplement.Data = JsonConvert.SerializeObject(receipts);
        tenantCtx.Supplements.Update(supplement);
        await tenantCtx.SaveChangesAsync();

        return Ok(receipts);
    }



    /// <summary>
    /// Update pending deposit
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<M.ClientResponseModel>> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var pid = GetPartyId();
        var item = await accountingService.DepositGetForPartyAsync(id, pid);
        if (item.IsEmpty()) return NotFound();

        if (item.StateId != (int)StateTypes.DepositCreated
            || item.PaymentStateId != (int)PaymentStatusTypes.Pending)
        {
            return BadRequest(Result.Error(MSG.InvalidParameters));
        }

        var result = await accountingService.DepositUpdateAsync(id, spec, pid);
        return result ? NoContent() : BadRequest(Result.Error(MSG.CannotCancel));
    }

    /// <summary>
    /// Get Deposit exchange rate
    /// </summary>
    /// <param name="fromCurrencyId"></param>
    /// <param name="toCurrencyId"></param>
    /// <returns></returns>
    [HttpGet("{fromCurrencyId:int}/to/{toCurrencyId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M.ClientResponseModel>> ExchangeRate(int fromCurrencyId, int toCurrencyId)
    {
        var (status, rate) = await GetExchangeRate((CurrencyTypes)fromCurrencyId, (CurrencyTypes)toCurrencyId);
        if (!status)
            return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
        return Ok(new { rate });
    }

    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> Cancel(long id)
    {
        var pid = GetPartyId();
        var valid = await tenantCtx.Deposits.AnyAsync(x => x.Id == id && x.PartyId == pid);
        if (!valid) return NotFound();
        
        var (result, msg) = await acctSvc.DepositCancelAsync(id, pid);
        return result ? NoContent() : BadRequest(Result.Error(msg));
    }

    private async Task TryExecutePayment(long id)
    {
        var deposit = await tenantCtx.Deposits
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment)
            .ThenInclude(x => x.PaymentMethod)
            .Where(x => x.Id == id)
            .SingleAsync();
        if (deposit.Payment.Status != (int)PaymentStatusTypes.Pending
            || deposit.Payment.PaymentMethod.Platform != (int)PaymentPlatformTypes.Wire)
            return;
        deposit.Payment.Status = (int)PaymentStatusTypes.Executing;
        deposit.Payment.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Payments.Update(deposit.Payment);
        await tenantCtx.SaveChangesAsync();
        await mediator.Publish(new PaymentExecutedEvent(deposit.PaymentId, deposit.IdNavigation));
    }

    private async Task<Tuple<bool, decimal>> GetExchangeRate(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return Tuple.Create(true, 1m);
        var exchangeRateEntity = await accountingService.GetExchangeRateAsync(from, to);
        return exchangeRateEntity == null
            ? Tuple.Create(false, 0m)
            : Tuple.Create(true,
                Math.Ceiling(exchangeRateEntity.SellingRate * (1 + exchangeRateEntity.AdjustRate / 100) * 1000) / 1000);
    }
}


// public async Task<IActionResult> CreateUnionPay([FromBody] DepositCreateRequestModel spec)
// {
//     var specValidator = new DepositCreateRequestModelValidator();
//     var specValidationResult = await specValidator.ValidateAsync(spec);
//     if (!specValidationResult.IsValid)
//         return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));
//
//     var partyId = GetPartyId();
//
//     if (await _tenantDbContext.Deposits
//             .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCreated)
//             .Where(x => x.PartyId == partyId)
//             .CountAsync() >= MaxIncompleteDeposit
//        )
//         return BadRequest(Result.Error(MSG.MaxIncompleteDepositReached));
//     
//     var paymentService = await _tenantDbContext.PaymentServiceAccesses
//         .Where(x => x.PartyId == partyId)
//         .Where(x => x.CanDeposit == 1)
//         .Where(x => x.FundType == (int)spec.FundType)
//         .Where(x => x.CurrencyId == (int)spec.CurrencyId)
//         .Where(x => x.PaymentServiceId == spec.PaymentServiceId)
//         .Where(x => x.PaymentService.IsActivated == 1)
//         .Where(x => x.PaymentService.CanDeposit == 1)
//         .Select(x => x.PaymentService)
//         .SingleOrDefaultAsync();
//     if (paymentService == null)
//         return BadRequest(Result.Error(MSG.TheSpecifiedServiceDoesNotHavePermissionForAccount));
// }

// /// <summary>
// /// Create Deposit
// /// </summary>
// /// <remarks>
// /// 1. Check if the payment service is available for current user
// /// 2. Create Deposit
// /// 3. Create Payment if Request provided correctly
// /// </remarks>
// /// <param name="spec"></param>
// /// <returns>Use PaymentId direct to payment</returns>
// [HttpPost]
// [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
// [ProducesResponseType(StatusCodes.Status400BadRequest)]
// public async Task<IActionResult> Create([FromBody] DepositCreateRequestModel spec)
// {
//     // model validation
//     var specValidator = new DepositCreateRequestModelValidator();
//     var specValidationResult = await specValidator.ValidateAsync(spec);
//     if (!specValidationResult.IsValid)
//         return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));
//
//     var partyId = GetPartyId();
//
//     if (await tenantCtx.Deposits
//             .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCreated)
//             .Where(x => x.PartyId == partyId)
//             .CountAsync() >= MaxIncompleteDeposit
//        )
//         return BadRequest(Result.Error(MSG.MaxIncompleteDepositReached));
//
//     // check User payment service access
//     var paymentService = await tenantCtx.PaymentServiceAccesses
//         .Where(x => x.PartyId == partyId)
//         // .Where(x => x.CanDeposit == 1)
//         .Where(x => x.FundType == (int)spec.FundType)
//         .Where(x => x.CurrencyId == (int)spec.CurrencyId)
//         .Where(x => x.PaymentServiceId == spec.PaymentServiceId)
//         // .Where(x => x.PaymentService.IsActivated == 1)
//         // .Where(x => x.PaymentService.CanDeposit == 1)
//         .Select(x => x.PaymentService)
//         .SingleOrDefaultAsync();
//     if (paymentService == null)
//         return BadRequest(Result.Error(MSG.TheSpecifiedServiceDoesNotHavePermissionForAccount));
//
//     // check amount by payment service settings
//     var isInitialDeposit = !await tenantCtx.Deposits.AnyAsync(x => x.TargetAccount != null &&
//                                                                    x.TargetAccount.Uid ==
//                                                                    spec.TargetTradeAccountUid);
//
//     var amountInDecimal = (spec.Amount / 100m);
//
//     if (isInitialDeposit && amountInDecimal < paymentService.InitialValue)
//         return BadRequest(Result.Error(MSG.InitialDepositAmountNotMatch));
//     if (!isInitialDeposit && amountInDecimal < paymentService.MinValue)
//         return BadRequest(Result.Error(MSG.DepositAmountLessThanMinValue));
//     if (amountInDecimal > paymentService.MaxValue)
//         return BadRequest(Result.Error(MSG.DepositAmountMoreThanMaxValue));
//
//     // check if target trade account Id valid
//     var targetTradeAccountId = 0L;
//     var account = new Account();
//     if (spec.TargetTradeAccountUid != 0)
//     {
//         account = await tenantCtx.Accounts
//             .Where(x => x.Uid == spec.TargetTradeAccountUid)
//             .Where(x => x.PartyId == partyId)
//             .SingleOrDefaultAsync();
//
//         if (account == null)
//             return BadRequest(Result.Error(MSG.TargetTradeAccountNotExists));
//
//         if (account.FundType != (int)spec.FundType)
//             return BadRequest(Result.Error(MSG.WalletAndAccountFundTypesNotMatch));
//
//         if (account.CurrencyId != (int)spec.CurrencyId)
//             return BadRequest(Result.Error(MSG.CurrencyNotMatch));
//
//         targetTradeAccountId = account.Id;
//     }
//
//     var supplement = spec.ToSupplement();
//     switch (paymentService.Platform)
//     {
//         // process payment
//         case (int)PaymentPlatformTypes.Wire:
//             return await ProcessWirePayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account, paymentService, supplement);
//         case (int)PaymentPlatformTypes.Manual:
//             return await ProcessManualPayment(partyId, spec, targetTradeAccountId, supplement);
//         case (int)PaymentPlatformTypes.Help2Pay:
//             return await ProcessHelp2PayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account, paymentService, supplement);
//         case (int)PaymentPlatformTypes.Poli:
//             return await ProcessPoliPayment(partyId, spec, (CurrencyTypes)paymentService.CurrencyId,
//                 targetTradeAccountId, supplement);
//         case (int)PaymentPlatformTypes.Ocex:
//             return await ProcessOcexPayment(partyId, spec, targetTradeAccountId, supplement);
//         case (int)PaymentPlatformTypes.ChipPay:
//             return await ProcessChipPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.ExLink:
//             return await ProcessExLinkPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.GPay:
//             return await ProcessGPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.BipiPay:
//             return await ProcessBipiPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account, paymentService, supplement);
//         case (int)PaymentPlatformTypes.Buy365:
//             return await ProcessBuy365Payment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.UnionePayX:
//             return await ProcessUnionePayXPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.Long77Pay:
//             return await ProcessLong77PayPayment(partyId, (CurrencyTypes)paymentService.CurrencyId, spec,
//                 targetTradeAccountId, supplement);
//         case (int)PaymentPlatformTypes.Long77PayUsdt:
//             return await ProcessLong77PayUsdtPayment(partyId, (CurrencyTypes)paymentService.CurrencyId, spec,
//                 targetTradeAccountId, supplement);
//         case (int)PaymentPlatformTypes.BigPay:
//             return await ProcessBigPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.UsePay:
//             return await ProcessUsePayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.FivePayF2F:
//             return await ProcessFivePayF2FPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.FivePayVA:
//             return await ProcessFivePayVAPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.UEnjoy:
//             return await ProcessUEnjoyPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.PaymentAsiaRMB:
//             return await ProcessPaymentAsiaPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.DragonPay:
//             return await ProcessDragonPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.Bakong:
//             return await ProcessBakongPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.OFAPay:
//             return await ProcessOFAPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.Monetix:
//             return await ProcessMonetixPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         case (int)PaymentPlatformTypes.Crypto:
//             return await ProcessCryptoPayPayment(partyId, spec.Amount, spec.CurrencyId, spec.FundType, account,
//                 paymentService, supplement);
//         default:
//             return BadRequest(MSG.PaymentPlatformNotFound);
//     }
//
//     // var result = await createDepositAsync(partyId, spec, targetTradeAccountId, supplement);
//     // if (result.IsEmpty())
//     //     return BadRequest(Result.Error(MSG.DepositCreateFailed));
//     //
//     // await _mediator.Publish(new DepositCreatedEvent(result));
//     //
//     // return Ok(result.ToClientResponseModel());
// }

// [HttpPut("{id:long}/cancel")]
// [ProducesResponseType(StatusCodes.Status204NoContent)]
// [ProducesResponseType(StatusCodes.Status404NotFound)]
// public async Task<ActionResult<M.ClientResponseModel>> Cancel(long id)
// {
//     var pid = GetPartyId();
//     var item = await tenantCtx.Deposits
//         .Where(x => x.PartyId == pid)
//         .Where(x => x.Id == id).SingleOrDefaultAsync();
//     if (item == null) return NotFound();
//
//     var result = await accountingService.DepositTryCancelAsync(id, pid);
//     if (!result)
//         return Problem(statusCode: 503, detail: MSG.CannotCancel);
//
//     await mediator.Publish(new DepositCanceledEvent(id));
//     return NoContent();
// }