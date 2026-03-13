using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Email.ViewModel;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Bacera.Gateway.Transaction;
using M = Bacera.Gateway.Transaction;
using MSG = Bacera.Gateway.ResultMessage.Transaction;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Transaction")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class TransactionController(
    IMediator mediator,
    AccountingService accountingService,
    TradingService tradingService,
    AccountManageService accManSvc,
    AcctService acctSvc,
    TenantDbContext tenantCtx,
    ISendMailService sendMailService,
    UserService userSvc,
    ILogger<TransactionController> logger)
    : ClientBaseController
{
    private readonly ILogger<TransactionController> _logger = logger;

    /// <summary>
    /// Transaction pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();

        return Ok(await accountingService.TransactionQueryForClientAsync(GetPartyId(), criteria));
    }

    /// <summary>
    /// Transfer view
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("query")]
    [ProducesResponseType(typeof(Result<List<TransferView>, TransferView.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferView([FromQuery] TransferView.Criteria? criteria)
    {
        criteria ??= new TransferView.Criteria();
        criteria.PartyId = GetPartyId();

        var items = await accountingService
            .TransferViewQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Transfer view
    /// </summary>
    /// <param name="currencyId"></param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="stage"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("currency/{currencyId:int}")]
    [ProducesResponseType(typeof(TransferView), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferViewQuery(int currencyId, [FromQuery] int? page, [FromQuery] int? size,
        [FromQuery] int? stage, [FromQuery] MatterTypes? type)
    {
        var items = await accountingService
            .TransferViews(GetPartyId(), (CurrencyTypes)currencyId, type, stage, page, size);
        return Ok(items);
    }

    /// <summary>
    /// Transaction get
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await accountingService.TransactionGetForPartyAsync(id, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Transfer from Wallet to Trade Account with currency conversion support
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("to/trade-account")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferFromWalletToTradeAccount([FromBody] M.ClientRequestModel spec)
    {
        if (spec.Amount < 100) return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));
        if (Utils.IsWithinCloseMarketTime())
            return BadRequest(Result.Error("Market is closed, please try again after 2 minutes."));
        
        // Validate verification code
        if (string.IsNullOrWhiteSpace(spec.VerificationCode))
        {
            return BadRequest(Result.Error(MSG.VerificationCodeRequired));
        }

        var partyId = GetPartyId();
        var userEmail = GetUserEmail();
        var isValidCode = await userSvc.ValidateAuthCodeAsync(
            AuthCode.EventLabel.WalletToTradeAccount,
            AuthCodeMethodTypes.Email,
            userEmail,
            spec.VerificationCode.Trim());

        if (!isValidCode)
        {
            _logger.LogWarning("Invalid verification code attempt for transfer, PartyId: {PartyId}", partyId);
            return BadRequest(Result.Error(MSG.InvalidVerificationCode));
        }

        var hasPending = await tenantCtx.Transactions
            .Where(x => x.PartyId == partyId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.TransferCreated ||
                        x.IdNavigation.StateId == (int)StateTypes.TransferAwaitingApproval)
            .AnyAsync();

        if (hasPending) return BadRequest(Result.Error(MSG.PendingTransferExist));

        var wallet = await tenantCtx.Wallets
            .Where(x => x.PartyId == partyId && x.Id == spec.WalletId)
            .Select(x => new { x.Balance, x.CurrencyId, x.Id, x.FundType })
            .SingleOrDefaultAsync();
        if (wallet == null) return NotFound(Result.Error("Wallet not found"));

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == spec.TradeAccountUid && x.PartyId == partyId)
            .Select(x => new { x.Id, x.CurrencyId, x.FundType })
            .SingleOrDefaultAsync();
        if (account == null) return NotFound(Result.Error("Account not found"));

        // Validate fund types must match
        if (account.FundType != wallet.FundType)
            return BadRequest(Result.Error("Fund type mismatch between wallet and account"));

        // Calculate required wallet amount considering exchange rate
        long requiredWalletAmount = spec.Amount;
        long requiredWalletAmountToCheckBalance = spec.Amount.ToScaledFromCents();
        if (wallet.CurrencyId != account.CurrencyId)
        {
            // Apply exchange rate conversion
            var exchangeRate = await acctSvc.GetExchangeRateAsync(
                (CurrencyTypes)wallet.CurrencyId, 
                (CurrencyTypes)account.CurrencyId);
            
            if (exchangeRate <= 0)
                return BadRequest(Result.Error("Exchange rate not available for currency conversion"));
            
            // Convert account amount to wallet currency for balance check
            requiredWalletAmount = (long)Math.Ceiling(spec.Amount / (double)exchangeRate);
            requiredWalletAmountToCheckBalance = (requiredWalletAmount * 100).ToScaledFromCents();
        }

        // *** Check wallet balance *** //
        if (wallet.Balance < requiredWalletAmountToCheckBalance) 
            return BadRequest(Result.Error(MSG.BalanceNotEnough));

        // *** Apply 10000x precision multiplier for consistency *** //
        spec.Amount = spec.Amount.ToScaledFromCents();

        // Create transaction - use different methods based on currency conversion
        Transaction? transaction;
        if (wallet.CurrencyId == account.CurrencyId)
        {
            // Same currency - use standard method WITH 10000x precision multiplier
            transaction = await acctSvc.CreateTransactionFromWalletToTradeAccountAsync(
                wallet.Id,
                account.Id,
                spec.Amount,
                partyId,
                "CreateByClient");
        }
        else
        {
            // Different currencies - use conversion method WITH 10000x multiplier
            transaction = await acctSvc.CreateTransactionFromWalletToTradeAccountWithConversionAsync(
                wallet.Id,
                account.Id,
                spec.Amount,
                (CurrencyTypes)wallet.CurrencyId,
                (CurrencyTypes)account.CurrencyId,
                partyId,
                "CreateByClient");
        }

        if (transaction == null) return BadRequest(Result.Error(MSG.TransferFailed));

        await mediator.Publish(new TransferCreatedEvent(transaction));
        return Ok(M.ClientResponseModel.From(transaction));

        // var aid = await tradingService.TradeAccountLookupByUidAsync(spec.TradeAccountUid);
        // if (aid == 0)
        //     return NotFound();
        //
        // //var account = await _tradeSvc.AccountGetAsync(aid);
        // //if (account.Status != (int)AccountStatusTypes.Activate)
        // //    return BadRequest(Result.Error(ResultMessage.Account.AccountInactivated));
        //
        // var wallet = await accountingService.WalletGetAsync(spec.WalletId);
        //
        // var pid = GetPartyId();
        // if (wallet.PartyId != pid)
        //     return BadRequest(Result.Error(ResultMessage.Wallet.WalletNotFound));
        //
        // if (wallet.Balance < spec.Amount)
        //     return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));
        //
        // var (result, transaction) =
        //     await accountingService.TransactionCreateFromWalletToTradeAccountAsync(pid, spec.WalletId, aid, spec.Amount,
        //         pid);
        //
        // if (result == TransactionCheckStatus.Passed)
        //     await mediator.Publish(new TransferCreatedEvent(transaction));
        //
        // return result switch
        // {
        //     TransactionCheckStatus.Passed => Ok(M.ClientResponseModel.From(transaction)),
        //     TransactionCheckStatus.WalletNotFound => BadRequest(Result.Error(MSG.WalletInvalided)),
        //     TransactionCheckStatus.BalanceNotEnough => BadRequest(Result.Error(MSG.BalanceNotEnough)),
        //     TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
        //     TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
        //     TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
        //     _ => BadRequest(Result.Error(MSG.TransferFailed)),
        // };
    }

    /// <summary>
    /// Request verification code for specific type, default is wallet-to-wallet transfer (2FA)
    /// Simply generates and sends a verification code to the current user's email
    /// </summary>
    /// <returns></returns>
    [HttpPost("to/downsidewallet/request-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestVerificationCode([FromBody] RequestCodeModel? request = null)
    {
        var eventType = AuthCode.EventLabel.Normalize(request?.AuthType, AuthCode.EventLabel.WalletToWalletTransfer);
        var partyId = GetPartyId();
        var userEmail = GetUserEmail();
        var userLanguage = GetUserLanguage();
        
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("User email not found in JWT claims for PartyId: {PartyId}", partyId);
            return NotFound(Result.Error(ResultMessage.Common.UserNotFound));
        }

        // Invalidate any previous codes for this transfer type
        var existingCodes = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == eventType)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .ToListAsync();
        
        foreach (var code in existingCodes)
        {
            code.Status = (short)AuthCodeStatusTypes.Invalid;
        }

        // Generate new verification code (6 digits, 2 minutes expiry)
        const int expireMinutes = 2;
        var authCode = AuthCode.Build(
            partyId,
            eventType,
            AuthCodeMethodTypes.Email,
            userEmail,
            codeLen: 6,
            expireMinutes: expireMinutes);

        tenantCtx.AuthCodes.Add(authCode);
        await tenantCtx.SaveChangesAsync();

        // Send verification code email
        try
        {
            var emailModel = new VerificationCodeViewModel
            {
                Email = userEmail,
                // ✅ Only BCC in development environment with valid email
                BccEmails = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
                    ? new List<string> { "xinsong.rao@edgeark.com.au", "renjie.jiang@edgeark.com.au" }  
                    : null,
                VerificationCode = authCode.Code,
                ExpireMinutes = expireMinutes
            };

            await sendMailService.SendEmailWithTemplateAsync(emailModel, userLanguage);
            
            _logger.LogInformation("Wallet transfer verification code sent to PartyId: {PartyId}, Email: {Email}", 
                partyId, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification code email to PartyId: {PartyId}", partyId);
            return BadRequest(Result.Error(MSG.FailedSendVerificationCode)); //"Failed to send verification code. Please try again."
        }

        return Ok(new { 
            message = "Verification code sent to your email",
            expiresIn = expireMinutes * 60 // Convert minutes to seconds
        });
    }

    /// <summary>
    /// Transfer from Wallet to another Wallet (for IB/Sales to transfer to their downline's wallet)
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("to/downsidewallet")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferFromWalletToWallet([FromBody] M.ClientRequestModel spec)
    {
        var targetWalletId = spec.TradeAccountUid;
        if (spec.Amount < 100) return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));
        if (Utils.IsWithinCloseMarketTime())
            return BadRequest(Result.Error("Market is closed, please try again after 10 minutes."));
        
        if (targetWalletId <= 0) 
            return BadRequest(Result.Error(MSG.TargetWalletRequired)); //"Target wallet is required"

        // Validate verification code is provided
        if (string.IsNullOrWhiteSpace(spec.VerificationCode))
            return BadRequest(Result.Error(MSG.VerificationCodeRequired)); //"Verification code is required"


        var partyId = GetPartyId();

        // Verify the verification code
        spec.VerificationCode = spec.VerificationCode.Trim();
        var authCode = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == AuthCode.EventLabel.WalletToWalletTransfer)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .Where(x => x.Code == spec.VerificationCode)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (authCode == null)
        {
            _logger.LogWarning("Invalid verification code attempt for PartyId: {PartyId}", partyId);
            return BadRequest(Result.Error(MSG.InvalidVerificationCode)); //"Invalid verification code"
        }

        // Check if code has expired
        if (authCode.ExpireOn.HasValue && authCode.ExpireOn.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired verification code used for PartyId: {PartyId}", partyId);
            authCode.Status = (short)AuthCodeStatusTypes.Invalid;
            await tenantCtx.SaveChangesAsync();
            return BadRequest(Result.Error(MSG.VerificationCodeExpired)); //"Verification code has expired. Please request a new code."
        }

        // Mark the code as used (invalid) to prevent reuse
        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        await tenantCtx.SaveChangesAsync();

        var hasPending = await tenantCtx.Transactions
            .Where(x => x.PartyId == partyId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.TransferCreated ||
                        x.IdNavigation.StateId == (int)StateTypes.TransferAwaitingApproval)
            .AnyAsync();

        if (hasPending) return BadRequest(Result.Error(MSG.PendingTransferExist));

        // Get source wallet (current user's wallet)
        var sourceWallet = await tenantCtx.Wallets
            .Where(x => x.PartyId == partyId && x.Id == spec.WalletId)
            .Select(x => new { x.Balance, x.CurrencyId, x.Id, x.FundType })
            .SingleOrDefaultAsync();
        if (sourceWallet == null) return NotFound(Result.Error(MSG.SourceWalletNotFound)); //"Source wallet not found"

        // Get target wallet (recipient's wallet)
        var targetWallet = await tenantCtx.Wallets
            .Where(x => x.Id == targetWalletId)
            .Select(x => new { x.Balance, x.CurrencyId, x.Id, x.FundType, x.PartyId })
            .SingleOrDefaultAsync();
        if (targetWallet == null) return NotFound(Result.Error(MSG.TargetWalletNotFound)); //"Target wallet not found"

        // Validate source and target are different
        if (sourceWallet.Id == targetWallet.Id)
            return BadRequest(Result.Error(MSG.CannotTransferToSameWallet)); //"Cannot transfer to the same wallet"

        // Validate fund types must match
        if (sourceWallet.FundType != targetWallet.FundType)
            return BadRequest(Result.Error(MSG.FundTypeNotMatch));//"Fund type mismatch between source and target wallets"

        // Validate currency types must match (for now, no conversion for wallet-to-wallet)
        if (sourceWallet.CurrencyId != targetWallet.CurrencyId)
            return BadRequest(Result.Error(MSG.CurrencyNotMatch)); //"Currency mismatch between source and target wallets"

        // Fixed balance check - wallet balance is in 10000x scale, spec.Amount has been * 100
        if (sourceWallet.Balance < spec.Amount.ToScaledFromCents()) 
            return BadRequest(Result.Error(MSG.BalanceNotEnough));

        // *** Apply 10000x precision multiplier for consistency *** //
        spec.Amount = spec.Amount.ToScaledFromCents();

        // Create transaction between wallets
        var transaction = await acctSvc.CreateTransactionBetweenWalletAsync(
            sourceWallet.Id,
            targetWallet.Id,
            spec.Amount,
            partyId,
            "CreateByClient - Transfer to downline wallet");

        if (transaction == null) return BadRequest(Result.Error(MSG.TransferFailed));

        await mediator.Publish(new TransferCreatedEvent(transaction));
        return Ok(M.ClientResponseModel.From(transaction));
    }

    /// <summary>
    /// Transfer from Trade Account to Wallet
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("to/wallet")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferFromTradeAccountToWallet([FromBody] M.ClientRequestModel spec)
    {
        if (spec.Amount < 100)
            return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));

        if (Utils.IsWithinCloseMarketTime())
            return BadRequest(Result.Error("Market is closed, please try again after 10 minutes."));

        var partyId = GetPartyId();
        if (await accountingService.TransactionPendingCheckAsync(partyId))
            return BadRequest(Result.Error(MSG.PendingTransferExist));

        var aid = await tradingService.TradeAccountLookupByUidAsync(spec.TradeAccountUid);
        if (aid == 0) return NotFound();
        var pid = GetPartyId();
        var (result, transaction) =
            await accountingService.TransactionCreateFromTradeAccountToWalletAsync(pid, aid, spec.WalletId, spec.Amount.ToScaledFromCents(),
                pid);

        return result switch
        {
            TransactionCheckStatus.Passed => Ok(M.ClientResponseModel.From(transaction)),
            TransactionCheckStatus.WalletNotFound => BadRequest(Result.Error(MSG.WalletInvalided)),
            TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
            TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
            TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
            _ => BadRequest(Result.Error(MSG.TransferFailed)),
        };
    }

    /// <summary>
    /// Transfer from Trade Account to Trade Account
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("between-trade-account")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferBetweenTradeAccount([FromBody] Transaction.CreateBetweenAccountSpec spec)
    {
        if (spec.SourceTradeAccountUid == spec.TargetTradeAccountUid)
            return BadRequest(Result.Error(ResultMessage.Common.InvalidInput));

        if (spec.Amount < 100)
            return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));

        if (Utils.IsWithinCloseMarketTime())
            return BadRequest(Result.Error("Market is closed, please try again after 10 minutes."));

        // Validate verification code
        if (string.IsNullOrWhiteSpace(spec.VerificationCode))
        {
            return BadRequest(Result.Error(MSG.VerificationCodeRequired));
        }

        var partyId = GetPartyId();
        var userEmail = GetUserEmail();
        var isValidCode = await userSvc.ValidateAuthCodeAsync(
            AuthCode.EventLabel.TradeAccountToTradeAccount,
            AuthCodeMethodTypes.Email,
            userEmail,
            spec.VerificationCode.Trim());

        if (!isValidCode)
        {
            _logger.LogWarning("Invalid verification code attempt for trade account to trade account transfer, PartyId: {PartyId}", partyId);
            return BadRequest(Result.Error(MSG.InvalidVerificationCode));
        }

        if (await accountingService.TransactionPendingCheckAsync(partyId))
            return BadRequest(Result.Error(MSG.PendingTransferExist));

        var sourceAccountId = await tradingService.TradeAccountLookupByUidAsync(spec.SourceTradeAccountUid);
        var targetAccountId = await tradingService.TradeAccountLookupByUidAsync(spec.TargetTradeAccountUid);

        if (sourceAccountId == 0 || targetAccountId == 0) return NotFound();

        var (sourceAccountStatus, _, sourceAccountBalance) =
            await tradingService.GetTradeAccountBalanceAndLeverageFromServer(sourceAccountId);
        if (!sourceAccountStatus)
            return BadRequest(Result.Error(MSG.TradeAccountInvalided));

        if (sourceAccountBalance < spec.Amount / 100d)
            return BadRequest(Result.Error(MSG.BalanceNotEnough));

        // *** Apply 10000x precision multiplier for consistency *** //
        spec.Amount = spec.Amount.ToScaledFromCents();

        var (targetAccountStatus, _, _) =
            await tradingService.GetTradeAccountBalanceAndLeverageFromServer(targetAccountId);
        if (!targetAccountStatus)
            return BadRequest(Result.Error(MSG.TradeAccountInvalided));

        var pid = GetPartyId();

        var (result, transaction) =
            await accountingService.TransactionCreateBetweenTradeAccountAsync(pid, sourceAccountId, targetAccountId,
                spec.Amount,
                pid);

        if (result != TransactionCheckStatus.Passed)
            return result switch
            {
                TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
                TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
                TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
                _ => BadRequest(Result.Error(MSG.TransferFailed)),
            };

        tenantCtx.Transactions.Update(transaction);
        await tenantCtx.SaveChangesAsync();
        await mediator.Publish(new TransferCreatedEvent(transaction));
        return Ok(M.ClientResponseModel.From(transaction));
    }

    /// <summary>
    /// Check if transfer from Wallet to Trade Account is possible
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("check/to/trade-account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CanTransferFromWalletToTradeAccount([FromBody] M.ClientRequestModel spec)
    {
        if (spec.Amount < 100)
            return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));

        var aid = await tradingService.TradeAccountLookupByUidAsync(spec.TradeAccountUid);
        if (aid == 0) return NotFound();
        var result =
            await accountingService.CanTransactionFromWalletToTradeAccountAsync(GetPartyId(), spec.WalletId, aid,
                spec.Amount);
        return result switch
        {
            TransactionCheckStatus.Passed => NoContent(),
            TransactionCheckStatus.WalletNotFound => BadRequest(Result.Error(MSG.WalletInvalided)),
            TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
            TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
            TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
            _ => BadRequest(Result.Error(MSG.TransferFailed))
        };
    }

    /// <summary>
    /// Check if transfer from Trade Account to Wallet is possible
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("check/to/wallet")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CanTransferFromTradeAccountToWallet([FromBody] M.ClientRequestModel spec)
    {
        if (spec.Amount < 100)
            return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));

        var aid = await tradingService.TradeAccountLookupByUidAsync(spec.TradeAccountUid);
        if (aid == 0) return NotFound();
        var result =
            await accountingService.CanTransactionFromTradeAccountToWalletAsync(GetPartyId(), aid, spec.WalletId);
        return result switch
        {
            TransactionCheckStatus.Passed => NoContent(),
            TransactionCheckStatus.WalletNotFound => BadRequest(Result.Error(MSG.WalletInvalided)),
            TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
            TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
            TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
            _ => BadRequest(Result.Error(MSG.TransferFailed)),
        };
    }
}