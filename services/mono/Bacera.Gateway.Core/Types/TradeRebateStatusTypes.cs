namespace Bacera.Gateway;

public enum TradeRebateStatusTypes : int
{
    Created = 0,
    Processing = 1,
    Completed = 2,
    CompletedWithZeroAmount = 3,
    SkippedWithOpenCloseTimeLessThanOneMinute = 4,
    PendingResend = 5,
    RuleNotFound = -1,
    HasNoRebate = -2,
    AccountNotFound = -3,
    AccountIsClosed = -4,
}