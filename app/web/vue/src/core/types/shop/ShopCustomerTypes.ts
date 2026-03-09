import i18n from "@/core/plugins/i18n";
import { computed } from "vue";
const { t } = i18n.global;
export enum EventPartyStatusTypes {
  Applied = 1,
  Approved = 2,
  Rejected = 3,
  Cancelled = 4,
}

export const EventPartyStatusTypeOptions = computed(() => [
  { value: EventPartyStatusTypes.Applied, label: t("status.applied") },
  { value: EventPartyStatusTypes.Approved, label: t("status.approved") },
  { value: EventPartyStatusTypes.Rejected, label: t("status.rejected") },
  { value: EventPartyStatusTypes.Cancelled, label: t("status.cancelled") },
  { value: 5, label: t("status.all") },
]);
