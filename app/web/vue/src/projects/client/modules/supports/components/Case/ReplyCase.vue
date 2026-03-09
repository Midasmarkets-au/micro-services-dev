<template>
  <el-form
    ref="ruleFormRef"
    :model="formData"
    :rules="rules"
    label-position="top"
    class="px-5 pb-10"
  >
    <p class="fs-4">Reply</p>
    <el-form-item label="Content" prop="content" required>
      <el-input
        v-model="formData.content"
        :autosize="{ minRows: 6, maxRows: 10 }"
        type="textarea"
      />
    </el-form-item>
    <el-form-item label="Upload">
      <el-upload
        ref="uploadRef"
        list-type="picture-card"
        :limit="5"
        :auto-upload="false"
        :http-request="uploadImage"
        multiple
        v-model:file-list="fileList"
      >
        <el-icon><Plus /></el-icon>

        <template #file="{ file }">
          <div>
            <img
              class="el-upload-list__item-thumbnail"
              :src="file.url"
              alt=""
            />
            <span class="el-upload-list__item-actions">
              <span
                class="el-upload-list__item-preview"
                @click="handlePictureCardPreview(file)"
              >
                <el-icon><zoom-in /></el-icon>
              </span>

              <span
                v-if="!disabled"
                class="el-upload-list__item-delete"
                @click="handleRemove(file)"
              >
                <el-icon><Delete /></el-icon>
              </span>
            </span>
          </div>
        </template>
      </el-upload>
      <div>
        <el-image-viewer
          v-if="dialogVisible"
          :url-list="[dialogImageUrl]"
          @close="dialogVisible = false"
        ></el-image-viewer>
      </div>
    </el-form-item>

    <el-form-item>
      <el-button
        type="primary"
        @click="submitForm(ruleFormRef)"
        :loading="isLoading"
      >
        {{ $t("action.submit") }}
      </el-button>
    </el-form-item>
  </el-form>
</template>
<script setup lang="ts">
import { ref, reactive, computed } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import type { UploadInstance, UploadUserFile, UploadFile } from "element-plus";
import { Delete, Plus, ZoomIn } from "@element-plus/icons-vue";
import SupportService from "../../services/SupportService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ElImageViewer } from "element-plus";
const props = defineProps<{
  item: any;
}>();

const caseId = computed(() => props.item.caseId);
const fileList = ref<UploadUserFile[]>([]);
const isLoading = ref(false);
const uploading = ref(false);
const ruleFormRef = ref<FormInstance>();
const uploadRef = ref<UploadInstance>();
const gUids = ref<any>([]);
const dialogImageUrl = ref("");
const dialogVisible = ref(false);
const disabled = ref(false);
const rules = reactive<FormRules>({
  content: [
    { required: true, message: "Please input content", trigger: "blur" },
  ],
});
const formData = ref<any>({
  categoryId: 1,
  content: "",
  mediumGuids: gUids.value,
});
const emits = defineEmits<{
  (e: "replied"): void;
}>();

const handlePictureCardPreview = (file: UploadFile) => {
  dialogImageUrl.value = file.url as string;
  dialogVisible.value = true;
};

const handleRemove = (file: UploadFile) => {
  const index = fileList.value.indexOf(file);
  fileList.value.splice(index, 1);
};

const uploadImage = async (file?: any) => {
  const formData = new FormData();
  formData.append("file", file.raw);
  try {
    const res = await SupportService.uploadCaseFile("case", formData);
    gUids.value.push(res.guid);
  } catch (error) {
    MsgPrompt.error("Upload file failed, file too large or invalid format");
  }
};

const processUpload = async () => {
  uploading.value = true;
  for (let i = 0; i < fileList.value.length; i++) {
    const file = fileList.value[i];
    await uploadImage(file);
  }
  uploading.value = false;
};

const submitForm = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;
  try {
    await formEl?.validate(async (valid, fields) => {
      if (valid) {
        await processUpload().then(() => {
          SupportService.replyCaseById(caseId.value, formData.value).then(
            () => {
              uploadRef.value?.clearFiles();
              formEl.resetFields();
              isLoading.value = false;
              MsgPrompt.success("Case Replied");
              emits("replied");
            }
          );
        });
      }
    });
  } catch (e) {
    MsgPrompt.error("Case Reply failed");
    isLoading.value = false;
    console.log(e);
  }
};
</script>
<style scoped>
:deep .el-upload-list--picture-card .el-upload-list__item {
  justify-content: center;
}
:deep .el-image-viewer__img {
  max-width: 90% !important;
  max-height: 90% !important;
}
</style>
