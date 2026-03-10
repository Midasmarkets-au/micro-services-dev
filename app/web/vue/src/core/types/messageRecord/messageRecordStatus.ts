import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;
import { computed } from "vue";

export enum MessageRecordStatus {
  pending = 0,
  sent = 1,
  failed = 2,
}

export const messageRecordStatusOptions = computed(() => {
  return [
    { value: MessageRecordStatus.pending, label: t("status.pending") },
    { value: MessageRecordStatus.sent, label: t("status.sent") },
    { value: MessageRecordStatus.failed, label: t("status.failed") },
  ];
});
