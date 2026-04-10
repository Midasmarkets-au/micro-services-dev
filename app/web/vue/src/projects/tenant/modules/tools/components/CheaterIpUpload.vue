<template>
  <el-dialog
    v-model="visible"
    :title="$t('cheaterIpUpload.dialogTitle')"
    width="600"
    align-center
    :close-on-click-modal="!isLoading"
    :close-on-press-escape="!isLoading"
    :before-close="beforeClose"
  >
    <div>
      <el-alert
        :title="$t('cheaterIpUpload.hint')"
        type="info"
        :closable="false"
        show-icon
        class="mb-4"
      />

      <el-upload
        ref="uploadRef"
        :limit="1"
        :auto-upload="false"
        :file-list="fileList"
        accept=".xlsx,.xls"
        :on-change="handleFileChange"
        :on-exceed="handleExceed"
      >
        <el-button type="primary" :disabled="isLoading || !!uploadResult">
          {{ $t("cheaterIpUpload.selectFile") }}
        </el-button>
      </el-upload>

      <el-descriptions
        v-if="uploadResult"
        border
        :column="1"
        :title="$t('cheaterIpUpload.resultTitle')"
        class="mt-6"
      >
        <el-descriptions-item :label="$t('cheaterIpUpload.inserted')">
          {{ uploadResult.inserted }}
        </el-descriptions-item>
        <el-descriptions-item :label="$t('cheaterIpUpload.updated')">
          {{ uploadResult.updated }}
        </el-descriptions-item>
        <el-descriptions-item
          :label="$t('cheaterIpUpload.skippedEmptyOrInvalid')"
        >
          {{ uploadResult.skippedEmptyOrInvalid }}
        </el-descriptions-item>
        <el-descriptions-item :label="$t('cheaterIpUpload.skippedDuplicate')">
          {{ uploadResult.skippedDuplicate }}
        </el-descriptions-item>
        <el-descriptions-item :label="$t('cheaterIpUpload.skippedParseError')">
          {{ uploadResult.skippedParseError }}
        </el-descriptions-item>
      </el-descriptions>
    </div>

    <template #footer>
      <div class="text-end w-100">
        <el-button @click="close" :disabled="isLoading">
          {{ $t("action.close") }}
        </el-button>
        <el-button
          type="success"
          @click="submit"
          :loading="isLoading"
          :disabled="!!uploadResult"
        >
          {{ $t("action.upload") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { UploadInstance, UploadProps, UploadRawFile } from "element-plus";
import { genFileId } from "element-plus";
import { useI18n } from "vue-i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ToolServices from "../services/ToolServices";

interface UploadCheaterIpsResult {
  inserted: number;
  updated: number;
  skippedEmptyOrInvalid: number;
  skippedDuplicate: number;
  skippedParseError: number;
  errors: string[];
}

const visible = ref(false);
const isLoading = ref(false);
const uploadRef = ref<UploadInstance>();
const fileList = ref<any[]>([]);
const uploadResult = ref<UploadCheaterIpsResult | null>(null);
const { t } = useI18n();

const emit = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const show = () => {
  visible.value = true;
  uploadResult.value = null;
  fileList.value = [];
};

const handleFileChange: UploadProps["onChange"] = (file) => {
  fileList.value = [file];
};

const handleExceed: UploadProps["onExceed"] = (files) => {
  uploadRef.value?.clearFiles();
  const file = files[0] as UploadRawFile;
  file.uid = genFileId();
  uploadRef.value?.handleStart(file);
  fileList.value = uploadRef.value?.uploadFiles ?? [];
};

const submit = async () => {
  if (uploadResult.value) {
    return;
  }

  const rawFile = fileList.value?.[0]?.raw;
  if (!rawFile) {
    MsgPrompt.warning(t("cheaterIpUpload.selectFileFirst"));
    return;
  }

  isLoading.value = true;
  try {
    const formData = new FormData();
    formData.append("file", rawFile);
    const res = (await ToolServices.uploadCheaterIps(
      formData
    )) as UploadCheaterIpsResult;
    uploadResult.value = res;
    emit("eventSubmit");
    MsgPrompt.success(t("cheaterIpUpload.uploadCompleted"));
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const close = () => {
  if (isLoading.value) return;
  visible.value = false;
};

const beforeClose = (done: () => void) => {
  if (isLoading.value) return;
  done();
};

defineExpose({ show });
</script>
