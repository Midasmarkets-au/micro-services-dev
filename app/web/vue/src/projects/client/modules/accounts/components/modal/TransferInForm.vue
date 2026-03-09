<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.transferIn')"
    :is-loading="isLoading"
    :submit="submit"
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
        {{ $t("tip.maxTransferIn") + ": " }}

        <BalanceShow :balance="userDeposit" :currency-id="currencyId" />
      </h2>
      <label class="required fs-6 fw-semobold mb-2">
        {{ $t("title.transferInAmount") }}
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
import { ref, reactive } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import AccountService from "../../services/AccountService";
import { FormInstance, FormRules } from "element-plus";
import BalanceShow from "@/components/BalanceShow.vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;

const emits = defineEmits<{
  (e: "onCreated"): void;
}>();
const formRef = ref<FormInstance>();
const modalRef = ref<InstanceType<typeof SimpleForm>>();

const accountNumber = ref(0);
const userDeposit = ref(0);
const currencyId = ref(0);
const isLoading = ref(true);
const formData = ref({
  tradeAccountUid: 0,
  walletId: 0,
  amount: null,
});

const show = async (
  _accountNumber: number,
  _uid: number,
  _currencyId: number,
  _fundType: number
) => {
  // showModal(newTargetModalRef.value);
  formRef.value?.resetFields();
  isLoading.value = true;
  modalRef.value?.show();

  currencyId.value = _currencyId;
  try {
    // TODO: get deposit from user wallet by currencyId
    const responseBody = await AccountService.getWalletInfos({
      currencyId: _currencyId,
      fundType: _fundType,
    });
    if (responseBody.data.length < 1) {
      MsgPrompt.error(t("tip.noWalletFound"));
    }

    formData.value.tradeAccountUid = _uid;
    formData.value.walletId = responseBody.data[0].id;
    userDeposit.value = responseBody.data[0].balance;
    isLoading.value = false;
  } catch (error) {
    MsgPrompt.error(error);
  }
  accountNumber.value = _accountNumber;
  isLoading.value = false;
};

const hide = () => {
  // hideModal(newTargetModalRef.value);
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
        isLoading.value = true;
        await AccountService.submitTransferInRequest({
          ...formData.value,
          amount: formData.value.amount * 100,
        });
        MsgPrompt.success(t("tip.transferInSubmit")).then(() => {
          hide();
          emits("onCreated");
        });
      } catch (error) {
        MsgPrompt.error(error);
      } finally {
        isLoading.value = false;
      }
    } else {
      MsgPrompt.warning(t("tip.validationError"));
    }
  });
};

// const checkIfValid = () => (isValid.value = amount.value <= userDeposit.value);

const validateAmount = (rule: any, value: any, callback: any) => {
  // console.log("validateAmount", value);
  if (value === "") {
    callback(new Error(t("tip.pleaseInputTheAmount")));
    return;
  }
  if (value <= 0) {
    callback(new Error(t("tip.amountGreaterThanZero")));
    return;
  }
  if (value > userDeposit.value / 100) {
    callback(new Error(t("tip.amountMustBeLessThanDeposit")));
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

const rules = reactive<FormRules>({
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
