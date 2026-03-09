<template>
  <SimpleForm2
    ref="modelRef"
    append-body
    :title="confirmationTitle"
    :is-loading="isLoading || isSubmitting"
    :submit="handleSubmitForm"
    :discard="handleDiscard"
    :save-title="props.confirmTitle ? props.confirmTitle : $t('action.confirm')"
    :saving-title="
      props.waitingTitle ? props.waitingTitle : $t('action.waiting')
    "
    :discard-title="props.cancelTitle ? props.cancelTitle : $t('action.cancel')"
    :width="props.width"
    :submit-color="confirmColor"
    :discard-color="props.discardColor"
    :disable-submit="props.isLoading"
    :disable-discard="props.isLoading"
    :disable-footer="disableFooter"
  >
    <div>
      <h2 class="text-gray-600 fw-semobold fs-2 p-0 m-6">
        {{ confirmationText }}
      </h2>
    </div>
  </SimpleForm2>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from "vue";
import SimpleForm2 from "@/components/SimpleForm.vue";
import { useI18n } from "vue-i18n";
import {
  HandleShowConfirmBoxType,
  ShowConfirmBoxOptionsType,
} from "@/core/types/TenantGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const randomId = ref("");

const props = withDefaults(
  defineProps<{
    isLoading?: boolean;
    handleConfirm?: () => void;
    cancel?: any;
    title?: string;
    confirmationPrompt?: string;
    confirmTitle?: string;
    waitingTitle?: string;
    cancelTitle?: string;
    elId?: string;
    width?: number;
    confirmColor?: string;
    discardColor?: string;
  }>(),
  {
    width: 650,
  }
);
const { t } = useI18n();

const isSubmitting = ref(false);
const confirmationText = ref<any>(null);
const confirmationTitle = ref<any>("");
const disableFooter = ref(false);

const confirmColor = ref(props.confirmColor ?? "primary");

const handleSubmitForm = ref(async () => {
  props.handleConfirm?.();
  hide();
});

const handleDiscard = ref(async () => {
  props.cancel?.();
  hide();
});
const modelRef = ref<any>();

const hide = () => {
  modelRef.value.hide();
};

const show: HandleShowConfirmBoxType = (
  handleConfirm?: () => any,
  handleCancel?: () => any,
  options?: ShowConfirmBoxOptionsType
) => {
  isSubmitting.value = false;
  handleSubmitForm.value = async () => {
    isSubmitting.value = true;
    try {
      await handleConfirm?.();
    } catch (error) {
      MsgPrompt.error(error);
    } finally {
      isSubmitting.value = false;
      hide();
    }
  };

  handleDiscard.value = async () => {
    isSubmitting.value = true;
    try {
      await handleCancel?.();
    } catch (error) {
      MsgPrompt.error(error);
    } finally {
      isSubmitting.value = false;
      hide();
    }
  };

  confirmColor.value = options?.confirmColor ?? "primary";
  confirmationText.value = options?.confirmText ?? t("tip.confirmPrompt");
  confirmationTitle.value = options?.confirmTitle ?? t("tip.confirm");
  disableFooter.value = options?.disableFooter ?? false;

  modelRef.value.show();
};

onMounted(() => {
  randomId.value = Math.random().toString(36).substring(2, 8);
});
defineExpose({
  hide,
  show,
});
</script>
