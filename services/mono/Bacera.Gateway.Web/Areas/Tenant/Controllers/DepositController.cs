using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using static Bacera.Gateway.Comment;
using M = Deposit;
using MSG = ResultMessage.Deposit;

[Tags("Tenant/Deposit")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class DepositController(
    IMediator mediator,
    AccountingService accountingService,
    AcctService acctService,
    TenantDbContext tenantDbContext,
    PaymentMethodService paymentMethodSvc,
    IStorageService storageService,
    ILogger<DepositController> logger)
    : TenantBaseController
{
    /// <summary>
    /// Deposit pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<DepositViewModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        var hideEmail = ShouldHideEmail();
        criteria ??= new M.Criteria();
        return Ok(await accountingService.DepositQueryForTenantAsync(criteria, hideEmail));
    } 

    /// <summary>
    /// Get deposit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await accountingService.DepositGetAsync(id);
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Cancel Deposit by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/cancel")]
    [HttpPut("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(long id)
    {
        var (res, msg) = await acctService.DepositCancelAsync(id, GetPartyId(), "Cancel by tenant");
        return !res ? BadRequest(Result.Error(msg)) : NoContent();
    }

    /// <summary>
    /// Reject Deposit by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("cancel/{id:long}/restore")]
    [HttpPut("reject/{id:long}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelRestore(long id)
    {
        var (res, msg) = await acctService.DepositRestoreToInitialAsync(id, GetPartyId(), "Restore by tenant");
        return !res ? BadRequest(Result.Error(msg)) : NoContent();

        // await _mediator.Publish(new DepositRejectedEvent(item));
        // return NoContent();
    }

    /// <summary>
    /// Try to complete payment for Deposit by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/complete-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompletePayment(long id)
    {
        var (res, msg) = await acctService.DepositCompletePaymentAsync(id, GetPartyId(), "Complete payment by tenant");
        return !res ? BadRequest(Result.Error(msg)) : NoContent();
    }

    /// <summary>
    /// Approve and Complete Deposit by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(long id)
    {
        var (res, msg) = await acctService.DepositCompleteAsync(id, GetPartyId(), "Approve by tenant");
        if (!res) return BadRequest(Result.Error(msg));

        await mediator.Publish(new DepositCompletedEvent(id));
        return Ok(msg);
    }


    [HttpPut("{id:long}/complete")]
    public async Task<IActionResult> Complete(long id)
    {
        var (res, msg) = await acctService.DepositCompleteFromCallbackAsync(id, GetPartyId(), "Complete cb by tenant");
        if (!res) return BadRequest(Result.Error(msg));
        return Ok(msg);
    }

    [HttpPut("{id:long}/delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await tenantDbContext.Deposits
            .Include(x => x.IdNavigation)
            .ThenInclude(x => x.Activities)
            .Include(x => x.Payment)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();

        if (StageTypes.CanDeleteDeposit.Contains(item.IdNavigation.StateId) == false)
        {
            return BadRequest(Result.Error("Deposit at this state cannot be deleted."));
        }

        tenantDbContext.Activities.RemoveRange(item.IdNavigation.Activities);
        tenantDbContext.Deposits.Remove(item);
        tenantDbContext.Payments.Remove(item.Payment);
        tenantDbContext.Matters.Remove(item.IdNavigation);
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }


    /// <summary>
    /// Create Deposit for Tenant
    /// </summary>
    /// <remarks>
    /// 1. Check if the payment service is available for current user
    /// 2. Create Deposit
    /// 3. Create Payment if Request provided correctly
    /// </remarks>
    /// <param name="spec"></param>
    /// <returns>Use PaymentId direct to payment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateByTenantSpec spec)
    {
        // model validation
        var specValidator = new DepositCreateByTenantSpecValidator();
        var specValidationResult = await specValidator.ValidateAsync(spec);
        if (!specValidationResult.IsValid) return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));

        // only check payment service access exits for tenant
        var method = await paymentMethodSvc.GetMethodByIdAsync(spec.PaymentServiceId);
        if (method == null) return BadRequest(Result.Error(MSG.PaymentPlatformNotFound));

        spec.Amount = spec.Amount.ToScaledFromCents();

        var deposit = M.Build(spec.PartyId, spec.FundType, spec.CurrencyId, spec.Amount);

        deposit.IdNavigation
            .SetState(StateTypes.DepositPaymentCompleted)
            .AddActivity(StateTypes.DepositCreated, StateTypes.DepositPaymentCompleted, GetPartyId(),
                $"Deposit created by tenant pid:{GetPartyId()}");

        deposit.Payment = Payment.Build(spec.PartyId, LedgerSideTypes.Credit, method.Id, spec.Amount, spec.CurrencyId);
        deposit.Payment.Status = (int)PaymentStatusTypes.Completed;

        if (spec.TargetTradeAccountUid != 0)
        {
            var tradeAccount = await tenantDbContext.TradeAccounts
                .Include(x => x.IdNavigation)
                .Where(x => x.IdNavigation.PartyId == spec.PartyId)
                .Where(x => x.IdNavigation.Uid == spec.TargetTradeAccountUid)
                .Select(x => new { x.Id, x.IdNavigation.FundType, x.CurrencyId })
                .SingleOrDefaultAsync();

            if (tradeAccount == null) return BadRequest(Result.Error(MSG.TargetTradeAccountNotExists));
            if (tradeAccount.FundType != (int)spec.FundType) return BadRequest(Result.Error(MSG.WalletAndAccountFundTypesNotMatch));
            // if (tradeAccount.CurrencyId != (int)spec.CurrencyId) return BadRequest(Result.Error(MSG.CurrencyNotMatch));

            deposit.TargetAccountId = tradeAccount.Id;
            if (tradeAccount.CurrencyId != method.CurrencyId)
            {
                var fromCurrency = (CurrencyTypes)tradeAccount.CurrencyId;
                var toCurrency = (CurrencyTypes)method.CurrencyId;
                
                // Check if this is a USC conversion that should be handled programmatically
                var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(fromCurrency, toCurrency);
                
                decimal exchangeRate;
                if (isUscConversion)
                {
                    if (uscRate == -1m)
                    {
                        // Multi-step USC conversion needed
                        exchangeRate = await CurrencyConversionHelper.CalculateUscConversionRateAsync(
                            fromCurrency, toCurrency, async (from, to) =>
                            {
                                var dbRate = await tenantDbContext.ExchangeRates
                                    .Where(x => x.FromCurrencyId == (int)from)
                                    .Where(x => x.ToCurrencyId == (int)to)
                                    .OrderByDescending(x => x.Id)
                                    .Select(x => new { x.AdjustRate, x.SellingRate })
                                    .FirstOrDefaultAsync();
                                
                                if (dbRate == null) return -1m;
                                return Math.Ceiling(dbRate.SellingRate * (1 + dbRate.AdjustRate / 100) * 1000) / 1000;
                            });
                        
                        if (exchangeRate == -1m)
                            return BadRequest(Result.Error("Exchange rate not found for USC conversion"));
                    }
                    else
                    {
                        // Direct USC conversion
                        exchangeRate = uscRate;
                    }
                }
                else
                {
                    // Regular exchange rate lookup
                    var rate = await tenantDbContext.ExchangeRates
                        .Where(x => x.FromCurrencyId == tradeAccount.CurrencyId)
                        .Where(x => x.ToCurrencyId == method.CurrencyId)
                        .OrderByDescending(x => x.Id)
                        .Select(x => new { x.AdjustRate, x.SellingRate })
                        .FirstOrDefaultAsync();

                    // Math.Ceiling(exchangeRateEntity.SellingRate * (1 + exchangeRateEntity.AdjustRate / 100) * 1000) / 1000)
                    if (rate == null) return BadRequest(Result.Error("Exchange rate not found"));
                    exchangeRate = Math.Ceiling(rate.SellingRate * (1 + rate.AdjustRate / 100) * 1000) / 1000;
                }
                
                var exchangedAmount = (long)Math.Round(spec.Amount * exchangeRate, 0);
                deposit.Payment.Amount = exchangedAmount;
            }
        }

        // Save deposit first to get the actual ID
        tenantDbContext.Deposits.Add(deposit);
        await tenantDbContext.SaveChangesAsync();

        // Now create supplement with the actual deposit ID
        var depositSupplement = new Supplement
        {
            RowId = deposit.Id,  // ✅ Now this has the actual saved ID
            Type = (int)SupplementTypes.Deposit,
            Data = JsonConvert.SerializeObject(spec.ToSupplement()),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        tenantDbContext.Supplements.Add(depositSupplement);
        await tenantDbContext.SaveChangesAsync();

        await mediator.Publish(new DepositCreatedEvent(deposit));
        return Ok(deposit.ToClientResponseModel());
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

        var deposit = await tenantDbContext.Deposits
            .FirstOrDefaultAsync(x => x.Id == id);
        if (deposit == null)
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
                GetTenantId(), deposit.PartyId);
        }
        catch (Exception e)
        {
            logger.LogError("Upload deposit receipt fail. {Message}", e.Message);
            return Problem();
        }

        await TryExecutePayment(id);

        var supplement = await tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReceipt)
            .Where(x => x.RowId == id)
            .FirstOrDefaultAsync();

        if (supplement == null)
        {
            var data = JsonConvert.SerializeObject(new List<string> { result.Guid });
            supplement = Supplement.Build(SupplementTypes.DepositReceipt, id, data);

            await tenantDbContext.Supplements.AddAsync(supplement);
            await tenantDbContext.SaveChangesAsync();
            return Ok(new List<string> { result.Guid });
        }

        var receipts = JsonConvert.DeserializeObject<List<string>>(supplement.Data) ?? new List<string>();
        receipts.Add(result.Guid);

        supplement.Data = JsonConvert.SerializeObject(receipts);
        tenantDbContext.Supplements.Update(supplement);
        await tenantDbContext.SaveChangesAsync();

        return Ok(receipts);
    }

    /// <summary>
    /// Get receipt for deposit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/receipt")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M.ClientResponseModel>> GetReceipt(long id)
    {
        var supplement = await tenantDbContext.Supplements
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
        var supplement = await tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReference)
            .Where(x => x.RowId == id)
            .FirstOrDefaultAsync();
        var items = JsonConvert.DeserializeObject<object>(supplement?.Data ?? "{}");
        return Ok(items);
    }

    [HttpGet("{id:long}/state-detail")]
    public async Task<ActionResult<M.ClientResponseModel>> GetStateDetails(long id)
    {
        var item = await tenantDbContext.Deposits
            .Where(x => x.Id == id)
            .ToStateDetailModel()
            .SingleOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }

    private async Task TryExecutePayment(long id)
    {
        var deposit = await tenantDbContext.Deposits
            .Include(x => x.Payment)
            .ThenInclude(x => x.PaymentMethod)
            .Where(x => x.Id == id)
            .SingleAsync();

        if (deposit.Payment.Status != (int)PaymentStatusTypes.Pending
            || deposit.Payment.PaymentMethod.Platform != (int)PaymentPlatformTypes.Wire)
            return;

        deposit.Payment.Status = (int)PaymentStatusTypes.Executing;
        deposit.Payment.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.Payments.Update(deposit.Payment);
        await tenantDbContext.SaveChangesAsync();
    }
// /// <summary>
// /// Reject Deposit by Tenant Admin
// /// </summary>
// /// <param name="id"></param>
// /// <returns></returns>
// [HttpPut("reject/{id:long}/restore")]
// [ProducesResponseType(StatusCodes.Status204NoContent)]
// [ProducesResponseType(StatusCodes.Status404NotFound)]
// [ProducesResponseType(StatusCodes.Status400BadRequest)]
// public async Task<IActionResult> RejectRestore(long id)
// {
//     var item = await accountingService.DepositGetAsync(id);
//
//     if (item.IsEmpty())
//         return NotFound();
//
//     var canTransit = await accountingService.CanTransitToStateAsync(item, ActionTypes.DepositTenantRejectRestore,
//         StateTypes.DepositPaymentCompleted, GetPartyId());
//     if (!canTransit)
//         return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));
//
//     var result =
//         await accountingService.DepositRestoreByTenantAsync(item, ActionTypes.DepositTenantRejectRestore,
//             GetPartyId());
//
//     var payment = await tenantDbContext.Deposits
//         .Where(x => x.Id == id)
//         .Select(x => x.Payment)
//         .FirstAsync();
//     payment.Status = (int)PaymentStatusTypes.Executing;
//     payment.UpdatedOn = DateTime.UtcNow;
//     tenantDbContext.Payments.Update(payment);
//     await tenantDbContext.SaveChangesAsync();
//
//     if (!result)
//         return BadRequest(Result.Error(MSG.DepositRestoreFailed));
//
//     // await _mediator.Publish(new DepositRejectedEvent(item));
//     return NoContent();
// }

    /// <summary>
    /// Query ExLink deposit order status
    /// </summary>
    /// <param name="paymentNumber">Merchant order number</param>
    /// <returns>Deposit order status details</returns>
    [HttpGet("exlink/collections/{paymentNumber}/status")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.DepositStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkCollectionOrderStatusForDeposit(string paymentNumber)
    {
        if (string.IsNullOrEmpty(paymentNumber))
            return BadRequest(Result.Error("Merchant order number is required"));

        // Get payment method configuration
        var paymentMethod = await tenantDbContext.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryDepositStatusAsync(
                options.Uid,
                options.SecretKey,
                paymentNumber,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve deposit status from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink deposit status for order {MerchantOrderNo}", paymentNumber);
            return BadRequest(Result.Error($"Failed to query deposit status: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get ExLink collection payment types for a specific currency
    /// </summary>
    /// <param name="currency">Currency code (e.g., VND, THB)</param>
    /// <returns>List of available payment types for deposits</returns>
    [HttpGet("exlink/payment-types")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.PaymentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkCollectionPaymentTypes([FromQuery] string currency)
    {
        if (string.IsNullOrEmpty(currency))
            return BadRequest(Result.Error("Currency is required"));

        // Get payment method configuration
        var paymentMethod = await tenantDbContext.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.MethodType == PaymentMethodTypes.Deposit)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryCollectionPaymentTypesAsync(
                options.Uid,
                options.SecretKey,
                currency,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve payment types from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink collection payment types for currency {Currency}", currency);
            return BadRequest(Result.Error($"Failed to query payment types: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get ExLink supported fiat currencies
    /// </summary>
    /// <returns>List of supported currencies</returns>
    [HttpGet("exlink/currencies")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.SupportedCurrenciesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkSupportedCurrencies()
    {
        // Get payment method configuration
        var paymentMethod = await tenantDbContext.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QuerySupportedCurrenciesAsync(
                options.Uid,
                options.SecretKey,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve supported currencies from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink supported currencies");
            return BadRequest(Result.Error($"Failed to query supported currencies: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get ExLink exchange rates
    /// </summary>
    /// <returns>Exchange rate information for all supported currency pairs</returns>
    [HttpGet("exlink/exchange-rates")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkExchangeRates()
    {
        // Get payment method configuration
        var paymentMethod = await tenantDbContext.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryExchangeRateAsync(
                options.Uid,
                options.SecretKey,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve exchange rates from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink exchange rates");
            return BadRequest(Result.Error($"Failed to query exchange rates: {ex.Message}"));
        }
    }
}
