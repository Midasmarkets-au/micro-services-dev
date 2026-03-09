<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('action.sendEmail') + ' ' + partyId"
    width="1500"
    align-center
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <div class="row row-cols-2">
        <el-form-item label="CC (Comma separated)">
          <el-input v-model="cc" :disabled="isLoading" class="w-500px" />
        </el-form-item>

        <el-form-item label="BCC (Comma separated)">
          <el-input v-model="bcc" :disabled="isLoading" class="w-500px" />
        </el-form-item>
      </div>
      <el-form-item :label="$t('fields.title')" prop="title">
        <el-input
          v-model="formData.title"
          :disabled="isLoading"
          class="w-500px"
        />
      </el-form-item>
      <el-form-item :label="$t('fields.content')" prop="content">
        <div class="w-100">
          <jodit-editor v-model="formData.content" :disabled="isLoading" />
        </div>
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="sendEmail" :loading="isLoading">
          {{ $t("action.send") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref, reactive, inject } from "vue";
import "jodit/build/jodit.min.css";
import { JoditEditor } from "jodit-vue";
import UserService from "../../modules/users/services/UserService";
import type { FormInstance } from "element-plus";
import notification from "@/core/plugins/notification";

const partyId = ref(0);
const dialogRef = ref(false);
const isLoading = ref(false);
const userInfos = inject<any>("userInfos");
const cc = ref("");
const bcc = ref("");
const formData = ref<any>({
  partyId: partyId.value,
  topicId: 10003,
  language: userInfos.value.language || "en",
  cc: [],
  bcc: [],
  title: "",
  subtitle: "",
  content: "",
});

const show = (_partyId) => {
  dialogRef.value = true;
  partyId.value = _partyId;
  formData.value.partyId = partyId.value;
};
const sendEmail = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  isLoading.value = true;
  try {
    formData.value.cc =
      cc.value.length !== 0
        ? cc.value.split(",").map((item) => item.trim())
        : [];
    formData.value.bcc =
      bcc.value.length !== 0
        ? bcc.value.split(",").map((item) => item.trim())
        : [];

    await UserService.sendEmailToUser(formData.value);
    dialogRef.value = false;
    formData.value.title = "";
    formData.value.content = "";
    notification.success();
  } catch (error) {
    console.error(error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  title: [
    { required: true, message: "This field is required", trigger: "change" },
  ],
  content: [
    { required: true, message: "This field is required", trigger: "change" },
  ],
});

defineExpose({
  show,
});
</script>
<style scoped>
:deep .jodit-workplace {
  min-height: 300px !important;
}
</style>
