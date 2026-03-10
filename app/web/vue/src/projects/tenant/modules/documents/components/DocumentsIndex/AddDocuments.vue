<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.createNewDocument')"
    width="900"
    align-center
    :before-close="hide"
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <el-form-item :label="$t('fields.documentName')" prop="title">
        <el-input
          v-model="formData.title"
          clearable
          :disabled="isLoading"
        ></el-input>
      </el-form-item>
      <el-form-item :label="$t('fields.documentUrl')">
        <el-input v-model="formData.link" clearable disabled></el-input>
      </el-form-item>
      <el-form-item :label="$t('fields.comment')">
        <el-input
          v-model="formData.comment"
          clearable
          :disabled="isLoading"
        ></el-input>
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="warning" @click="submit" :loading="isLoading" plain>
          {{ $t("action.upload") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, watch, reactive } from "vue";
import DocsServices from "../../services/DocsServices";
import { ElMessage } from "element-plus";
import { useStore } from "@/store";
import i18n from "@/core/plugins/i18n";
import type { FormInstance } from "element-plus";
const t = i18n.global.t;
const store = useStore();
const user = store.state.AuthModule.user;
const site = ref(user.tenancy == "au" ? "ba" : user.tenancy);
const dialogRef = ref(false);
const isLoading = ref(false);
const emits = defineEmits(["submitted"]);
const operatorInfo = ref<any>({
  name: user.nativeName,
  email: user.email,
  updatedAt: new Date().toISOString(),
});
const formData = ref<any>({
  title: "",
  link: "",
  comment: "",
  site: site.value,
  operator_info: operatorInfo.value,
});

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  title: [
    { required: true, message: "Please input document name", trigger: "blur" },
  ],
});

const show = () => {
  dialogRef.value = true;
};

const hide = () => {
  dialogRef.value = false;
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  isLoading.value = true;

  try {
    await DocsServices.addNewDocument(formData.value);
    ElMessage.success("Documents Created Successfully");
    emits("submitted");
    hide();
  } catch (error) {
    ElMessage.error("Failed to create document");
  }
  isLoading.value = false;
};

watch(
  () => formData.value.title,
  (newTitle) => {
    formData.value.link = newTitle.trim().replace(/\s/g, "-").toLowerCase();
  }
);

defineExpose({
  show,
});
</script>
