import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;
export enum WalletTypes {
  Fund = 1,
  Cash = 2,
  Bonus = 3,
  Reward = 4,
  Commission = 11,
  Operation = 12,
  Customs = 13,
}

export enum WalletTransactionTypes {
  Deposit = 1,
  Withdraw = 2,
  Transfer = 3,
  Rebate = 4,
  Refund = 5,
  Adjust = 6,
}

enum WalletAdjustSourceTypes {
  ManualAdjust = 0,
  SalesRebate = 1,
  EventRewardCard = 2,
}

export const WalletTransactionOptions = [
  { value: WalletTransactionTypes.Deposit, label: t("title.deposit") },
  { value: WalletTransactionTypes.Withdraw, label: t("title.withdraw") },
  { value: WalletTransactionTypes.Transfer, label: t("title.transfer") },
  { value: WalletTransactionTypes.Rebate, label: t("title.rebate") },
  { value: WalletTransactionTypes.Refund, label: t("title.refund") },
  { value: WalletTransactionTypes.Adjust, label: t("fields.adjust") },
];
