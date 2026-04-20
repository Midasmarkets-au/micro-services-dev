using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Email.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    // [MIGRATED] _bcrEventTradeQueue (SQS) removed — Deposit events now published to NATS BCR_EVENT_TRADE.
    // private readonly string _bcrEventTradeQueue = sqsOptions.Value.BCREventTrade;

    public async Task<(bool, string)> DepositCreatedAsync(long tenantId, long depositId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var deposit = await ctx.Deposits
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.CurrencyId,
                x.TargetAccountId, x.Payment.PaymentMethod.Platform
            })
            .SingleAsync(x => x.Id == depositId);
        if (deposit.TargetAccountId == null) return (true, " __DEPOSIT_HAS_NO_TARGET_ACCOUNT__");

        var account = await ctx.Accounts
            .Select(x => new
            {
                x.Id, x.AccountNumber, x.Group,
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .SingleAsync(x => x.Id == deposit.TargetAccountId);

        // a user can only use Credit Card once, disable for all accounts
        if (deposit.Platform == (int)PaymentPlatformTypes.EuPay)
        {
            var accesses = await ctx.AccountPaymentMethodAccesses
                .Where(x => x.Account.Status == 0)
                .Where(x => x.Account.PartyId == deposit.PartyId)
                .Where(x => x.Status == (int)PaymentMethodAccessStatusTypes.Active)
                .ToListAsync();

            accesses.ForEach(x => x.Status = (int)PaymentMethodAccessStatusTypes.Inactive);
            await ctx.SaveChangesAsync();
        }

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == deposit.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == deposit.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var model = new DepositApplicationViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            AccountNumber = account.AccountNumber,
            Date = DateTime.UtcNow,
            Group = account.Group,
            NativeName = selfUser.GuessUserNativeName(),
            FormattedAmount = $"{(deposit.Amount / 100d).ToCentsFromScaled():0.00}",
            Currency = Enum.GetName(typeof(CurrencyTypes), deposit.CurrencyId) ?? string.Empty
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new DepositApplicationForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                AccountNumber = account.AccountNumber,
                Group = account.Group,
                FormattedAmount = $"{(deposit.Amount / 100d).ToCentsFromScaled():0.00}",
                Currency = Enum.GetName(typeof(CurrencyTypes), deposit.CurrencyId) ?? string.Empty,
                NativeName = selfUser.GuessUserNativeName(),
                Date = DateTime.UtcNow,
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    /// <summary>
    /// 用户上传Ticket 需要邮件通知
    /// </summary>
    public async Task<(bool, string)> DepositReceiptUploadedAsync(long tenantId, long depositId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var deposit = await ctx.Deposits
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.CurrencyId,
                x.TargetAccountId
            })
            .SingleAsync(x => x.Id == depositId);
        if (deposit.TargetAccountId == null) return (true, " __DEPOSIT_HAS_NO_TARGET_ACCOUNT__");

        var account = await ctx.Accounts
            .Select(x => new
            {
                x.Id, x.AccountNumber, x.Group,
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .SingleAsync(x => x.Id == deposit.TargetAccountId);

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == deposit.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == deposit.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var model = new DepositReceiptUploadedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            // In Development environment, Bcc to internal team for testing
            BccEmails = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? new List<string> { "xinsong.rao@edgeark.com.au", "renjie.jiang@edgeark.com.au" }
                    : null,
            AccountNumber = account.AccountNumber,
            Date = DateTime.UtcNow,
            Group = account.Group,
            NativeName = selfUser.GuessUserNativeName(),
            FormattedAmount = $"{(deposit.Amount / 100d).ToCentsFromScaled():0.00}",
            Currency = Enum.GetName(typeof(CurrencyTypes), deposit.CurrencyId) ?? string.Empty
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };

        //foreach (var parentUser in parentUsers)
        //{
        //    if (string.IsNullOrEmpty(parentUser.Email) || !parentUser.Email.Contains("@")) continue;
        //    var parentModel = new DepositReceiptUploadedForIBandSalesViewModel
        //    {
        //        Email = parentUser.Email ?? string.Empty,
        //        // In Development environment, Bcc to internal team for testing
        //        BccEmails = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
        //            ? new List<string> { "renjie.jiang@edgeark.com.au" }
        //            : null,
        //        AccountNumber = account.AccountNumber,
        //        Group = account.Group,
        //        FormattedAmount = $"{(deposit.Amount / 100d).ToCentsFromScaled():0.00}",
        //        Currency = Enum.GetName(typeof(CurrencyTypes), deposit.CurrencyId) ?? string.Empty,
        //        NativeName = selfUser.GuessUserNativeName(),
        //        Date = DateTime.UtcNow,
        //    };
        //    await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
        //    list.Add(parentUser.Email);
        //}

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    public async Task<(bool, string)> DepositCompletedAsync(long tenantId, long depositId)
    {
        var updateAccountStatusTask = TryUpdateTradeAccountStatus(tenantId, depositId);
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var deposit = await ctx.Deposits
            .Where(x => x.Id == depositId)
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.TargetAccountId,
                x.Payment.Number, x.Payment.PaymentMethod.Platform
            })
            .SingleAsync();
        if (deposit.TargetAccountId == null) return (true, " __DEPOSIT_HAS_NO_TARGET_ACCOUNT__");

        // [MIGRATED] SQS BCREventTrade.fifo publish replaced by NATS BCR_EVENT_TRADE.
        // Consumed by: scheduler/src/jobs/event_trade_handler.rs (source_type=3, Deposit)
        // Legacy SQS code:
        // var message = new EventShopPointTransaction.MQSource
        // {
        //     SourceType = EventShopPointTransactionSourceTypes.Deposit,
        //     RowId = deposit.Id,
        //     TenantId = tenantId
        // }.ToString();
        // await mqService.SendAsync(message, _bcrEventTradeQueue, _bcrEventTradeQueue);
        try
        {
            await natsPublisher.PublishAsync(
                EventShopPointTransactionSourceTypes.Deposit,
                deposit.Id,
                tenantId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed_to_publish_nats_deposit: {depositId}", deposit.Id);
        }

        var account = await ctx.Accounts
            .Where(x => x.Id == deposit.TargetAccountId)
            .SingleAsync();

        var isFirstDeposit = await ctx.Deposits
            .Where(x => x.PartyId == deposit.PartyId && x.IdNavigation.StateId == (int)StateTypes.DepositCompleted)
            .AnyAsync();
        if (isFirstDeposit)
        {
            account.ActiveOn = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        var parentPartyIds = await ctx.Accounts
            .Where(x => x.Id == account.SalesAccountId || x.Id == account.AgentAccountId)
            .Select(x => x.PartyId)
            .Distinct()
            .ToListAsync();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == deposit.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == deposit.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var model = new DepositCompletedForClientViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            Amount = (deposit.Amount / 100d).ToCentsFromScaled(),
            AccountNumber = account.AccountNumber,
            Group = account.Group,
            NativeName = selfUser.GuessUserNativeName(),
            PaymentNumber = deposit.Number,
            UserEmail = selfUser.Email ?? string.Empty,
            UserPhone = selfUser.PhoneNumber ?? string.Empty,
            Date = DateTime.Now.ToString("yyyy-MM-dd")
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };

        foreach (var parentUser in parentUsers)
        {
            if (string.IsNullOrEmpty(parentUser.Email) || !parentUser.Email.Contains("@")) continue;
            var parentModel = new DepositCompletedForParentViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                Amount = (deposit.Amount / 100d).ToCentsFromScaled(),
                AccountNumber = account.AccountNumber,
                Group = account.Group,
                NativeName = selfUser.GuessUserNativeName(),
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        await updateAccountStatusTask;
        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }
}