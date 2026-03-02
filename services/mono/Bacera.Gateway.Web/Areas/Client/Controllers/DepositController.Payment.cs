using OpenIddict.Validation.AspNetCore;
// using Bacera.Gateway.Auth;
// using Bacera.Gateway.Core.Types;
// using Bacera.Gateway.DTO;
// using Bacera.Gateway.Services;
// using Bacera.Gateway.Services.Models;
// using Bacera.Gateway.Vendor;
// using Bacera.Gateway.Vendor.Bakong;
// using Bacera.Gateway.Vendor.BigPay;
// using Bacera.Gateway.Vendor.BigPay.Models;
// using Bacera.Gateway.Vendor.BipiPay;
// using Bacera.Gateway.Vendor.Buy365;
// using Bacera.Gateway.Vendor.ChipPay;
// using Bacera.Gateway.Vendor.ExLink.Models;
// using Bacera.Gateway.Vendor.FivePayF2F;
// using Bacera.Gateway.Vendor.FivePayVA;
// using Bacera.Gateway.Vendor.GPay;
// using Bacera.Gateway.Vendor.Help2Pay;
// using Bacera.Gateway.Vendor.Help2Pay.Models;
// using Bacera.Gateway.Vendor.Long77Pay;
// using Bacera.Gateway.Vendor.Monetix;
// using Bacera.Gateway.Vendor.MonetixPay;
// using Bacera.Gateway.Vendor.Ocex.Models;
// using Bacera.Gateway.Vendor.OFAPay;
// using Bacera.Gateway.Vendor.PaymentAsia;
// using Bacera.Gateway.Vendor.Poli.Models;
// using Bacera.Gateway.Vendor.UnionePay;
// using Bacera.Gateway.Vendor.UniotcPay;
// using Bacera.Gateway.Vendor.UsePay;
// using Bacera.Gateway.Web.EventHandlers;
// using Bacera.Gateway.Web.Request;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
//
// namespace Bacera.Gateway.Web.Areas.Client.Controllers;
//
// using M = Deposit;
// using MSG = ResultMessage.Deposit;
//
// [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
//     Roles = UserRoleTypesString.ClientOrTenantAdmin)]
// partial class DepositController
// {
//     private async Task<IActionResult> ProcessWirePayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId, (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         // Executing payment 
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId, Payment.GenerateNumber());
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId, fundType, targetAccount.Id, supplement);
//
//         var response = new DepositCreatedResponseModel
//         {
//             IsSuccess = true,
//             Action = PaymentResponseActionTypes.None,
//             PaymentNumber = payment.Number
//         };
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessManualPayment(long partyId, DepositCreateRequestModel spec,
//         long targetTradeAccountId, Supplement.DepositSupplement supplement)
//     {
//         if (spec.Request == null)
//             return BadRequest(Result.Error(MSG.InvalidParameters));
//         ManualPaymentRequestModel request = ManualPaymentRequestModel.FromDynamic(spec.Request);
//         var validator = new ManualPaymentModelValidator();
//         var validationResult = await validator.ValidateAsync(request);
//         if (validationResult.IsValid == false)
//             return BadRequest(Result.Error(MSG.InvalidParameters, validationResult));
//
//         var depositResult = await CreateDepositWithPaymentAsync(partyId, spec, targetTradeAccountId, supplement);
//
//         var paymentResult = await paymentProxyService
//             .ProcessPaymentAsync(depositResult.PaymentId, request);
//         if (!paymentResult.IsSuccess)
//             return BadRequest(Result.Error(MSG.InvalidParameters, paymentResult));
//
//         await SaveReferenceSupplementAsync(depositResult.Id, paymentResult);
//
//         return Ok(paymentResult);
//     }
//
//     private async Task<IActionResult> ProcessPoliPayment(long partyId,
//         DepositCreateRequestModel spec, CurrencyTypes paymentServiceCurrencyId,
//         long targetTradeAccountId, Supplement.DepositSupplement supplement)
//     {
//         if (spec.Request == null)
//             return BadRequest(Result.Error(MSG.InvalidParameters, new { Message = "Request is null" }));
//
//         PoliRequestModel request = PoliRequestModel.FromDynamic(supplement);
//         var validator = new PoliRequestModelValidator();
//         var validationResult = await validator.ValidateAsync(request);
//         if (validationResult.IsValid == false)
//             return BadRequest(Result.Error(MSG.InvalidParameters, validationResult));
//
//         var (exchangeRateResult, exchangeRate) =
//             await GetExchangeRate(spec.CurrencyId, paymentServiceCurrencyId);
//         if (!exchangeRateResult)
//             return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         // create payment
//         var amount = (long)Math.Round(spec.Amount * exchangeRate, 0);
//         var payment = await accountingService.PaymentCreateAsync(partyId, spec.PaymentServiceId, paymentServiceCurrencyId,
//             amount, LedgerSideTypes.Debit, "Prepare for Deposit");
//
//         // Executing payment
//         request.Amount = (int)Math.Round(payment.Amount / 100m, 0);
//         request.CurrencyCode = Enum.GetName(typeof(CurrencyTypes), paymentServiceCurrencyId) ?? "AUD";
//         request.MerchantReference = payment.Number;
//
//         var paymentResult = await paymentProxyService
//             .ProcessPaymentAsync(payment.Id, request);
//         if (!paymentResult.IsSuccess)
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, paymentResult));
//
//         // create deposit
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, spec, targetTradeAccountId, supplement);
//         await SaveReferenceSupplementAsync(depositResult.Id, paymentResult);
//
//         // return the payment request object for client to redirect
//         return Ok(paymentResult);
//     }
//
//     private async Task<IActionResult> ProcessOcexPayment(long partyId, DepositCreateRequestModel spec,
//         long targetTradeAccountId, Supplement.DepositSupplement supplement)
//     {
//         var tenantId = GetTenantId();
//         var user = await authDbContext.Users.FirstOrDefaultAsync(u => u.PartyId == partyId && u.TenantId == tenantId);
//         if (user == null)
//             return BadRequest(Result.Error(ResultMessage.Common.UserNotFound));
//
//         // create payment
//         var amount = spec.Amount;
//         var payment = await accountingService.PaymentCreateAsync(partyId, spec.PaymentServiceId, spec.CurrencyId,
//             amount, LedgerSideTypes.Debit, "Prepare for Deposit");
//
//         // Executing payment
//         var request = OcexRequestModel.Build(user.Email ?? string.Empty, user.Uid, payment.Number);
//         var paymentResult = await paymentProxyService
//             .ProcessPaymentAsync(payment.Id, request);
//         if (!paymentResult.IsSuccess)
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, paymentResult));
//
//         var depositResult =
//             await CreateDepositAsync(partyId, payment.Id, spec, targetTradeAccountId, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, paymentResult);
//         // return the payment request object for client to redirect
//         return Ok(paymentResult);
//     }
//
//     private async Task<IActionResult> ProcessBipiPayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         // var user = await _tenantDbContext.Parties
//         //     .Where(x => x.Id == partyId)
//         //     .Select(x => new { x.NativeName })
//         //     .FirstOrDefaultAsync();
//
//         var option = BipiPayOptions.FromJson(paymentSvc.Configuration);
//         var request = new BipiPay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             UsdAmount = 0,
//             PaymentNumber = Payment.GenerateNumber(),
//             AccountUid = targetAccount.Uid,
//             ReturnUrl = supplement.Request.returnUrl ?? string.Empty,
//             CurrencyId = supplement.Request.currency ?? CurrencyTypes.CNY,
//             Options = option
//         };
//
//         var response = await request.RequestAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessLong77PayPayment(long partyId, CurrencyTypes paymentServiceCurrencyId,
//         DepositCreateRequestModel spec,
//         long targetTradeAccountId, Supplement.DepositSupplement supplement)
//     {
//         var (exchangeRateResult, exchangeRate) =
//             await GetExchangeRate(spec.CurrencyId, paymentServiceCurrencyId);
//         if (!exchangeRateResult)
//             return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         // create payment
//         var amount = RoundUp(spec.Amount * exchangeRate);
//         var payment = await accountingService.PaymentCreateAsync(partyId, spec.PaymentServiceId, paymentServiceCurrencyId,
//             amount, LedgerSideTypes.Debit, "Prepare for Deposit");
//
//         // Executing payment    
//         var request = Long77PayRequestModel.FromDynamic(supplement);
//         request.Amount = RoundUp(payment.Amount / 100m);
//         request.PaymentNumber = payment.Number;
//
//         var paymentResult = await paymentProxyService
//             .ProcessPaymentAsync(payment.Id, request);
//         if (!paymentResult.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, paymentResult);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{payment.PaymentMethod.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, paymentResult));
//         }
//
//         var depositResult =
//             await CreateDepositAsync(partyId, payment.Id, spec, targetTradeAccountId, supplement);
//         await SaveReferenceSupplementAsync(depositResult.Id, paymentResult);
//
//         return Ok(paymentResult);
//     }
//
//     private async Task<IActionResult> ProcessLong77PayUsdtPayment(long partyId, CurrencyTypes paymentServiceCurrencyId,
//         DepositCreateRequestModel spec,
//         long targetTradeAccountId, Supplement.DepositSupplement supplement)
//     {
//         var (exchangeRateResult, exchangeRate) =
//             await GetExchangeRate(spec.CurrencyId, paymentServiceCurrencyId);
//         if (!exchangeRateResult)
//             return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         // create payment
//         var amount = RoundUp(spec.Amount * exchangeRate);
//         var payment = await accountingService.PaymentCreateAsync(partyId, spec.PaymentServiceId, paymentServiceCurrencyId,
//             amount, LedgerSideTypes.Debit, "Prepare for Deposit");
//
//         // Executing payment
//         var request = Long77PayUsdtRequestModel.FromDynamic(supplement);
//         request.Amount = RoundUp(payment.Amount / 100m);
//         request.PaymentNumber = payment.Number;
//         request.UserId = partyId;
//
//         var paymentResult = await paymentProxyService
//             .ProcessPaymentAsync(payment.Id, request);
//         if (!paymentResult.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, paymentResult);
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, paymentResult));
//         }
//
//         var depositResult =
//             await CreateDepositAsync(partyId, payment.Id, spec, targetTradeAccountId, supplement);
//         await SaveReferenceSupplementAsync(depositResult.Id, paymentResult);
//
//         return Ok(paymentResult);
//     }
//
//     private async Task<long> GetExchangedAmountAsync(long amount, CurrencyTypes currencyId,
//         CurrencyTypes targetCurrencyId)
//     {
//         var (exchangeRateResult, exchangeRate) = await GetExchangeRate(currencyId, targetCurrencyId);
//         if (!exchangeRateResult)
//             return -1;
//         return (long)Math.Round(amount * exchangeRate, 0);
//     }
//
//     private async Task<Payment> CreatePaymentAsync(long partyId, long paymentServiceId, long amount,
//         CurrencyTypes currencyId, string number, string? referenceNumber = null, string note = "Prepare for Deposit")
//     {
//         var payment = Payment.BuildForDeposit(partyId, paymentServiceId, amount, currencyId, number, referenceNumber);
//         await tenantCtx.Payments.AddAsync(payment);
//         await tenantCtx.SaveChangesAsync();
//
//         var supplementObj = new Supplement.PaymentSupplement { Note = note };
//         supplementObj.AddActivity("Payment created");
//         await SetPaymentNumberToTenantIdAsync(payment.Number);
//         if (referenceNumber != null) await SetPaymentNumberToTenantIdAsync(payment.ReferenceNumber);
//
//         var supplementEntity = new Supplement
//         {
//             Type = (short)SupplementTypes.Payment,
//             RowId = payment.Id,
//             Data = Utils.JsonSerializeObject(supplementObj),
//         };
//         await tenantCtx.Supplements.AddAsync(supplementEntity);
//         await tenantCtx.SaveChangesAsync();
//         return payment;
//     }
//
//
//     private async Task<IActionResult> ProcessBigPayPayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         // Executing payment 
//         var option = BigPayOptions.FromJson(paymentSvc.Configuration);
//         var request = new BigPay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             AccountId = targetAccount.Id,
//             ReturnUrl = supplement.Request.ReturnUrl ?? string.Empty,
//             CancelUrl = supplement.Request.CancelUrl ?? supplement.Request.ReturnUrl ?? string.Empty,
//             Options = option
//         };
//
//         var response = await request.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessUnionePayXPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = UnionePayOptions.FromJson(paymentSvc.Configuration);
//         var user = await tenantCtx.Parties
//             .Where(x => x.Id == partyId)
//             .Select(x => new { x.NativeName, x.IdNumber })
//             .FirstOrDefaultAsync();
//
//         var request = new UnionePay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             Options = options,
//             IdNumber = user?.IdNumber ?? "",
//             NativeName = user?.NativeName ?? ""
//         };
//         var response = await request.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return Ok(response);
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessChipPayPayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var option = ChipPayOptions.FromJson(paymentSvc.Configuration);
//         var request = new ChipPay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             CurrencyId = (CurrencyTypes)paymentSvc.CurrencyId,
//             Options = option
//         };
//         var response = await request.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber, response.ReferenceNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//
//     private async Task<IActionResult> ProcessExLinkPayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = ExLinkOptions.FromJson(paymentSvc.Configuration);
//         if (!options.IsValid())
//         {
//             return BadRequest(Result.Error("__PAYMENT_SERVICE_INVALID__"));
//         }
//
//         var user = await userSvc.GetPartyAsync(partyId);
//         var request = new ExLink.RequestClient
//         {
//             UniqueCode = targetAccount.Uid.ToString(),
//             PaymentNumber = Payment.GenerateNumber(),
//             Amount = RoundUp(exchangedAmount / 100m),
//             PayerName = user.GuessNativeName(),
//             Options = options
//         };
//
//         var response = await request.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//
//     private async Task<IActionResult> ProcessBuy365Payment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var tenantId = GetTenantId();
//         var user = await authDbContext.Users.SingleAsync(u => u.PartyId == partyId && u.TenantId == tenantId);
//         // Executing payment 
//         var options = Buy365Options.FromJson(paymentSvc.Configuration);
//         var request = new Buy365.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             NativeName = user.GuessUserNativeName(),
//             AccountUid = targetAccount.Uid,
//             Options = options,
//             Logger = logger
//         };
//
//         var response = await request.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessUsePayPayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var tenantId = GetTenantId();
//         // Executing payment 
//         var options = UsePayOptions.FromJson(paymentSvc.Configuration);
//         var request = new UsePay.RequestClient
//         {
//             TenantId = tenantId,
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             Options = options,
//             ReturnUrl = supplement.Request.returnUrl ?? string.Empty,
//             Logger = logger
//         };
//
//         var response = await request.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", request, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             request.PaymentNumber);
//
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessFivePayF2FPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         CurrencyTypes targetCurrency = supplement.Request.currency ?? currencyId;
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, (CurrencyTypes)targetAccount.CurrencyId, targetCurrency);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = FivePayF2FOptions.FromJson(paymentSvc.Configuration);
//         var client = new FivePayF2F.RequestClient
//         {
//             TenantId = GetTenantId(),
//             AccountUid = targetAccount.Uid,
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             ReturnUrl = supplement.Request.ReturnUrl ?? string.Empty,
//             CurrencyId = targetCurrency,
//             Options = options,
//             Logger = logger
//         };
//         var response = await client.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber, client.GetEncryptPaymentNumber());
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessUEnjoyPayment(long partyId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         var kycInfos = UEnjoy.KycInfos.FromDynamic(supplement.Request);
//         if (!kycInfos.IsValid()) return BadRequest(Result.Error("__NAME_AND_PHONE_NUMBER_REQUIRED__"));
//
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var option = UEnjoyOptions.FromJson(paymentSvc.Configuration);
//         var client = new UEnjoy.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             Logger = logger,
//             KycName = kycInfos.NativeName,
//             UserAreaCode = kycInfos.UserAreaCode,
//             UserMobile = kycInfos.UserMobile,
//             Options = option,
//             ReturnUrl = supplement.Request.ReturnUrl ?? supplement.Request.returnUrl ?? string.Empty,
//         };
//
//         var response = await client.SendAsync();
//         if (!response.IsSuccess)
//         {
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(response.Message));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessFivePayVAPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId,
//         FundTypes fundType, Account targetAccount, PaymentService paymentSvc, Supplement.DepositSupplement supplement)
//     {
//         if (supplement.Request.returnUrl == null)
//             return BadRequest(Result.Error(MSG.InvalidParameters, new { Message = "returnUrl is null" }));
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = FivePayVAOptions.FromJson(paymentSvc.Configuration);
//         var client = new FivePayVA.RequestClient
//         {
//             TenantId = GetTenantId(),
//             AccountUid = targetAccount.Uid,
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             ReturnUrl = supplement.Request.returnUrl ?? string.Empty,
//             // CurrencyId = supplement.Request.currency ?? string.Empty,
//             Options = options,
//             Logger = logger
//         };
//         var response = await client.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber, client.GetEncryptPaymentNumber());
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessGPayPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         var nativeName = supplement.Request.nativeName?.ToString() as string;
//         if (string.IsNullOrWhiteSpace(nativeName))
//             nativeName = await GetChinaUserKycName(partyId);
//
//         if (string.IsNullOrWhiteSpace(nativeName))
//             return BadRequest(Result.Error("__NATIVE_NAME_REQUIRED__"));
//
//         if (supplement.Request.returnUrl == null)
//             return BadRequest(Result.Error(MSG.InvalidParameters, new { Message = "returnUrl is null" }));
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = GPayOptions.FromJson(paymentSvc.Configuration);
//         var client = new GPay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             ReturnUrl = supplement.Request.ReturnUrl ?? string.Empty,
//             Ip = GetRemoteIpAddress(),
//             NativeName = nativeName,
//             Options = options,
//             Logger = logger,
//         };
//
//         var response = await client.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber, response.ReferenceNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessPaymentAsiaPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         var request = supplement.Request;
//         var (res, msg) = PaymentAsiaRMB.EnsureRequest((object)request);
//         if (!res) return BadRequest(Result.Error(msg));
//
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var tenantId = GetTenantId();
//         var user = await authDbContext.Users
//             .Where(x => x.PartyId == partyId && x.TenantId == tenantId)
//             .SingleAsync();
//
//         var options = PaymentAsiaRMBOptions.FromJson(paymentSvc.Configuration);
//         var client = new PaymentAsiaRMB.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             Language = user.Language,
//             Email = user.Email ?? string.Empty,
//             ReturnUrl = request.returnSuccessUrl,
//             ReturnFailUrl = request.returnFailUrl,
//             NativeName = user.GuessUserNativeName(),
//             RmbOptions = options,
//             Logger = logger,
//         };
//
//         var response = await client.SendAsync();
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessDragonPayPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         var request = supplement.Request;
//         var (res, msg) = DragonPayPHP.EnsureRequest((object)request);
//         if (!res) return BadRequest(Result.Error(msg));
//
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var tenantId = GetTenantId();
//         var options = DragonPayPHPOptions.FromJson(paymentSvc.Configuration);
//         var user = await authDbContext.Users
//             .Where(x => x.PartyId == partyId && x.TenantId == tenantId)
//             .Select(x => new { x.FirstName, x.LastName, x.PhoneNumber, x.Email })
//             .SingleAsync();
//
//         var client = new DragonPayPHP.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             Email = user.Email!,
//             FirstName = user.FirstName,
//             LastName = user.LastName,
//             Phone = user.PhoneNumber ?? string.Empty,
//             IpAddress = GetRemoteIpAddress(),
//             ReturnUrl = request.returnSuccessUrl,
//             Options = options,
//             Logger = logger,
//         };
//
//         var response = await client.SendAsync();
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessBakongPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         var request = supplement.Request;
//         if (supplement.Request.currency == null) return BadRequest(Result.Error("__CURRENCY_REQUIRED__"));
//         var selectedCurrencyId = (CurrencyTypes)supplement.Request.currency.Value;
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, selectedCurrencyId, CurrencyTypes.USD);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = BakongOptions.FromJson(paymentSvc.Configuration);
//         var user = await tenantCtx.Parties
//             .Where(x => x.Id == partyId)
//             .Select(x => new { x.FirstName, x.LastName, x.PhoneNumber, x.Email })
//             .SingleAsync();
//
//         var client = new Bakong.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PaymentNumber = Payment.GenerateNumber(),
//             Email = user.Email!,
//             FirstName = user.FirstName,
//             LastName = user.LastName,
//             CurrencyId = selectedCurrencyId,
//             Phone = user.PhoneNumber ?? string.Empty,
//             IpAddress = GetRemoteIpAddress(),
//             ReturnUrl = request.returnUrl,
//             Options = options,
//             Logger = logger,
//         };
//
//         var response = await client.RequestAsync();
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessCryptoPayPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var crypto = await cryptoSvc.GetRandomCryptoForPaymentAsync();
//         if (crypto == null) return BadRequest(Result.Error("__CRYPTO_SERVICE_NOT_AVAILABLE__"));
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId, Payment.GenerateNumber(), crypto.Name);
//
//         crypto.InUsePaymentId = payment.Id;
//         crypto.Status = (int)CryptoStatusTypes.InUse;
//         crypto.UpdatedOn = DateTime.UtcNow;
//         tenantCtx.Cryptos.Update(crypto);
//         await tenantCtx.SaveChangesAsync();
//
//         // var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//         //     client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, true);
//
//         var setting = await cfgSvc.GetAsync<Crypto.Setting>(nameof(Public), 0, ConfigKeys.CryptoSetting);
//
//         var response = new DepositCreatedResponseModel
//         {
//             IsSuccess = true,
//             Message = setting!.PayExpiredTimeInMinutes.ToString(),
//             PaymentNumber = payment.Number,
//             TextForQrCode = crypto.Address,
//             Action = PaymentResponseActionTypes.QrCode,
//         };
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessOFAPayPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId,
//             (CurrencyTypes)paymentSvc.CurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = OFAPayOptions.FromJson(paymentSvc.Configuration);
//         var user = await userSvc.GetPartyAsync(partyId);
//
//         var client = new OFAPay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             PartyUid = user.Uid,
//             PaymentNumber = Payment.GenerateNumber(),
//             ReturnUrl = supplement.Request.returnUrl ?? string.Empty,
//             NativeName = user.GuessNativeName(),
//             Options = options,
//             Logger = logger,
//         };
//
//         var response = await client.RequestAsync();
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private async Task<IActionResult> ProcessMonetixPayPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         CurrencyTypes targetCurrencyId = supplement.Request.currency;
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId, targetCurrencyId);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = MonetixOptions.FromJson(paymentSvc.Configuration);
//         var user = await userSvc.GetPartyAsync(partyId);
//
//         var client = new Monetix.RequestClient
//         {
//             Amount = exchangedAmount,
//             PaymentNumber = Payment.GenerateNumber(),
//             AccountUid = targetAccount.Uid,
//             Email = user.Email,
//             FirstName = user.FirstName,
//             LastName = user.LastName,
//             CurrencyId = targetCurrencyId,
//             Options = options
//         };
//
//         var response = await client.RequestAsync();
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//
//     private async Task<IActionResult> ProcessHelp2PayPayment(long partyId, long initialAmount,
//         CurrencyTypes currencyId, FundTypes fundType, Account targetAccount, PaymentService paymentSvc,
//         Supplement.DepositSupplement supplement)
//     {
//         if (!Help2Pay.RequestSupplement.TryParse(supplement.RequestJson, out var request))
//             return BadRequest(Result.Error(request.ValidationMessage));
//
//         var exchangedAmount = await GetExchangedAmountAsync(initialAmount, currencyId, request.Currency);
//         if (exchangedAmount < 0) return BadRequest(Result.Error(MSG.ExchangeRateNotExists));
//
//         var options = Help2PayOptions.FromJson(paymentSvc.Configuration);
//         var tenantId = GetTenantId();
//         var lang = await authDbContext.Users
//             .Where(x => x.PartyId == partyId && x.TenantId == tenantId)
//             .Select(x => x.Language)
//             .SingleOrDefaultAsync() ?? LanguageTypes.English;
//
//         var client = new Help2Pay.RequestClient
//         {
//             Amount = RoundUp(exchangedAmount / 100m),
//             AccountUid = targetAccount.Uid,
//             PaymentNumber = Payment.GenerateNumber(),
//             ReturnUrl = request.GetReturnUrl(),
//             Currency = request.Currency,
//             Ip = GetRemoteIpAddress(),
//             Language = lang,
//             Bank = request.GetBank(),
//             // Bank = "VIETQR",
//             Options = options,
//             Logger = logger,
//         };
//
//         var response = await client.SendAsync();
//
//         if (!response.IsSuccess)
//         {
//             logger.LogWarning("Payment process error {@Request} {@Result}", client, response);
//             await sendMsgSvc.SendEventToManagerAsync(GetTenantId(),
//                 MessagePopupDTO.BuildInfo("Payment Failed", $"{paymentSvc.Name} Payment Failed").ToEventNotice());
//             return BadRequest(Result.Error(MSG.PaymentExecuteFailed, response));
//         }
//
//         var payment = await CreatePaymentAsync(partyId, paymentSvc.Id, exchangedAmount, currencyId,
//             client.PaymentNumber);
//         var depositResult = await CreateDepositAsync(partyId, payment.Id, initialAmount, currencyId,
//             fundType, targetAccount.Id, supplement);
//
//         await SaveReferenceSupplementAsync(depositResult.Id, response.IsSuccess);
//         return Ok(response);
//     }
//
//     private Task SetPaymentNumberToTenantIdAsync(string paymentNumber)
//         => myCache.SetStringAsync(Payment.GetPaymentNumberTenantIdKey(paymentNumber), GetTenantId().ToString(),
//             TimeSpan.FromDays(3));
//
//     private async Task<M> CreateDepositWithPaymentAsync(long partyId, DepositCreateRequestModel spec,
//         long targetTradeAccountId, Supplement.DepositSupplement supplement)
//     {
//         // create deposit
//         var depositResult = await accountingService.DepositCreateWithPaymentAsync(partyId, spec.FundType,
//             spec.CurrencyId, spec.Amount, spec.PaymentServiceId, partyId, targetTradeAccountId,
//             supplement);
//         if (!depositResult.IsEmpty())
//             await mediator.Publish(new DepositCreatedEvent(depositResult));
//         return depositResult;
//     }
//
//     private async Task<M> CreateDepositAsync(long partyId, long paymentId, DepositCreateRequestModel spec,
//         long targetTradeAccountId,
//         Supplement.DepositSupplement supplement)
//     {
//         // create deposit
//         var depositResult = await accountingService.DepositCreateAsync(partyId, spec.FundType,
//             spec.CurrencyId, spec.Amount, paymentId, partyId, targetTradeAccountId,
//             supplement);
//         if (!depositResult.IsEmpty())
//             await mediator.Publish(new DepositCreatedEvent(depositResult));
//
//         return depositResult;
//     }
//
//     private async Task<M> CreateDepositAsync(long partyId, long paymentId, long initialAmount, CurrencyTypes currencyId,
//         FundTypes fundType, long targetAccountId, Supplement.DepositSupplement supplement)
//     {
//         // create deposit
//         var depositResult = await accountingService.DepositCreateAsync(partyId, fundType, currencyId, initialAmount,
//             paymentId, partyId, targetAccountId, supplement);
//         if (!depositResult.IsEmpty())
//             await mediator.Publish(new DepositCreatedEvent(depositResult));
//
//         return depositResult;
//     }
//
//     private async Task SaveReferenceSupplementAsync(long depositId, object? supplement)
//     {
//         if (supplement == null) return;
//
//         var refSupplement = await tenantCtx.Supplements
//             .Where(x => x.Type == (int)SupplementTypes.DepositReference)
//             .Where(x => x.RowId == depositId)
//             .SingleOrDefaultAsync();
//         if (refSupplement == null)
//         {
//             refSupplement = Supplement.Build(SupplementTypes.DepositReference, depositId, Utils.JsonSerializeObject(supplement));
//             await tenantCtx.Supplements.AddAsync(refSupplement);
//         }
//         else
//         {
//             refSupplement.Data = Utils.JsonSerializeObject(supplement);
//             tenantCtx.Update(refSupplement);
//         }
//
//         await tenantCtx.SaveChangesAsync();
//     }
//
//     private async Task TryExecutePayment(long id)
//     {
//         var deposit = await tenantCtx.Deposits
//             .Include(x => x.IdNavigation)
//             .Include(x => x.Payment)
//             .ThenInclude(x => x.PaymentMethod)
//             .Where(x => x.Id == id)
//             .SingleAsync();
//
//         if (deposit.Payment.Status != (int)PaymentStatusTypes.Pending
//             || deposit.Payment.PaymentMethod.Platform != (int)PaymentPlatformTypes.Wire)
//             return;
//
//         deposit.Payment.Status = (int)PaymentStatusTypes.Executing;
//         deposit.Payment.UpdatedOn = DateTime.UtcNow;
//         tenantCtx.Payments.Update(deposit.Payment);
//         await tenantCtx.SaveChangesAsync();
//
//         await mediator.Publish(new PaymentExecutedEvent(deposit.PaymentId, deposit.IdNavigation));
//     }
//
//     private async Task<Tuple<bool, decimal>> GetExchangeRate(CurrencyTypes from, CurrencyTypes to)
//     {
//         if (from == to) return Tuple.Create(true, 1m);
//         var exchangeRateEntity = await accountingService.GetExchangeRateAsync(from, to);
//         return exchangeRateEntity == null
//             ? Tuple.Create(false, 0m)
//             : Tuple.Create(true, Math.Ceiling(exchangeRateEntity.SellingRate * (1 + exchangeRateEntity.AdjustRate / 100) * 1000) / 1000);
//     }
//
//     private static long RoundUp(decimal value)
//     {
//         var rounded = (long)Math.Round(value, 0, MidpointRounding.AwayFromZero);
//         if (rounded < value)
//             rounded++;
//         return rounded;
//     }
//
//     private async Task<string> GetChinaUserKycName(long partyId)
//     {
//         var tid = GetTenantId();
//         var nativeName = await authDbContext.Users.Where(u => u.PartyId == partyId && u.TenantId == tid)
//             .Select(x => x.NativeName)
//             .SingleOrDefaultAsync();
//         return nativeName == null ? string.Empty : nativeName.Replace(" ", "");
//     }
// }