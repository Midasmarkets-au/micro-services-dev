using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Email.ViewModel;
using Bacera.Gateway.Services.Extension;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    public async Task<(bool, string)> UserRegisteredAsync(long tenantId, long partyId, string password)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var user = await authDbContext.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .Select(x => new { x.Email, x.Language })
            .SingleOrDefaultAsync();
        if (user == null || user.Email == null) return (false, "__USER_NOT_FOUND__");
        var model = new RegisteredWelcomeViewModel { Email = user.Email, Password = password };
        await sendMailSvc.SendEmailWithTemplateAsync(model, user.Language);
        return (true, $"__EMAIL_SENT__{user.Email}");
    }

    public async Task<(bool, string)> VerificationDocumentRejectedAsync(long tenantId, long partyId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var userSvc = scope.ServiceProvider.GetRequiredService<UserService>();
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var user = await userSvc.GetPartyAsync(partyId);
        var bccEmails = new List<string>();
        if (!string.IsNullOrEmpty(user.ReferCode))
        {
            var parentEmails = await tenantCtx.ReferralCodes
                .Where(x => x.Code == user.ReferCode)
                .Select(x => new
                {
                    AgentEmail = x.Account.Party.Email,
                    SalesEmail = x.Account.SalesAccount != null ? x.Account.SalesAccount.Party.Email : ""
                })
                .FirstOrDefaultAsync();

            if (parentEmails != null)
            {
                bccEmails.Add(parentEmails.AgentEmail);
                bccEmails.Add(parentEmails.SalesEmail);
            }

            bccEmails = bccEmails.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        }

        var vm = new VerificationDocumentRejectedViewModel
        {
            Email = user.EmailRaw,
            BccEmails = bccEmails,
            NativeName = user.GuessNativeName()
        };

        await sendMailSvc.SendEmailWithTemplateAsync(vm, user.Language);
        return (true, $"__EMAIL_SENT_TO__{user.EmailRaw}");
    }

    public async Task GenerateAuthCodeAndSendEmailAsync(long tenantId, string email, string @event)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();

        var user = await authDbContext.Users
            .Where(x => x.Email == email && x.TenantId == tenantId)
            .Select(x => new { x.Email, x.PartyId, x.Language })
            .FirstAsync();

        var item = AuthCode.Build(user.PartyId, @event, AuthCodeMethodTypes.Email, email);

        ctx.AuthCodes.Add(item);
        await ctx.SaveChangesAsync();

        // Calculate expire minutes from ExpireOn
        var expireMinutes = item.ExpireOn.HasValue 
            ? (int)Math.Ceiling((item.ExpireOn.Value - item.CreatedOn).TotalMinutes)
            : 10;

        var model = new VerificationCodeViewModel
        {
            Email = email,
            VerificationCode = item.Code,
            ExpireMinutes = expireMinutes
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, user.Language);
    }

    public async Task ResetAuthCodeAndSendAsync(long tenantId, long authCodeId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        var item = await ctx.AuthCodes
            .Where(x => x.Id == authCodeId)
            .Include(x => x.Party)
            .SingleAsync();

        item.Status = (short)AuthCodeStatusTypes.Valid;
        item.ExpireOn = DateTime.UtcNow.AddMinutes(11);
        ctx.AuthCodes.Update(item);
        await ctx.SaveChangesAsync();

        // Calculate expire minutes from ExpireOn
        var expireMinutes = item.ExpireOn.HasValue 
            ? (int)Math.Ceiling((item.ExpireOn.Value - item.CreatedOn).TotalMinutes)
            : 10;

        var model = new VerificationCodeViewModel
        {
            Email = item.Party.Email,
            VerificationCode = item.Code,
            ExpireMinutes = expireMinutes
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, item.Party.Language);
    }

    public async Task<(bool, string)> ResetTradeAccountPasswordAsync(long tenantId, long accountId, string cbUrl,
        string token)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var item = await ctx.Accounts
            .Where(x => x.Id == accountId)
            .Select(x => new
                { x.Uid, x.AccountNumber, x.ServiceId, x.Party.NativeName, x.Party.Email, x.PartyId, x.Party.Language })
            .SingleAsync();
        var platform = scope.ServiceProvider.GetDbPool().GetPlatformByServiceId(item.ServiceId);

        var model = new ResetTradeAccountPasswordViewModel(tenantId, item.Email, item.PartyId, item.Uid,
            item.AccountNumber,
            item.NativeName, cbUrl, token, platform);

        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        await sendMailSvc.SendEmailWithTemplateAsync(model, item.Language);
        return (true, $"__EMAIL_SENT_TO__{item.Email}");
    }

    public async Task EquityCheckEmailAsync(long tenantId, long accountNumber, string email, string language,
        List<string> bccEmails, DateOnly date)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();

        var model = new PromotionNotificationViewModel
        {
            AccountNumber = accountNumber,
            Email = email,
            Date = date,
            BccEmails = bccEmails,
        };

        await sendMailSvc.SendEmailWithTemplateAsync(model, language);
    }
}