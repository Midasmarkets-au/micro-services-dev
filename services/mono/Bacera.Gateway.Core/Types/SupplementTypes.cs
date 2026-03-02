namespace Bacera.Gateway;

public enum SupplementTypes
{
    Payment = 10,
    Deposit = 20,
    Withdraw = 30,
    WithdrawBackCode = 31, // rowId should be PaymentMethodId
    TradeServiceSyncedOn = 100,
    PaymentServicePolicy = 110,
    PaymentServiceInstruction = 120,
    KycFormHistory = 131, //partyId
    SocialMediaRecord = 135,

    // TradeAccountApplication = 200,
    // UserPaymentAccess = 210, //TODO: remove, not in use
    // AccountPaymentAccess = 220,
    // UserReferral = 230,
    ReferralCode = 231,

    MigrationUserInfo = 240,
    MigrationPersonalInfo = 241, // PartyId
    MigrationFinancialInfo = 242,
    MigrationDocuments = 243,
    MigrationImportedMark = 244,
    MigrationAllowDepositMethod = 245,
    MigrationAllowWithdrawalMethod = 246,
    MigrationKycForm = 247,
    MigrationKycCorrection = 248,

    UserWizard = 300,
    AccountWizard = 301,
    DepositReceipt = 302,
    DepositReference = 303,

    DailyEquityReport = 400, // RowId should be zero
    VerificationQuiz = 500, // RowId should be languageId
    BcrLegalDocument = 600, // RowId should be zero
}