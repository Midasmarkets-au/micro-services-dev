using Bacera.Gateway.Services.Email.ViewModel;
using Microsoft.Extensions.Logging;
using RazorEngineCore;

namespace Bacera.Gateway.Services;

partial class SendMailService
{
    public async Task<Tuple<bool, string>> DebugAsync(DebugEmailRequest request)
    {
        // var razorEngine = new RazorEngine();
        // var topicContent = await getTemplate(EmailTemplateTypes.ClientDailyConfirmation, LanguageTypes.English);
        //
        // var template = await razorEngine.CompileAsync(topicContent.Content);
        // var obb = ClientDailyConfirmationViewModel.NoTradesBuildTest();
        // // var result = await template.RunAsync(ClientDailyConfirmationViewModel.NoTradesBuildTest());
        // var result = await template.RunAsync(obb);
        // await sendEmailWithTemplateAsync(request.To, request.Title, request.Model, request.Language);

        // await Task.Delay(0);
        // return new Tuple<bool, string>(true, "");

        if (LanguageTypes.IsExists(request.Language) == false) request.Language = LanguageTypes.English;

        var template = await GetTemplate(request.Title, request.Language);
        // If template not found, send email without template
        if (template.Id == 0)
        {
            return Tuple.Create(false, "__TEMPLATE_NOT_FOUND__");
        }

        // return Tuple.Create(false, "__TEMPLATE_NOT_FOUND__");

        var html = template.Content;
        html = await ApplyDefaultLayout(request.Language, html, template.Title, template.Subtitle);
        var result = await _sendMailSvc.SendEmailAsync(request.To, template.Title, html);
        if (!result.Item1)
            _logger.LogError("Email send out failed: {Email} : {Title}", request.To, request.Title);
        return result;
    }

    public async Task<Tuple<bool, string>> ApplicationForWholesaleCreatedAsync(
        ApplicationForWholesaleCreatedViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ApplicationForWholesaleApprovedAsync(
        ApplicationForWholesaleApprovedViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ApplicationForWholesaleRejectedAsync(
        ApplicationForWholesaleRejectedViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceNeededAsync(
        ApplicationForWholesaleEvidenceNeededViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededAsync(
        ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ReadOnlyCodeNoticeAsync(ReadOnlyCodeNoticeViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> TradeAccountCreatedAsync(TradeAccountCreatedViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ResetTradeAccountPasswordAsync(ResetTradeAccountPasswordViewModel model,
        string language = LanguageTypes.English)
        => await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> TradeDemoAccountCreatedAsync(TradeDemoAccountCreatedViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> TradeAccountLeverageChangedAsync(
        TradeAccountLeverageChangedViewModel model,
        string language = LanguageTypes.English) =>
        await SendEmailWithTemplateAsync(model, language);

    public async Task<Tuple<bool, string>> ResetPasswordAsync(ResetPasswordViewModel model,
        string language = LanguageTypes.English)
    {
        var template = await GetTemplate(EmailTemplateTypes.ResetPassword, EnsureLanguage(language));
        // if template not found, send email without template
        if (template.Id != 0) return await SendEmailWithTemplateAsync(model, language);

        _logger.LogWarning("Email template not found {Title} in {Language}", EmailTemplateTypes.ResetPassword,
            language);
        var (isSuccess, _) = await _sendMailSvc
            .SendEmailAsync(model.Email, "Reset Password",
                $"Please reset your password by <a href='{model.CallbackUrl}'>clicking here</a>.",
                await _cfgSvc.GetDefaultEmailAddressAsync(),
                await _cfgSvc.GetDefaultEmailDisplayNameAsync());
        if (!isSuccess)
            _logger.LogError("Email send out failed: {Email} : {Url}", model.Email, model.CallbackUrl);
        return Tuple.Create(isSuccess, "__TEMPLATE_NOT_FOUND__");
    }

    public async Task<Tuple<bool, string>> ConfirmEmailAsync(ConfirmEmailViewModel model,
        string language = LanguageTypes.English)
    {
        language = EnsureLanguage(language);
        var template = await GetTemplate(EmailTemplateTypes.ConfirmEmail, language);
        // If template not found, send email without template
        if (template.Id != 0)
            return await SendEmailWithTemplateAsync(model, language);

        _logger.LogWarning("Email template not found {Title} in {Language}", EmailTemplateTypes.ConfirmEmail,
            language);
        var (isSuccess, _) = await _sendMailSvc
            .SendEmailAsync(model.Email, "Confirm your email",
                $"Please confirm your account by <a href='{model.CallbackUrl}'>clicking here</a>.",
                await _cfgSvc.GetDefaultEmailAddressAsync(),
                await _cfgSvc.GetDefaultEmailDisplayNameAsync());
        if (!isSuccess)
            _logger.LogError("Email send out failed: {Email} : {Url}", model.Email, model.CallbackUrl);
        return Tuple.Create(isSuccess, "__TEMPLATE_NOT_FOUND__");
    }

    private static string EnsureLanguage(string? language)
        => !string.IsNullOrEmpty(language) && LanguageTypes.IsExists(language) ? language : LanguageTypes.English;

    public async Task<Tuple<bool, string>> SendEmailWithTemplateAsync<T>(T model, string? language = LanguageTypes.English, bool? applyLayout = true)
        where T : class, IEmailViewModel
    {
        // check model.Email is valid email
        if (!model.IsValidReceiverEmail())
        {
            _logger.LogError("SendEmailWithTemplateAsync_Invalid_email_address {Email}", model.Email);
            return Tuple.Create(false, "__INVALID_EMAIL__");
        }

        language = EnsureLanguage(language);
        var template = await GetTemplate(model.TemplateTitle, language);
        // If template not found, send email without template
        if (template.Id == 0)
        {
            _logger.LogWarning("Email template not found {Title} in {Language}", model.TemplateTitle, language);
            return Tuple.Create(false, "__TEMPLATE_NOT_FOUND__");
        }

        var html = await ApplyVariablesInTemplate(template.Content, model);
        if (applyLayout == true)
        {
            html = await ApplyDefaultLayout(language, html, model.GetDisplayTitle(template.Title), template.Subtitle);
        }

        var defaultEmail = await _cfgSvc.GetDefaultEmailAddressAsync();
        var defaultDisplayName = await _cfgSvc.GetDefaultEmailDisplayNameAsync();
        model.BccEmails ??= [];
        model.BccEmails.Add(defaultEmail);

        var result = await _sendMailSvc.SendEmailAsync(model.Email
            , template.Title
            , html
            , defaultEmail
            , defaultDisplayName
            , model.BccEmails);
        if (!result.Item1)
            _logger.LogError("Email send out failed: {Email}", model.Email);

        _logger.LogInformation("Email sent to {Email} with model {Model}", model.Email, model);
        return result;
    }

    public async Task<string> GenerateEmailWithTemplateAsync<T>(T model, string? language)
        where T : class, IEmailViewModel, new()

    {
        language = EnsureLanguage(language);
        var template = await GetTemplate(model.TemplateTitle, language);
        var html = await ApplyVariablesInTemplate(template.Content, model);
        return await ApplyDefaultLayout(language, html, template.Title, template.Subtitle);
    }

    private async Task<string> GetDefaultLayout(string language)
        => (await GetTemplate(EmailTemplateTypes.DefaultLayout, language)).Content;

    public async Task<string> ApplyVariablesInTemplate<T>(string template, T model) where T : class, IEmailViewModel
    {
        if (typeof(IRazorModel).IsAssignableFrom(typeof(T)))
        {
            var razorEngine = new RazorEngine();
            var compiledTemplate = await razorEngine.CompileAsync(template);
            template = await compiledTemplate.RunAsync(model) ?? string.Empty;
            return template;
        }

        var properties = model.GetType().GetProperties();
        foreach (var property in properties)
        {
            var placeholder = $"{{{{{property.Name}}}}}";
            var content = property.GetValue(model, null)?.ToString() ?? string.Empty;
            template = template.Replace(placeholder, content);
        }

        return template;
    }

    public static async Task<string> CompileRazorTemplate<T>(T model, string template) where T : class, IRazorModel
    {
        var razorEngine = new RazorEngine();
        var compiledTemplate = await razorEngine.CompileAsync(template);
        template = await compiledTemplate.RunAsync(model) ?? string.Empty;
        return template;
    }

    private async Task<string> ApplyDefaultLayout(string language, string content, string title = "", string subtitle = "")
    {
        var layout = await GetDefaultLayout(language);
        return layout
            .Replace("{{Title}}", title)
            .Replace("{{Content}}", content)
            .Replace("{{Subtitle}}", subtitle);
    }
}