using System.Security.Authentication;
using System.Text;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

using M = User;

public class UserService(
    TenantDbContext tenantCtx,
    AuthDbContext authCtx,
    CentralDbContext centralCtx,
    IMyCache cache,
    MyDbContextPool pool,
    ITenantGetter tenantGetter)
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    private static readonly string RolesCacheKey = CacheKeys.GetRolesKey();
    private readonly string _nameKey = CacheKeys.GetBlackedUserNameHashKey();
    private readonly string _phoneKey = CacheKeys.GetBlackedUserPhoneHashKey();
    private readonly string _emailKey = CacheKeys.GetBlackedUserEmailHashKey();
    private readonly string _idNumberKey = CacheKeys.GetBlackedUserIdNumberHashKey();
    private readonly string _ipKey = CacheKeys.GetBlackedIpHashKey();

    public Task<M.TenantDetailModel> GetPartyAsync(long partyId, bool hideEmail = false) => tenantCtx.Parties
        .Where(x => x.Id == partyId)
        .ToTenantDetailModel()
        .SingleAsync();

    //
    // cache.HGetOrSetAsync(
    //     CacheKeys.GetPartyIdToPartyHashKey(_tenantId)
    //     , partyId.ToString()
    //     , async () =>
    //         await tenantCtx.Parties.ToTenantDetailModel(hideEmail)
    //             .SingleOrDefaultAsync(x => x.Id == partyId) ?? new M.TenantDetailModel()
    //     , TimeSpan.FromDays(1));

    public Task<List<ApplicationRole.BasicModel>> GetAllRolesAsync() => cache.GetOrSetAsync(RolesCacheKey
        , () => authCtx.ApplicationRoles.Select(x => new ApplicationRole.BasicModel { Id = x.Id, Name = x.Name }).ToListAsync()
        , TimeSpan.FromDays(1));

    // hasRole?
    public async Task<bool> HasRoleAsync(long partyId, string roleName)
    {
        return await authCtx.UserRoles
            .Where(x => x.User.TenantId == _tenantId && x.User.PartyId == partyId)
            .Where(x => x.ApplicationRole.Name == roleName)
            .AnyAsync();
    }

    public async Task<List<string>> GetUserRolesAsync(long partyId)
    {
        var roles = await authCtx.UserRoles
            .Where(x => x.User.TenantId == _tenantId && x.User.PartyId == partyId)
            .Select(x => x.ApplicationRole.Name)
            .ToListAsync();
        return roles.Select(x => x ?? "").ToList();
    }

    public async Task<bool> AddRoleAsync(long partyId, string roleName, long operatorPartyId)
    {
        var roleId = await authCtx.ApplicationRoles
            .Where(x => x.Name == roleName)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (roleId == 0) return false;

        var userId = await authCtx.Users
            .Where(x => x.TenantId == _tenantId && x.PartyId == partyId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (userId == 0) return false;

        var exists = await authCtx.UserRoles
            .Where(x => x.UserId == userId && x.RoleId == roleId)
            .AnyAsync();
        if (exists) return true;

        authCtx.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });
        await authCtx.SaveChangesWithAuditAsync(operatorPartyId);
        return true;
    }

    public async Task<bool> RemoveRoleAsync(long partyId, string roleName, long operatorPartyId)
    {
        var roleId = await authCtx.ApplicationRoles
            .Where(x => x.Name == roleName)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (roleId == 0) return false;

        var userId = await authCtx.Users
            .Where(x => x.TenantId == _tenantId && x.PartyId == partyId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (userId == 0) return false;

        var userRole = await authCtx.UserRoles
            .Where(x => x.UserId == userId && x.RoleId == roleId)
            .SingleOrDefaultAsync();
        if (userRole == null) return true;

        authCtx.UserRoles.Remove(userRole);
        await authCtx.SaveChangesWithAuditAsync(operatorPartyId);
        return true;
    }

    public async Task LockUserAsync(long partyId, long operatorPartyId, DateTimeOffset? dateTimeOffset = null)
    {
        var user = await authCtx.Users.SingleAsync(x => x.TenantId == _tenantId && x.PartyId == partyId);
        user.LockoutEnabled = true;
        user.LockoutEnd = dateTimeOffset ?? DateTimeOffset.UtcNow.AddYears(10);
        user.UpdatedOn = DateTime.UtcNow;
        authCtx.Users.Update(user);

        _ = SetLockStatusCacheAsync(partyId, true);
        await authCtx.SaveChangesWithAuditAsync(operatorPartyId);
    }

    public async Task UnlockUserAsync(long partyId, long operatorPartyId)
    {
        var user = await authCtx.Users.SingleAsync(
            x => x.TenantId == _tenantId && x.PartyId == partyId);
        user.LockoutEnabled = true;
        user.LockoutEnd = null;
        user.UpdatedOn = DateTime.UtcNow;
        authCtx.Users.Update(user);
        _ = SetLockStatusCacheAsync(partyId, false);
        await authCtx.SaveChangesWithAuditAsync(operatorPartyId);
    }

    public async Task<bool> IsUserLockedOutAsync(long partyId)
    {
        var value = await cache.HGetStringAsync(CacheKeys.GetUserLockedHashKey(_tenantId), partyId.ToString());
        if (value is "1") return true;

        var valueFromDb = await authCtx.Users
            .Where(x => x.TenantId == _tenantId && x.PartyId == partyId)
            .Where(x => x.LockoutEnd > DateTime.UtcNow)
            .AnyAsync();

        _ = SetLockStatusCacheAsync(partyId, valueFromDb);
        return valueFromDb;
    }

    private Task SetLockStatusCacheAsync(long partyId, bool isLocked)
        => cache.HSetStringAsync(CacheKeys.GetUserLockedHashKey(_tenantId), partyId.ToString(), isLocked ? "1" : "0", TimeSpan.FromDays(7));

    public Task BanVerificationQuizAsync(long partyId) => cache.SetStringAsync(GetBannedCacheKey(partyId),
        DateTime.UtcNow.AddDays(1).ToLongDateString(),
        TimeSpan.FromDays(1)
    );

    public async Task<bool> IsBannedVerificationQuizAsync(long partyId)
        => await cache.GetStringAsync(GetBannedCacheKey(partyId)) != null;

    public async Task AllowVerificationQuizAsync(long partyId)
        => await cache.KeyDeleteAsync(GetBannedCacheKey(partyId));

    public async Task ApplyUserBlackListInfo(List<TenantUserBasicModel> items)
    {
        foreach (var item in items)
        {
            item.IsInIpBlackList = await cache.HGetStringAsync(_ipKey, item.LastLoginIp) == "1";
            item.IsInUserBlackList = await cache.HGetStringAsync(_nameKey, item.NativeName) == "1"
                                     || await cache.HGetStringAsync(_phoneKey, item.Phone) == "1"
                                     || await cache.HGetStringAsync(_emailKey, item.Email) == "1"
                                     || await cache.HGetStringAsync(_idNumberKey, item.IdNumber) == "1";
        }
    }

    public async Task UpdateSearchAsync(M.Criteria criteria)
    {
        criteria.Page = 1;
        criteria.Size = 1000;
        criteria.SortField = "Id";
        criteria.SortFlag = false;
        var tenantId = _tenantId;
        var total = 0;
        do
        {
            var users = await authCtx.Users
                .Where(x => x.TenantId == tenantId)
                .PagedFilterBy(criteria)
                .ToListAsync();

            var parties = await tenantCtx.Parties
                .Where(x => users.Select(u => u.PartyId).Contains(x.Id))
                .Include(x => x.Accounts.Where(y => y.Status == (int)AccountStatusTypes.Activate))
                .ToListAsync();

            total += users.Count;

            foreach (var t in parties)
            {
                var party = t;
                var user = users.Single(x => x.PartyId == party.Id);
                user.ApplyToParty(ref party);
                party.UpdateSearchText();
                await cache.HSetDeleteByKeyFieldAsync(CacheKeys.GetPartyIdToPartyHashKey(_tenantId), party.Id.ToString());
                tenantCtx.Parties.Update(party);

                foreach (var account in t.Accounts)
                {
                    account.UpdateSearchText();
                    tenantCtx.Accounts.Update(account);
                }
            }

            await tenantCtx.SaveChangesAsync();
            criteria.Page++;
        } while (total == criteria.Size);
    }

    private string GetBannedCacheKey(long partyId) => $"verification_banned_T{_tenantId}_P{partyId}_util";

    public async Task<bool> ValidateAuthCodeAsync(string @event, AuthCodeMethodTypes methodType, string methodValue, string code)
    {
        var authCode = await tenantCtx.AuthCodes
            .Where(x => x.Event == @event)
            .Where(x => x.Method == (short)methodType && x.MethodValue == methodValue)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == code)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (authCode == null) return false;
  
        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        tenantCtx.AuthCodes.Update(authCode);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Check if user's email or phone number was changed within the last 24 hours.
    /// This is a security measure similar to OKX withdrawal restrictions.
    /// </summary>
    public async Task<bool> CheckRecentEmailOrPhoneChangeAsync(long partyId)
    {
        var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);

        // Get all UserAudit records for this party within last 24 hours
        var recentAudits = await authCtx.UserAudits
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Type == (int)AuditTypes.User)
            .Where(x => x.Action == (int)AuditActionTypes.Update)
            .Where(x => x.CreatedOn >= twentyFourHoursAgo)
            .Select(x => x.Data)
            .ToListAsync();

        if (!recentAudits.Any())
            return false;

        // Check each audit record for email or phone number changes
        foreach (var auditDataJson in recentAudits)
        {
            try
            {
                var entityChanges = JsonConvert.DeserializeObject<Audit.EntityChanges>(auditDataJson);
                if (entityChanges == null)
                    continue;

                // Check if Email, PhoneNumber, or CCC (country code) was changed
                var hasEmailChange = entityChanges.OriginalValues.ContainsKey("Email") &&
                                     entityChanges.CurrentValues.ContainsKey("Email") &&
                                     entityChanges.OriginalValues["Email"]?.ToString() != 
                                     entityChanges.CurrentValues["Email"]?.ToString();

                var hasPhoneChange = (entityChanges.OriginalValues.ContainsKey("PhoneNumber") &&
                                      entityChanges.CurrentValues.ContainsKey("PhoneNumber") &&
                                      entityChanges.OriginalValues["PhoneNumber"]?.ToString() != 
                                      entityChanges.CurrentValues["PhoneNumber"]?.ToString()) ||
                                     (entityChanges.OriginalValues.ContainsKey("CCC") &&
                                      entityChanges.CurrentValues.ContainsKey("CCC") &&
                                      entityChanges.OriginalValues["CCC"]?.ToString() != 
                                      entityChanges.CurrentValues["CCC"]?.ToString());

                if (hasEmailChange || hasPhoneChange)
                {
                    return true;
                }
            }
            catch
            {
                // Continue checking other audit records if one fails to parse
            }
        }

        return false;
    }

    /// <summary>
    /// Record a password change in the UserAudit trail.
    /// UserManager.ResetPasswordAsync uses SaveChangesAsync internally (not SaveChangesWithAuditAsync),
    /// so we create the audit record manually.
    /// </summary>
    public async Task RecordPasswordChangeAuditAsync(long partyId, long userId)
    {
        var changes = Audit.EntityChanges.Create(
            new Dictionary<string, object> { { "PasswordHash", "***" } },
            new Dictionary<string, object> { { "PasswordHash", "***changed***" } }
        );
        var audit = new UserAudit
        {
            PartyId = partyId,
            RowId = userId,
            Type = (int)AuditTypes.User,
            Action = (int)AuditActionTypes.Update,
            CreatedOn = DateTime.UtcNow,
            Environment = System.Net.Dns.GetHostName(),
            Data = changes.ToJson()
        };
        authCtx.UserAudits.Add(audit);
        await authCtx.SaveChangesAsync();
    }

    /// <summary>
    /// Check if user's password was changed within the last 24 hours.
    /// </summary>
    public async Task<bool> CheckRecentPasswordChangeAsync(long partyId)
    {
        var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);

        var recentAudits = await authCtx.UserAudits
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Type == (int)AuditTypes.User)
            .Where(x => x.Action == (int)AuditActionTypes.Update)
            .Where(x => x.CreatedOn >= twentyFourHoursAgo)
            .Select(x => x.Data)
            .ToListAsync();

        if (!recentAudits.Any())
            return false;

        foreach (var auditDataJson in recentAudits)
        {
            try
            {
                var entityChanges = JsonConvert.DeserializeObject<Audit.EntityChanges>(auditDataJson);
                if (entityChanges == null)
                    continue;

                var hasPasswordChange = entityChanges.OriginalValues.ContainsKey("PasswordHash") &&
                                        entityChanges.CurrentValues.ContainsKey("PasswordHash") &&
                                        entityChanges.OriginalValues["PasswordHash"]?.ToString() !=
                                        entityChanges.CurrentValues["PasswordHash"]?.ToString();

                if (hasPasswordChange)
                    return true;
            }
            catch
            {
                // Continue checking other audit records if one fails to parse
            }
        }

        return false;
    }

    public async Task<(bool, string)> DuplicateUserToOtherTenantAsync(long partyId, long toTid,
        bool includeVerification = false,
        long operatorPartyId = 1)
    {
        await using var targetTenantCtx = pool.CreateTenantDbContext(toTid);

        var newUser = await DuplicateUserByPartyIdAsync(targetTenantCtx, partyId, toTid, operatorPartyId);
        if (newUser == null) return (false, "Duplicate user failed");

        if (!includeVerification) return (true, "");

        var (verificationResult, msg) =
            await DuplicateUserVerificationByNewUserAsync(targetTenantCtx, partyId, newUser);
        if (!verificationResult) return (false, msg);

        var guestRole = await authCtx.UserRoles
            .Where(x => x.UserId == newUser.Id)
            .Where(x => x.RoleId == (long)UserRoleTypes.Guest)
            .SingleOrDefaultAsync();

        if (guestRole != null) authCtx.UserRoles.Remove(guestRole);

        authCtx.UserRoles.Add(new UserRole { UserId = newUser.Id, RoleId = (long)UserRoleTypes.Client });
        await authCtx.SaveChangesAsync();
        return (true, "");
    }

    private async Task<(bool, string)> DuplicateUserVerificationByNewUserAsync(TenantDbContext targetTenantCtx,
        long partyId,
        M newUser)
    {
        var verifications = await tenantCtx.Verifications
            .AsNoTracking()
            .Where(x => x.PartyId == partyId)
            .Include(x => x.VerificationItems)
            .ToListAsync();

        foreach (var verification in verifications)
        {
            verification.Id = 0;
            verification.PartyId = newUser.PartyId;
            foreach (var verificationItem in verification.VerificationItems)
            {
                verificationItem.Id = 0;
                verificationItem.VerificationId = verification.Id;
            }
        }

        await targetTenantCtx.Verifications.AddRangeAsync(verifications);

        var medium = await tenantCtx.Media
            .AsNoTracking()
            .Where(x => x.PartyId == partyId)
            .ToListAsync();

        foreach (var m in medium)
        {
            m.PartyId = newUser.PartyId;
            m.Id = 0;
        }

        await targetTenantCtx.Media.AddRangeAsync(medium);

        try
        {
            await targetTenantCtx.SaveChangesAsync();
            return (true, "");
        }
        catch (Exception e)
        {
            BcrLog.Slack($"DuplicateUserVerificationByNewUserAsync_Error_SaveChanges: {e.GetFullMessage()}");
            return (false, e.Message);
        }
    }

    private async Task<M?> DuplicateUserByPartyIdAsync(TenantDbContext targetTenantCtx, long partyId, long toTid,
        long operatorPartyId)
    {
        var user = await authCtx.Users
            .AsNoTracking()
            .SingleAsync(x => x.TenantId == _tenantId && x.PartyId == partyId);

        var existingTargetUser = await authCtx.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == toTid && x.Email == user.Email);

        if (existingTargetUser != null) return existingTargetUser;

        await using var transaction = await authCtx.Database.BeginTransactionAsync();
        await using var centralTransaction = await centralCtx.Database.BeginTransactionAsync();
        await using var targetTenantTransaction = await targetTenantCtx.Database.BeginTransactionAsync();
        try
        {
            var centralParty = new CentralParty
            {
                Email = user.Email!,
                Name = user.FirstName + " " + user.LastName,
                NativeName = user.FirstName + " " + user.LastName,
                Code = "",
                Note = "",
                Uid = await GeneratePartyUidAsync(),
                TenantId = toTid,
                CreatedOn = DateTime.UtcNow,
            };
            if (centralParty.SiteId == 0) centralParty.SiteId = (int)SiteTypes.BritishVirginIslands;

            await centralCtx.CentralParties.AddAsync(centralParty);
            await centralCtx.SaveChangesAsync();

            //
            var party = centralParty.ToParty();
            user.ApplyToParty(ref party);
            await targetTenantCtx.PartyRoles.AddAsync(new PartyRole
                { Party = party, RoleId = (int)UserRoleTypes.Client });

            party.PartyComments.Add(new PartyComment
            {
                Content = $"{user.Email} duplicated from tenant {_tenantId}",
                OperatorPartyId = 1
            });
            await targetTenantCtx.SaveChangesAsync();

            user.Id = 0;
            user.TenantId = toTid;
            user.UserName = $"{toTid}_{user.Email}";
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email!.ToUpper();
            user.PartyId = party.Id;
            user.UserRoles.Add(new UserRole { RoleId = (long)UserRoleTypes.Guest });

            authCtx.Users.Add(user);
            await authCtx.SaveChangesAsync();
            await transaction.CommitAsync();
            await centralTransaction.CommitAsync();
            await targetTenantTransaction.CommitAsync();
            return user;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"DuplicateUserByPartyIdAsync_Error: {e.GetFullMessage()}");
            await transaction.RollbackAsync();
            await centralTransaction.RollbackAsync();
            await targetTenantTransaction.RollbackAsync();
            return null;
        }
    }

    public async Task<(bool, List<string>)> MigrateUserAsync(long partyId, long toTid, string referCode = "")
    {
        await using var transaction = await tenantCtx.Database.BeginTransactionAsync();
        try
        {
            var party = await tenantCtx.Parties.SingleAsync(x => x.Id == partyId);
            party.Medium = await tenantCtx.Media
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.ApiLogs = await tenantCtx.ApiLogs
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.PartyComments = await tenantCtx.PartyComments
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.PartyRoles = await tenantCtx.PartyRoles
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.Verifications = await tenantCtx.Verifications
                .Where(x => x.PartyId == partyId)
                .Include(x => x.VerificationItems)
                .ToListAsync();
            party.TradeDemoAccounts = await tenantCtx.TradeDemoAccounts
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.LoginLogs = await tenantCtx.LoginLogs
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.Leads = await tenantCtx.Leads
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.AuthCodes = await tenantCtx.AuthCodes
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.Applications = await tenantCtx.Applications
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.Accounts = await tenantCtx.Accounts
                .Where(x => x.PartyId == partyId)
                .Include(x => x.RebateClientRule)
                .Include(x => x.AccountComments)
                .Include(x => x.TradeAccountStatus)
                .Include(x => x.TradeAccount)
                .Include(x => x.AccountLogs)
                .Include(x => x.AccountPaymentMethodAccesses)
                .Include(x => x.RebateAgentRule)
                .Include(x => x.AccountReports)
                .Include(x => x.ReferralCodes)
                .ToListAsync();

            tenantCtx.RebateAgentRules.RemoveRange(party.Accounts.Where(x => x.RebateAgentRule != null)
                .Select(x => x.RebateAgentRule!));
            tenantCtx.ReferralCodes.RemoveRange(party.Accounts.SelectMany(x => x.ReferralCodes));

            var accountIds = party.Accounts.Select(y => y.Id).ToList();
            var tradeRebates = await tenantCtx.TradeRebates
                .Where(x => x.AccountId != null)
                .Where(x => accountIds.Contains(x.AccountId!.Value))
                .Include(x => x.Rebates)
                .ThenInclude(x => x.IdNavigation)
                .ThenInclude(x => x.Activities)
                .ToListAsync();

            var mtIds = tradeRebates.SelectMany(x => x.Rebates).Select(x => x.Id).ToList();
            var rebateWalletTransactions = await tenantCtx.WalletTransactions
                .Where(x => mtIds.Contains(x.MatterId))
                .ToListAsync();
            tenantCtx.WalletTransactions.RemoveRange(rebateWalletTransactions);
            foreach (var tr in tradeRebates)
            {
                tenantCtx.Activities.RemoveRange(tr.Rebates.SelectMany(x => x.IdNavigation.Activities));
                tenantCtx.Rebates.RemoveRange(tr.Rebates);
                tenantCtx.Matters.RemoveRange(tr.Rebates.Select(x => x.IdNavigation));
                tenantCtx.TradeRebates.Remove(tr);
            }
            
            party.Wallets = await tenantCtx.Wallets
                .Where(x => x.PartyId == partyId)
                .Include(x => x.WalletPaymentMethodAccesses)
                .Include(x => x.WalletTransactions)
                .Include(x => x.WalletDailySnapshots)
                .ToListAsync();

            tenantCtx.WalletTransactions.RemoveRange(party.Wallets.SelectMany(y => y.WalletTransactions));
            tenantCtx.WalletDailySnapshots.RemoveRange(party.Wallets.SelectMany(y => y.WalletDailySnapshots));
            
            party.EventParties = await tenantCtx.EventParties
                .Where(x => x.PartyId == partyId)
                .Include(x => x.EventShopPoint)
                .ToListAsync();
            party.CommunicateHistoryParties = await tenantCtx.CommunicateHistories
                .Where(x => x.PartyId == partyId)
                .ToListAsync();
            party.CommunicateHistoryOperatorParties = await tenantCtx.CommunicateHistories
                .Where(x => x.OperatorPartyId == partyId)
                .ToListAsync();
            party.ReferralReferredParties = await tenantCtx.Referrals
                .Where(x => x.ReferredPartyId == partyId)
                .ToListAsync();
            party.ReferralReferrerParties = await tenantCtx.Referrals
                .Where(x => x.ReferrerPartyId == partyId)
                .ToListAsync();
            party.Deposits = await tenantCtx.Deposits
                .Where(x => x.PartyId == partyId)
                .Include(x => x.Payment).ThenInclude(x => x.CryptoTransaction)
                .Include(x => x.IdNavigation)
                .ThenInclude(x => x.Activities).ToListAsync();

            party.MessageRecords = await tenantCtx.MessageRecords
                .Where(x => x.ReceiverPartyId == partyId)
                .ToListAsync();

            tenantCtx.MessageRecords.RemoveRange(party.MessageRecords);

            var verificationComments = await tenantCtx.Comments
                .Where(x => x.Type == (int)CommentTypes.Verification && party.Verifications.Select(y => y.Id).Contains(x.RowId))
                .ToListAsync();
            var verificationCmtMap = party.Verifications.ToDictionary(x => x, x => verificationComments.Where(y => y.RowId == x.Id).ToList());

            var leadComments = await tenantCtx.Comments
                .Where(x => x.Type == (int)CommentTypes.Lead && party.Leads.Select(y => y.Id).Contains(x.RowId))
                .ToListAsync();

            var leadCmtMap = party.Leads.ToDictionary(x => x, x => leadComments.Where(y => y.RowId == x.Id).ToList());

            tenantCtx.Comments.RemoveRange(verificationComments);
            tenantCtx.TradeDemoAccounts.RemoveRange(party.TradeDemoAccounts);
            tenantCtx.VerificationItems.RemoveRange(party.Verifications.SelectMany(x => x.VerificationItems));
            tenantCtx.Verifications.RemoveRange(party.Verifications);
            tenantCtx.PartyRoles.RemoveRange(party.PartyRoles);
            tenantCtx.PartyComments.RemoveRange(party.PartyComments);
            tenantCtx.Comments.RemoveRange(party.OperatedComments);
            tenantCtx.ApiLogs.RemoveRange(party.ApiLogs);
            tenantCtx.Media.RemoveRange(party.Medium);
            tenantCtx.LoginLogs.RemoveRange(party.LoginLogs);
            tenantCtx.Leads.RemoveRange(party.Leads);
            tenantCtx.AuthCodes.RemoveRange(party.AuthCodes);
            tenantCtx.Applications.RemoveRange(party.Applications);
            tenantCtx.AccountReports.RemoveRange(party.Accounts.SelectMany(x => x.AccountReports));
            tenantCtx.RebateClientRules.RemoveRange(party.Accounts.Where(x => x.RebateClientRule != null).Select(x => x.RebateClientRule!));
            tenantCtx.AccountComments.RemoveRange(party.Accounts.SelectMany(x => x.AccountComments));
            tenantCtx.TradeAccountStatuses.RemoveRange(party.Accounts.Where(x => x.TradeAccountStatus != null).Select(x => x.TradeAccountStatus!));
            tenantCtx.TradeAccounts.RemoveRange(party.Accounts.Where(x => x.TradeAccount != null).Select(x => x.TradeAccount!));
            tenantCtx.AccountLogs.RemoveRange(party.Accounts.SelectMany(x => x.AccountLogs));
            tenantCtx.AccountPaymentMethodAccesses.RemoveRange(
                party.Accounts.SelectMany(x => x.AccountPaymentMethodAccesses));
            tenantCtx.Accounts.RemoveRange(party.Accounts);
            tenantCtx.Wallets.RemoveRange(party.Wallets);
            tenantCtx.EventShopPoints.RemoveRange(party.EventParties.Select(x => x.EventShopPoint));
            tenantCtx.EventParties.RemoveRange(party.EventParties);
            tenantCtx.CommunicateHistories.RemoveRange(party.CommunicateHistoryParties);
            tenantCtx.CommunicateHistories.RemoveRange(party.CommunicateHistoryOperatorParties);

            tenantCtx.Deposits.RemoveRange(party.Deposits);
            tenantCtx.Payments.RemoveRange(party.Deposits.Select(x => x.Payment));
            tenantCtx.Activities.RemoveRange(party.Deposits.SelectMany(x => x.IdNavigation.Activities));
            tenantCtx.Matters.RemoveRange(party.Deposits.Select(x => x.IdNavigation));

            tenantCtx.Referrals.RemoveRange(party.ReferralReferredParties);
            tenantCtx.Referrals.RemoveRange(party.ReferralReferrerParties);

            tenantCtx.Parties.Remove(party);

            await tenantCtx.SaveChangesAsync();

            await using var targetTenantCtx = pool.CreateTenantDbContext(toTid);

            // reset their id
            party.Pid = null;
            party.Medium.ForEach(x =>
            {
                x.Id = 0;
                x.TenantId = toTid;
            });
            party.ApiLogs.ForEach(x => x.Id = 0);
            party.PartyComments.ForEach(x =>
            {
                x.Id = 0;
                x.OperatorPartyId = 1;
            });
            party.PartyRoles.ForEach(x => x.Id = 0);
            party.Verifications.ForEach(x => x.Id = 0);
            party.Verifications.SelectMany(x => x.VerificationItems).ForEach(x => x.Id = 0);
            party.TradeDemoAccounts.ForEach(x => x.Id = 0);
            party.OperatedComments.ForEach(x => x.Id = 0);
            party.LoginLogs.ForEach(x => x.Id = 0);
            party.Leads.ForEach(x => x.Id = 0);
            party.AuthCodes.ForEach(x => x.Id = 0);

            party.Applications.ForEach(x =>
            {
                x.Id = 0;
                x.ReferenceId = 0;
                if (_tenantId == 1 || x.Supplement == null || x.Type != 100) return;

                x.Supplement = x.Supplement.Replace("\"serviceId\":20", "\"serviceId\":10");
            });
            var code = await targetTenantCtx.ReferralCodes.Where(x => x.Code == referCode)
                .Select(x => new { x.Account.Role, x.AccountId, x.Account.SalesAccountId, x.Account.ReferPath, x.Account.Level, x.Code })
                .SingleOrDefaultAsync();

            party.Accounts.ForEach(x =>
            {
                x.Id = 0;
                x.SalesAccountId = code?.SalesAccountId;
                x.AgentAccountId = code?.AccountId;
                x.ReferCode = code?.Code;
                x.ReferPath = code?.ReferPath ?? x.ReferPath;
                x.Level = (code?.Level ?? 0) + 1;
                if (x.RebateClientRule != null)
                {
                    x.RebateClientRule.Id = 0;
                    x.RebateClientRule.RebateDirectSchemaId = null;
                }

                if (x.TradeAccountStatus != null)
                {
                    x.TradeAccountStatus.Id = 0;
                }

                if (x.TradeAccount != null)
                {
                    x.TradeAccount.Id = 0;
                }

                x.AccountComments.ForEach(y => y.Id = 0);
                x.AccountLogs.ForEach(y =>
                {
                    y.Id = 0;
                    y.OperatorPartyId = 1;
                });
                x.AccountPaymentMethodAccesses = [];

                var trades = tradeRebates
                    .Where(y => y.AccountId == x.Id)
                    .ToList();
                trades.ForEach(y =>
                {
                    y.Id = 0;
                    y.AccountId = 0;
                    y.Rebates = [];
                });
                x.TradeRebates = trades;

                x.AccountReports.ForEach(y =>
                {
                    y.Id = 0;
                    y.AccountId = 0;
                });
                if (x.RebateAgentRule != null)
                {
                    x.RebateAgentRule.Id = 0;
                    x.RebateAgentRule.AgentAccountId = 0;
                }

                x.ReferralCodes.ForEach(y =>
                {
                    y.Id = 0;
                    y.AccountId = 0;
                });
            });

            party.Wallets.ForEach(x =>
            {
                x.Id = 0;
                x.WalletPaymentMethodAccesses = [];
                x.WalletTransactions = [];
                x.WalletDailySnapshots = [];
            });
            party.CommunicateHistoryParties = [];
            party.CommunicateHistoryOperatorParties = [];
            party.ReferralReferredParties = [];
            party.ReferralReferrerParties = [];
            party.MessageRecords = [];
            party.Deposits.ForEach(x =>
            {
                x.Id = 0;
                x.IdNavigation.Id = 0;
                x.IdNavigation.Activities = [];
                x.Payment.Id = 0;
                x.PaymentId = 0;
                // x.Payment.PaymentServiceId = 10055;
                // x.Payment.CryptoTransaction = null;
            });
            targetTenantCtx.Parties.Add(party);
            await targetTenantCtx.SaveChangesAsync();

            foreach (var (k, v) in verificationCmtMap)
                v.ForEach(x =>
                {
                    x.Id = 0;
                    x.RowId = k.Id;
                });

            var newVComments = verificationCmtMap.SelectMany(x => x.Value).ToList();
            targetTenantCtx.Comments.AddRange(newVComments);

            foreach (var (k, v) in leadCmtMap)
                v.ForEach(x =>
                {
                    x.Id = 0;
                    x.RowId = k.Id;
                });

            var newLComments = leadCmtMap.SelectMany(x => x.Value).ToList();
            targetTenantCtx.Comments.AddRange(newLComments);
            await targetTenantCtx.SaveChangesAsync();

            var user = await authCtx.Users.SingleAsync(x => x.TenantId == _tenantId && x.PartyId == partyId);
            var centralParty = await centralCtx.CentralParties.SingleAsync(x => x.Id == partyId);
            var centralAccounts = await centralCtx.CentralAccounts
                .Where(x => party.Accounts.Select(y => y.AccountNumber).Contains(x.AccountNumber))
                .ToListAsync();
            user.TenantId = toTid;
            user.ReferCode = referCode;
            centralParty.TenantId = toTid;
            centralAccounts.ForEach(x =>
            {
                x.TenantId = toTid;
                centralCtx.Entry(x).Property(y => y.TenantId).IsModified = true;
            });

            authCtx.Users.Update(user);
            centralCtx.CentralParties.Update(centralParty);
            await authCtx.SaveChangesAsync();
            await centralCtx.SaveChangesAsync();
            await cache.HSetDeleteByKeyFieldAsync(CacheKeys.GetPartyIdToPartyHashKey(_tenantId), partyId.ToString());

            var messages = new List<string>
            {
                $"Create Medium: {party.Medium.Count}",
                $"Create ApiLogs: {party.ApiLogs.Count}",
                $"Create PartyComments: {party.PartyComments.Count}",
                $"Create PartyRoles: {party.PartyRoles.Count}",
                $"Create Verifications: {party.Verifications.Count}",
                $"Create VerificationItems: {party.Verifications.SelectMany(x => x.VerificationItems).Count()}",
                $"Create TradeDemoAccounts: {party.TradeDemoAccounts.Count}",
                $"Create OperatedComments: {party.OperatedComments.Count}",
                $"Create LoginLogs: {party.LoginLogs.Count}",
                $"Create Verification Comments: {newVComments.Count}",
                $"Create Lead Comments: {newLComments.Count}",
                $"Create Leads: {party.Leads.Count}",
                $"Create AuthCodes: {party.AuthCodes.Count}",
                $"Create Applications: {party.Applications.Count}",
                $"Create Accounts: {party.Accounts.Count}",
                $"Create Wallets: {party.Wallets.Count}",
                $"Create EventParties: {party.EventParties.Count}",
                $"Create CommunicateHistoryParties: {party.CommunicateHistoryParties.Count}",
                $"Create CommunicateHistoryOperatorParties: {party.CommunicateHistoryOperatorParties.Count}",
                $"Create ReferralReferredParties: {party.ReferralReferredParties.Count}",
                $"Create ReferralReferrerParties: {party.ReferralReferrerParties.Count}",
                $"Create Deposits: {party.Deposits.Count}",
                $"Create Payments: {party.Deposits.Select(x => x.Payment).Count()}",
                $"Create Activities: {party.Deposits.SelectMany(x => x.IdNavigation.Activities).Count()}",
                $"Create Matters: {party.Deposits.Select(x => x.IdNavigation).Count()}",
                $"Update User TenantId: {toTid}",
            };

            await transaction.CommitAsync();
            await cache.SetStringAsync(CacheKeys.UserTokenInvalidKey(_tenantId, partyId), "1", TimeSpan.FromDays(7));
            return (true, messages);
        }
        catch (Exception e)
        {
            BcrLog.Slack($"Migration user error: {e.GetFullMessage()}, pid: {partyId}, toTid: {toTid}");
            await transaction.RollbackAsync();
            return (false, ["Migrate user failed", e.Message]);
        }
    }

    public async Task<long> GeneratePartyUidAsync()
    {
        const string prefix = "9";
        var end = _tenantId switch
        {
            1 => "1",
            10000 => "2",
            10004 => "3",
            10005 => "4",
            _ => "0",
        };

        var sec = DateTime.UtcNow.Year - 2023 + 1;
        var randA = new Random().Next(10, 99);
        var randB = new Random().Next(10, 99);
        var randT = 1;
        // uidString = uidString + sec + randA + randT + randB + end;
        var uidString = $"{prefix}{sec}{randA}{randT}{randB}{end}";
        var tryTime = 0;
        while (true)
        {
            if (tryTime > 10) throw new AuthenticationException($"Generate uid failed. uid:{uidString}");
            // var uid = long.Parse(uidString);
            if (!long.TryParse(uidString, out var uid))
            {
                tryTime++;
                continue;
            }

            var exist = await centralCtx.CentralParties.AnyAsync(x => x.Uid == uid);
            if (!exist) return uid;

            randT++;
            if (randT > 9)
            {
                randA = new Random().Next(10, 99);
            }

            // uidString = uidString + sec + randA + randT + randB + end;
            uidString = $"{prefix}{sec}{randA}{randT}{randB}{end}";
            tryTime++;
        }
    }
}