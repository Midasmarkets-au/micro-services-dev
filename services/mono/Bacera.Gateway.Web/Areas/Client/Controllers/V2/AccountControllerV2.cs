using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Withdraw;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Middlewares;
using Bacera.Gateway.Web.Request;
using Bacera.Gateway.Web.Services;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using MSG = Bacera.Gateway.ResultMessage.Transaction;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Account")]
[Area("Client")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = UserRoleTypesString.AllClient)]
[Route("api/" + VersionTypes.V2 + "/[Area]/account")]
public class AccountControllerV2(
    TenantDbContext tenantCtx,
    ReferralCodeService referralCodeSvc,
    PaymentMethodService paymentMethodSvc,
    DepositService depositSvc,
    WithdrawalService withdrawalSvc,
    AccountManageService accManageSvc,
    AcctService acctSvc,
    ApplicationService applicationSvc,
    IApplicationTokenService tokenSvc,
    IBackgroundJobClient jobClient,
    UserService userSvc,
    IStorageService storageSvc,
    MyDbContextPool pool,
    IMediator mediator,
    ISendMailService sendMailService,
    ILogger<WalletControllerV2> logger)
    : ClientBaseControllerV2
{
    /// <summary>
    /// Get account page
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Account.ClientCriteria? criteria = null)
    {
        var updateBalanceTask = accManageSvc.ConcurrentUpdateBalanceByPartyId(GetPartyId());
        criteria ??= new Account.ClientCriteria();
        criteria.Size = 100;
        criteria.PartyId = GetPartyId();
        
        var items = await accManageSvc.GetIncompleteAccountForPartyAsync(GetPartyId());

        criteria.HasTradeAccount = true;
        var accounts = await accManageSvc.QueryAccountForClientAsync(criteria);
        items.AddRange(accounts);
        criteria.Total = items.Count + accounts.Count;

        await updateBalanceTask;
        return Ok(Result<List<Account.ClientPageModel>, Account.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get demo account page
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("demo")]
    public async Task<IActionResult> DemoIndex([FromQuery] TradeDemoAccount.ClientCriteria? criteria = null)
    {
        criteria ??= new TradeDemoAccount.ClientCriteria();
        criteria.PartyId = GetPartyId();
        var items = await accManageSvc.QueryDemoAccountForClientAsync(criteria);
        return Ok(Result<List<TradeDemoAccount.ClientPageModel>, TradeDemoAccount.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Create Trade Demo Account
    /// </summary>
    /// <returns></returns>
    [HttpPost("demo")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = UserRoleTypesString.AllClient)]
    [ProducesResponseType(typeof(TradeDemoAccount.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TradeDemoAccount>> DemoAccountCreate([FromBody] TradeDemoAccount.CreateSpecV2 spec)
    {
        if (!pool.IsServiceExisted(spec.ServiceId)) return BadRequest(Result.Error("Service_not_found"));

        var platform = pool.GetPlatformByServiceId(spec.ServiceId);
        if (platform != PlatformTypes.MetaTrader4Demo && platform != PlatformTypes.MetaTrader5Demo)
            return BadRequest(Result.Error("Service_not_found"));

        var partyId = GetPartyId();
        if (await accManageSvc.DemoAccountCountAsync(partyId) >= 5)
            return BadRequest(Result.Error("You_can_only_have_5_demo_accounts"));

        var defaultGroup = await accManageSvc.GetDemoTradeAccountDefaultGroupAsync(spec.ServiceId, spec.AccountType, spec.CurrencyId);
        if (defaultGroup == null) return BadRequest(Result.Error("Failed_to_get_default_group"));

        var password = Utils.GenerateSimplePassword();
        var user = await tenantCtx.Parties.Where(x => x.Id == partyId).ToTenantDetailModel().SingleAsync();

        // *** USC precision adjustment *** //
        spec.Amount = spec.Amount.ToScaledFromCents();

        var accountNumber = await accManageSvc.CreateTradeDemoAccountAsync(GetPartyId(), spec.ServiceId, spec.AccountType, user.GuessNativeName(),
            user.EmailRaw, password, spec.Amount, spec.CurrencyId, spec.Leverage, defaultGroup);

        if (accountNumber == -1) return BadRequest(Result.Error("Failed_to_create_demo_account"));

        await mediator.Publish(new DemoAccountCreatedEvent(GetPartyId(), password, user.GuessNativeName(),
            user.EmailRaw, user.PhoneNumberRaw, accountNumber, pool.GetServiceNameByServiceId(spec.ServiceId)));

        return NoContent();
    }

    /// <summary>
    /// Get configurations to open an account
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("application-config")]
    public async Task<IActionResult> GetAccountApplicationConfig()
    {
        var partyId = GetPartyId();
        var result = new AccountApplicationConfig
        {
            AccountTypeAvailable = await accManageSvc.GetAvailableAccountTypes(partyId),
            CurrencyAvailable = await accManageSvc.GetAvailableCurrencyTypes(partyId),
            LeverageAvailable = await accManageSvc.GetAvailableLeverages(partyId),
            TradingPlatformAvailable = await accManageSvc.GetAvailableTradingPlatforms(partyId),
            ReferCode = await accManageSvc.GetUserDefaultSelfReferCodeAsync(partyId),
        };
        return Ok(result);
    }

    /// <summary>
    /// Get configurations to open a demo account
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("demo/application-config")]
    public async Task<IActionResult> GetDemoAccountApplicationConfig()
    {
        var partyId = GetPartyId();
        var result = new AccountApplicationConfig
        {
            AccountTypeAvailable = await accManageSvc.GetDemoAvailableAccountTypes(partyId),
            CurrencyAvailable = await accManageSvc.GetDemoAvailableCurrencyTypes(partyId),
            LeverageAvailable = await accManageSvc.GetAvailableLeverages(partyId),
            TradingPlatformAvailable = await accManageSvc.GetAvailableDemoTradingPlatforms(partyId),
            ReferCode = await accManageSvc.GetUserDefaultSelfReferCodeAsync(partyId),
        };
        return Ok(result);
    }

    /// <summary>
    /// Get account types by referral code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("type/referral")]
    public async Task<IActionResult> GetAccountTypesByReferralCode([FromQuery] string code)
    {
        var partyId = GetPartyId();

        var selfCodes = await tenantCtx.ReferralCodes
            .Where(x => x.PartyId == partyId)
            .Select(x => x.Code)
            .ToListAsync();

        if (selfCodes.Contains(code))
        {
            var selfResult = await referralCodeSvc.GetAccountTypesInReferralCodeAsync(code);
            return Ok(selfResult);
        }

        var selfAccounts = await tenantCtx.Accounts.Where(x => x.PartyId == partyId)
            .OrderByDescending(x => x.Id)
            .Select(x => new { x.Id, x.AgentAccountId, x.SalesAccountId })
            .ToListAsync();

        var parentAccountIds = selfAccounts
            .Select(x => x.AgentAccountId)
            .Union(selfAccounts.Select(x => x.SalesAccountId))
            .Distinct()
            .ToList();

        var isCodeBelongToParent = await tenantCtx.ReferralCodes
            .Where(x => parentAccountIds.Contains(x.AccountId) && x.Code == code)
            .AnyAsync();

        if (!isCodeBelongToParent) return BadRequest(Result.Error("Invalid_referral_code"));

        var result = await referralCodeSvc.GetAccountTypesInReferralCodeAsync(code);
        return Ok(result);
    }

    /// <summary>
    /// Get Account Config
    /// </summary>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpGet("{accountUid:long}/config")]
    public async Task<IActionResult> AccountConfig(long accountUid)
    {
        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);

        var config = new AccountUpdateConfig
        {
            LeverageAvailable = await accManageSvc.GetAccountAvailableLeverages(id),
        };

        return Ok(config);
    }

    /// <summary>
    /// Query Account Deposit
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpGet("{accountUid:long}/deposit")]
    public async Task<IActionResult> AccountDepositIndex(long accountUid, [FromQuery] Deposit.ClientCriteria? criteria = null)
    {
        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);

        criteria ??= new Deposit.ClientCriteria();
        criteria.AccountId = id;
        var items = await depositSvc.QueryForClientAsync(criteria);
        return Ok(Result<List<Deposit.ClientPageModel>, Deposit.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Query Account Deposit
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="depositHashId"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpGet("{accountUid:long}/deposit/{depositHashId}/guide")]
    public async Task<IActionResult> AccountDepositIndex(long accountUid, string depositHashId)
    {
        var depositId = Deposit.HashDecode(depositHashId);
        var partyId = GetPartyId();
        var user = await userSvc.GetPartyAsync(partyId);
        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var deposit = await tenantCtx.Deposits
            .Where(x => x.Id == depositId && x.PartyId == partyId)
            .Where(x => x.TargetAccountId == id)
            .Select(x => new { x.Id, x.Payment.PaymentServiceId, x.PaymentId })
            .SingleOrDefaultAsync();

        if (deposit == null) return NotFound("Deposit_not_found");

        var method = await paymentMethodSvc.GetMethodByIdAsync(deposit.PaymentServiceId);
        if (method == null) return BadRequest("Payment_method_not_found");

        var instruction = await paymentMethodSvc.GetInstructionAsync(method, user.Language);
        object info = new();
        if (method.Name == "USDT Crypto")
        {
            var usedCrypto = await tenantCtx.Cryptos
                .Where(x => x.Status == (int)CryptoStatusTypes.InUse && x.InUsePaymentId == deposit.PaymentId)
                .Select(x => new { x.Name, x.Address })
                .SingleOrDefaultAsync();
            if (usedCrypto != null) info = usedCrypto;
        }

        var result = new PaymentMethodDTO.DepositGuideInfo
        {
            PaymentMethodName = method.Name,
            Platform = (PlatformTypes)method.Platform,
            Instruction = instruction,
            Info = info
        };

        return Ok(result);
    }

    private const int MaxIncompleteDeposit = 5;

    /// <summary>
    /// Create Deposit
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpPost("{accountUid:long}/deposit")]
    public async Task<IActionResult> CreateDeposit(long accountUid, [FromBody] CreateDepositRequestModel spec)
    {
        var (isValid, error) = spec.Validate();
        if (!isValid) return BadRequest(Result.Error(error));

        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        if (id == 0) return BadRequest(Result.Error("Account_not_found"));

        var pendingCount = await accManageSvc.AccountCreatedDepositCountAsync(id);
        if (pendingCount >= MaxIncompleteDeposit) return BadRequest(Result.Error("Too_many_pending_deposits"));

        // *** USC precision adjustment *** //
        spec.Amount = spec.Amount.ToScaledFromCents();

        // Get account currency and adjust amount
        var accountCurrency = await accManageSvc.GetAccountCurrencyIdAsync(id);
        // If currency is USC, convert to USD
        var exchangeRatedAmount = spec.Amount;
        if (accountCurrency == CurrencyTypes.USC)
        {
            var rate = await acctSvc.GetExchangeRateAsync(CurrencyTypes.USC, CurrencyTypes.USD);
            if (rate <= 0) return BadRequest(Result.Error("USC_to_USD_exchange_rate_not_available"));
            exchangeRatedAmount = (long)(spec.Amount * rate);
        }

        var method = await paymentMethodSvc.GetMethodByIdAsync(spec.PaymentMethodId);
        if (method == null) return BadRequest(Result.Error("Payment_method_not_found"));
        if (new List<string> { "Union Pay", "EXG" }.Contains(method.Group))
        {
            method = await paymentMethodSvc.GetGroupMethodRandomlyAsync(id, spec.Amount, method.Group);
            if (method == null) return BadRequest(Result.Error("No_available_Union_Pay_method"));
        }

        var isEnabled = await paymentMethodSvc.IsAccountAccessEnabledAsync(id, method.Id);
        if (!isEnabled) return BadRequest(Result.Error("Payment_method_not_enabled"));

        var isAmountValid = await paymentMethodSvc.IsAmountValidAsync(method.Id, id, exchangeRatedAmount);
        if (!isAmountValid) return BadRequest(Result.Error("Amount_not_valid"));

        spec.Request["ip"] = GetRemoteIpAddress();
        spec.Request["referer"] = GetReferer();
        spec.Request["baseUrl"] = GetBaseUrl();
        var response = await depositSvc.CreateDepositForAccountAsync(method.Id, id, spec.Amount, spec.Request, spec.Note, GetPartyId());
        if (!response.IsSuccess) return BadRequest(response);

        var deposit = await tenantCtx.Deposits.FindAsync(response.DepositId);
        if (deposit != null)
            await mediator.Publish(new DepositCreatedEvent(deposit));

        return Ok(response);
    }


    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpPost("{accountUid:long}/deposit/{depositHashId}/receipt")]
    public async Task<IActionResult> UploadDepositReceipt(long accountUid, string depositHashId, IFormFile file)
    {
        if (file.Length < 1) return BadRequest();
        var partyId = GetPartyId();
        var accountId = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var depositId = Deposit.HashDecode(depositHashId);
        var deposit = await tenantCtx.Deposits
            .Where(x => x.PartyId == partyId && x.Id == depositId && x.TargetAccountId == accountId)
            .Select(x => new { x.Id, x.PaymentId })
            .SingleOrDefaultAsync();

        if (deposit == null) return NotFound();

        const string type = "DepositReceipt";

        Medium result;
        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            result = await storageSvc.UploadFileAndSaveMediaAsync(memoryStream, Guid.NewGuid().ToString(),
                Path.GetExtension(file.FileName).ToLower(), type, depositId, file.ContentType, GetTenantId(), GetPartyId());
        }
        catch (Exception e)
        {
            BcrLog.Slack($"UploadDepositReceipt_Error: {e.Message}_tid: {GetTenantId()}_pid: {partyId}_aid: {accountId}_did: {depositId}");
            return Problem();
        }

        await acctSvc.DepositExecutePaymentAsync(depositId, partyId);

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReceipt)
            .Where(x => x.RowId == depositId)
            .FirstOrDefaultAsync();

        if (supplement == null)
        {
            var data = JsonConvert.SerializeObject(new List<string> { result.Guid });
            supplement = Supplement.Build(SupplementTypes.DepositReceipt, depositId, data);

            await tenantCtx.Supplements.AddAsync(supplement);
            await tenantCtx.SaveChangesAsync();
            return Ok(new List<string> { result.Guid });
        }

        var receipts = JsonConvert.DeserializeObject<List<string>>(supplement.Data) ?? [];
        receipts.Add(result.Guid);
        supplement.Data = JsonConvert.SerializeObject(receipts);
        tenantCtx.Supplements.Update(supplement);
        await tenantCtx.SaveChangesAsync();
        return Ok(receipts);
    }

    /// <summary>
    /// Get receipt for deposit
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="depositHashId"></param>
    /// <returns></returns>
    [HttpGet("{accountUid:long}/deposit/{depositHashId}/receipt")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UploadDepositReceipt(long accountUid, string depositHashId)
    {
        var partyId = GetPartyId();
        var depositId = Deposit.HashDecode(depositHashId);
        var accountId = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var exists = await tenantCtx.Deposits.AnyAsync(x => x.PartyId == partyId && x.Id == depositId && x.TargetAccountId == accountId);
        if (!exists) return NotFound();

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReceipt)
            .Where(x => x.RowId == depositId)
            .FirstOrDefaultAsync();
        var items = Utils.JsonDeserializeObjectWithDefault<List<string>>(supplement?.Data ?? "[]");
        return Ok(items);
    }


    /// <summary>
    /// Query Account Withdrawal
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpGet("{accountUid:long}/withdrawal")]
    public async Task<IActionResult> AccountWithdrawalIndex(long accountUid, [FromQuery] Withdrawal.ClientCriteria? criteria = null)
    {
        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);

        criteria ??= new Withdrawal.ClientCriteria();
        criteria.AccountId = id;
        var items = await withdrawalSvc.QueryForClientAsync(criteria);
        return Ok(Result<List<Withdrawal.ClientPageModel>, Withdrawal.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Create Withdrawal
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpPost("{accountUid:long}/withdrawal")]
    public async Task<IActionResult> CreateWithdrawal(long accountUid, [FromBody] CreateWithdrawalRequestModel spec)
    {
        var (isValid, error) = spec.Validate();
        if (!isValid) return BadRequest(Result.Error(error));

        // Validate verification code
        if (string.IsNullOrWhiteSpace(spec.VerificationCode))
        {
            return BadRequest(Result.Error(MSG.VerificationCodeRequired));
        }

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var isValidCode = await userSvc.ValidateAuthCodeAsync(
            AuthCode.EventLabel.Withdrawal,
            AuthCodeMethodTypes.Email,
            userEmail,
            spec.VerificationCode.Trim());

        if (!isValidCode)
        {
            logger.LogWarning("Invalid verification code attempt for withdrawal, PartyId: {PartyId}", GetPartyId());
            return BadRequest(Result.Error(MSG.InvalidVerificationCode));
        }

        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        if (id == 0) return BadRequest(Result.Error("Account_not_found"));

        var method = await paymentMethodSvc.GetMethodByIdAsync(spec.PaymentMethodId);
        if (method == null) return BadRequest(Result.Error("Payment_method_not_found"));
        var hasSuccessWithdrawal = await tenantCtx.Withdrawals
            .Where(x => x.SourceAccountId == id)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.WithdrawalCompleted)
            .AnyAsync();

        if (hasSuccessWithdrawal)
        {
            if (spec.Amount < (method.InitialValue * 100))
            {
                return BadRequest(Result.Error("Minimum withdrawal amount is " + method.InitialValue));
            }
        }
        else
        {
            if (spec.Amount < (method.MinValue * 100))
            {
                return BadRequest(Result.Error("Minimum withdrawal amount is " + method.MinValue));
            }
        }

        // *** Adjust scale with * 10000 *** //
        spec.Amount = spec.Amount.ToScaledFromCents();

        if (method.Platform == (short)PaymentPlatformTypes.USDT)
        {
            try
            {
                var partyId = GetPartyId();
                string walletAddress = spec.Request["walletAddress"];
                var addressValid = await tenantCtx.PaymentInfos
                    .Where(x => x.PartyId == partyId)
                    .Where(x => x.PaymentPlatform == (int)PaymentPlatformTypes.USDT)
                    .Where(x => x.Info.Contains(walletAddress))
                    .AnyAsync();
                if (!addressValid) return BadRequest(Result.Error("Invalid_wallet_address"));
            }
            catch
            {
                return BadRequest(Result.Error("Validate_wallet_address_failed"));
            }
        }

        var isAmountValid = await withdrawalSvc.IsAmountValidForAccountWithdrawal(id, spec.Amount);
        if (!isAmountValid) return BadRequest(Result.Error("Amount_not_valid"));

        Withdrawal? withdrawal = await withdrawalSvc.CreateWithdrawalForAccountAsync(spec.PaymentMethodId, id,
            spec.Amount, spec.Request, spec.Note, GetPartyId());
        if (withdrawal == null) return BadRequest(Result.Error("Failed_to_create_withdrawal"));

        await mediator.Publish(new WithdrawalCreatedEvent(withdrawal));
        return Ok();
    }

    /// <summary>
    /// Query Account Transfer
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpGet("{accountUid:long}/transfer")]
    public async Task<IActionResult> AccountTransferIndex(long accountUid, [FromQuery] Transaction.ClientCriteria? criteria = null)
    {
        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);

        criteria ??= new Transaction.ClientCriteria();
        criteria.AccountId = id;
        var items = await acctSvc.QueryForTransactionClientAsync(criteria);
        return Ok(Result<List<Transaction.ClientPageModel>, Transaction.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Set account as default parent account for Agent or Sales
    /// </summary>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpPut("{accountUid:long}/default-parent")]
    public async Task<IActionResult> SetAsDefaultParentAccount(long accountUid)
    {
        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var result = await accManageSvc.SetAsDefaultParentAccountAsync(id);
        if (!result) return BadRequest("__SET_DEFAULT_PARENT_ACCOUNT_FAILED__");
        return NoContent();
    }

    /// <summary>
    /// Change Trade Account Leverage
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpPost("{accountUid:long}/change-leverage")]
    public async Task<IActionResult> ChangeTradeAccountLeverage(long accountUid, [FromBody] TradeAccountApplicationSupplement.ChangeLeverageSpec spec)
    {
        if (!spec.IsValid()) return BadRequest(Result.Error("Invalid_Parameter"));

        var partyId = GetPartyId();
        var count = await tenantCtx.Applications
            .Where(x => x.PartyId == partyId && x.Type == (int)ApplicationTypes.TradeAccountChangeLeverage)
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval)
            .CountAsync();

        if (count > 2) return BadRequest(Result.Error("Two_maximum_pending_applications_to_change_leverage"));

        var availableLeverages = await accManageSvc.GetAvailableLeverages(GetPartyId());
        if (!availableLeverages.Contains(spec.Leverage)) return BadRequest(Result.Error("Invalid_Leverage"));

        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var accountNumber = await tenantCtx.Accounts
            .Where(x => x.Id == id && x.HasTradeAccount == true && x.AccountNumber > 0)
            .Select(x => x.AccountNumber)
            .SingleAsync();

        var supplement = new TradeAccountApplicationSupplement.ChangeLeverageDTO
        {
            AccountUid = accountUid,
            AccountNumber = accountNumber,
            Leverage = spec.Leverage
        };
        var result = await applicationSvc.CreateApplication(GetPartyId(), ApplicationTypes.TradeAccountChangeLeverage, supplement,
            supplement.AccountUid);

        if (result.Id == 0) return BadRequest(result);

        await mediator.Publish(new ApplicationCreatedEvent(result));
        return NoContent();
    }

    /// <summary>
    /// Change Trade Account Leverage
    /// </summary>
    /// <param name="accountUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientAccountFilter))]
    [HttpPost("{accountUid:long}/change-password")]
    public async Task<IActionResult> ChangeTradeAccountPassword(long accountUid, [FromBody] TradeAccountApplicationSupplement.ChangePasswordSpec spec)
    {
        if (!spec.IsValid()) return BadRequest(Result.Error("Invalid_Parameter"));

        var id = await accManageSvc.GetAccountIdByUidAsync(accountUid);
        var accountNumber = await tenantCtx.Accounts
            .Where(x => x.Id == id && x.HasTradeAccount == true && x.AccountNumber > 0)
            .Select(x => x.AccountNumber)
            .SingleOrDefaultAsync();
        if (accountNumber == 0) return NotFound();

        var supplement = new TradeAccountApplicationSupplement.ChangePasswordDTO
        {
            AccountUid = accountUid,
            AccountNumber = accountNumber,
            CallbackUrl = spec.CallbackUrl
        };
        var result = await applicationSvc.CreateApplication(GetPartyId(), ApplicationTypes.TradeAccountChangePassword, supplement, accountUid,
            ApplicationStatusTypes.Approved);
        if (result.Id == 0) return BadRequest(result);

        var tokenRequest = ApplicationToken.Build(GetPartyId(), TokenTypes.TradeAccountChangePasswordToken, accountUid);

        var token = await tokenSvc.GenerateTokenAsync(tokenRequest, TimeSpan.FromHours(24));
        if (token == null) return BadRequest(Result.Error(ResultMessage.Common.TokenGenerateFail));

        jobClient.Enqueue<IGeneralJob>(x => x.ResetTradeAccountPasswordAsync(GetTenantId(), id, spec.CallbackUrl, token.Token));
        return NoContent();
    }

    [HttpGet("{accountUid:long}/payment/{paymentNumber}/status")]
    public async Task<IActionResult> GetPaymentStatus(long accountUid, string paymentNumber)
    {
        try
        {
            // Get payment by number
            var payment = await tenantCtx.Payments
                .Include(x => x.PaymentMethod)
                .FirstOrDefaultAsync(x => x.Number == paymentNumber);

            if (payment == null)
            {
                return NotFound(new { status = "not_found" });
            }

            // Check if this payment belongs to the requesting account's party
            var account = await tenantCtx.Accounts
                .FirstOrDefaultAsync(x => x.Uid == accountUid);
                
            if (account == null || payment.PartyId != account.PartyId)
            {
                return Unauthorized(new { status = "unauthorized" });
            }

            var status = payment.Status switch
            {
                (int)PaymentStatusTypes.Pending => "pending",
                (int)PaymentStatusTypes.Executing => "processing", 
                (int)PaymentStatusTypes.Completed => "completed",
                (int)PaymentStatusTypes.Failed => "failed",
                (int)PaymentStatusTypes.Cancelled => "cancelled",
                (int)PaymentStatusTypes.Rejected => "rejected",
                _ => "unknown"
            };

            return Ok(new 
            { 
                status,
                paymentNumber,
                amount = payment.Amount,
                updatedOn = payment.UpdatedOn,
                paymentMethod = payment.PaymentMethod?.Name
            });
        }
        catch (Exception ex)
        {
            BcrLog.Slack($"Error checking payment status for {paymentNumber}: {ex.Message}");
            return StatusCode(500, new { status = "error", message = "Internal server error" });
        }
    }
}