<template>
  <SimpleForm
    ref="modalRef"
    :title="t('action.createRefund')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 380px"
  >
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-width="100px"
          class="demo-ruleForm"
          status-icon
        >
          <el-form-item :label="t('fields.walletId')" prop="targetId">
            <el-input v-model="formData.targetId" :disabled="staticId" />
          </el-form-item>
          <el-form-item label="转入货币" prop="transferCurrencyId">
            <el-select
              v-model="formData.transferCurrencyId"
              placeholder="Please select currency"
            >
              <el-option
                v-for="option in currencyOptions"
                :key="option.value"
                :label="option.label"
                :value="option.value"
              />
            </el-select>
          </el-form-item>
          <el-form-item :label="t('fields.amount')" prop="amount">
            <el-input-number v-model="formData.amount" :precision="2" />
          </el-form-item>
          <el-form-item :label="t('action.comment')" prop="comment">
            <el-input v-model="formData.comment" />
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
import PaymentService from "../../services/PaymentService";
import { useI18n } from "vue-i18n";

const t = useI18n().t;
const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const staticId = ref(false);
const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  targetId: [
    { required: true, message: "Please input wallet ID", trigger: "blur" },
  ],
  amount: [{ required: true, message: "Please input amount", trigger: "blur" }],
  comment: [
    { required: true, message: "Please input comment", trigger: "blur" },
  ],
  transferCurrencyId: [
    { required: true, message: "Please select currency", trigger: "change" },
  ],
});

// 货币选项
const currencyOptions = ref([
  { value: 840, label: "USD" },
  { value: 841, label: "USC" },
]);

const formData = ref<any>({
  targetId: 0,
  targetType: 1,
  amount: 0,
  comment: "",
  transferCurrencyId: 840, // 默认USD
});

const show = async (_targetId = 0) => {
  isLoading.value = true;
  modalRef.value?.show();
  //   resetForm();
  if (_targetId != 0) {
    staticId.value = true;
  }
  formData.value.targetId = _targetId;
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const submit = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid) => {
    isValid = valid;
  });
  if (!isValid) return;

  // Debug: 输出表单数据
  console.log("Form Data:", {
    ...formData.value,
    amount: formData.value.amount * 100, // 显示转换后的金额
  });

  try {
    await PaymentService.createRefund({
      ...formData.value,
      amount: formData.value.amount * 100,
    });
    emits("eventSubmit");
    MsgPrompt.success("success");
    hide();
  } catch (error) {
    //if res.data[0].id is null or undefined then account not found
    MsgPrompt.error(error);
  }
};

// const resetForm = () => {
//   if (!ruleFormRef.value) return;
//   ruleFormRef.value.resetFields();
// };

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
