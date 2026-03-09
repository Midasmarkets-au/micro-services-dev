<template>
  <el-form
    ref="ruleFormRef"
    :model="formData"
    :rules="rules"
    label-position="top"
    class="px-5 py-10"
  >
    <el-form-item label="Category" prop="category">
      <div>
        <div
          v-for="(options, level) in optionsList"
          :key="level"
          class="row mb-3 ms-1 category-select"
        >
          <select
            class="form-select"
            required
            v-if="options.length > 0"
            v-model="selectedOptions[level]"
            @change="updateSelection(level, selectedOptions[level])"
          >
            <option value="" selected disabled>Select an option...</option>
            <option v-for="option in options" :key="option.id" :value="option">
              {{ $t("title." + option.name) }}
            </option>
          </select>
        </div>
      </div>
    </el-form-item>

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
        class="btn btn-primary btn-sm btn-radius"
        @click="submitForm(ruleFormRef)"
        :loading="isLoading"
      >
        {{ $t("action.submit") }}
      </el-button>
    </el-form-item>
  </el-form>
</template>
<script setup lang="ts">
import { ref, onMounted, reactive, inject, watch } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import type {
  UploadInstance,
  UploadUserFile,
  UploadFile,
  UploadProps,
} from "element-plus";
import { FileFormatTypes } from "@/core/types/FileFormatTypes";
import SupportService from "../../services/SupportService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ClientView } from "@/core/types/SupportStatusTypes";
import { Delete, Plus, ZoomIn } from "@element-plus/icons-vue";
import { ElImageViewer } from "element-plus";
import { ElMessage } from "element-plus";

const isLoading = ref(false);
const uploading = ref(false);
const ruleFormRef = ref<FormInstance>();
const uploadRef = ref<UploadInstance>();
const changeView = inject<(status: any) => void>("changeView");
const selectedOptions = ref(<any>[]);
const optionsList = ref([[]]);
const gUids = ref<any>([]);
const fileList = ref<UploadUserFile[]>([]);
const dialogImageUrl = ref("");
const dialogVisible = ref(false);
const disabled = ref(false);
const rules = reactive<FormRules>({
  content: [
    { required: true, message: "Please input content", trigger: "blur" },
  ],
  category: [
    // Adding a custom validation rule
    {
      validator: async (rule, value) => {
        // Assuming selectedOptions is reactive and accessible in this context
        if (
          selectedOptions.value[0] !== null &&
          selectedOptions.value[0] !== ""
        ) {
          return true; // Validation passed
        }
        return Promise.reject("Please select a category"); // Validation failed
      },
      trigger: "change",
    },
  ],
});
const formData = ref<any>({
  categoryId: 1,
  content: "",
  mediumGuids: gUids.value,
});
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
const beforeUpload: UploadProps["beforeUpload"] = (rawFile) => {
  console.log("beforeUpload", rawFile);
  if (
    rawFile.type !== FileFormatTypes.JPG &&
    rawFile.type !== FileFormatTypes.JPEG &&
    rawFile.type !== FileFormatTypes.PNG
  ) {
    ElMessage.error("Picture must be JPG, JPEG, or PNG format.");
    return false;
  } else if (rawFile.size / 1024 / 1024 > 10) {
    ElMessage.error("Picture size can not exceed 10MB!");
    return false;
  }
  return true;
};
watch(fileList, (newVal) => {
  console.log("newVal", newVal.value);
});
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
  if (!formEl) return;
  try {
    await formEl?.validate(async (valid, fields) => {
      if (valid) {
        await processUpload().then(() => {
          SupportService.createCase(formData.value).then(() => {
            MsgPrompt.success("Create case successfully");
            changeView(ClientView.CaseList);
            isLoading.value = false;
          });
        });
        // await SupportService.createCase(formData.value);
        // MsgPrompt.success("Create case successfully");
        // changeView(ClientView.CaseList);
      }
    });
  } catch (e) {
    MsgPrompt.error("Create case failed");
    isLoading.value = false;
  }
  isLoading.value = false;
};

const updateSelection = (level, selectedOption) => {
  // Update the selection for the current level
  formData.value.categoryId = selectedOption.id;
  selectedOptions.value[level] = selectedOption;

  // Clear selections and options for all deeper levels
  if (selectedOption && selectedOption.hasChild) {
    selectedOptions.value = selectedOptions.value.slice(0, level + 1);
    optionsList.value = optionsList.value.slice(0, level + 1);
    optionsList.value[level + 1] = []; // Prepare the next level
    fetchData(level + 1, selectedOption.id); // Fetch data for the next level
  } else {
    // If the selected option has no children, clear all deeper levels
    selectedOptions.value = selectedOptions.value.slice(0, level + 1);
    optionsList.value = optionsList.value.slice(0, level + 1);
  }
};

const fetchData = async (level, parentId = null) => {
  try {
    const response =
      parentId == null
        ? await SupportService.getCaseCategory()
        : await SupportService.getCaseCategory(parentId);
    const data = response || [];
    if (data.length > 0) {
      optionsList.value[level] = data;
      selectedOptions.value[level] = "";
    } else {
      optionsList.value[level] = [];
    }
  } catch (error) {
    console.error("Error fetching case categories:", error);
    optionsList.value[level] = [];
  }
};

onMounted(() => {
  fetchData(0); // Initial fetch
});
</script>
<style scoped>
.category-select {
  min-width: 350px;
  width: max-content;
}
.upload {
  min-width: 150px;
  width: max-content;
}
:deep .el-upload-list--picture-card .el-upload-list__item {
  justify-content: center;
}
:deep .el-image-viewer__img {
  max-width: 90% !important;
  max-height: 90% !important;
}
</style>
