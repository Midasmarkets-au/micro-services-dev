namespace Bacera.Gateway.Services;

// 登录注册部分
// 1.	验证email url - ConfirmEmail
// 注册成功
// 3.	找回密码 - ResetPassword
// 以下部分发送email和站内通知
// 账户部分
// 验证信息填写完成 -
// 验证不通过
// 验证通过
// 账户修改信息通知
// 账号部分
// 交易账号申请 - Msg: Account
// 交易账号开通 - Msg: Account - AccountOpened
// 交易账号拒绝 - Msg: Account
// 23.	模拟账号开通 -  Msg: Account - DemoAccountOpened
// 账号入金申请 -  Msg: Deposit
// 25.	账号入金提示 - Msg: Deposit - AccountDepositNotice
// 25.	账号入金链接 - Msg: Deposit - AccountDepositLink
// 27.	账号入金成功 -  Msg: Deposit - AccountDepositCompleted
// 账号入金失败 -  Msg: Deposit
// 29.	账号密码修改完成 - Msg: Account - MT4PasswordChanged
// 账号Transfer Out申请 -  Msg: Withdraw
// 账号Transfer Out失败 -  Msg: Withdraw
// 账号Transfer Out成功 -  Msg: Withdraw
// 账号修改杠杆申请 - Msg: Account
// 32.	账号修改杠杆成功 - Msg: Account - LeverageChanged
// 账号修改杠杆失败 - Msg: Account
// 申请WholeSale账号 - Msg: Account
// 批准WholeSale账号 - Msg: Account
// 拒绝WholeSale账号 - Msg: Account
// 钱包部分
// 钱包入金成功 -  Msg: Deposit
// 钱包入金失败 -  Msg: Deposit
// 42.	钱包出金申请 -  Msg: Withdraw - WithdrawalConfirmation
// 43.	钱包出金失败 -  Msg: Withdraw - WithdrawalFailed
// 钱包出金取消 -  Msg: Withdraw - WithdrawCancelled
public static class EmailTemplateTypes
{
    public static string DefaultLayout => "DefaultLayout";
    public static string ConfirmEmail => "ConfirmEmail";
    public static string RegisteredWelcome => "RegistedWelcome";
    public static string ResetPassword => "ResetPassword";
    public static string VerificationCode => "VerificationCode";
    public static string TradeAccountCreated => "TradeAccountCreated";
    public static string TradeDemoAccountCreated => "TradeDemoAccountCreated";
    public static string PuTradeAccountCreated => "PUTradeAccountCreated";
    public static string ResetMt5TradeAccountPassword => "ResetTradeAccountPassword";
    public static string ResetMt4TradeAccountPassword => "ResetMt4TradeAccountPassword";
    public static string TradeAccountLeverageChanged => "TradeAccountLeverageChanged";
    public static string TransferCreated => "TransferCreated";
    public static string TransferCreatedForIBandSales => "TransferCreatedForIBandSales";
    public static string TransferCompleted => "TransferCompleted";
    public static string TransferCompletedForIBandSales => "TransferCompletedForIBandSales";
    public static string TransferFailed => "TransferFailed";
    public static string TransferFailedForIBandSales => "TransferFailedForIBandSales";
    public static string DepositApplicationForIBandSales => "DepositApplicationForIBandSales";
    public static string DepositApplication => "DepositApplication";
    public static string WithdrawalApplication => "WithdrawalApplication";
    public static string AgentWithdrawalCreated => "AgentWithdrawalCreated";
    public static string AgentWithdrawalCompleted => "AgentWithdrawalCompleted";
    public static string AgentWithdrawalFailed => "AgentWithdrawalFailed";
    public static string AgentWithdrawalCreatedForParent => "AgentWithdrawalCreatedForParent";
    public static string AgentWithdrawalCompletedForParent => "AgentWithdrawalCompletedForParent";
    public static string AgentWithdrawalFailedForParent => "AgentWithdrawalFailedForParent";
    public static string TradeAccountCreatedForIBandSales => "TradeAccountCreatedForIBandSales";
    public static string IBCreated => "IBCreated";
    public static string IBCreatedForIBandSales => "IBCreatedForIBandSales";
    public static string EventShopOrderPlacedForSales => "EventShopOrderPlacedForSales";

    public static string WithdrawalConfirmationForClient => "WithdrawalConfirmation";
    public static string WithdrawalCreatedForParent => "WithdrawalCreatedForIBandSales";
    public static string WithdrawalConfirmationForAgentAndSales => "WithdrawalConfirmationForIBandSales";
    public static string WithdrawalRejectedForClient => "WithdrawalFailed";
    public static string WithdrawalRejectedForAgentAndSales => "WithdrawalFailedForIBandSales";
    public static string WithdrawalCancelledForClient => "WithdrawCancelled";
    public static string WithdrawalCancelledForAgentAndSales => "WithdrawCancelledForIBandSales";

    public static string DepositReceiptUploaded => "DepositReceiptUploaded";
    public static string DepositReceiptUploadedForIBandSales => "DepositReceiptUploadedForIBandSales";
    public static string DepositCompletedForClient => "DepositCompleted";
    public static string DepositCompletedForAgentAndSales => "DepositCompletedForIBandSales";
    public static string ReadOnlyCodeNotice => "ReadOnlyCodeNotice";
    public static string VerificationDocumentRejected => "VerificationDocumentRejected";
    public static string ApplicationForWholesaleCreated => "ApplicationForWholesaleCreated";
    public static string ApplicationForWholesaleRejected => "ApplicationForWholesaleRejected";
    public static string ApplicationForWholesaleApproved => "ApplicationForWholesaleApproved";
    public static string ApplicationForWholesaleEvidenceNeeded => "ApplicationForWholesaleEvidenceNeeded";
    public static string PromotionNotification => "PromotionNotification";

    public static string ApplicationForWholesaleEvidenceForSophisticatedInvestorNeeded =>
        "ApplicationForWholesaleEvidenceForSophisticatedInvestorNeeded";

    public static string ClientDailyConfirmation => "ClientDailyConfirmation";

    public static bool IsExists(string type) => All().Contains(type);

    public static List<string> All() => new()
    {
        DefaultLayout,
        ConfirmEmail,
        ResetPassword,
        TradeAccountCreated,
        TradeDemoAccountCreated,
        ResetMt5TradeAccountPassword,
        TradeAccountLeverageChanged,
        WithdrawalConfirmationForClient,
        DepositCompletedForClient,
        ReadOnlyCodeNotice,
        VerificationDocumentRejected,
        ApplicationForWholesaleCreated,
        ApplicationForWholesaleRejected,
        ApplicationForWholesaleApproved,
        ApplicationForWholesaleEvidenceNeeded,
        ApplicationForWholesaleEvidenceForSophisticatedInvestorNeeded,
    };
}