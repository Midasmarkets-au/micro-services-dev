
using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = TradeAccount;
using MSG = Bacera.Gateway.ResultMessage.TradeAccount;

[Area("Client")]
[Route("api/" + VersionTypes.V1 + "/[Area]/trade-account")]
[Tags("Client/Trade Account")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class TradeAccountController : ClientBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly AccountingService _acctSvc;
    private readonly TenantDbContext _tenantDbContext;
    private readonly UserManager<User> _userManager;
    private readonly IApplicationTokenService _tokenService;


    public TradeAccountController(
        TradingService tradingService,
        AccountingService accountingService,
        UserManager<Auth.User> userManager,
        IApplicationTokenService tokenService, 
        TenantDbContext tenantDbContext)
    {
        _userManager = userManager;
        _acctSvc = accountingService;
        _tokenService = tokenService;
        _tenantDbContext = tenantDbContext;
        _tradingSvc = tradingService;
    }

    /// <summary>
    /// TradeAccount Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>>>> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await _tradingSvc.TradeAccountForClientQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Check target email and get primary USD wallet for wallet transfer (IB/Sales to child accounts)
    /// </summary>
    /// <param name="email">Target user's email</param>
    /// <returns>Wallet information including walletId, email, and name</returns>
    [HttpGet("check-target-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTargetWalletByEmail([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(Result.Error(MSG.EmailIsRequired));//"Email is required"

            var normalizedEmail = email.Trim().ToLower();

            // Step 1: Get current user's email for validation
            var currentPartyId = GetPartyId();
            var currentParty = await _tenantDbContext.Parties
                .Where(x => x.Id == currentPartyId)
                .Select(x => new { x.Email, x.Id })
                .FirstOrDefaultAsync();

            if (currentParty == null)
                return BadRequest(Result.Error("Current user not found"));

            // Step 1.1: Validate that user is not transferring to themselves
            if (currentParty.Email.ToLower() == normalizedEmail)
                return BadRequest(Result.Error(MSG.CannotTransferOwnAccount));//"You cannot transfer to your own account"

            // Step 1.2: Find target party by email
            var targetParty = await _tenantDbContext.Parties
                .Where(x => x.Email.ToLower() == normalizedEmail)
                .Select(x => new { x.Id, x.Name, x.Email })
            .FirstOrDefaultAsync();

            if (targetParty == null)
            return NotFound(Result.Error($"No party found for email: {email}"));

            // Step 2: Get current user's IB/Sales account UIDs (Role != 400, across all parties)
            // First, get all current user's IB/Sales accounts across all parties
            var currentUserAccounts = await _tenantDbContext.Accounts
                .Where(x => x.PartyId == currentPartyId && x.Role != (short)AccountRoleTypes.Client)
                .Select(x => new { x.PartyId, x.Uid, x.ReferPath })
                .ToListAsync();

            if (!currentUserAccounts.Any())
                return BadRequest(Result.Error(MSG.NoIBSalesAccount));//"Current user does not have an IB/Sales account"

            // Step 3: Find target accounts that match all criteria:
            // 1. ReferPath contains current user's UID (in hierarchy)
            // 2. Email matches target email
            // 3. Target account's UID is the LAST element in ReferPath (ensures we get the leaf account, not intermediate)
            // 4. Has a primary wallet

            // Build patterns for current user UIDs
            var currentUidPatterns = currentUserAccounts.Select(x => $".{x.Uid}.").ToList();
            
            // Query to find matching accounts with wallet information
            var matchingAccounts = await _tenantDbContext.Accounts
                .Where(a => 
                    // ReferPath contains any current user's UID
                    currentUidPatterns.Any(pattern => a.ReferPath.Contains(pattern))
                    // Party email matches (case-insensitive)
                    && a.Party.Email.ToLower() == normalizedEmail
                    // Has a primary wallet
                    && a.Wallet != null && a.Wallet.IsPrimary == 1
                )
                .Select(a => new
                {
                    a.Id,
                    a.Uid,
                    a.PartyId,
                    a.ReferPath,
                    Email = a.Party.Email,
                    PartyName = a.Party.Name,
                    WalletId = a.WalletId,
                    WalletCurrencyId = a.Wallet != null ? a.Wallet.CurrencyId : 0
                })
                .ToListAsync();

            if (!matchingAccounts.Any())
                return Ok(new
                {
                    success = false,
                    message = $"No valid accounts found for email: {email}",
                    data = ""
                });

            // Step 4: Validate each matching account
            // The target account's UID must be the LAST element in its ReferPath (leaf node)
            // and must appear AFTER current user's UID in the path
            var validAccount = matchingAccounts.FirstOrDefault(account =>
            {
                // Split ReferPath by '.' and get the last non-empty element
                var pathParts = account.ReferPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
                var lastUidInPath = pathParts.Length > 0 ? pathParts[^1] : "";
                
                // Check if this account's UID is the last element in ReferPath (leaf node)
                if (lastUidInPath != account.Uid.ToString())
                    return false;
                
                // Verify that this account appears AFTER current user's UID in the hierarchy
                foreach (var currentAccount in currentUserAccounts)
                {
                    var currentUidPattern = $".{currentAccount.Uid}.";
                    var currentUidIndex = account.ReferPath.IndexOf(currentUidPattern);
                    
                    if (currentUidIndex == -1) continue;
                    
                    // Get the portion AFTER current UID
                    var afterCurrentUid = account.ReferPath.Substring(currentUidIndex + currentUidPattern.Length);
                    
                    // Check if this account's UID appears in the "after" portion
                    if (afterCurrentUid.Contains($".{account.Uid}") || afterCurrentUid.EndsWith(account.Uid.ToString()))
                    {
                        return true;
                    }
                }
                
                return false;
            });

            if (validAccount == null)
                return BadRequest(Result.Error(MSG.AccountIsNotDownline));//"Target account is not in your downline or does not meet the criteria. You can only transfer to your child IB/Sales/Client accounts."

            // Step 5: Use the wallet from the validated account
            // The account already has a primary wallet (validated in the query)
            var walletId = validAccount.WalletId ?? 0;
            
            if (walletId == 0)
            {
                // This shouldn't happen due to our query filter, but handle it just in case
                return BadRequest(Result.Error(MSG.TargetAccountNoPrimaryWallet));//"Target account does not have a valid primary wallet"
            }

            // Step 6: Return wallet information
            return Ok(new
            {
                walletId = walletId,
                email = validAccount.Email,
                name = validAccount.PartyName,
                currencyId = validAccount.WalletCurrencyId,
                accountUid = validAccount.Uid,
                partyId = validAccount.PartyId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Error($"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get TradeAccount by uid
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> TradeAccount(long uid)
    {
        var item = await _tradingSvc.TradeAccountGetForPartyAsync(uid, GetPartyId());
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// TradeTransaction pagination (trade history)
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/trade")]
    [ProducesResponseType(typeof(Result<List<TradeViewModel>, TradeViewModel.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> TradeTransaction(long uid, [FromQuery] TradeViewModel.Criteria? criteria)
    {
        var partyId = GetPartyId();
        var tradeAccount = await _tenantDbContext.Accounts.Where(x => x.Uid == uid && x.PartyId == partyId)
            .Select(x => new { x.ServiceId, x.AccountNumber })
            .SingleOrDefaultAsync();
        if (tradeAccount == null) return NotFound();

        criteria ??= new TradeViewModel.Criteria();
        criteria.ServiceId = tradeAccount.ServiceId;
        criteria.AccountNumber = tradeAccount.AccountNumber;
        criteria.Commands = [0, 1];
        return Ok(await _tradingSvc.QueryTrade(criteria));
    }

    /// <summary>
    /// Request OTP for change trade account password
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpPost("{uid:long}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RequestChangePassword(long uid)
    {
        var tokenRequest = ApplicationToken.Build(GetPartyId(), TokenTypes.TradeAccountChangePasswordToken, uid);

        var user = await _userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == GetPartyId() && x.TenantId == GetTenantId());
        if (user == null)
            return BadRequest(Result.Error(ResultMessage.Common.UserNotFound));

        var token = await _tokenService.GenerateTokenAsync(tokenRequest, TimeSpan.FromHours(1));
        if (token == null)
            return BadRequest(Result.Error(ResultMessage.Common.TokenGenerateFail));

        return Ok(new { Token = token });
    }

    /// <summary>
    /// Transaction pagination (accounting history)
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/transaction")]
    [ProducesResponseType(typeof(Result<List<Transaction.ClientResponseModel>, Transaction.Criteria>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<List<Transaction.ClientResponseModel>, Transaction.Criteria>>>
        AccountingTransaction(long uid, [FromQuery] Transaction.Criteria? criteria)
    {
        var accountId = await _tradingSvc.TradeAccountLookupByUidAsync(uid);
        if (accountId == 0)
            return NotFound();

        criteria ??= new Transaction.Criteria();
        criteria.PartyId = GetPartyId();
        criteria.AccountId = accountId;
        var result = await _acctSvc.TransactionQueryForClientAsync(GetPartyId(), criteria);
        var res = Result<List<Transaction.ClientResponseModel>, Transaction.Criteria>.Of(result.Data, criteria);
        return Ok(res);
    }

    /// <summary>
    /// Check TradeAccount and Email Exist
    /// </summary>
    /// <returns></returns>
    [HttpGet("wholesale-referral-check")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> WholesaleReferralCheck(
        [FromQuery] M.WholesaleReferralInfo spec)
    {
        var tenantId = GetTenantId();
        var user = await _userManager.Users
            .Where(x => x.TenantId == tenantId && x.Email == spec.Email)
            .Select(x => new
            {
                x.PartyId,
                Roles = x.UserRoles.Where(y => y.ApplicationRole.Name == UserRoleTypesString.Wholesale)
                    .Select(r => r.ApplicationRole.Name)
            })
            .SingleOrDefaultAsync();

        if (user == null) return BadRequest(Result.Error(ResultMessage.User.UserNotFound));
        if (user.Roles.Contains(UserRoleTypesString.Wholesale))
            return BadRequest(Result.Error(ResultMessage.Account.AccountIsWholesale));

        var account = await _tenantDbContext.Accounts
            .Where(x => x.AccountNumber == spec.AccountNumber && x.PartyId == user.PartyId)
            .AnyAsync();

        if (!account) return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));
        return Ok();
    }
}