import { computed } from "vue";
import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;

export enum CryptoStatusTypes {
  Idle = 0,
  InUse = 1,
  Inactive = 2,
}

export enum CryptoTransactionStatusTypes {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Cancelled = 3,
}

export const CryptoStatusTypeOptions = computed(() => {
  return [
    {
      label: t("status.idle"),
      value: CryptoStatusTypes.Idle,
    },
    {
      label: t("status.inUse"),
      value: CryptoStatusTypes.InUse,
    },
    {
      label: t("status.inactive"),
      value: CryptoStatusTypes.Inactive,
    },
  ];
});

export const CryptoTransactionStatusOptions = computed(() => {
  return [
    {
      label: t("status.pending"),
      value: CryptoTransactionStatusTypes.Pending,
    },
    {
      label: t("status.completed"),
      value: CryptoTransactionStatusTypes.Completed,
    },
    {
      label: t("status.failed"),
      value: CryptoTransactionStatusTypes.Failed,
    },
    {
      label: t("status.cancelled"),
      value: CryptoTransactionStatusTypes.Cancelled,
    },
  ];
});
