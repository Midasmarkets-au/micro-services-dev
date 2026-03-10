<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeCredit')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 450px"
  >
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-width="70px"
          class="demo-ruleForm"
          status-icon
        >
          <el-form-item v-show="showOption" label="Login" prop="accountId">
            <el-input
              placeholder="Input Login"
              v-model="formData.accountId"
              :disabled="isLoading"
            />
          </el-form-item>
          <el-form-item :label="t('fields.amount')" prop="amount">
            <!-- <el-input v-model="formData.amount" :disabled="isLoading" /> -->
            <el-input-number
              v-model="formData.amount"
              :precision="2"
              :disabled="isLoading"
            />
          </el-form-item>
          <el-form-item :label="t('fields.remark')" prop="comment">
            <el-input
              v-model="formData.comment"
              maxlength="40"
              show-word-limit
              :disabled="isLoading"
            />
          </el-form-item>
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import type { FormInstance } from "element-plus";
import PaymentService from "../services/PaymentService";
import { creditAdjustTypes } from "@/core/types/CreditAdjustTypes";
import { useI18n } from "vue-i18n";
import Decimal from "decimal.js";
const t = useI18n().t;
const isLoading = ref(true);
const showOption = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const finalData = ref<any>();
const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  accountId: [
    { required: true, message: "Please input Login", trigger: "blur" },
  ],
  amount: [
    { required: true, message: "Please input Amount", trigger: "blur" },
    {
      validator: (rule: any, value: any, callback: any) => {
        if (value == 0) {
          callback(new Error("Amount must be greater or  than 0"));
        } else {
          callback();
        }
      },
      trigger: "blur",
    },
  ],
  comment: [
    { required: true, message: "Please input Remark", trigger: "blur" },
  ],
});

const formData = ref<any>({
  accountId: 0,
  amount: 0,
  comment: "",
  type: creditAdjustTypes.Credit,
});

const show = async (_accountId = 0, options = null) => {
  isLoading.value = true;
  processOptions(options);
  modalRef.value?.show();
  resetForm();
  formData.value.accountId = _accountId;
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const processOptions = (options: any) => {
  if (!options) return;
  if (options == "hideLoginNo") {
    showOption.value = false;
  }
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  isLoading.value = true;
  await ruleFormRef.value.validate(async (valid) => {
    isValid = valid;
  });
  if (!isValid) {
    isLoading.value = false;
    return;
  }
  try {
    const res = await PaymentService.getAccountIdByAccountNumber(
      formData.value.accountId
    );

    finalData.value = {
      ...formData.value,
      amount: new Decimal(formData.value.amount).times(100).toNumber(),
    };
    finalData.value.accountId = res.data[0].id;
    await PaymentService.createAdjustRecord(finalData.value);
    emits("eventSubmit");
    MsgPrompt.success("success");
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
  hide();
};

// const submit = async () => {
//   if (!ruleFormRef.value) return;
//   let isValid = false;
//   await ruleFormRef.value.validate(async (valid, fields) => {
//     isValid = valid;
//   });
//   if (!isValid) return;
//   try {
//     const res = await PaymentService.getAccountIdByAccountNumber(
//       formData.value.accountId
//     );

//     finalData.value = { ...formData.value };
//     finalData.value.accountId = res.data[0].id;

//     await PaymentService.createCredit(
//       finalData.value.accountId,
//       finalData.value
//     );

//     emits("eventSubmit");
//     MsgPrompt.success("success");
//   } catch (error) {
//     MsgPrompt.error(error);
//   }
//   hide();
// };

const resetForm = () => {
  if (!ruleFormRef.value) return;
  ruleFormRef.value.resetFields();
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

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
