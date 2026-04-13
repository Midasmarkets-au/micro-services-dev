using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Email.ViewModel;

namespace Bacera.Gateway.Web.BackgroundJobs;

public interface IGeneralJob
{
    Task<(bool, string)> VerificationDocumentRejectedAsync(long tenantId, long partyId);

    Task<Tuple<bool, string>> ReadOnlyCodeNoticeAsync(long tenantId, ReadOnlyCodeNoticeViewModel model,
        string language = LanguageTypes.English);


    Task<Tuple<bool, string>> ConfirmEmailAsync(long tenantId, ConfirmEmailViewModel model,
        string language = LanguageTypes.English);

    Task<(bool, string)> UserRegisteredAsync(long tenantId, long partyId, string password);

    Task<Tuple<bool, string>> ResetPasswordAsync(long tenantId, ResetPasswordViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> TradeDemoAccountCreatedAsync(long tenantId, TradeDemoAccountCreatedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ResetTradeAccountPasswordAsync(long tenantId, ResetTradeAccountPasswordViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> TradeAccountLeverageChangedAsync(long tenantId,
        TradeAccountLeverageChangedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleCreatedAsync(long tenantId,
        ApplicationForWholesaleCreatedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleApprovedAsync(long tenantId,
        ApplicationForWholesaleApprovedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleRejectedAsync(long tenantId,
        ApplicationForWholesaleRejectedViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceNeededAsync(long tenantId,
        ApplicationForWholesaleEvidenceNeededViewModel model,
        string language = LanguageTypes.English);

    Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededAsync(long tenantId,
        ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededViewModel model,
        string language = LanguageTypes.English);

    Task<(bool, string)> SendEmailByTopicIdWithContent(long tenantId, string uuid);

    Task<(bool, string)> TransactionBetweenTradeAccountCreatedAsync(long tenantId, long id);
    Task<(bool, string)> TransactionBetweenTradeAccountCompletedAsync(long tenantId, long id);
    Task<(bool, string)> TransactionBetweenTradeAccountFailedAsync(long tenantId, long id);
    Task<(bool, string)> TransactionWalletToWalletCreatedAsync(long tenantId, long id);
    Task<(bool, string)> TransactionCompleteAsync(long tenantId, long id);
    Task<(bool, string)> DepositCreatedAsync(long tenantId, long id);
    Task<(bool, string)> DepositReceiptUploadedAsync(long tenantId, long depositId);
    Task<(bool, string)> DepositCompletedAsync(long tenantId, long id);
    Task<(bool, string)> WithdrawalCreatedAsync(long tenantId, long id);
    Task<(bool, string)> WithdrawalCompletedAsync(long tenantId, long id);
    Task<(bool, string)> WithdrawalCancelledAsync(long tenantId, long id);
    Task<(bool, string)> WithdrawalRejectedAsync(long tenantId, long id);

    Task<(bool, string)> TradeAccountCreatedAsync(long tenantId, long id,
        string password,
        string investorPassword = "",
        string phonePassword = "");

    Task<(bool, string)> AgentAccountCreatedAsync(long tenantId, long id);

    Task TryUpdateTradeAccountStatus(long tenantId, long accountId, bool fromApi = false);

    // Task TryUpdateTradeAccountFromApiStatus(long tenantId, long accountId);
    Task GenerateAuthCodeAndSendEmailAsync(long tenantId, string email, string @event);
    Task ResetAuthCodeAndSendAsync(long tenantId, long authCodeId);

    Task<(bool, string)> ResetTradeAccountPasswordAsync(long tenantId, long accountId, string cbUrl, string token);

    Task<(bool, string)> UserEventShopOrderPlaced(long tenantId, long eventShopOrderId);

    Task EquityCheckEmailAsync(long tenantId, long accountNumber, string email, string language, List<string> bccEmails,
        DateOnly date);
}