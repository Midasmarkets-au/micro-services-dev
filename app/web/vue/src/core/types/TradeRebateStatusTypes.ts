import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;

export enum TradeRebateStatusTypes {
  Created = 0,
  Processing = 1,
  Completed = 2,
  CompletedWithZeroAmount = 3,
  SkippedWithOpenCloseTimeLessThanOneMinute = 4,
  RuleNotFound = -1,
  HasNoRebate = -2,
  AccountNotFound = -3,
}

export const getTradeRebateStatusSelections = (
  values?: Array<TradeRebateStatusTypes>
) => {
  values ??= Object.values(TradeRebateStatusTypes)
    .filter((v) => typeof v === "number")
    .map((v) => v as TradeRebateStatusTypes);

  return values.map((v) => ({
    value: v,
    label: t(`type.tradeRebateStatus.${v}`),
  }));
};
