using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.DTO;
using Bacera.Gateway.Services.Email.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    public async Task<(bool, string)> WithdrawalCreatedAsync(long tenantId, long withdrawalId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sourceAccountId = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => x.SourceAccountId)
            .SingleAsync();

        if (sourceAccountId != null) return await ClientWithdrawalCreatedAsync(scope, tenantId, withdrawalId);

        var salesRes = await SalesWithdrawalCreatedAsync(scope, tenantId, withdrawalId);
        var agentRes = await AgentWithdrawalCreatedAsync(scope, tenantId, withdrawalId);

        return (salesRes.Item1 || agentRes.Item1, salesRes.Item2 + agentRes.Item2);
    }

    public async Task<(bool, string)> WithdrawalCompletedAsync(long tenantId, long withdrawalId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sourceAccountId = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => x.SourceAccountId)
            .SingleAsync();

        if (sourceAccountId != null) return await ClientWithdrawalCompletedAsync(scope, tenantId, withdrawalId);

        return await AgentWithdrawalCompletedAsync(scope, tenantId, withdrawalId);
    }

    public async Task<(bool, string)> WithdrawalCancelledAsync(long tenantId, long withdrawalId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sourceAccountId = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => x.SourceAccountId)
            .SingleAsync();

        if (sourceAccountId != null) return await ClientWithdrawalCancelledAsync(scope, tenantId, withdrawalId);

        return await AgentWithdrawalCancelledAsync(scope, tenantId, withdrawalId);
    }


    public async Task<(bool, string)> WithdrawalRejectedAsync(long tenantId, long withdrawalId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sourceAccountId = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => x.SourceAccountId)
            .SingleAsync();

        if (sourceAccountId != null) return await ClientWithdrawalFailedAsync(scope, tenantId, withdrawalId);

        return await AgentWithdrawalFailedAsync(scope, tenantId, withdrawalId);
    }

    private async Task<(bool, string)> ClientWithdrawalCreatedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.CurrencyId, x.SourceAccountId,
                PaymentMethod = x.Payment.PaymentMethod.Name,
                PaymentNumber = x.Payment.Number
            })
            .SingleAsync(x => x.Id == withdrawalId);

        var account = await ctx.Accounts
            .Select(x => new
            {
                x.Id, x.PartyId, x.AccountNumber, SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .SingleAsync(x => x.Id == item.SourceAccountId);

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == account.PartyId)
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == account.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;
        var list = new List<string?>();
        var model = new WithdrawalCreatedForClientViewModel
        {
            Email = selfUser.Email ?? string.Empty,
            AccountNumber = account.AccountNumber,
            FormattedAmount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
            Currency = currency,
            NativeName = selfUser.GuessUserNativeName(),
            Date = DateTime.UtcNow
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        list.Add(selfUser.Email);

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalCreatedForParentViewModel
            {
                Email = parentUser.Email ?? string.Empty,
                AccountNumber = account.AccountNumber,
                Amount = (item.Amount / 100d).ToCentsFromScaled(),
                Currency = currency,
                NativeName = selfUser.GuessUserNativeName(),
                PaymentMethod = item.PaymentMethod,
                PaymentNumber = item.PaymentNumber,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }


    private async Task<(bool, string)> ClientWithdrawalCompletedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.CurrencyId, x.SourceAccountId,
                PaymentMethod = x.Payment.PaymentMethod.Name,
                PaymentNumber = x.Payment.Number, x.Payment.ReferenceNumber
            })
            .SingleAsync(x => x.Id == withdrawalId);

        var account = await ctx.Accounts
            .Select(x => new
            {
                x.Id, x.PartyId, x.AccountNumber, SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .SingleAsync(x => x.Id == item.SourceAccountId);

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == account.PartyId)
            .ToEmailNameModel()
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == account.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;
        var list = new List<string?>();

        var model = new WithdrawalConfirmationForClientViewModel
        {
            Email = selfUser.Email,
            Amount = (item.Amount / 100d).ToCentsFromScaled(),
            Currency = currency,
            AccountNumber = account.AccountNumber,
            PaymentMethod = item.PaymentMethod,
            ReferenceNumber = item.ReferenceNumber,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        list.Add(selfUser.Email);

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalConfirmationForParentViewModel
            {
                Email = parentUser.Email,
                Amount = (item.Amount / 100d).ToCentsFromScaled(),
                Currency = currency,
                AccountNumber = account.AccountNumber,
                PaymentMethod = item.PaymentMethod,
                ReferenceNumber = item.ReferenceNumber,
                NativeName = selfUser.GuessNativeName(),
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    private async Task<(bool, string)> ClientWithdrawalCancelledAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.CurrencyId, x.SourceAccountId,
                PaymentMethod = x.Payment.PaymentMethod.Name,
                PaymentNumber = x.Payment.Number, x.Payment.ReferenceNumber
            })
            .SingleAsync(x => x.Id == withdrawalId);

        var account = await ctx.Accounts
            .Select(x => new
            {
                x.Id, x.PartyId, x.AccountNumber, SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .SingleAsync(x => x.Id == item.SourceAccountId);

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == account.PartyId)
            .ToEmailNameModel()
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == account.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;
        var list = new List<string?>();

        var model = new WithdrawalCancelledForClientViewModel
        {
            Email = selfUser.Email,
            Amount = (item.Amount / 100d).ToCentsFromScaled(),
            Currency = currency,
            AccountNumber = account.AccountNumber,
            ReferenceNumber = item.ReferenceNumber,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        list.Add(selfUser.Email);

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalCancelledForAgentAndSalesViewModel
            {
                Email = parentUser.Email,
                Amount = (item.Amount / 100d).ToCentsFromScaled(),
                Currency = currency,
                AccountNumber = account.AccountNumber,
                ReferenceNumber = item.ReferenceNumber,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    private async Task<(bool, string)> ClientWithdrawalFailedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Select(x => new
            {
                x.Id, x.PartyId, x.Amount, x.CurrencyId, x.SourceAccountId,
                PaymentMethod = x.Payment.PaymentMethod.Name,
                PaymentNumber = x.Payment.Number, x.Payment.ReferenceNumber
            })
            .SingleAsync(x => x.Id == withdrawalId);

        var reason = ctx.Comments
            .Where(x => x.RowId == withdrawalId && x.Type == (int)CommentTypes.Withdrawal)
            .Select(x => x.Content)
            .FirstOrDefault() ?? string.Empty;

        var account = await ctx.Accounts
            .Select(x => new
            {
                x.Id, x.PartyId, x.AccountNumber, x.Group, x.Code,
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .SingleAsync(x => x.Id == item.SourceAccountId);

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == account.PartyId)
            .ToEmailNameModel()
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == account.PartyId);
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;
        var list = new List<string?>();

        var model = new WithdrawalRejectedForClientViewModel
        {
            Email = selfUser.Email,
            Amount = (item.Amount / 100d).ToCentsFromScaled(),    
            AccountNumber = account.AccountNumber,
            Group = account.Group,
            Code = account.Code,
            NativeName = selfUser.GuessNativeName(),
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Currency = currency,
            Reason = reason
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        list.Add(selfUser.Email);

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalRejectedForAgentAndSalesViewModel
            {
                Email = parentUser.Email,
                Amount = (item.Amount / 100d).ToCentsFromScaled(),
                AccountNumber = account.AccountNumber,
                Group = account.Group,
                Code = account.Code,
                NativeName = selfUser.GuessNativeName(),
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Currency = currency,
                Reason = reason
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    private async Task<(bool, string)> AgentWithdrawalCreatedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => new { x.PartyId, x.Amount, x.CurrencyId, x.FundType })
            .SingleAsync();
        var account = await ctx.Accounts
            // .Where(x => x.PartyId == item.PartyId && x.Role == (int)AccountRoleTypes.Agent)
            .OrderByDescending(x => x.Id)
            .Where(x => x.Status == 0)
            .Where(x => x.PartyId == item.PartyId && x.Role == (int)AccountRoleTypes.Agent)
            .Where(x => x.CurrencyId == item.CurrencyId && x.FundType == item.FundType)
            .Select(x => new
            {
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0,
            })
            .FirstOrDefaultAsync();
        if (account == null) return (false, "__NO_AGENT_ACCOUNT_FOUND__");

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == item.PartyId)
            .ToEmailNameModel()
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == item.PartyId);
        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;
        var model = new WithdrawalCreatedForAgentSelfViewModel
        {
            Email = selfUser.Email,
            Currency = currency,
            FormattedAmount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
            Date = DateTime.UtcNow,
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };

        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalCreatedForAgentParentViewModel
            {
                Email = parentUser.Email,
                AgentEmail = selfUser.Email,
                Currency = currency,
                Amount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
                Date = DateTime.UtcNow,
                NativeName = selfUser.GuessNativeName(),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    private async Task<(bool, string)> SalesWithdrawalCreatedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();

        var config = await cfgSvc.GetAsync<SalesWithdrawalDTO.Config>(
            ConfigCategoryTypes.Public
            , 0
            , ConfigKeys.SalesWithdrawalCreatedEmailConfig);

        if (config == null) return (false, "__CONFIG_NOT_FOUND__");
            
        var item = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Where(x => x.Party.Tags.Any(y => y.Name == config.IncludePartyTag))
            .Select(x => new { x.PartyId, x.Amount, x.CurrencyId, x.FundType, x.Party.Email })
            .FirstOrDefaultAsync();

        if (item == null) return (false, "__NO_SALES_ACCOUNT_FOUND__");

        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var currency = (Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty)
                       + $"<br> Sales Withdrawal ({item.Email})";
        var model = new WithdrawalCreatedForAgentSelfViewModel
        {
            Email = config.Email,
            BccEmails = config.BccEmails,
            Currency = currency,
            FormattedAmount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
            Date = DateTime.UtcNow,
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model);
        return (true, $"__EMAIL_SENT_TO__{model.Email}");
    }

    private async Task<(bool, string)> AgentWithdrawalCompletedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => new { x.PartyId, x.Amount, x.CurrencyId, x.FundType })
            .SingleAsync();
        var account = await ctx.Accounts
            .OrderByDescending(x => x.Id)
            .Where(x => x.Status == 0)
            .Where(x => x.PartyId == item.PartyId && x.Role == (int)AccountRoleTypes.Agent)
            .Where(x => x.CurrencyId == item.CurrencyId && x.FundType == item.FundType)
            .Select(x => new
            {
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .FirstOrDefaultAsync();
        if (account == null) return (false, "__NO_AGENT_ACCOUNT_FOUND__");

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == item.PartyId)
            .ToEmailNameModel()
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == item.PartyId);
        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;

        var model = new WithdrawalCompletedForAgentSelfViewModel
        {
            Email = selfUser.Email,
            Currency = currency,
            FormattedAmount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
            Date = DateTime.UtcNow,
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);
        var list = new List<string?> { selfUser.Email };

        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalCompletedForAgentParentViewModel
            {
                Email = parentUser.Email,
                AgentEmail = selfUser.Email,
                Currency = currency,
                Amount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
                Date = DateTime.UtcNow,
                NativeName = selfUser.GuessNativeName(),
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
            list.Add(parentUser.Email);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }

    private async Task<(bool, string)> AgentWithdrawalCancelledAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        return await AgentWithdrawalFailedAsync(scope, tenantId, withdrawalId);
    }

    private async Task<(bool, string)> AgentWithdrawalFailedAsync(IServiceScope scope, long tenantId,
        long withdrawalId)
    {
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Withdrawals
            .Where(x => x.Id == withdrawalId)
            .Select(x => new { x.PartyId, x.Amount, x.CurrencyId, x.FundType })
            .SingleAsync();

        var reason = ctx.Comments
            .Where(x => x.RowId == withdrawalId && x.Type == (int)CommentTypes.Withdrawal)
            .Select(x => x.Content)
            .FirstOrDefault() ?? "Cancelled";

        var account = await ctx.Accounts
            .OrderByDescending(x => x.Id)
            .Where(x => x.Status == 0)
            .Where(x => x.PartyId == item.PartyId && x.Role == (int)AccountRoleTypes.Agent)
            .Where(x => x.CurrencyId == item.CurrencyId && x.FundType == item.FundType)
            .Select(x => new
            {
                SalesPartyId = x.SalesAccount != null ? x.SalesAccount.PartyId : 0,
                AgentPartyId = x.AgentAccount != null ? x.AgentAccount.PartyId : 0
            })
            .FirstOrDefaultAsync();
        if (account == null) return (false, "__NO_AGENT_ACCOUNT_FOUND__");

        var parentPartyIds = new[] { account.SalesPartyId, account.AgentPartyId }.Distinct().ToList();
        var users = await authDbContext.Users
            .Where(x => x.TenantId == tenantId)
            .Where(x => parentPartyIds.Contains(x.PartyId) || x.PartyId == item.PartyId)
            .ToEmailNameModel()
            .ToListAsync();

        var selfUser = users.Single(x => x.PartyId == item.PartyId);
        var currency = Enum.GetName(typeof(CurrencyTypes), item.CurrencyId) ?? string.Empty;

        var model = new WithdrawalFailedForAgentSelfViewModel
        {
            Email = selfUser.Email,
            Currency = currency,
            FormattedAmount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
            Date = DateTime.UtcNow,
            Reason = reason
        };
        await sendMailSvc.SendEmailWithTemplateAsync(model, selfUser.Language);

        var list = new List<string?> { selfUser.Email };
        var parentUsers = users.Where(x => parentPartyIds.Contains(x.PartyId))
            .DistinctBy(x => x.Email)
            .ToList();

        foreach (var parentUser in parentUsers)
        {
            var parentModel = new WithdrawalFailedForAgentParentViewModel
            {
                Email = parentUser.Email,
                AgentEmail = selfUser.Email,
                Currency = currency,
                Amount = $"{(item.Amount / 100d).ToCentsFromScaled():0.00}",
                Date = DateTime.UtcNow,
                Reason = reason
            };
            await sendMailSvc.SendEmailWithTemplateAsync(parentModel, parentUser.Language);
        }

        return (true, $"__EMAIL_SENT_TO__ {string.Join(", ", list)}");
    }
}