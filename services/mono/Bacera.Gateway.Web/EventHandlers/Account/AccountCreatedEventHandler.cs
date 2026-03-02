using System.Security.Claims;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Bacera.Gateway.Web.Services.Interface;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Web.EventHandlers;

public class AccountCreatedEvent(long accountId) : INotification
{
    public long AccountId { get; } = accountId;
}

public class AccountCreatedEventHandler(
    Tenancy tenancy,
    TenantDbContext tenantCtx,
    ISendMessageService sendMessageService,
    TradingService tradingSvc,
    AccountManageService accManSvc,
    AuthDbContext authDbContext,
    IOptions<AmazonSQSOptions> sqsOptions,
    AccountingService accountingService,
    ILogger<AccountCreatedEventHandler> logger,
    UserManager<User> userManager,
    IBackgroundJobClient backgroundJobClient,
    IMessageQueueService mqService)
    : INotificationHandler<AccountCreatedEvent>
{
    private readonly string _bcrEventTradeQueue = sqsOptions.Value.BCREventTrade;

    public async Task Handle(AccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == notification.AccountId)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.PartyId,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                FundType = (FundTypes)x.FundType,
                Role = (AccountRoleTypes)x.Role,
                ClientPartyId = x.PartyId,
                SalesPartyId = x.SalesAccount != null
                    ? x.SalesAccount.PartyId
                    : 0,
                AgentPartyId = x.AgentAccount != null
                    ? x.AgentAccount.PartyId
                    : 0,
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (account == null)
            return;

        var user = await authDbContext.Users
            .Where(x => x.PartyId == account.PartyId && x.TenantId == tenancy.GetTenantId())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user == null)
            return;

        var notice = EventNotice.Build("__CLIENT_ACCOUNT_CREATED__", account.Uid, 0, account.Id.ToString());

        await sendMessageService.SendEventToPartyAsync(tenancy.GetTenantId(), account.ClientPartyId, notice);
        if (account.AgentPartyId > 0)
            await sendMessageService.SendEventToPartyAsync(tenancy.GetTenantId(), account.AgentPartyId,
                notice);
        if (account.SalesPartyId > 0)
            await sendMessageService.SendEventToPartyAsync(tenancy.GetTenantId(), account.SalesPartyId,
                notice);


        var userRole = account.Role switch
        {
            AccountRoleTypes.Rep => UserRoleTypesString.Rep,
            AccountRoleTypes.Sales => UserRoleTypesString.Sales,
            AccountRoleTypes.Client => UserRoleTypesString.Client,
            AccountRoleTypes.Agent => UserRoleTypesString.IB,
            _ => null
        };

        if (userRole != null && false == await userManager.IsInRoleAsync(user, userRole))
        {
            await userManager.AddToRoleAsync(user, userRole);
        }

        if (await userManager.IsInRoleAsync(user, UserRoleTypesString.Guest))
        {
            await userManager.RemoveFromRoleAsync(user, UserRoleTypesString.Guest);
        }

        if (false == await userManager.IsInRoleAsync(user, UserRoleTypesString.Client))
        {
            await userManager.AddToRoleAsync(user, UserRoleTypesString.Client);
        }

        switch (account.Role)
        {
            case AccountRoleTypes.Client:
                var distributionType = await tenantCtx.RebateClientRules
                    .Where(x => x.ClientAccountId == account.Id)
                    .Select(x => x.DistributionType)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (distributionType == (short)RebateDistributionTypes.LevelPercentage)
                {
                    await userManager.AddToRoleAsync(user, UserRoleTypesString.MLM);
                }

                break;
            case AccountRoleTypes.Agent:
                await userManager.AddClaimAsync(user, new Claim(UserClaimTypes.AgentAccount, account.Uid.ToString()));
                var levelSettingString = await tenantCtx.RebateAgentRules.Where(x => x.AgentAccountId == account.Id)
                    .Select(x => x.LevelSetting)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (levelSettingString == null)
                    break;

                var levelSetting = Utils.JsonDeserializeObjectWithDefault<RebateAgentRule.RebateLevelSetting>(levelSettingString);
                if (levelSetting.DistributionType == RebateDistributionTypes.LevelPercentage)
                {
                    await userManager.AddToRoleAsync(user, UserRoleTypesString.MLM);
                }

                backgroundJobClient.Enqueue<IGeneralJob>(x => x.AgentAccountCreatedAsync(tenancy.GetTenantId(), account.Id));
                break;
            case AccountRoleTypes.Sales:
                await userManager.AddClaimAsync(user, new Claim(UserClaimTypes.SalesAccount, account.Uid.ToString()));
                break;
            case AccountRoleTypes.Rep:
                await userManager.AddClaimAsync(user, new Claim(UserClaimTypes.RepAccount, account.Uid.ToString()));
                break;
        }

        // _backgroundJobClient.Enqueue<IProcessAccountStatJob>("account-stat-event", x =>
        //     x.ClientAccountAddedAsync(_tenancyResolver.GetTenantId(), account.Id));

        Wallet? wallet = null;
        if (AccountRoleTypes.Client == account.Role)
        {
            wallet = await accountingService.WalletGetOrCreateForClientAsync(account.PartyId, account.CurrencyId, account.FundType);
        }
        else
        {
            wallet = await accountingService.WalletGetOrCreateAsync(account.PartyId, account.CurrencyId, account.FundType);
        }

        var accountForUpdate = await tenantCtx.Accounts
            .SingleAsync(x => x.Id == account.Id, cancellationToken: cancellationToken);

        accountForUpdate.WalletId = wallet.Id;
        accountForUpdate.UpdatedOn = DateTime.UtcNow;
        await tenantCtx.SaveChangesAsync(cancellationToken: cancellationToken);
        
        await accManSvc.UpdateAccountSearchText(account.Id);
        try
        {
            var message = EventShopPointTransaction.MQSource.Build(EventShopPointTransactionSourceTypes.OpenAccount,
                account.Id, tenancy.GetTenantId()).ToString();
            await mqService.SendAsync(message, _bcrEventTradeQueue, _bcrEventTradeQueue, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed_to_send_message_to_queue_account: {AccountId}", account.Id);
        }
    }
}