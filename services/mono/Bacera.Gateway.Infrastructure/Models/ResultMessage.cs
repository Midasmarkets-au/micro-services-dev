namespace Bacera.Gateway;

public static class ResultMessage
{
    public static class Common
    {
        public const string InvalidInput = "__INVALID_INPUT__";
        public const string InvalidType = "__INVALID_TYPE__";
        public const string BrokerUidNotFound = "__BROKER_UID_NOT_FOUND__";
        public const string AgentUidNotFound = "__AGENT_UID_NOT_FOUND__";
        public const string UidNotFound = "__UID_NOT_FOUND__";
        public const string RepUidNotFound = "__REP_UID_NOT_FOUND__";
        public const string SalesUidNotFound = "__SALES_UID_NOT_FOUND__";
        public const string ActionNotAllow = "__ACTION_NOT_ALLOW__";
        public const string UserNotFound = "__USER_NOT_FOUND__";
        public const string SendMailFailed = "__SEND_MAIL_FAILED__";
        public const string RecordExists = "__RECORD_ALREADY_EXISTS__";
        public const string PendingRecordExists = "__PENDING_RECORD_ALREADY_EXISTS__";
        public const string OutOfRange = "__OUT_OF_RANGE__";
        public const string LanguageNotSupport = "__LANGUAGE_NOT_SUPPORT__";
        public const string InvalidToken = "__INVALID_TOKEN__";
        public const string ActionFail = "__ACTION_FAIL__";
        public const string TokenGenerateFail = "__TOKEN_GENERATE_FAIL__";
        public const string InvalidAmount = "__INVALID_AMOUNT__";
        public const string RecordNotFound = "__RECORD_NOT_FOUND__";
        public const string DeleteFail = "__DELETE_FAIL__";
        public const string RecordAlreadyExists = "__RECORD_ALREADY_EXISTS__";
        public const string InvalidDataFormat = "__INVALID_DATA_FORMAT__";
        public const string ZeroAmountNotAllowed = "__ZERO_AMOUNT_ARE_NOT_ALLOWED__";
    }

    public static class CopyTrade
    {
        public const string AccountIsNotOwnedByCurrentParty = "__ACCOUNT_IS_NOT_OWNED_BY_CURRENT_PARTY__";
        public const string DeleteRuleFailed = "__DELETE_RULE_FAILED__";
        public const string MaxRuleCountReached = "__MAX_RULE_COUNT_REACHED__";
        public const string ModeInvalid = "__MODE_INVALID__";
        public const string RecordExists = "__RECORD_ALREADY_EXISTS__";
        public const string RecordNotFound = "__RECORD_NOT_FOUND__";
        public const string RuleCreationFailed = "__RULE_CREATION_FAILED__";
        public const string SourceAccountNotFound = "__SOURCE_ACCOUNT_NOT_FOUND__";
        public const string TargetAccountNotFound = "__TARGET_ACCOUNT_NOT_FOUND__";
    }

    public static class Group
    {
        public const string IdNotMatch = "__ID_NOT_MATCH__";
        public const string GroupNameExists = "__GROUP_NAME_EXISTS__";
        public const string GroupNotMatch = "__GROUP_NOT_MATCH__";
        public const string NoAccountAdded = "__NO_ACCOUNT_ADDED__";
        public const string GroupNotFound = "__GROUP_NOT_FOUND__";
        public const string NoAccountRemoved = "__NO_ACCOUNT_REMOVED__";
        public const string PleaseRemoveAllAccountFirst = "__PLEASE_REMOVE_ALL_ACCOUNT_FIRST__";
    }

    public static class Register
    {
        public const string EmailExists = "__EMAIL_EXISTS__";
        public const string RegisterFail = "__REGISTER_FAIL__";
        public const string InvalidEmail = "__INVALID_EMAIL__";
        public const string InvalidToken = "__INVALID_TOKEN__";
        public const string RequestFail = "__REQUEST_FAIL__";
        public const string UserNotExists = "__USER_NOT_EXISTS__";
        public const string ChangePasswordFail = "__CHANGE_PASSWORD_FAIL__";

        public const string AlreadyRegisterOtherSite =
            "You have already registered in another site, please contact your sales if you want to register at this site.";
    }

    public static class TwoFactor
    {
        public const string InvalidUser = "__INVALID_USER__";
        public const string InvalidAuthenticatorKey = "__INVALID_AUTHENTICATOR_KEY__";
        public const string InvalidAuthenticatorCode = "__INVALID_AUTHENTICATOR_CODE__";
        public const string InvalidData = "__INVALID_DATA__";

        public const string Success = "__SUCCESS__";

        //"Your authenticator app key has been reset, you will need to configure your authenticator app using the new key."
        public const string ResetSuccess = "__RESET_SUCCESS__";

        //"Cannot disable 2FA as it's not currently enabled"
        public const string CannotEnable2Fa = "__CANNOT_ENABLE_2FA__";
        public const string CannotDisable2Fa = "__CANNOT_DISABLE_2FA__";

        //"Cannot disable 2FA as it's not currently enabled"
        public const string Enable2FaFail = "__ENABLE_2FA_FAILED__";
        public const string Disable2FaFail = "__DISABLE_2FA_FAILED__";

        public const string Enable2FaSuccess = "__2FA_HAS_BEEN_SUCCESSFULLY_ENABLED__";
        public const string Disable2FaSuccess = "__2FA_HAS_BEEN_SUCCESSFULLY_DISABLED__";
    }

    public static class Account
    {
        public const string AccountCodeLengthShouldBeGreaterThanFour =
            "__ACCOUNT_CODE_LENGTH_SHOULD_BE_GREATER_THAN_4__";

        public const string InvalidAccountStatus = "__INVALID_ACCOUNT_STATUS__";
        public const string GroupNameAlreadyExist = "__GROUP_NAME_ALREADY_EXIST__";
        public const string CodeAlreadyExist = "__CODE_ALREADY_EXIST__";
        public const string InvalidReferPath = "__INVALID_REFER_PATH__";
        public const string ReportGenerationFailed = "__REPORT_GENERATION_FAILED__";
        public const string InvalidAccountTag = "__INVALID_ACCOUNT_TAG__";
        public const string AccountTagAlreadyExists = "__ACCOUNT_TAG_ALREADY_EXISTS__";
        public const string AccountTagNotFound = "__ACCOUNT_TAG_NOT_FOUND__";
        public const string ParentAccountNotFound = "__PARENT_ACCOUNT_NOT_FOUND__";
        public const string InvalidLeverage = "__INVALID_LEVERAGE__";
        public const string InvalidAmount = "__INVALID_AMOUNT__";
        public const string InvalidComment = "__INVALID_COMMENT__";
        public const string AccountCreateFailed = "__ACCOUNT_CREATE_FAILED__";
        public const string AccountTypeAlreadyAssigned = "__ACCOUNT_TYPE_ALREADY_ASSIGNED__";
        public const string AccountInactivated = "__ACCOUNT_INACTIVATED__";
        public const string AccountIsAlreadyAssignedToThisAgent = "__ACCOUNT_IS_ALREADY_ASSIGNED_TO_THIS_AGENT__";
        public const string AccountIsAlreadyAssignedToThisSales = "__ACCOUNT_IS_ALREADY_ASSIGNED_TO_THIS_SALES__";
        public const string AccountIsNotAClient = "__ACCOUNT_IS_NOT_A_CLIENT__";
        public const string GroupChangeActionNotAllowed = "__GROUP_CHANGE_ACTION_NOT_ALLOWED__";
        public const string GroupNotUnderThisAccount = "__GROUP_NOT_UNDER_THIS_ACCOUNT__";
        public const string AccountIsNotAClientOrAgent = "__ACCOUNT_IS_NOT_A_CLIENT_OR_AGENT_";
        public const string AccountIsNotASales = "__ACCOUNT_IS_NOT_A_SALES__";
        public const string AccountIsNotAnAgent = "__ACCOUNT_IS_NOT_AN_AGENT__";
        public const string AccountIsNotARep = "__ACCOUNT_IS_NOT_A_REP__";
        public const string AccountNotExists = "__ACCOUNT_NOT_EXISTS__";
        public const string AccountNotWholesale = "__NOT_WHOLESALE_ACCOUNT__";
        public const string AccountIsWholesale = "__ACCOUNT_IS_ALREADY_WHOLESALE__";
        public const string AssignAgentFailed = "__ASSIGN_AGENT_FAILED__";
        public const string AssignSalesFailed = "__ASSIGN_SALES_FAILED__";
        public const string AssignRepFailed = "__ASSIGN_REP_FAILED__";
        public const string RemoveAgentFailed = "__REMOVE_AGENT_FAILED__";
        public const string RemoveSalesFailed = "__REMOVE_SALES_FAILED__";
        public const string RemoveRepFailed = "__ACCOUNT_GROUP_NOT_EXISTS__";
        public const string GroupExists = "__GROUP_EXISTS__";
        public const string CodeExists = "__CODE_EXISTS__";
        public const string GroupLengthShouldBeBetweenThreeAndTwenty = "__GROUP_LENGTH_SHOULD_BE_BETWEEN_3_AND_20__";
        public const string PasswordNotMatchRequirement = "__PASSWORD_NOT_MATCH_REQUIREMENT__";
        public const string ServiceNotAllowAccountCreation = "__SERVICE_NOT_ALLOW_ACCOUNT_CREATION__";
        public const string ServiceNotFound = "__SERVICE_NOT_FOUND__";
        public const string TradeAccountCreateFailed = "__TRADE_ACCOUNT_CREATE_FAILED__";
        public const string TradeServerError = "__TRADE_SERVER_ERROR__";
        public const string ReadyOnlyCodeNotExists = "__READ_ONLY_CODE_NOT_EXISTS__";
    }

    public static class Matter
    {
    }

    public static class Tenant
    {
        public const string DomainExists = "__DOMAIN_EXISTS__";
        public const string IdsNotMatched = "__IDS_NOT_MATCHED__";
        public const string InvalidDomain = "__INVALID_DOMAIN__";
        public const string DeleteDomainFailed = "__DELETE_DOMAIN_FAILED__";
    }

    public static class Application
    {
        public const string AccountGroupNotProvided = "__ACCOUNT_GROUP_NOT_PROVIDED__";
        public const string AssignReferralCodeFailed = "__ASSIGN_REFERRAL_CODE_FAILED__";
        public const string ApplicationStateCannotApprove = "__APPLICATION_STATE_CANNOT_APPROVE__";
        public const string ApplicationStateCannotReject = "__APPLICATION_STATE_CANNOT_REJECT__";
        public const string ApplicationStateCannotReverseReject = "__APPLICATION_STATE_CANNOT_REVERSE_REJECT__";
        public const string ApplicationStateCannotComplete = "__APPLICATION_STATE_CANNOT_COMPLETE__";
        public const string ApplicationStateCannotUpdate = "__APPLICATION_STATE_CANNOT_UPDATE__";
        public const string ApplicationTypeInvalid = "__APPLICATION_TYPE_INVALID__";
        public const string ApproveFailed = "__APPROVE_FAILED__";
        public const string AssignUserRoleFailed = "__ASSIGN_USER_ROLE_FAILED__";
        public const string CreateAccountFailed = "__CREATE_ACCOUNT_FAILED__";
        public const string AssignRepAccountFailed = "__ASSIGN_REP_ACCOUNT_FAILED__";
        public const string CreateDefaultReferralCodeFailed = "__CREATE_DEFAULT_REFERRAL_CODE_FAILED__";
        public const string LeverageInvalid = "__LEVERAGE_INVALID__";
        public const string RejectFailed = "__REJECT_FAILED__";
        public const string ReverseRejectFailed = "__REVERSE_REJECT_FAILED__";
        public const string CompleteFailed = "__COMPLETE_FAILED__";
        public const string TradeServiceNotFount = "__TRADE_SERVICE_NOT_FOUNT__";
        public const string ServiceIdNotProvided = "__SERVICE_ID_NOT_PROVIDED__";
        public const string TradeServiceGroupNotProvided = "__TRADE_SERVICE_GROUP_NOT_PROVIDED__";
        public const string ServiceIdNotFound = "__SERVICE_ID_NOT_FOUND__";
        public const string SalesCodeExistsForCreateSalesAccount = "__SALES_CODE_EXISTS_FOR_CREATE_SALES_ACCOUNT__";

        public const string SalesGroupCodeExistsForCreateSalesAccount =
            "__SALES_GROUP_CODE_EXISTS_FOR_CREATE_SALES_ACCOUNT__";

        public const string RepAccountNotExists = "__REP_ACCOUNT_NOT_EXISTS__";

        public const string AgentGroupExists = "__AGENT_GROUP_EXISTS__";
        public const string GroupExists = "__GROUP_EXISTS__";
        public const string AlreadyOwnsGroupOfThisType = "__ALREADY_OWNS_GROUP_OF_THIS_TYPE__";
        public const string SalesCodeNotExistsForCreateIbAccount = "__SALES_CODE_NOT_EXISTS_FOR_CREATE_IB_ACCOUNT__";
        public const string SalesCodeRequireForCreateSalesAccount = "__SALES_CODE_REQUIRE_FOR_CREATE_SALES_ACCOUNT__";
        public const string SalesHasNoRepAccount = "__SALES_HAS_NO_REP_ACCOUNT__";
        public const string AgentCodeRequireForCreateAgentAccount = "__AGENT_CODE_REQUIRE_FOR_CREATE_AGENT_ACCOUNT__";

        public const string SalesCodeNotExistsForCreateClientAccount =
            "__SALES_CODE_NOT_EXISTS_FOR_CREATE_CLIENT_ACCOUNT__";

        public const string NotAllowToCreateAccountForBroker =
            "__NOT_ALLOW_TO_CREATE_ACCOUNT_FOR_BROKER__";

        public const string GroupExistsForCreateSalesAccount = "__GROUP_EXISTS_FOR_CREATE_SALES_ACCOUNT__";
        public const string InvalidParameter = "__INVALID_PARAMETER__";

        public const string ChangePasswordDataError = "__CHANGE_PASSWORD_DATA_ERROR__";
        public const string ChangeLeverageDataError = "__CHANGE_LEVERAGE_DATA_ERROR__";
        public const string HasSameLeverage = "__HAS_SAME_LEVERAGE__";
        public const string TradeServiceErrorPleaseRetry = "__TRADE_SERVICE_ERROR_PLEASE_RETRY__";
        public const string ChangeAccountTypeError = "__CHANGE_ACCOUNT_TYPE_ERROR__";
    }

    public static class User
    {
        public const string UserNotFound = "__USER_NOT_FOUND__";
        public const string AddRoleFailed = "__ADD_ROLE_FAILED__";
        public const string RemoveRoleFailed = "__REMOVE_ROLE_FAILED__";
        public const string UserGodModeNotAllowed = "__USER_GOD_MODE_NOT_ALLOWED__";
        public const string PermissionNotFound = "__PERMISSION_NOT_FOUND__";
        public const string AddPermissionFailed = "__ADD_PERMISSION_FAILED__";
        public const string RemovePermissionFailed = "__REMOVE_PERMISSION_FAILED__";
        public const string RemoveClaimFailed = "__REMOVE_CLAIM_FAILED__";
        public const string AddClaimFailed = "__ADD_CLAIM_FAILED__";
        public const string SalesUidNotFoundForUser = "__SALES_UID_NOT_FOUND_FOR_CURRENT_USER__";
        public const string AgentUidNotFoundForUser = "__AGENT_UID_NOT_FOUND_FOR_CURRENT_USER__";
        public const string UserEmailNotSet = "__USER_EMAIL_NOT_SET__";
    }

    public static class Verification
    {
        public const string CannotUpdateAtThisStatus = "__CANNOT_UPDATE_AT_THIS_STATUS__";
        public const string CannotUnderReview = "__CANNOT_UNDER_REVIEW_AT_THIS_STATUS__";
        public const string CannotAwaitingApprove = "__CANNOT_AWAITING_APPROVE_AT_THIS_STATUS__";
        public const string CannotApprove = "__CANNOT_APPROVE_AT_THIS_STATUS__";
        public const string CannotReject = "__CANNOT_REJECT_AT_THIS_STATUS__";
        public const string PhoneNumberInvalid = "__PHONE_NUMBER_INVALID__";
        public const string RegionCodeInvalid = "__REGION_CODE_INVALID__";
        public const string VerificationFail = "__VERIFICATION_FAIL__";
        public const string VerificationDisabled = "__VERIFICATION_DISABLED__";
        public const string UploadedDocumentNotExists = "__UPLOADED_DOCUMENT_NOT_EXISTS__";
        public const string UploadedDocumentMissed = "__UPLOADED_DOCUMENT_MISSED__";
        public const string UploadedDocumentAlreadyApproved = "__UPLOADED_DOCUMENT_ALREADY_APPROVED__";
        public const string UploadedDocumentAlreadyRejected = "__UPLOADED_DOCUMENT_ALREADY_REJECTED__";
    }

    public static class Payment
    {
        public const string PaymentPlatformNotMatched = "__PAYMENT_PLATFORM_NOT_MATCHED__";
        public const string CurrentStatusCannotChange = "__CURRENT_STATUS_CANNOT_CHANGE__";
        public const string PaymentNotFoundForCurrentAction = "__PAYMENT_NOT_FOUND_FOR_CURRENT_ACTION__";
        public const string PaymentCancelFailed = "__PAYMENT_CANCEL_FAILED__";
        public const string PaymentCompleteFailed = "__PAYMENT_COMPLETE_FAILED__";
        public const string PaymentExecuteFailed = "__PAYMENT_EXECUTEE_FAILED__";
        public const string PaymentAlreadyCompleted = "__PAYMENT_ALREADY_COMPLETED__";
        public const string InvalidPayment = "__INVALID_PAYMENT__";
    }

    public static class Deposit
    {
        public const string ApproveFailed = "__APPROVE_FAILED__";
        public const string CancelFailed = "__CANCEL_FAILED__";
        public const string CannotCancel = "__CANNOT_CANCEL_THIS_DEPOSIT__";
        public const string CompleteFailed = "__COMPLETE_FAILED__";
        public const string CompletePaymentFailed = "__COMPLETE_PAYMENT_FAILED__";
        public const string DepositCreateFailed = "__DEPOSIT_CREATE_FAILED__";
        public const string DepositRestoreFailed = "__DEPOSIT_RESTORE_FAILED__";
        public const string InvalidParameters = "__INVALID_PARAMETERS__";
        public const string PaymentExecuteFailed = "__PAYMENT_EXECUTE_FAILED__";
        public const string EmptyRequestParameter = "__EMPTY_REQUEST_PARAMETER__";
        public const string PaymentAmountNotMatch = "__PAYMENT_AMOUNT_NOT_MATCH__";
        public const string PaymentDoesNotComplete = "__PAYMEN_DOES_NOT_COMPLETE__";
        public const string PaymentNotFound = "__PAYMENT_NOT_FOUND__";
        public const string PaymentServiceNotFound = "__PAYMENT_SERIVCE_NOT_FOUND__";
        public const string PaymentPlatformNotFound = "__PAYMENT_PLATFORM_NOT_FOUND__";
        public const string TargetTradeAccountNotExists = "__TARGET_TRADE_ACCOUNT_NOT_EXISTS__";
        public const string PartyNotMatch = "__PARTY_NOT_MATCH__";
        public const string CurrencyNotMatch = "__CURRENCY_NOT_MATCH__";
        public const string ExchangeRateNotExists = "__EXCHANGE_RATE_NOT_EXISTS__";
        public const string InitialDepositAmountNotMatch = "__INITIAL_DEPOSIT_AMOUNT_NOT_MATCH__";
        public const string DepositAmountLessThanMinValue = "__DEPOSIT_AMOUNT_LESS_THAN_MIN_VALUE__";
        public const string DepositAmountMoreThanMaxValue = "__DEPOSIT_AMOUNT_MORE_THAN_MAX_VALUE__";
        public const string MaxIncompleteDepositReached = "__MAX_INCOMPLETE_DEPOSIT_REACHED__";

        public const string TheSpecifiedServiceDoesNotHavePermissionForUser =
            "__THE_SPECIFIED_SERVICE_DOES_NOT_HAVE_PERMISSION_FOR_USER__";

        public const string TheSpecifiedServiceDoesNotHavePermissionForAccount =
            "__THE_SPECIFIED_SERVICE_DOES_NOT_HAVE_PERMISSION_FOR_ACCOUNT__";

        public const string WalletTransferFailed = "__WALLET_TRANSFER_FAILED__";
        public const string WalletAndAccountFundTypesNotMatch = "__WALLET_AND_ACCOUNT_FUND_TYPE_NOT_MATCH__";
    }

    public static class Refund
    {
        public const string CreateFailed = "__CREATE_FAILED__";
        public const string CompleteFailed = "__COMPLETE_FAILED__";
        public const string OnlyPrimaryWalletAllowed = "__ONLY_PRIMARY_WALLET_ALLOWED__";
    }

    public static class Wallet
    {
        public const string WalletNotFound = "__WALLET_NOT_FOUND__";
        public const string OnlyPrimaryWalletAllowed = "__ONLY_PRIMARY_WALLET_ALLOWED__";
    }

    public static class Withdrawal
    {
        public const string CannotCancel = "__CANNOT_CANCEL_THIS_WITHDRAWAL__";
        public const string InvalidParameters = "__INVALID_PARAMETERS__";
        public const string CurrentStateCannotEdit = "__CURRENT_STATE_CANNOT_EDIT__";
        public const string CurrentPaymentStateCannotEdit = "__CURRENT_PAYMENT_STATE_CANNOT_EDIT__";
        public const string ApproveFailed = "__APPROVE_FAILED__";
        public const string RejectFailed = "__REJECT_FAILED__";
        public const string CancelFailed = "__CANCEL_FAILED__";
        public const string CompleteFailed = "__COMPLETE_FAILED__";
        public const string RefundFailed = "__REFUND_FAILED__";
        public const string WalletTransferFailed = "__WALLET_TRANSFER_FAILED__";
        public const string SourceAccountNotFound = "__SOURCE_ACCOUNT_NOT_FOUND__";
        public const string SourceAccountChangeBalanceFail = "__SOURCE_ACCOUNT_CHANGE_BALANCE_FAIL__";
        public const string SourceAccountBalanceNotEnough = "__SOURCE_ACCOUNT_BALANCE_NOT_ENOUGH__";
        public const string CannotGetBalance = "__CANNOT_GET_BALANCE__";
        public const string CannotGetTradeAccountBalance = "__CANNOT_GET_TRADE_ACCOUNT_BALANCE__";

        public const string TheSpecifiedServiceDoesNotHavePermission =
            "__THE_SPECIFIED_SERVICE_DOES_NOT_HAVE_PERMISSION__";
    }

    public static class Transaction
    {
        public const string TransferFailed = "__TRANSACTION_TRANSFER_FAILED__";
        public const string ApproveFailed = "__TRANSACTION_APPROVE_FAILED__";
        public const string CancelFailed = "__TRANSACTION_CANCEL_FAILED__";
        public const string RejectFailed = "__TRANSACTION_REJECT_FAILED__";
        public const string CompleteFailed = "__TRANSACTION_COMPLETE_FAILED__";
        public const string WalletInvalided = "__WALLET_INVALIDED__";
        public const string BalanceNotEnough = "__BALANCE_NOT_ENOUGH__";
        public const string TradeAccountInvalided = "__TRADE_ACCOUNT_INVALIDED__";
        public const string CurrencyNotMatched = "__CURRENCY_NOT_MATCHED__";
        public const string FundTypeNotMatched = "__FUND_TYPE_NOT_MATCHED__";
        public const string TransactionTypeNotSupported = "__TRANSACTION_TYPE_NOT_SUPPORTED__";
        public const string TransactionPartialFailed = "__TRANSACTION_PARTIAL_FAILED__";
        public const string ChangeTradeAccountBalanceFailed = "__CHANGE_TRADE_ACCOUNT_BALANCE_FAILED__";
        public const string ChangeWalletBalanceFailed = "__CHANGE_WALLET_BALANCE_FAILED__";
        public const string PendingTransferExist = "__PENDING_TRANSFER_EXIST__";
        public const string VerificationCodeExpired = "__VERIFICATION_CODE_EXPIRED__";
        public const string SourceWalletNotFound = "__SOURCE_WALLET_NOT_FOUND__";
        public const string TargetWalletNotFound = "__TARGET_WALLET_NOT_FOUND__";
        public const string CannotTransferToSameWallet = "__CANNOT_TRANSFER_SAME_WALLET__";
        public const string FundTypeNotMatch = "__FUND_TYPE_MISMATCH_BETWEEN_WALLET__";
        public const string CurrencyNotMatch = "__CURRENCY_MISMATCH_BETWEEN_WALLET__";
        public const string InvalidVerificationCode = "__INVALID_VERIFICATION_CODE__";
        public const string VerificationCodeRequired = "__VERIFICATION_CODE_REQUIRED__";
        public const string TargetWalletRequired = "__TARGET_WALLET_REQUIRED__";
        public const string FailedSendVerificationCode = "__FAILED_SEND_VERIFICATION_CODE__";
        public const string WithdrawalBlockedAfterEmailPhoneChange = "__WITHDRAWAL_BLOCKED_AFTER_EMAIL_PHONE_CHANGE__";
    }

    public static class TradeAccount
    {
        public const string EmailIsRequired = "__EMAIL_IS_REQUIRED__";
        public const string CannotTransferOwnAccount = "__CANNOT_TRANSFER_TO_OWN_ACCOUNT__";
        public const string NoIBSalesAccount = "__NO_IB_SALES_ACCOUNT__";
        public const string AccountIsNotDownline = "__ACCOUNT_IS_NOT_DOWNLINE__";
        public const string TargetAccountNoPrimaryWallet = "__TARGET_ACCOUNT_NO_PRIMARY_WALLET__";
    }

    public static class TradePassword
    {
        public const string PasswordComplexityFailed = "__PASSWORD_COMPLEXITY_FAILED__";
        public const string PasswordTooShort = "__PASSWORD_TOO_SHORT__";
        public const string PasswordTooLong = "__PASSWORD_TOO_LONG__";
        public const string PasswordChangeRateLimitExceeded = "__PASSWORD_CHANGE_RATE_LIMIT_EXCEEDED__";
        public const string InitialPasswordNotFound = "__INITIAL_PASSWORD_NOT_FOUND__";
        public const string Mt5PasswordChangeFailed = "__MT5_PASSWORD_CHANGE_FAILED__";
        public const string InvalidPasswordType = "__INVALID_PASSWORD_TYPE__";
        public const string TradeAccountNotFound = "__TRADE_ACCOUNT_NOT_FOUND__";
    }

    public static class RebateRule
    {
        public const string CreateFailed = "__CREATE_FAILED__";
        public const string ParentAgentRuleNotExist = "__PARENT_AGENT_RULE_NOT_EXIST__";
        public const string CanNotConfirmByCreator = "__CAN_NOT_CONFIRM_BY_CREATOR__";
        public const string UpdateFailed = "__UPDATE_FAILED__";
        public const string UpdateInfoFailed = "__UPDATE_INFO_FAILED__";
        public const string DeleteItemFailed = "__DELETE_ITEM_FAILED__";
        public const string SymbolIdNotMatch = "__SYMBOL_ID_NOT_MATCH__";
        public const string RuleIdNotMatch = "__RULE_ID_NOT_MATCH__";
        public const string SymbolIdNotExists = "__SYMBOL_ID_NOT_EXISTS__";
        public const string SymbolCategoryIdNotExists = "__SYMBOL_CATEGORY_NOT_EXISTS__";
        public const string RebateRuleNotExists = "__REBATE_RULE_NOT_EXISTS__";
        public const string RebateRuleSettingNotExists = "__REBATE_RULE_SETTING_NOT_EXISTS__";
        public const string SchemaItemRateCouldNotIncrease = "__SCHEMA_ITEM_RATE_COULD_NOT_INCREASE__";
        public const string ItemsShouldNotBeEmpty = "__ITEMS_SHOULD_NOT_BE_EMPTY__";
        public const string AgentUidOrClientUidNotMatches = "__AGENT_UID_OR_CLIENT_UID_NOT_MATCHES__";
        public const string DirectRuleNotExists = "__DISTRIBUTION_RULE_NOT_EXISTS__";
        public const string DirectRuleExists = "__DISTRIBUTION_RULE_EXISTS__";
        public const string PercentageOutOfRange = "__PERCENTAGE_OUT_OF_RANGE__";
        public const string NameExists = "__NAME_EXISTS__";
        public const string AccountHasAgent = "__ACCOUNT_HAS_AGENT__";
        public const string AccountHasBroker = "__ACCOUNT_HAS_BROKER__";
        public const string AccountShouldNotHasAnAgent = "__ACCOUNT_SHOULD_NOT_HAS_AN_AGENT__";
        public const string BrokerSchemaShouldHaveTypeZero = "__BROKER_SCHEMA_SHOULD_HAVE_TYPE_ZERO__";
        public const string BrokerExists = "__BROKER_EXISTS__";
        public const string CreateBrokerFailed = "__CREATE_BROKER_FAILED__";
        public const string AccountTypeNotExists = "__ACCOUNT_TYPE_NOT_EXISTS__";

        public const string PipsAndCommissionCannotBeSetAtTheSameTime =
            "__PIPS_AND_COMMISSION_CANNOT_BE_SET_AT_THE_SAME_TIME__";

        public const string SchemaContainsNotExistsRuleId = "__SCHEMA_CONTAINS_NOT_EXISTS_RULE_ID__";
        public const string CannotDeleteSchemaWithRelatedAccount = "__CANNOT_DELETE_SCHEMA_WITH_RELATED_ACCOUNT__";
        public const string SchemaInUse = "__SCHEMA_IN_USE__";
        public const string CommissionNotAllow = "__COMMISSION_NOT_ALLOW__";
        public const string PipNotAllow = "__PIP_NOT_ALLOW__";
    }

    public static class Referral
    {
        public const string InvalidSupplement = "__INVALID_SUPPLEMENT__";
        public const string ReferralCodeNotExist = "__REFERRAL_CODE_NOT_EXIST__";

        // You have reached the maximum number of referral codes
        public const string YouHaveReachedTheMaximumNumberOfReferralCodes =
            "__YOU_HAVE_REACHED_THE_MAXIMUM_NUMBER_OF_REFERRAL_CODES__";

        public const string CodeExists = "__CODE_EXISTS__";
        public const string CodeMinLenghtMustBeGreaterThenFive = "__CODE_MIN_LENGTH_MUST_BE_GREATER_THEN_5__";
        public const string CurrentAccountDoesNotHaveBroker = "__CURRENT_ACCOUNT_DOES_NOT_HAVE_BROKER__";
        public const string CurrentAgentDoesNotHaveRebateRule = "__CURRENT_AGENT_DOES_NOT_HAVE_REBATE_RULE__";
        public const string BaseSchemaEmpty = "__BASE_SCHEMA_EMPTY__";
        public const string BrokerageNotExists = "__BROKERAGE_NOT_EXISTS__";
        public const string InvalidAllocationSchema = "__INVALID_ALLOCATION_SCHEMA__";
        public const string InvalidReferralType = "__INVALID_REFERRAL_TYPE__";
        public const string InvalidBaseSchema = "__INVALID_BASE_SCHEMA__";
        public const string InvalidAccountType = "__INVALID_ACCOUNT_TYPE__";
        public const string SchemasNotMatchBrokerRule = "__SCHEMAS_NOT_MATCH_BROKER_RULE__";
        public const string AlphaSchemasNotMatchBrokerRule = "__ALPHA_SCHEMAS_NOT_MATCH_BROKER_RULE__";
        public const string AdvanceSchemasNotMatchBrokerRule = "__ADVANCE_SCHEMAS_NOT_MATCH_BROKER_RULE__";
        public const string StandardSchemasNotMatchBrokerRule = "__STANDARD_SCHEMAS_NOT_MATCH_BROKER_RULE__";
        public const string CodeMustBeAlphanumeric = "__CODE_MUST_BE_ALPHANUMERIC__";
    }

    public static class Lead
    {
        public const string AssignFailed = "__ASSIGN_FAILED__";
        public const string ArchivedFailed = "__ARCHIVED_FAILED__";
        public const string AddedCommentFailed = "__ADDED_COMMENT_FAILED__";
        public const string InvalidParameters = "__INVALID_PARAMETERS__";
    }

    public static class HostingService
    {
        public const string CommandExists = "__COMMAND_EXISTS__";
        public const string CommandNotFound = "__COMMAND_NOT_FOUND__";
        public const string StopHandlerNotFound = "__STOP_HANDLER_NOT_FOUND__";
        public const string StopHandlerExists = "__STOP_HANDLER_EXISTS__";
    }

    public static class SalesRebate
    {
        public const string ExceedTotalValue = "__EXCEED_TOTAL_VALUE__";
    }
}