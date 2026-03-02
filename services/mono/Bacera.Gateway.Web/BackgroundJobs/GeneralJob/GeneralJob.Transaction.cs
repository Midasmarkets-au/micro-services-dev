using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Email.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    public async Task<(bool, string)> TransactionBetweenTradeAccountCreatedAsync(long tenantId, long transactionId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var transaction = await ctx.Transactions
            .Where(x => x.Id == transactionId)
            .Select(x => new
                { x.PartyId, x.Amount, x.SourceAccountId, x.TargetAccountId, x.SourceAccountType, x.TargetAccountType })
            .SingleOrDefaultAsync();
        if (transaction == null || transaction.SourceAccountType != (int)TransactionAccountTypes.Account ||
            transaction.TargetAccountType != (int)TransactionAccountTypes.Account)
            return (false, "__TRANSACTION_NOT_FOUND__");

        var accounts = await ctx.Accounts
            .Where(x => x.Id == transaction.SourceAccountId || x.Id == transaction.TargetAccountId)
            .Select(x => new
            {
                x.Id, x.AccountNumber, SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .ToListAsync();

        var sourceAccount = accounts.Single(x => x.Id == transaction.SourceAccountId);
        var targetAccount = accounts.Single(x => x.Id == transaction.TargetAccountId);

        var parentPartyIds = accounts
            .SelectMany(x => new[] { x.SalesPartyId, x.AgentPartyId })
            .Distinct()
            .ToList();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == transaction.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == transaction.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var sentEmails = new List<string?>();
        var model = new TransferCreatedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            SourceAccountNumber = sourceAccount.AccountNumber,
            TargetAccountNumber = targetAccount.AccountNumber,
            Date = DateTime.UtcNow,
            FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        sentEmails.Add(selfUser.Email);
        foreach (var parentUser in parentUsers)
        {
            var parentModel = new TransferCreatedForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                SourceAccountNumber = sourceAccount.AccountNumber,
                TargetAccountNumber = targetAccount.AccountNumber,
                Date = DateTime.UtcNow,
                FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
                NativeName = selfUser.GuessUserNativeName(),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            sentEmails.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", sentEmails)}");
    }

    public async Task<(bool, string)> TransactionBetweenTradeAccountCompletedAsync(long tenantId, long transactionId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var transaction = await ctx.Transactions
            .Where(x => x.Id == transactionId)
            .Select(x => new
            {
                x.PartyId, x.Amount, x.CurrencyId, x.SourceAccountId, x.TargetAccountId, x.SourceAccountType,
                x.TargetAccountType
            })
            .SingleOrDefaultAsync();
        if (transaction == null || transaction.SourceAccountType != (int)TransactionAccountTypes.Account ||
            transaction.TargetAccountType != (int)TransactionAccountTypes.Account)
            return (false, "__TRANSACTION_NOT_FOUND__");

        var accounts = await ctx.Accounts
            .Where(x => x.Id == transaction.SourceAccountId || x.Id == transaction.TargetAccountId)
            .Select(x => new
            {
                x.Id, x.AccountNumber, SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .ToListAsync();

        var sourceAccount = accounts.Single(x => x.Id == transaction.SourceAccountId);
        var targetAccount = accounts.Single(x => x.Id == transaction.TargetAccountId);

        var parentPartyIds = accounts
            .SelectMany(x => new[] { x.SalesPartyId, x.AgentPartyId })
            .Distinct()
            .ToList();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == transaction.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == transaction.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var sentEmails = new List<string?>();
        var model = new TransferCompletedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            SourceAccountNumber = sourceAccount.AccountNumber,
            TargetAccountNumber = targetAccount.AccountNumber,
            Date = DateTime.UtcNow,
            FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
            Currency = Enum.GetName(typeof(CurrencyTypes), transaction.CurrencyId) ?? string.Empty
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        sentEmails.Add(selfUser.Email);
        foreach (var parentUser in parentUsers)
        {
            var parentModel = new TransferCompletedForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                SourceAccountNumber = sourceAccount.AccountNumber,
                TargetAccountNumber = targetAccount.AccountNumber,
                Date = DateTime.UtcNow,
                FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
                NativeName = selfUser.GuessUserNativeName(),
                Currency = Enum.GetName(typeof(CurrencyTypes), transaction.CurrencyId) ?? string.Empty
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            sentEmails.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", sentEmails)}");
    }

    public async Task<(bool, string)> TransactionBetweenTradeAccountFailedAsync(long tenantId, long transactionId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var transaction = await ctx.Transactions
            .Where(x => x.Id == transactionId)
            .Select(x => new
            {
                x.PartyId, x.Amount, x.CurrencyId, x.SourceAccountId, x.TargetAccountId, x.SourceAccountType,
                x.TargetAccountType
            })
            .SingleOrDefaultAsync();
        if (transaction == null || transaction.SourceAccountType != (int)TransactionAccountTypes.Account ||
            transaction.TargetAccountType != (int)TransactionAccountTypes.Account)
            return (false, "__TRANSACTION_NOT_FOUND__");

        var accounts = await ctx.Accounts
            .Where(x => x.Id == transaction.SourceAccountId || x.Id == transaction.TargetAccountId)
            .Select(x => new
            {
                x.Id, x.AccountNumber, SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .ToListAsync();

        var sourceAccount = accounts.Single(x => x.Id == transaction.SourceAccountId);
        var targetAccount = accounts.Single(x => x.Id == transaction.TargetAccountId);

        var parentPartyIds = accounts
            .SelectMany(x => new[] { x.SalesPartyId, x.AgentPartyId })
            .Distinct()
            .ToList();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == transaction.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == transaction.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var sentEmails = new List<string?>();
        var model = new TransferFailedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            SourceAccountNumber = sourceAccount.AccountNumber,
            TargetAccountNumber = targetAccount.AccountNumber,
            Date = DateTime.UtcNow,
            FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
            Currency = Enum.GetName(typeof(CurrencyTypes), transaction.CurrencyId) ?? string.Empty
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        sentEmails.Add(selfUser.Email);
        foreach (var parentUser in parentUsers)
        {
            var parentModel = new TransferFailedForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                SourceAccountNumber = sourceAccount.AccountNumber,
                TargetAccountNumber = targetAccount.AccountNumber,
                Date = DateTime.UtcNow,
                FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
                NativeName = selfUser.GuessUserNativeName(),
                Currency = Enum.GetName(typeof(CurrencyTypes), transaction.CurrencyId) ?? string.Empty
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            sentEmails.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", sentEmails)}");
    }

    public async Task<(bool, string)> TransactionWalletToWalletCreatedAsync(long tenantId, long transactionId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        
        var transaction = await ctx.Transactions
            .Where(x => x.Id == transactionId)
            .Select(x => new
                { x.PartyId, x.Amount, x.SourceAccountId, x.TargetAccountId, x.SourceAccountType, x.TargetAccountType })
            .SingleOrDefaultAsync();
        
        if (transaction == null || transaction.SourceAccountType != (int)TransactionAccountTypes.Wallet ||
            transaction.TargetAccountType != (int)TransactionAccountTypes.Wallet)
            return (false, "__TRANSACTION_NOT_FOUND__");

        // Get source and target wallets with their party IDs
        var wallets = await ctx.Wallets
            .Where(x => x.Id == transaction.SourceAccountId || x.Id == transaction.TargetAccountId)
            .Select(x => new { x.Id, x.PartyId })
            .ToListAsync();

        var sourceWallet = wallets.Single(x => x.Id == transaction.SourceAccountId);
        var targetWallet = wallets.Single(x => x.Id == transaction.TargetAccountId);

        // Get client accounts for these parties to find their IB/Sales relationships
        var partyIds = wallets.Select(x => x.PartyId).Distinct().ToList();
        var accounts = await ctx.Accounts
            .Where(x => partyIds.Contains(x.PartyId))
            .Where(x => x.Role == (short)AccountRoleTypes.Client) // Only client accounts have IB/Sales
            .Select(x => new
            {
                x.PartyId,
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .ToListAsync();

        // Get parent party IDs (IB/Sales) from client accounts
        var parentPartyIds = accounts
            .SelectMany(x => new[] { x.SalesPartyId, x.AgentPartyId })
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == transaction.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == transaction.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var sentEmails = new List<string?>();
        
        // Send email to the sender (IB/Sales)
        var model = new TransferCreatedViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            // In Development environment, Bcc to internal team for testing
            BccEmails = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? new List<string> { "xinsong.rao@edgeark.com.au", "renjie.jiang@edgeark.com.au" }
                    : null,
            SourceAccountNumber = transaction.SourceAccountId, // Wallet ID as source
            TargetAccountNumber = transaction.TargetAccountId, // Wallet ID as target
            Date = DateTime.UtcNow,
            FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        sentEmails.Add(selfUser.Email);
        
        // Send email to parent IB/Sales
        foreach (var parentUser in parentUsers)
        {
            var parentModel = new TransferCreatedForIBandSalesViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                // In Development environment, Bcc to internal team for testing
                BccEmails = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? new List<string> { "xinsong.rao@edgeark.com.au", "renjie.jiang@edgeark.com.au" }
                    : null,
                SourceAccountNumber = transaction.SourceAccountId,
                TargetAccountNumber = transaction.TargetAccountId,
                Date = DateTime.UtcNow,
                FormattedAmount = $"{(transaction.Amount / 100d).ToCentsFromScaled():0.00}",
                NativeName = selfUser.GuessUserNativeName(),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            sentEmails.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", sentEmails)}");
    }

    public async Task<(bool, string)> TransactionCompleteAsync(long tenantId, long id)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var svc = scope.ServiceProvider.GetRequiredService<AcctService>();
        return await svc.CompleteTransactionAsync(id);
    }
}