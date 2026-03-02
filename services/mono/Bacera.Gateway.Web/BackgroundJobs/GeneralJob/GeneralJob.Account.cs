using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Email.ViewModel;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    public async Task<(bool, string)> TradeAccountCreatedAsync(long tenantId, long id,
        string password,
        string investorPassword = "",
        string phonePassword = "")
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var account = await ctx.Accounts
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ServiceId,
                ClientPartyId = x.PartyId,
                AccountNumber = x.TradeAccount != null ? x.TradeAccount.AccountNumber : 0,
                SalesPartyId = x.SalesAccount != null
                    ? x.SalesAccount.PartyId
                    : 0,
                AgentPartyId = x.AgentAccount != null
                    ? x.AgentAccount.PartyId
                    : 0
            })
            .SingleAsync();

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == account.ClientPartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == account.ClientPartyId);

        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var tradeServiceName = await ctx.TradeServices
            .Where(x => x.Id == account.ServiceId)
            .Select(x => x.Name)
            .FirstAsync();

        var model = new TradeAccountCreatedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            UserName = selfUser.Email ?? string.Empty,
            NativeName = selfUser.GuessUserNativeName(),
            AccountNumber = account.AccountNumber,
            Password = password,
            InvestorPassword = investorPassword,
            PhonePassword = phonePassword,
            TradeServiceName = tradeServiceName,
            OpenedDate = DateTime.UtcNow,
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };
        foreach (var parentUser in parentUsers)
        {
            var parentModel = new TradeAccountCreatedForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                UserName = selfUser.GuessUserNativeName(),
                AccountNumber = account.AccountNumber,
                ReadOnlyPassword = investorPassword,
                TradeServiceName = tradeServiceName,
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    public async Task<(bool, string)> AgentAccountCreatedAsync(long tenantId, long id)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var account = await ctx.Accounts
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.PartyId,
                x.Uid,
                x.Group,
                SalesPartyId = x.SalesAccount != null
                    ? x.SalesAccount.PartyId
                    : 0,
                AgentPartyId = x.AgentAccount != null
                    ? x.AgentAccount.PartyId
                    : 0
            })
            .SingleAsync();

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users.Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == account.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == account.PartyId);

        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();
        var model = new IBCreatedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            UserName = selfUser.GuessUserNativeName(),
            AccountUid = account.Uid,
            IbCode = account.Group,
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };
        foreach (var parentUser in parentUsers)
        {
            var parentModel = new IBCreatedForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                IbEmail = selfUser.Email ?? string.Empty,
                UserName = selfUser.GuessUserNativeName(),
                AccountUid = account.Uid,
                IbCode = account.Group,
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    // public Task TryUpdateTradeAccountStatus(long tenantId, long accountId)
    //     => UpdateTradeAccountAsync(tenantId, accountId);
    //
    // public Task TryUpdateTradeAccountFromApiStatus(long tenantId, long accountId)
    //     => UpdateTradeAccountAsync(tenantId, accountId, true);

    public async Task TryUpdateTradeAccountStatus(long tenantId, long accountId, bool fromApi = false)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var accManSvc = scope.ServiceProvider.GetRequiredService<AccountManageService>();
        await accManSvc.ConcurrentTryUpdateTradeAccountStatus(accountId, fromApi);
    }
}