using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class TradeAccountCreatedEvent : INotification
{
    public long AccountId { get; set; }

    public string Password { get; set; } = string.Empty;
    public string InvestorPassword { get; set; } = string.Empty;
    public string PhonePassword { get; set; } = string.Empty;

    public TradeAccountCreatedEvent(
        long accountId,
        string password,
        string investorPassword = "",
        string phonePassword = "")
    {
        AccountId = accountId;
        Password = password;
        PhonePassword = phonePassword;
        InvestorPassword = investorPassword;
    }
}

public class TradeAccountCreatedEventHandler(
    Tenancy tenancy,
    ILeadService leadService,
    AccountManageService accManSvc,
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    ISendMessageService sendMessageService,
    IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<TradeAccountCreatedEvent>
{
    public async Task Handle(TradeAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        var account = await tenantDbContext.Accounts
            .Where(x => x.Id == notification.AccountId)
            .Select(x => new
            {
                x.Id,
                ClientPartyId = x.PartyId,
                Login = x.TradeAccount != null ? x.TradeAccount.AccountNumber : 0,
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

        await accManSvc.UpdateAccountSearchText(account.Id);

        var user = await tenantDbContext.Parties
            .Where(x => x.Id == account.ClientPartyId)
            .ToTenantBasicViewModel()
            .SingleAsync(cancellationToken: cancellationToken);

        var notice = EventNotice.Build("__CLIENT_TRADE_ACCOUNT_CREATED__", account.Login);

        await sendMessageService.SendEventToPartyAsync(tenancy.GetTenantId(), account.ClientPartyId, notice);
        if (account.AgentPartyId > 0)
            await sendMessageService.SendEventToPartyAsync(tenancy.GetTenantId(), account.AgentPartyId,
                notice);
        if (account.SalesPartyId > 0)
            await sendMessageService.SendEventToPartyAsync(tenancy.GetTenantId(), account.SalesPartyId,
                notice);


        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.TradeAccountCreatedAsync(tenancy.GetTenantId(), account.Id, notification.Password, notification.InvestorPassword,
                notification.PhonePassword));

        var model = await tenantDbContext.TradeAccounts.SingleAsync(x => x.Id == notification.AccountId, cancellationToken);
        await leadService.AppendEvent(user.PartyId, model, LeadStatusTypes.TradeAccountCreated);
    }
}