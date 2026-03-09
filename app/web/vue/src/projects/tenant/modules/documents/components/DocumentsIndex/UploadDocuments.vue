<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.uploadDocumentsFor') + ' : ' + title"
    width="900"
    align-center
    :before-close="hide"
  >
    <div v-for="(item, index) in docsLanguages" :key="index">
      <div class="d-flex gap-4 align-items-center mb-4">
        <el-tag :for="'file-input-' + index" class="w-75px" type="warning">
          {{ index }}</el-tag
        >
        <input
          :id="'file-input-' + index"
          type="file"
          :ref="(el) => (uploadRefs[index] = el)"
          @change="handleFileUpload($event, index)"
          :disabled="isLoading"
          class="form-control"
        />
        <el-button
          v-if="fileList[index]"
          @click="removeFile(index)"
          :disabled="isLoading"
          type="danger"
          :icon="Delete"
          circle
        />
      </div>
    </div>

    <div v-for="(item, index) in uploadedLinks" :key="index">
      <el-link
        :href="item"
        target="_blank"
        type="success"
        class="ms-3"
        underline
        >{{ item }}</el-link
      >
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit" :loading="isLoading" plain>
          {{ $t("action.upload") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, nextTick } from "vue";
import DocsServices from "../../services/DocsServices";
import { ElMessage } from "element-plus";
import { useStore } from "@/store";
import { Delete } from "@element-plus/icons-vue";

const store = useStore();
const user = store.state.AuthModule.user;
const title = ref("");
const dialogRef = ref(false);
const isLoading = ref(false);
const id = ref(0);
const fileList = ref<any>({});
const uploadedLinks = ref<any>([]);
const operatorInfo = ref<any>({
  name: user.nativeName,
  email: user.email,
  updatedAt: new Date().toISOString(),
});
const docsLanguages = ref<any>({});

const uploadRefs = ref<any>({});

for (const [language, file] of Object.entries(fileList.value)) {
  uploadRefs.value[language] = null;
}
const emits = defineEmits(["submitted"]);
const handleFileUpload = (event: Event, key: any) => {
  const target = event.target as HTMLInputElement;
  if (target.files) {
    const files = Array.from(target.files);
    files.forEach((file) => {
      const fileType = file.type;
      const validTypes = [
        "application/msword", // .doc
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
      ];

      if (!validTypes.includes(fileType)) {
        alert("Please upload a valid Word document (.doc or .docx)");
        removeFile(key);
        return;
      }

      const markedFile = Object.assign(file, {
        language: key,
      });
      fileList.value[key] = markedFile;
    });
  }
  console.log(fileList.value);
};

const show = (_data: any, _languages) => {
  id.value = _data.id;
  title.value = _data.title;
  dialogRef.value = true;
  docsLanguages.value = _languages;
};

const hide = () => {
  fileList.value = {};
  docsLanguages.value = {};
  uploadedLinks.value = [];
  dialogRef.value = false;
};

const removeFile = (key: any) => {
  delete fileList.value[key];
  nextTick(() => {
    const inputRef = uploadRefs.value[key];
    if (inputRef) {
      inputRef.value = "";
    }
  });
};

const submit = async () => {
  isLoading.value = true;

  try {
    const formData = new FormData();
    for (const [language, file] of Object.entries(fileList.value)) {
      formData.append("files[]", file, file.name);
      formData.append("languages[]", language);
    }
    formData.append("operator_info", JSON.stringify(operatorInfo.value));
    const res = await DocsServices.uploadDocuments(id.value, formData);
    uploadedLinks.value = res;
    ElMessage.success("Documents uploaded successfully");
    emits("submitted");
  } catch (error) {
    ElMessage.error("Upload file failed, file too large or invalid format");
  }
  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
