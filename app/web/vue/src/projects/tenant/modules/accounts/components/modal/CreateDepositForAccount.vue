<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.deposit')"
    :is-loading="isLoading"
    :submit="submit"
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="160px"
      class="demo-ruleForm"
      status-icon
    >
      <el-form-item
        :label="$t('status.paymentChannel')"
        prop="paymentServiceId"
      >
        <el-select v-model="formData.paymentServiceId">
          <el-option
            v-for="item in accountAvailablePaymentServices.deposit"
            :key="item.id"
            :label="item.name"
            :value="item.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item :label="$t('fields.amount')" prop="amount">
        <el-input v-model="formData.amount" />
      </el-form-item>

      <el-form-item :label="$t('fields.note')" prop="note">
        <el-input type="textarea" v-model="formData.note" />
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { inject, ref } from "vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { PaymentServiceTypes } from "@/core/types/PaymentTypes";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { CreateByTenantSpec } from "@/core/models/Deposit";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";

const emits = defineEmits(["fetchData"]);

const ruleFormRef = ref();
const modalRef = ref();
const isLoading = ref(true);
const accountDetails = inject<any>(AccountInjectionKeys.ACCOUNT_DETAILS, {});

const resetForm = async () => {
  formData.value = {
    partyId: accountDetails.value.partyId,
    fundType: accountDetails.value.fundType,
    currencyId: accountDetails.value.currencyId,
    targetTradeAccountUid: accountDetails.value.uid,
  } as CreateByTenantSpec;

  const paymentMethods = await PaymentService.getAccountPaymentMethodById(
    accountDetails.value.id
  );

  accountAvailablePaymentServices.value = Object.keys(paymentMethods)
    .filter((group) => {
      return paymentMethods[group].some((item) => {
        return item.status == 10;
      });
    })
    .reduce((index, group) => {
      index[group] = paymentMethods[group].filter((item) => {
        return item.status == 10;
      });
      return index;
    }, {});

  ruleFormRef.value?.resetFields();
};
const formData = ref<CreateByTenantSpec>({} as CreateByTenantSpec);

const rules = ref({
  paymentServiceId: [
    {
      required: true,
      message: "Please select a payment service",
      trigger: "change",
    },
  ],
  amount: [
    {
      required: true,
      message: "Please input amount",
      trigger: "blur",
    },
  ],
});
const accountAvailablePaymentServices = ref<any>(null);

const submit = async () => {
  formData.value.request = {};
  if (formData.value.paymentServiceId === PaymentServiceTypes.Poli) {
    formData.value.request.MerchantHomepageURL =
      window.location.protocol +
      "//" +
      window.location.host +
      "/wallet/deposit";
    formData.value.request.SuccessURL =
      window.location.protocol +
      "//" +
      window.location.host +
      "/wallet/deposit";
  }
  try {
    isLoading.value = true;
    const res = await AccountService.createDeposit({
      ...formData.value,
      amount: formData.value.amount * 100,
    });
    MsgPrompt.success("Deposit created successfully").then(() => {
      hide();
      emits("fetchData");
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
const show = async () => {
  await resetForm();
  modalRef.value?.show();
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

defineExpose({
  show,
  hide,
});
</script>

<style scoped></style>
