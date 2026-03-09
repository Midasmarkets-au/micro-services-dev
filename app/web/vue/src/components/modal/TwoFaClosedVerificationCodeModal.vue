<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.close2FaVerification')"
    :is-loading="isLoading"
    :submit="submit"
  >
    <div class="mx-5">
      <label class="mb-3 fs-4">Verification Code</label>
      <el-input v-model="code" />
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const emits = defineEmits<{
  (e: "code-sent", val: boolean): void;
}>();
const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const code = ref("");
const show = async () => {
  modalRef.value?.show();
  isLoading.value = true;

  isLoading.value = false;
};

const submit = async () => {
  if (code.value === "") {
    MsgPrompt.error("Please enter verification code");
    return;
  }
  isLoading.value = true;
  TenantGlobalService.disable2Fa(code.value)
    .then(() => MsgPrompt.success("Two Factor Authentication Disabled"))
    .then(() => emits("code-sent", false))
    .then(() => (isLoading.value = false))
    .then(() => modalRef.value?.hide())
    .catch(() => MsgPrompt.error("Verification Code Error"));
};

defineExpose({
  show,
  hide: () => modalRef.value?.hide(),
});
</script>

<style scoped></style>
