import i18n from "@/core/plugins/i18n";
import { computed } from "vue";
const { t } = i18n.global;

export enum RebateStatus {
  RebateCreated = 500,
  RebateCanceled = 505,
  RebateOnHold = 510,
  RebateReleased = 520,
  RebateCompleted = 550,
  RebateTradeClosedLessThanOneMinute = 590,
}

export const getTradeRebateStatusSelections = computed(() => {
  return Object.keys(RebateStatus)
    .filter((key) => {
      return !isNaN(Number(key));
    })
    .map((key) => {
      return {
        label: t(`type.rebateStatus.${key}`),
        value: Number(key),
      };
    });
});
