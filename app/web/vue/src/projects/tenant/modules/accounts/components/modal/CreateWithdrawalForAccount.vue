<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.withdrawal')"
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
      <el-form-item label="Payment Service" prop="paymentServiceId">
        <el-select v-model="formData.paymentServiceId">
          <el-option
            v-for="item in accountAvailablePaymentServices.withdrawal"
            :key="item.id"
            :label="item.name"
            :value="item.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="Amount" prop="amount">
        <el-input v-model="formData.amount" />
      </el-form-item>

      <el-form-item label="Target Account" prop="targetAccount">
        <el-select v-model="formData.request">
          <el-option
            v-for="(item, index) in userPaymentInfos"
            :label="item.name"
            :key="index"
            :value="item.info"
          />
        </el-select>
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
import { Field } from "vee-validate";
import { PaymentInfoTenantModal } from "@/core/models/PaymentInfos";
import { CreateFromAccountByTenantSpec } from "@/core/models/Withdrawal";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import { PaymentInfoCriteria } from "@/core/models/PaymentInfos";

const emits = defineEmits(["fetchData"]);

const ruleFormRef = ref();
const modalRef = ref();
const isLoading = ref(true);
const accountDetails = inject<any>(AccountInjectionKeys.ACCOUNT_DETAILS, {});

const getAccountAvailablePaymentServices = inject<any>(
  AccountInjectionKeys.GET_ACCOUNT_AVAILABLE_PAYMENT_SERVICES
);

/**
 * public sealed class CreateFromAccountByTenantSpec
 *     {
 *         [Required] public long PartyId { get; set; }
 *         [Required] public long Amount { get; set; }
 *         [Required] public long AccountUid { get; set; }
 *         [Required] public long PaymentServiceId { get; set; }
 *         [Required] public object Request { get; set; } = null!;
 *     }
 */

const userPaymentInfos = ref<Array<PaymentInfoTenantModal> | null>(null);

const resetForm = async () => {
  formData.value = {
    partyId: accountDetails.value.partyId,
    accountUid: accountDetails.value.uid,
    amount: null,
    paymentServiceId: null,
    request: null,
  };

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

  userPaymentInfos.value = (
    await PaymentService.getPaymentInfos({
      partyId: accountDetails.value.partyId,
    } as PaymentInfoCriteria)
  ).data;

  ruleFormRef.value?.resetFields();
};
const formData = ref<CreateFromAccountByTenantSpec>(
  {} as CreateFromAccountByTenantSpec
);

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
  targetAccount: [
    {
      required: true,
      message: "Please select a target account",
      trigger: "blur",
    },
  ],
});
const accountAvailablePaymentServices = ref<any>(null);

const submit = async () => {
  formData.value.request = {};
  if (formData.value.paymentServiceId === PaymentServiceTypes.Poli) {
    /**
     * TODO: Poli
     */
  }

  try {
    isLoading.value = true;
    await AccountService.createWithdrawalForAccount(accountDetails.value.uid, {
      ...formData.value,
      amount: (formData.value.amount ?? 0) * 100,
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
