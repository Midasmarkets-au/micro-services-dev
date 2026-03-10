<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.createLead')"
    :is-loading="isLoading"
    disable-footer
  >
    <el-form
      ref="ruleFormRef"
      :model="ruleForm"
      :rules="rules"
      label-width="120px"
      class="demo-ruleForm"
      size="default"
      status-icon
    >
      <el-form-item :label="$t('fields.userName')" prop="name">
        <el-input v-model="ruleForm.name" :disabled="isLoading" />
      </el-form-item>
      <el-form-item :label="$t('fields.email')" prop="email">
        <el-input v-model="ruleForm.email" :disabled="isLoading" />
      </el-form-item>
      <el-form-item :label="$t('fields.phoneNum')" prop="phoneNumber">
        <el-input v-model="ruleForm.phoneNumber" :disabled="isLoading" />
      </el-form-item>

      <el-form-item>
        <el-button
          type="warning"
          plain
          @click="submitForm(ruleFormRef)"
          :loading="isLoading"
        >
          {{ $t("action.submit") }}
        </el-button>
        <el-button @click="resetForm(ruleFormRef)" :disabled="isLoading">{{
          $t("action.reset")
        }}</el-button>
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { ref } from "vue";
import type { FormInstance } from "element-plus";
import UserService, { CreateLeadSpec } from "../../services/UserService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const emits = defineEmits<{
  (e: "create-success"): void;
}>();
const modalRef = ref();
const isLoading = ref(false);
const ruleFormRef = ref<FormInstance>();

const ruleForm = ref<CreateLeadSpec>({
  name: "",
  email: "",
  phoneNumber: "",
});

const submitForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid, fields) => {
    if (valid) {
      isLoading.value = true;
      await UserService.createLead(ruleForm.value);
      MsgPrompt.success("Create lead success").then(() => {
        isLoading.value = false;
        emits("create-success");
        hide();
      });
    } else {
      MsgPrompt.error("Error submit!!!");
    }
  });
};

const resetForm = (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  formEl.resetFields();
};

const rules = ref<any>({
  name: [
    { required: true, message: "Please input user name", trigger: "blur" },
  ],
  email: [
    { required: true, message: "Please input email address", trigger: "blur" },
    {
      type: "email",
      message: "Please input correct email address",
      trigger: "blur",
    },
  ],
  phoneNumber: [
    { required: true, message: "Please input phone number", trigger: "blur" },
  ],
});

const show = () => {
  modalRef.value?.show();
  resetForm(ruleFormRef.value);
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
