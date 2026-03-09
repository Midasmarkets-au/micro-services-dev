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
        <el-input v-model="ruleForm.name" />
      </el-form-item>
      <el-form-item :label="$t('fields.email')" prop="email">
        <el-input v-model="ruleForm.email" />
      </el-form-item>
      <el-form-item :label="$t('fields.phoneNum')" prop="phoneNumber">
        <el-input v-model="ruleForm.phoneNumber" />
      </el-form-item>

      <el-form-item>
        <el-button type="warning" plain @click="submitForm(ruleFormRef)">
          {{ $t("action.submit") }}
        </el-button>
        <el-button @click="resetForm(ruleFormRef)">{{
          $t("action.reset")
        }}</el-button>
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { computed, ref } from "vue";
import type { FormInstance } from "element-plus";
import RepService, {
  CreateLeadSpec,
} from "@/projects/client/modules/rep/services/RepService";
import { useStore } from "@/store";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const emits = defineEmits<{
  (e: "create-success"): void;
}>();
const modalRef = ref();
const isLoading = ref(true);
const store = useStore();
const repAccount = computed(() => store.state.RepModule.repAccount);
const ruleFormRef = ref<FormInstance>();
const ruleForm = ref<CreateLeadSpec>({
  name: "",
  email: "",
  phoneNumber: "",
});

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

const submitForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid, fields) => {
    if (valid) {
      await RepService.createLead(ruleForm.value);
      MsgPrompt.success("Create lead success").then(() => {
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
