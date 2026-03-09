import i18n from "@/core/plugins/i18n";
import { computed } from "vue";
const { t } = i18n.global;

export enum MatterTypes {
  System = 0,
  InternalTransfer = 200,
  Deposit = 300,
  Withdrawal = 400,
  Rebate = 500,
  Refund = 600,
  WalletAdjust = 700,
  TransferRewards = 800,
}

export const MatterTypesOptions = computed(() => [
  { value: MatterTypes.Withdrawal, label: t("title.withdraw") },
  {
    value: MatterTypes.InternalTransfer,
    label: t("title.transfer"),
  },
  { value: MatterTypes.Rebate, label: t("title.rebate") },
  { value: MatterTypes.Refund, label: t("title.refund") },
  { value: MatterTypes.WalletAdjust, label: t("fields.adjust") },
  { value: MatterTypes.TransferRewards, label: t("fields.TransferRewards") },
]);
