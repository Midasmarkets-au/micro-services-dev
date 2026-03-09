import i18n from "@/core/plugins/i18n";
import { computed } from "vue";

const { t } = i18n.global;
export enum EventShopOrderStatusTypes {
  Pending = 0,
  Processing = 1,
  Shipped = 2,
  Succeed = 3,
  Cancelled = 4,
}

export enum EventShopPointTransactionSourceTypes {
  OpenAccount = 0,
  Deposit = 1,
  Trade = 2,
  Adjust = 3,
  Purchase = 4,
}

export enum EventShopPointTransactionTypes {
  Pending = 0,
  Success = 1,
  Fail = 2,
}

export enum EventShopRewardRebateStatusTypes {
  Pending = 0,
  Processing = 1,
  Succeed = 2,
  Failed = 3,
}

export enum EventShopRewardStatusTypes {
  Pending = 0,
  Processing = 1,
  Approved = 2,
  Succeeded = 3,
  Cancelled = 4,
  Expired = 5,
  Active = 10,
  Inactive = 11,
}

export const EventShopPointTransactionSourceOptions = computed(() => [
  {
    label: t("action.openAccountActions"),
    value: EventShopPointTransactionSourceTypes.OpenAccount,
  },
  {
    label: t("fields.deposit"),
    value: EventShopPointTransactionSourceTypes.Deposit,
  },
  {
    label: t("fields.trade"),
    value: EventShopPointTransactionSourceTypes.Trade,
  },
  {
    label: t("fields.adjust"),
    value: EventShopPointTransactionSourceTypes.Adjust,
  },

  {
    label: t("fields.purchase"),
    value: EventShopPointTransactionSourceTypes.Purchase,
  },

  { label: t("status.all"), value: null },
]);

export const EventShopOrderStatusOptions = computed(() => [
  { label: t("status.pending"), value: EventShopOrderStatusTypes.Pending },
  {
    label: t("status.processing"),
    value: EventShopOrderStatusTypes.Processing,
  },
  { label: t("status.shipped"), value: EventShopOrderStatusTypes.Shipped },
  { label: t("status.completed"), value: EventShopOrderStatusTypes.Succeed },
  { label: t("status.cancelled"), value: EventShopOrderStatusTypes.Cancelled },
  { label: t("status.all"), value: null },
]);

export const EventShopRewardRebateStatusOptions = computed(() => [
  {
    label: t("status.completed"),
    value: EventShopRewardRebateStatusTypes.Succeed,
  },
  {
    label: t("status.pending"),
    value: EventShopRewardRebateStatusTypes.Pending,
  },
  {
    label: t("status.processing"),
    value: EventShopRewardRebateStatusTypes.Processing,
  },

  { label: t("status.failed"), value: EventShopRewardRebateStatusTypes.Failed },
  { label: t("status.all"), value: null },
]);

export const EventShopRewardStatusOptions = computed(() => [
  { label: t("status.pending"), value: EventShopRewardStatusTypes.Pending },
  { label: t("status.approved"), value: EventShopRewardStatusTypes.Approved },
  { label: t("status.cancelled"), value: EventShopRewardStatusTypes.Cancelled },
  { label: t("status.active"), value: EventShopRewardStatusTypes.Active },
  { label: t("status.inactive"), value: EventShopRewardStatusTypes.Inactive },
  { label: t("status.all"), value: null },
]);

export const EventShopPointTransactionTypesOptions = computed(() => [
  { label: t("status.success"), value: EventShopPointTransactionTypes.Success },
  { label: t("status.pending"), value: EventShopPointTransactionTypes.Pending },
  { label: t("status.failed"), value: EventShopPointTransactionTypes.Fail },
  { label: t("status.all"), value: null },
]);
