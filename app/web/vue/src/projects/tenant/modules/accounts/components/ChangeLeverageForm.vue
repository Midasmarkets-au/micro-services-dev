<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeLeverage')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 380px"
  >
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div class="d-flex align-items-center rounded p-5 mb-1 bg-light-primary">
        <span class="svg-icon svg-icon-warning me-5">
          <span class="svg-icon svg-icon-1">
            <inline-svg
              class="rounded"
              src="/images/icons/technology/teh005.svg"
            />
          </span>
        </span>

        <div class="flex-grow-1 me-2">
          <a href="#" class="fw-bold text-gray-800 text-hover-primary fs-2">{{
            $t("tip.currentLeverage")
          }}</a>
        </div>

        <span class="fw-bold py-1 text-primary fs-2">{{
          leverage + ":1"
        }}</span>
      </div>

      <div>
        <label class="required fs-6 fw-semobold mb-2 mt-4">
          {{ $t("tip.newLeverage") }}
        </label>
        <el-form-item>
          <el-select v-model="newLeverage">
            <el-option
              v-for="item in backendConfigLeverageSelections"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { inject, ref } from "vue";
import { backendConfigLeverageSelections } from "@/core/types/LeverageTypes";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const accountId = ref(0);
const leverage = ref<any>();
const newLeverage = ref<any>();

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);

const show = async (_accountId: number) => {
  isLoading.value = true;
  modalRef.value?.show();
  accountId.value = _accountId;
  leverage.value =
    accountDetails.value?.tradeAccount?.tradeAccountStatus?.leverage;
  newLeverage.value = leverage.value;
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const submit = async () => {
  if (leverage.value === newLeverage.value) {
    return;
  }
  try {
    await AccountService.updateTradeAccountLeverage(
      accountId.value,
      newLeverage.value
    );
    MsgPrompt.success("success");
    accountDetails.value.tradeAccount.tradeAccountStatus.leverage = parseInt(
      newLeverage.value
    );
  } catch (error) {
    MsgPrompt.error(error);
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
