<template>
  <el-upload
    v-model:file-list="fileList"
    :auto-upload="false"
    :disabled="isLoading"
    class="w-350px"
    :show-file-list="false"
    multiple
    drag
  >
    <div>
      <el-icon class="el-icon--upload"><upload-filled /></el-icon>
      <div class="el-upload__text">
        <div v-html="$t('fields.dropFileHereOrClickHere')"></div>
      </div>
    </div>
  </el-upload>

  <div class="mt-5" v-if="fileList.length > 0">
    <div class="mb-5">
      <h4>{{ $t("fields.previewLinks") }}</h4>
    </div>

    <div v-for="(file, index) in fileList" :key="index">
      <div class="d-flex align-items-center mb-3">
        <el-input
          v-model="file.name"
          :disabled="isLoading"
          style="max-width: 1100px"
        >
          <template #prepend
            >https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/</template
          >
        </el-input>
        <el-button
          @click="handleRemove(file)"
          :disabled="isLoading"
          class="ms-4"
          :icon="Delete"
          circle
          type="danger"
          size="small"
        />
      </div>
    </div>
  </div>
  <div class="mt-5">
    <el-button
      @click="submitUpload"
      :loading="isLoading"
      :disabled="fileList.length <= 0"
      type="success"
      plain
    >
      {{ $t("action.clickToUpload") }}
    </el-button>
  </div>

  <div class="mt-5" v-if="uploadedLinks.length > 0">
    <el-divider />
    <div class="mb-5">
      <h4>{{ $t("fields.uploadedFileLinks") }}</h4>
    </div>
    <div v-for="(link, index) in uploadedLinks" :key="index">
      <el-link :href="link" target="_blank" type="primary" class="mb-3">{{
        link
      }}</el-link>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { useStore } from "@/store";
import type { UploadUserFile } from "element-plus";
import { UploadFilled } from "@element-plus/icons-vue";
import { Delete } from "@element-plus/icons-vue";
import ItServices from "../../services/ItServices";
import notification from "@/core/plugins/notification";

const isLoading = ref(false);
const fileList = ref<UploadUserFile[]>([]);
const store = useStore();
const user = store.state.AuthModule.user;
const uploadedLinks = ref<string[]>([]);
const operatorInfo = ref<any>({
  name: user.nativeName,
  email: user.email,
  updatedAt: new Date().toISOString(),
});

const submitUpload = async () => {
  isLoading.value = true;
  try {
    const formData = new FormData();
    fileList.value.forEach((file) => {
      formData.append("files[]", file.raw);
      formData.append("fileNames[]", file.name);
    });
    formData.append("operatorInfo", JSON.stringify(operatorInfo.value));
    const res = await ItServices.uploadPublicFiles(formData);
    if (res) {
      notification.success();
      fileList.value = [];
      uploadedLinks.value = res;
    }
  } catch (e) {
    notification.danger();
    console.log(e);
  } finally {
    isLoading.value = false;
  }
};

const handleRemove = (file: UploadUserFile) => {
  fileList.value = fileList.value.filter((f) => f !== file);
};
</script>
