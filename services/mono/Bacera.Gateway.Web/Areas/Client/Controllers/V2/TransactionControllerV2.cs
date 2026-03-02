using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Request;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Transaction")]
[Area("Client")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = UserRoleTypesString.ClientOrTenantAdmin)]
[Route("api/" + VersionTypes.V2 + "/[Area]/transaction")]
public class TransactionControllerV2(
    TenantDbContext tenantCtx,
    ITradingApiService tradingApiSvc,
    AcctService acctSvc,
    AccountManageService accManSvc,
    IMediator mediator)
    : ClientBaseControllerV2
{
    /// <summary>
    /// Transfer from Wallet to Trade Account
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("to-trade-account")]
    public async Task<IActionResult> TransferFromWalletToTradeAccount([FromBody] CreateTransactionRequestModel spec)
    {
        var partyId = GetPartyId();
        if (spec.Amount < 100) return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));

        var hasPending = await tenantCtx.Transactions
            .Where(x => x.PartyId == partyId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.TransferCreated ||
                        x.IdNavigation.StateId == (int)StateTypes.TransferAwaitingApproval)
            .AnyAsync();

        if (hasPending) return BadRequest(Result.Error(ResultMessage.Transaction.PendingTransferExist));

        var walletId = Wallet.HashDecode(spec.WalletHashId);
        var wallet = await tenantCtx.Wallets
            .Where(x => x.PartyId == partyId && x.Id == walletId)
            .Select(x => new
            {
                x.Balance, x.CurrencyId, x.Id, x.FundType,
                acc = x.Accounts.FirstOrDefault(a => a.Uid == spec.TradeAccountUid)
            })
            .SingleOrDefaultAsync();
        if (wallet == null) return NotFound(Result.Error("Wallet not found"));

        // Get account details (allow different currencies now)
        var account = wallet.acc != null 
            ? new { Id = wallet.acc.Id, CurrencyId = wallet.acc.CurrencyId, FundType = wallet.acc.FundType }
            : await tenantCtx.Accounts
                .Where(x => x.Uid == spec.TradeAccountUid && x.PartyId == partyId)
                .Select(x => new { x.Id, x.CurrencyId, x.FundType })
                .SingleOrDefaultAsync();
        
        if (account == null) return NotFound(Result.Error("Account not found"));

        // Validate fund types must match
        if (wallet.FundType != account.FundType)
            return BadRequest(Result.Error("Fund type mismatch between wallet and account"));

        // Calculate required wallet amount considering exchange rate
        long requiredWalletAmount = spec.Amount;
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
        }

        // Fixed balance check - wallet balance is in 10000x scale, spec.Amount is in cents
        if (wallet.Balance < (requiredWalletAmount * 100).ToScaledFromCents()) // Convert cents to wallet scale (10000x)
            return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));

        // Link wallet to account if not already linked
        if (wallet.acc == null)
        {
            var accountToUpdate = await tenantCtx.Accounts
                .Where(x => x.Id == account.Id)
                .SingleOrDefaultAsync();
            if (accountToUpdate != null)
            {
                accountToUpdate.WalletId = walletId;
                accountToUpdate.UpdatedOn = DateTime.UtcNow;
            }
        }

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

        if (transaction == null) return BadRequest(Result.Error("Transaction failed"));
        
        await mediator.Publish(new TransferCreatedEvent(transaction));
        return Ok();

        // var account = await tenantCtx.Accounts
        //     .Where(x => x.PartyId == partyId && x.Uid == spec.TradeAccountUid)
        //     .Where(x => x.Status == (short)AccountStatusTypes.Activate)
        //     .Select(x => new { x.Id, x.CurrencyId, x.FundType })
        //     .SingleOrDefaultAsync();
        //
        // if (account == null) return BadRequest(Result.Error("Account not found"));
        //
        // var transaction = Transaction.Build(
        //     partyId,
        //     TransactionAccountTypes.Wallet,
        //     walletId,
        //     partyId,
        //     TransactionAccountTypes.Account,
        //     account.Id,
        //     LedgerSideTypes.Debit, spec.Amount,
        //     (FundTypes)wallet.FundType,
        //     (CurrencyTypes)wallet.CurrencyId
        // );
        //
        // transaction.IdNavigation.Activities.Add(new Activity
        // {
        //     PerformedOn = DateTime.UtcNow,
        //     OnStateId = 0,
        //     ToStateId = (int)StateTypes.TransferAwaitingApproval,
        //     PartyId = partyId,
        //     Data = string.Empty,
        //     ActionId = 1
        // });
        // tenantCtx.Transactions.Add(transaction);
        // await tenantCtx.SaveChangesAsync();

        // await mediator.Publish(new TransferCreatedEvent(transaction));
        // return Ok();
    }

    /// <summary>
    /// Available Trade Account for Trade Account to transfer
    /// </summary>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{accountUid:long}/available-transfer-account")]
    public async Task<IActionResult> GetAvailableTradeAccountForTransfer(long accountUid)
    {
        var partyId = GetPartyId();

        var sourceAccount = await tenantCtx.Accounts
            .Where(x => x.PartyId == partyId && x.Uid == accountUid && x.Status == (short)AccountStatusTypes.Activate)
            .Select(x => new { x.Uid, x.CurrencyId, x.FundType })
            .SingleOrDefaultAsync();

        if (sourceAccount == null) return NotFound(Result.Error("Account not found"));

        // Show all trade accounts regardless of currency (exchange rate conversion will be handled)
        var availableAccounts = await tenantCtx.Accounts
            .Where(x => x.PartyId == partyId &&
                        x.HasTradeAccount &&
                        x.Uid != sourceAccount.Uid &&
                        x.Role == (short)AccountRoleTypes.Client &&
                        x.Status == (short)AccountStatusTypes.Activate &&
                        x.FundType == sourceAccount.FundType) // Only fund type must match
            .Select(x => new 
            {
                x.Uid, 
                x.AccountNumber,
                x.CurrencyId,
                CurrencyName = ((CurrencyTypes)x.CurrencyId).ToString(),
                SameCurrency = x.CurrencyId == sourceAccount.CurrencyId
            })
            .ToListAsync();

        return Ok(availableAccounts);
    }
    
    /// <summary>
    /// Transfer from Trade Account to Wallet
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("between-trade-account")]
    [ProducesResponseType(typeof(Transaction.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferBetweenTradeAccount([FromBody] Transaction.CreateBetweenTradeAccountSpec spec)
    {
        if (spec.SourceTradeAccountUid == spec.TargetTradeAccountUid) return BadRequest(Result.Error(ResultMessage.Common.InvalidInput));
        if (spec.Amount < 100) return BadRequest(Result.Error(ResultMessage.Common.InvalidAmount));

        var partyId = GetPartyId();
        var hasPending = await tenantCtx.Transactions
            .Where(x => x.PartyId == partyId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.TransferCreated ||
                        x.IdNavigation.StateId == (int)StateTypes.TransferAwaitingApproval)
            .AnyAsync();

        if (hasPending) return BadRequest(Result.Error(ResultMessage.Transaction.PendingTransferExist));

        var accounts = await tenantCtx.Accounts
            .Where(x => x.PartyId == partyId && (x.Uid == spec.SourceTradeAccountUid || x.Uid == spec.TargetTradeAccountUid))
            .Where(x => x.Status == (short)AccountStatusTypes.Activate)
            .ToListAsync();

        var srcAccount = accounts.SingleOrDefault(x => x.Uid == spec.SourceTradeAccountUid);
        if (srcAccount == null) return NotFound("Source Account not found");
        if (!srcAccount.CanTransfer())
            return BadRequest(Result.Error("Transfer not allowed for source account"));
        
        var targetAccount = accounts.SingleOrDefault(x => x.Uid == spec.TargetTradeAccountUid);
        if (targetAccount == null) return NotFound("Target Account not found");
        if (!targetAccount.CanTransfer())
            return BadRequest(Result.Error("Transfer not allowed for target account"));

        if (srcAccount.CurrencyId != targetAccount.CurrencyId || srcAccount.FundType != targetAccount.FundType)
            return BadRequest(Result.Error(ResultMessage.Transaction.CurrencyNotMatched));


        var (sourceAccountStatus, _, sourceAccountBalance) =
            await tradingApiSvc.GetAccountBalanceAndLeverage(srcAccount.ServiceId, srcAccount.AccountNumber);
       
        if (!sourceAccountStatus)
            return BadRequest(Result.Error(ResultMessage.Transaction.TradeAccountInvalided));

        if (sourceAccountBalance < (spec.Amount / 100d).ToCentsFromScaled())
            return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));

        var (targetAccountStatus, _, _) =
            await tradingApiSvc.GetAccountBalanceAndLeverage(targetAccount.ServiceId, targetAccount.AccountNumber);
        if (!targetAccountStatus) return BadRequest(Result.Error(ResultMessage.Transaction.TradeAccountInvalided));

        var transaction = Transaction.Build(
            partyId,
            TransactionAccountTypes.Account,
            srcAccount.Id,
            partyId,
            TransactionAccountTypes.Account,
            targetAccount.Id,
            LedgerSideTypes.Debit, spec.Amount,
            (FundTypes)srcAccount.FundType,
            (CurrencyTypes)srcAccount.CurrencyId
        );

        transaction.IdNavigation.Activities.Add(new Activity
        {
            PerformedOn = DateTime.UtcNow,
            OnStateId = 0,
            ToStateId = (int)StateTypes.TransferAwaitingApproval,
            PartyId = partyId,
            Data = string.Empty,
            ActionId = 1
        });
        tenantCtx.Transactions.Add(transaction);
        await tenantCtx.SaveChangesAsync();
        await mediator.Publish(new TransferCreatedEvent(transaction));
        return Ok();
    }
}