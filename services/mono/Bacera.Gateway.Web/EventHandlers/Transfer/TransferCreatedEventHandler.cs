using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class TransferCreatedEvent(Transaction model) : INotification
{
    public Transaction Model { get; } = model;
}

public class TransferCreatedEventHandler(
    ISendMessageService sendMessageService,
    IBackgroundJobClient backgroundJobClient,
    // AcctService acctService,
    IServiceProvider serviceProvider,
    ConfigService configSvc,
    ITenantGetter tenantGetter)
    : INotificationHandler<TransferCreatedEvent>
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    
    public async Task Handle(TransferCreatedEvent notification, CancellationToken cancellationToken)
    {
        var transaction = notification.Model;
        var setting = await configSvc.GetAsync<ApplicationConfigure.AutoCompleteTransactionAmountValue>(nameof(Public),
                          0, ConfigKeys.AutoCompleteTransactionAmount)
                      ?? ApplicationConfigure.AutoCompleteTransactionAmountValue.Of();

        using var scope = serviceProvider.CreateTenantScope(_tenantId);
        var acctService = scope.ServiceProvider.GetRequiredService<AcctService>();
        
        if (transaction.IsBetweenTradeAccounts())
        {
            backgroundJobClient.Enqueue<IGeneralJob>(x => x.TransactionBetweenTradeAccountCreatedAsync(_tenantId, transaction.Id));

            if (setting.Enabled && transaction.Amount <= setting.Amount)
            {
                await acctService.CompleteTransactionAsync(transaction.Id);
            }
            else
            {
                await sendMessageService.SendEventToManagerAsync(_tenantId, EventNotice.Build("__TRANSFER_CREATED__", transaction.Id));
            }
        }
        else if (transaction.IsBetweenWallets())
        {
            // Wallet → Wallet transfer (IB/Sales reward transfer to downline)
            // Enqueue background job to send email notifications
            backgroundJobClient.Enqueue<IGeneralJob>(x => x.TransactionWalletToWalletCreatedAsync(_tenantId, transaction.Id));
            
            // Check if transaction is already completed (auto-completed in CreateTransactionBetweenWalletAsync)
            if (transaction.IdNavigation.StateId == (int)StateTypes.TransferCompleted)
            {
                // Already completed, no action needed
                return;
            }

            // Transaction is in TransferAwaitingApproval state
            if (setting.Enabled && transaction.Amount <= setting.Amount)
            {
                // Amount is within auto-approval threshold, complete it now
                await acctService.CompleteTransactionAsync(transaction.Id);
            }
            else
            {
                // Amount exceeds threshold or auto-complete is disabled, requires manager approval
                await sendMessageService.SendEventToManagerAsync(_tenantId, EventNotice.Build("__TRANSFER_CREATED__", transaction.Id));
            }
        }
        else if (setting.Enabled)
        {
            // backgroundJobClient.Enqueue<IGeneralJob>(x => x.TransactionCompleteAsync(_tenantId, transaction.Id));
            // await acctService.CompleteTransactionAsync(transaction.Id);
        }
        else
        {
            await sendMessageService.SendEventToManagerAsync(_tenantId, EventNotice.Build("__TRANSFER_CREATED__", transaction.Id));
        }
    }
}