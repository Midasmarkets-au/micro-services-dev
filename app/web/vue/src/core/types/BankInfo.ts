import i18n from "@/core/plugins/i18n";
import { computed } from "vue";

const { t } = i18n.global;

export enum BankInfoTypes {
  Wire = 1,
  USDT = 2,
  Other = 3,
}

export const BankInfoOptions = computed(() => {
  return [
    { value: BankInfoTypes.Wire, label: t("fields.wire") },
    { value: BankInfoTypes.USDT, label: t("fields.usdt") },
    { value: BankInfoTypes.Other, label: t("fields.other") },
  ];
});

export const BankInfoNoUSDT = computed(() => {
  return [
    { value: BankInfoTypes.Wire, label: t("fields.wire") },
    { value: BankInfoTypes.Other, label: t("fields.other") },
  ];
});
