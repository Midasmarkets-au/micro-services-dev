using Bacera.Gateway.Services.Email.ViewModel;

namespace Bacera.Gateway.Services;

public interface ISendMailService
{
    Task<Tuple<bool, string>> DebugAsync(DebugEmailRequest request);
    Task<Tuple<bool, string>> SendEmailWithTemplateAsync<T>(T model,
        string? language = LanguageTypes.English,
        bool? applyLayout = true)
        where T : class, IEmailViewModel;

    Task<TopicContent> GetTemplate(string title, string language);
    Task<string> ApplyVariablesInTemplate<T>(string template, T model) where T : class, IEmailViewModel;

    Task<string> GenerateEmailWithTemplateAsync<T>(T model, string? language) where T : class, IEmailViewModel, new();

    Task<Tuple<bool, string>> ReadOnlyCodeNoticeAsync(ReadOnlyCodeNoticeViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ConfirmEmailAsync(ConfirmEmailViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ResetPasswordAsync(ResetPasswordViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ResetTradeAccountPasswordAsync(ResetTradeAccountPasswordViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> TradeDemoAccountCreatedAsync(TradeDemoAccountCreatedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> TradeAccountLeverageChangedAsync(TradeAccountLeverageChangedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleCreatedAsync(
        ApplicationForWholesaleCreatedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleApprovedAsync(
        ApplicationForWholesaleApprovedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleRejectedAsync(
        ApplicationForWholesaleRejectedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceNeededAsync(
        ApplicationForWholesaleEvidenceNeededViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededAsync(
        ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededViewModel model,
        string language = LanguageTypes.English);
}