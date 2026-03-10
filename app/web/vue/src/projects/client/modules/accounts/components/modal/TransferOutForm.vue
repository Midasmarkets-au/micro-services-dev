<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.transferOut')"
    :is-loading="isSubmitting"
    :submit="submit"
    :reset="hide"
  >
    <table
      v-if="isLoading"
      class="table align-middle table-row-bordered gy-5 mb-0"
      id="kt_permissions_table"
    >
      <tbody>
        <LoadingRing />
      </tbody>
    </table>

    <el-form v-else :rules="rules" :model="formData" ref="formRef">
      <h2 class="text-gray-600 fw-semobold fs-2 p-0 mb-6">
        {{ $t("tip.maxTransferOut") + ": " }}
        <BalanceShow
          :balance="currentBalance * 100"
          :currency-id="currencyId"
        />
      </h2>

      <label class="required fs-6 fw-semobold mb-2">
        {{ $t("title.transferOutAmount") }}
      </label>
      <el-form-item prop="amount">
        <el-input
          v-model="formData.amount"
          :placeholder="$t('tip.pleaseInput')"
        />
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref } from "vue";
import AccountService from "../../services/AccountService";
import { FormInstance, FormRules } from "element-plus";
import BalanceShow from "@/components/BalanceShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import i18n from "@/core/plugins/i18n";

const emits = defineEmits<{
  (e: "onCreated"): void;
}>();
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const formRef = ref<FormInstance>();
const { t } = i18n.global;

const isLoading = ref(true);
const isSubmitting = ref(false);
const accountNumber = ref(0);
const currentBalance = ref(0);
const currencyId = ref(840);
const fundType = ref(-1);
const formData = ref({
  amount: null,
  tradeAccountUid: 0,
  walletId: 0,
});

const show = async (
  _accountNumber: number,
  _uid: number,
  _currencyId: number,
  _fundType: number,
  _currentBalance: number
) => {
  // showModal(newTargetModalRef.value);
  modalRef.value?.show();
  isLoading.value = true;
  try {
    formData.value.tradeAccountUid = _uid;
    currentBalance.value = _currentBalance;
    currencyId.value = _currencyId;
    fundType.value = _fundType;
    // TODO: get deposit from user wallet by currencyId
    const responseBody = await AccountService.getWalletInfos({
      currencyId: _currencyId,
      fundType: _fundType,
    });
    if (responseBody.data.length < 1) {
      MsgPrompt.error(t("tip.noWalletFound"));
    }

    formData.value.walletId = responseBody.data[0].id;
  } catch (error) {
    MsgPrompt.error(t("tip.fail"));
  } finally {
    isLoading.value = false;
  }
  accountNumber.value = _accountNumber;
};

const hide = () => {
  modalRef.value?.hide();
  formRef.value?.resetFields();
};

const submit = () => {
  if (!formRef.value) {
    return;
  }
  formRef.value.validate(async (valid) => {
    if (valid && formData.value.amount) {
      try {
        isSubmitting.value = true;
        await AccountService.submitTransferOutRequest({
          ...formData.value,
          amount: formData.value.amount * 100,
        });
        MsgPrompt.success(t("tip.transferOutSubmit")).then(() => {
          hide();
          emits("onCreated");
        });
      } catch (error) {
        MsgPrompt.error(error);
      } finally {
        isSubmitting.value = false;
      }
    } else {
      MsgPrompt.warning(t("tip.validationError"));
    }
  });
};

const validateAmount = (rule: any, value: any, callback: any) => {
  if (value === "") {
    callback(new Error(t("tip.pleaseInputTheAmount")));
    return;
  }
  if (value <= 0) {
    callback(new Error(t("tip.amountGreaterThanZero")));
    return;
  }
  if (value > currentBalance.value) {
    callback(new Error(t("tip.amountMustBeLessThanBalance")));
    return;
  }
  if (isNaN(value)) {
    callback(new Error(t("tip.pleaseEnterAValidNumber")));
    return;
  }
  if (!/^\d+(\.\d{1,2})?$/.test(value)) {
    callback(new Error(t("tip.upToTwoDecimalPlaces")));
    return;
  }
  callback();
};

const rules = ref<FormRules>({
  amount: [{ validator: validateAmount, trigger: "blur" }],
});

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
