<template>
  <SimpleForm
    :submit="submit"
    :discard="close"
    :title="$t('title.confirm')"
    :isLoading="isLoading"
    :submitted="submited"
    :saveTitle="displaySubmitButtonText"
    :discard-title="displayCancelButtonText"
    submitColor="success"
    elId="liveAccountCreate"
    ref="simpleFormRef"
  >
    <div class="d-flex flex-column gap-4" v-if="!isLoading">
      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2"> Comment </label>
        <div>
          <Field v-model="formData.comment" name="comment">
            <el-input
              v-model="formData.comment"
              name="comment"
              type="textarea"
              :placeholder="$t('tip.enterComment')"
          /></Field>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="comment"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
      </div>
    </div>
    <!-- <div>{{ formData }}</div> -->
    <!-- <div>{{ confirmedPassword }}</div> -->
  </SimpleForm>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  handleSubmit?: (formData: any) => Promise<any>;
}>();

const simpleFormRef = ref<InstanceType<typeof SimpleForm>>();
const isLoading = ref(true);
const submited = ref(false);
const formData = ref({} as any);

const validationSchema = {
  comment: "required",
};

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});
const { t } = useI18n();

const confirmationText = ref<any>(null);
const confirmationTitle = ref<any>(null);
const cancelButtonText = ref<any>(null);
const submitButtonText = ref<any>(null);

const handleHandleSubmit = ref(props.handleSubmit ?? (() => Promise.reject()));
const displayConfirmationTitle = computed(
  () => confirmationTitle.value ?? t("title.reject")
);

const displayConfirmationText = computed(() => confirmationText.value ?? "");

const displayCancelButtonText = computed(
  () => cancelButtonText.value ?? t("action.cancel")
);

const displaySubmitButtonText = computed(
  () => submitButtonText.value ?? t("action.confirm")
);

const submit = handleSubmit(async () => {
  isLoading.value = true;
  try {
    await handleHandleSubmit.value(formData.value);
    submited.value = true;
    simpleFormRef.value?.hide();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
});

const close = () => {
  simpleFormRef.value?.hide();
};

const show = async (
  _handler?: (form: any) => Promise<any>,
  _confirmationText?: string,
  _confirmationTitle?: string,
  _cancelButtonText?: string,
  _submitButtonText?: string
) => {
  // demoAccountCreateRef.value?.hide();
  if (_handler) {
    handleHandleSubmit.value = _handler;
  }
  if (_confirmationText) {
    confirmationText.value = _confirmationText;
  }
  if (_confirmationTitle) {
    confirmationTitle.value = _confirmationTitle;
  }
  if (_cancelButtonText) {
    cancelButtonText.value = _cancelButtonText;
  }
  if (_submitButtonText) {
    submitButtonText.value = _submitButtonText;
  }

  simpleFormRef.value?.show();
  isLoading.value = true;
  submited.value = false;
  formData.value = {
    comment: "Withdrawal ",
  };

  // resetForm({ values: {} });

  isLoading.value = false;
};

defineExpose({
  show,
});
</script>

<style scoped></style>
