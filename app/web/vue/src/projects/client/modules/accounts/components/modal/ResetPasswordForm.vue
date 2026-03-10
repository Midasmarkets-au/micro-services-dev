<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.resetMT4Password')"
    :is-loading="false"
    :submit="submit"
    ><span class="text-gray-600 fw-semobold fs-2 p-0 m-0">{{
      $t("tip.clickSubmitPwd") + tradeAccount?.accountNumber
    }}</span></SimpleForm
  >
</template>

<script setup lang="ts">
import { ref } from "vue";
import AccountService from "../../services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import SimpleForm from "@/components/SimpleForm.vue";

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const tradeAccount = ref<any>();
const { t } = useI18n();

const show = (_tradeAccount) => {
  // showModal(newTargetModalRef.value);
  modalRef.value?.show();
  tradeAccount.value = _tradeAccount;
};

const submit = async () => {
  var baseUrl =
    window.location.protocol +
    "//" +
    window.location.hostname +
    (window.location.port ? ":" + window.location.port : "");
  // if (window.location.href.includes("portal")) {
  //   baseUrl += "/portal";
  // }
  const callbackUrl = baseUrl + "/change-account-password";
  try {
    await AccountService.submitPasswordResetRequest({
      accountUid: tradeAccount.value.uid,
      accountNumber: tradeAccount.value.accountNumber,
      callbackUrl,
    });
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("submit");
      modalRef.value?.hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};
defineExpose({
  show,
  hide: () => {
    modalRef.value?.hide();
  },
});
</script>

<style lang="scss">
.el-select {
  width: 100%;
}

.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}
</style>
