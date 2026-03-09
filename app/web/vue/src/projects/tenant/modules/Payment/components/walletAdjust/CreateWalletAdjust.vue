<template>
  <el-dialog v-model="dialogRef" title="Wallet Adjust" width="800" align-center>
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="120px"
      class="demo-ruleForm"
      status-icon
    >
      <el-form-item label="Adjust Type" prop="sourceType">
        <el-input v-model="formData.sourceType" :disabled="isLoading" />
      </el-form-item>
      <el-form-item label="Wallet Id" prop="walletId">
        <el-input v-model="formData.walletId" :disabled="isLoading" />
      </el-form-item>
      <el-form-item :label="t('fields.amount')" prop="amount">
        <!-- <el-input v-model="formData.amount" :disabled="isLoading" /> -->
        <el-input-number
          v-model="formData.amount"
          :precision="2"
          :disabled="isLoading"
        />
      </el-form-item>
      <el-form-item :label="t('fields.comment')" prop="comment">
        <el-input v-model="formData.comment" :disabled="isLoading" />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit" :loading="isLoading">
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, reactive } from "vue";
import type { FormInstance } from "element-plus";
import Decimal from "decimal.js";
import PaymentService from "../../services/PaymentService";
import { useI18n } from "vue-i18n";
import { ElNotification } from "element-plus";
const t = useI18n().t;
const dialogRef = ref(false);
const isLoading = ref(false);
const ruleFormRef = ref<FormInstance>();
const formData = ref<any>({
  sourceType: 0,
  walletId: 0,
  amount: 0,
  comment: "",
});
const rules = reactive<any>({
  walletId: [
    { required: true, message: "Please input Login", trigger: "blur" },
  ],
  amount: [{ required: true, message: "Please input Amount", trigger: "blur" }],
  comment: [
    { required: true, message: "Please input Comment", trigger: "blur" },
  ],
});

const submit = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  try {
    isLoading.value = true;
    const finalData = {
      amount: new Decimal(formData.value.amount).times(100).toNumber(),
      comment: formData.value.comment,
    };
    await PaymentService.createWalletAdjust(formData.value.walletId, finalData);
    ElNotification({
      title: "Success",
      message: "Wallet Adjusted",
      type: "success",
    });
    emits("eventSubmit");
    hide();
  } catch (error) {
    console.error(error);
    ElNotification({
      title: "Error",
      message: error,
      type: "error",
    });
  }
  isLoading.value = false;
};
const show = () => {
  dialogRef.value = true;
};
const hide = () => {
  dialogRef.value = false;
  formData.value = {
    sourceType: 0,
    walletId: 0,
    amount: 0,
    comment: "",
  };
};
const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();
defineExpose({
  show,
  hide,
});
</script>
