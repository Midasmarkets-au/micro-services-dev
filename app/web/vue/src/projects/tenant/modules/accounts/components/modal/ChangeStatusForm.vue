<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeStatus')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 380px"
  >
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div class="d-flex align-items-center rounded p-5 mb-1 bg-light-primary">
        <span class="svg-icon svg-icon-warning me-5">
          <span class="svg-icon svg-icon-1"> </span>
        </span>

        <div class="flex-grow-1 me-2">
          <a href="#" class="fw-bold text-gray-800 text-hover-primary fs-2"
            >{{ $t("tip.currentStatus") }}:
          </a>
        </div>

        <span class="fw-bold py-1 text-primary fs-2">{{
          {
            0: $t("status.normal"),
            1: $t("status.paused"),
            2: $t("status.closed"),
          }[accountDetails.status]
        }}</span>
      </div>

      <div class="mt-4">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("tip.newStatus") }}
        </label>
        <el-form-item>
          <el-select
            v-model="newStatus"
            name="status"
            type="text"
            :placeholder="$t('action.select')"
          >
            <el-option :label="$t('status.normal')" :key="0" :value="0" />
            <el-option :label="$t('status.paused')" :key="1" :value="1" />
            <el-option :label="$t('status.closed')" :key="2" :value="2" />
          </el-select>
        </el-form-item>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { inject, ref } from "vue";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

const emits = defineEmits<{
  (event: "changeStatus", status): void;
}>();
const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);

const newStatus = ref<any>();
const show = async (account: any) => {
  isLoading.value = true;
  modalRef.value?.show();
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const submit = async () => {
  if (newStatus.value === accountDetails.value.status) return;
  try {
    isLoading.value = true;
    const res = await AccountService.updateAccountStatus(
      accountDetails.value.id,
      newStatus.value
    );
    accountDetails.value.status = newStatus.value;
    emits("changeStatus", newStatus.value);
    MsgPrompt.success("change success").then(() => hide());
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
  hide();
};

defineExpose({
  show,
  hide,
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
