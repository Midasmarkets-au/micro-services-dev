<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.add')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 600px"
  >
    <div>
      <el-form
        ref="ruleFormRef"
        :model="formData"
        :rules="rules"
        label-width="120px"
        class="demo-ruleForm"
        status-icon
      >
        <el-form-item :label="$t('fields.name')" prop="name">
          <el-input v-model="formData.name" />
        </el-form-item>
        <el-form-item :label="$t('fields.type')" prop="type">
          <el-select
            v-model="formData.type"
            :placeholder="$t('error.SELECT_REQUIRE')"
          >
            <el-option
              v-for="option in typeOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="$t('fields.walletAddress')" prop="address">
          <el-input v-model="formData.address" type="textarea" />
        </el-form-item>
      </el-form>
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

const { t } = useI18n();
const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const ruleFormRef = ref<FormInstance>();

const rules = reactive<any>({
  name: [
    {
      required: true,
      message: t("tip.pleaseInput") + t("fields.name"),
      trigger: "blur",
    },
  ],
  type: [
    {
      required: true,
      message: t("error.SELECT_REQUIRE") + t("fields.type"),
      trigger: "change",
    },
  ],
  address: [
    {
      required: true,
      message: t("tip.pleaseInput") + t("fields.walletAddress"),
      trigger: "blur",
    },
  ],
});

// 类型选项
const typeOptions = ref([{ value: "USDT-TRC20", label: "USDT-TRC20" }]);

const formData = ref<any>({
  name: "",
  type: "USDT-TRC20", // 默认值
  address: "",
});

const show = async () => {
  isLoading.value = true;
  modalRef.value?.show();
  resetForm();
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

  try {
    await PaymentService.createCryptoWallet(formData.value);
    emits("eventSubmit");
    MsgPrompt.success(t("status.success"));
    hide();
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const resetForm = () => {
  formData.value = {
    name: "",
    type: "USDT-TRC20",
    address: "",
  };
  if (ruleFormRef.value) {
    ruleFormRef.value.clearValidate();
  }
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

defineExpose({
  show,
  hide,
});
</script>

<style lang="scss" scoped>
.el-select {
  width: 100%;
}
</style>
