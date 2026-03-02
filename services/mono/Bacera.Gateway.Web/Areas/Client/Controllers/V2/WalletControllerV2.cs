using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Email.ViewModel;
using Bacera.Gateway.Services.Withdraw;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Middlewares;
using Bacera.Gateway.Web.Request;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using MSG = Bacera.Gateway.ResultMessage.Transaction;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Wallet")]
[Area("Client")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
[Route("api/" + VersionTypes.V2 + "/[Area]/wallet")]
public class WalletControllerV2(
    TenantDbContext tenantCtx,
    WalletService walletSvc,
    AcctService acctSvc,
    PaymentMethodService paymentMethodSvc,
    WithdrawalService withdrawalSvc,
    IMediator mediator,
    ISendMailService sendMailService,
    UserService userSvc,
    ILogger<WalletControllerV2> logger)
    : ClientBaseControllerV2
{
    /// <summary>
    /// Query Client Wallets
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Wallet.ClientCriteria? criteria = null)
    {
        criteria ??= new Wallet.ClientCriteria();
        criteria.PartyId = GetPartyId();
        var items = await walletSvc.QueryForClientAsync(criteria);
        return Ok(Result<List<Wallet.ClientPageModel>, Wallet.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Query Wallet Transfer
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpGet("{hashId}/transfer")]
    public async Task<IActionResult> WalletTransferIndex(string hashId, [FromQuery] Transaction.ClientCriteria? criteria = null)
    {
        var id = Wallet.HashDecode(hashId);
        criteria ??= new Transaction.ClientCriteria();
        criteria.WalletId = id;
        var items = await acctSvc.QueryForTransactionClientAsync(criteria);
        return Ok(Result<List<Transaction.ClientPageModel>, Transaction.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Query Wallet Withdrawal
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpGet("{hashId}/withdrawal")]
    public async Task<IActionResult> WalletWithdrawalIndex(string hashId, [FromQuery] Withdrawal.ClientCriteria? criteria = null)
    {
        var id = Wallet.HashDecode(hashId);
        var wallet = await tenantCtx.Wallets
            .Where(x => x.Id == id)
            .Select(x => new { x.PartyId, x.CurrencyId, x.FundType })
            .SingleOrDefaultAsync();
        if (wallet == null) return BadRequest(Result.Error("Wallet not found"));

        criteria ??= new Withdrawal.ClientCriteria();
        criteria.AccountId = null;
        criteria.PartyId = wallet.PartyId;
        criteria.CurrencyId = (CurrencyTypes?)wallet.CurrencyId;
        criteria.FundType = (FundTypes?)wallet.FundType;

        var items = await withdrawalSvc.QueryForClientAsync(criteria);
        return Ok(Result<List<Withdrawal.ClientPageModel>, Withdrawal.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Create Withdrawal
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpPost("{hashId}/withdrawal")]
    public async Task<IActionResult> CreateWithdrawal(string hashId, [FromBody] CreateWithdrawalRequestModel spec)
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

        var method = await paymentMethodSvc.GetMethodByIdAsync(spec.PaymentMethodId);
        if (method == null) return BadRequest(Result.Error("Payment method not found"));

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
                if (!addressValid) return BadRequest(Result.Error("Invalid wallet address"));
            }
            catch
            {
                return BadRequest(Result.Error("Validate wallet address failed"));
            }
        }

        spec.Amount = spec.Amount.ToScaledFromCents();

        var id = Wallet.HashDecode(hashId);
        var isAmountValid = await withdrawalSvc.IsAmountValidForWalletWithdrawal(id, spec.Amount);
        if (!isAmountValid) return BadRequest(Result.Error("Amount not valid"));

        Withdrawal? withdrawal = await withdrawalSvc.CreateWithdrawalForWalletAsync(spec.PaymentMethodId, id,
            spec.Amount, spec.Request, spec.Note, GetPartyId());
        if (withdrawal == null) return BadRequest(Result.Error("Failed to create withdrawal"));

        await mediator.Publish(new WithdrawalCreatedEvent(withdrawal));
        return Ok();
    }
    
    /// <summary>
    /// Query Wallet Rebate
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpGet("{hashId}/rebate")]
    public async Task<IActionResult> WalletRebateIndex(string hashId, [FromQuery] Rebate.ClientCriteria? criteria = null)
    {
        var walletId = Wallet.HashDecode(hashId);
        criteria ??= new Rebate.ClientCriteria();
        criteria.WalletId = walletId;
        var items = await acctSvc.QueryRebateForClientAsync(criteria);
        return Ok(Result<List<Rebate.ClientPageModel>, Rebate.ClientCriteria>.Of(items, criteria));
    }
    
    /// <summary>
    /// Query Wallet Refund
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpGet("{hashId}/refund")]
    public async Task<IActionResult> WalletRefundIndex(string hashId, [FromQuery] Refund.ClientCriteria? criteria = null)
    {
        var walletId = Wallet.HashDecode(hashId);
        criteria ??= new Refund.ClientCriteria();
        criteria.TargetType = RefundTargetTypes.Wallet;
        criteria.TargetId = walletId;
        var items = await acctSvc.QueryRefundForClientAsync(criteria);
        return Ok(Result<List<Refund.ClientPageModel>, Refund.ClientCriteria>.Of(items, criteria));
    }
    
    /// <summary>
    /// Query Wallet Adjust
    /// </summary>
    /// <param name="hashId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpGet("{hashId}/adjust")]
    public async Task<IActionResult> WalletAdjustIndex(string hashId, [FromQuery] WalletAdjust.ClientCriteria? criteria = null)
    {
        var walletId = Wallet.HashDecode(hashId);
        criteria ??= new WalletAdjust.ClientCriteria();
        criteria.WalletId = walletId;
        var items = await acctSvc.QueryWalletAdjustForClientAsync(criteria);
        return Ok(Result<List<WalletAdjust.ClientPageModel>, WalletAdjust.ClientCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Query Wallet-to-Wallet Reward Transfers 
    /// For IB/Sales: Shows outgoing transfers to downline AND incoming transfers (paginated)
    /// For regular clients: Shows all incoming and outgoing wallet-to-wallet transfers (paginated)
    /// </summary>
    /// <param name="hashId">Wallet hash ID</param>
    /// <param name="criteria">Optional filtering criteria (supports pagination)</param>
    /// <returns>Paginated list of wallet-to-wallet transfers</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ClientWalletFilter))]
    [HttpGet("{hashId}/downline-reward-transfer")]
    public async Task<IActionResult> WalletDownlineRewardTransferIndex(string hashId, [FromQuery] Transaction.ClientCriteria? criteria = null)
    {
        var currentWalletId = Wallet.HashDecode(hashId);
        var currentPartyId = GetPartyId();

        // Validate wallet belongs to current user
        var wallet = await tenantCtx.Wallets
            .Where(x => x.Id == currentWalletId && x.PartyId == currentPartyId)
            .Select(x => new { x.Id, x.PartyId })
            .FirstOrDefaultAsync();

        if (wallet == null)
            return BadRequest(Result.Error("Wallet not found or does not belong to current user"));

        // Initialize criteria with defaults if not provided
        criteria ??= new Transaction.ClientCriteria();
        var skip = (criteria.Page - 1) * criteria.Size;

        // Step 1: Build base query for all wallet-to-wallet transfers
        IQueryable<Transaction> allWalletToWalletTransactions = GetWalletToWalletBaseQuery();

        // Step 2: Get IB/Sales account UIDs
        List<long> currentIBSalesAccountUids = await GetCurrentIBSalesAccountUids(currentPartyId);
        
        // Step 3: Get downline wallet IDs if user is IB/Sales
        List<long> downlineWalletIds = currentIBSalesAccountUids.Any() 
            ? await GetDownlineWalletIds(currentIBSalesAccountUids)
            : new List<long>();
        
        // Step 4: Build filtered query based on user type (IB/Sales vs Client)
        IQueryable<Transaction> filteredTransactions = GetRelatedTransactions(
            currentWalletId, 
            allWalletToWalletTransactions, 
            currentIBSalesAccountUids,
            downlineWalletIds);

        // Step 5: Get total count BEFORE pagination
        var totalCount = await filteredTransactions.CountAsync();

        // Step 6: Get paginated data
        var paginatedTransactions = await filteredTransactions
            .OrderByDescending(x => x.CreatedOn)
            .Skip(skip)
            .Take(criteria.Size)
            .ToClientPageModel()
            .ToListAsync();

        // Step 7: Enrich with wallet info and flow type
        EnrichPaginatedTransactions(currentWalletId, paginatedTransactions);

        // Step 8: Update criteria with pagination info
        criteria.Total = totalCount;
        criteria.PageCount = (int)Math.Ceiling((double)totalCount / criteria.Size);

        return Ok(Result<List<Transaction.ClientPageModel>, Transaction.ClientCriteria>.Of(paginatedTransactions, criteria));
    }

    private void EnrichPaginatedTransactions(long currentWalletId, List<Transaction.ClientPageModel> paginatedTransactions)
    {
        if (paginatedTransactions.Any())
        {
            foreach (var item in paginatedTransactions)
            {
                // Determine flow type
                if (item.SourceId == currentWalletId)
                {
                    item.FlowType = "out"; // Current wallet is sender
                }
                else if (item.TargetId == currentWalletId)
                {
                    item.FlowType = "in"; // Current wallet is receiver
                }

                // Reset account numbers (these are wallet transfers)
                item.SourceAccountNumber = 0;
                item.TargetAccountNumber = 0;
            }
        }
    }

    private IQueryable<Transaction> GetRelatedTransactions(
        long currentWalletId, 
        IQueryable<Transaction> allWalletToWalletTransactions, 
        List<long> currentIBSalesAccountUids,
        List<long> downlineWalletIds)
    {
        if (currentIBSalesAccountUids.Any())
        {
            // IB/Sales user: Show outgoing transfers to downline + all incoming transfers
            return allWalletToWalletTransactions.Where(x =>
                (x.SourceAccountId == currentWalletId && downlineWalletIds.Contains(x.TargetAccountId)) || // Outgoing to downline
                (x.TargetAccountId == currentWalletId)); // Any incoming
        }
        else
        {
            // Regular client: Show all transfers where current wallet is source OR target
            return allWalletToWalletTransactions.Where(x =>
                x.SourceAccountId == currentWalletId ||
                x.TargetAccountId == currentWalletId);
        }
    }

    private async Task<List<long>> GetDownlineWalletIds(List<long> currentIBSalesAccountUids)
    {
        var currentUidPatterns = currentIBSalesAccountUids.Select(uid => $".{uid}.").ToList();

        // Get all downline party IDs
        var downlinePartyIds = await tenantCtx.Accounts
            .Where(a => currentUidPatterns.Any(pattern => a.ReferPath.Contains(pattern)))
            .Select(a => a.PartyId)
            .Distinct()
            .ToListAsync();

        // Get downline wallet IDs
        return await tenantCtx.Wallets
            .Where(w => downlinePartyIds.Contains(w.PartyId) && w.IsPrimary == 1)
            .Select(w => w.Id)
            .ToListAsync();
    }

    private async Task<List<long>> GetCurrentIBSalesAccountUids(long currentPartyId)
    {
        // Only get IB/Sales accounts (exclude Client role 400)
        return await tenantCtx.Accounts
            .Where(x => x.PartyId == currentPartyId && x.Role != (short)AccountRoleTypes.Client)
            .Select(x => x.Uid)
            .ToListAsync();
    }

    private IQueryable<Transaction> GetWalletToWalletBaseQuery()
    {
        return tenantCtx.Transactions
            .Where(x => x.SourceAccountType == (short)TransactionAccountTypes.Wallet &&
                       x.TargetAccountType == (short)TransactionAccountTypes.Wallet);
    }
}