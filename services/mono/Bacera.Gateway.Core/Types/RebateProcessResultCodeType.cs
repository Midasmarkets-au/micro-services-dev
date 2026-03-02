namespace Bacera.Gateway;

public enum RebateProcessResultCodeType
{
    TradeRebateNotFound = -1,
    RebateRuleNotFound = -2,
    TradeRebateUpdateFailed = -3,
    RebateRuleTypeUndefined = -4,
    Success = 1,
    TradeRebateCompletedWithZeroVolume = -7,
    TradeRebateSkippedWithOpenCloseTimeLessThanOneMinute = -8,
}