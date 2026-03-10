<template>
  <el-upload
    ref="uploadRef"
    class="avatar-uploader"
    :before-upload="beforeAvatarUpload"
  >
    <div v-if="!imageUrl">
      <el-icon class="avatar-uploader-icon"><Plus /></el-icon>
    </div>
  </el-upload>

  <div v-if="imageUrl" class="avatar-uploader image-container">
    <img :src="imageUrl" class="avatar" />
    <div class="image-overlay">
      <div>
        <el-icon class="fs-2x text-white" @click="handlePreview()">
          <ZoomIn />
        </el-icon>
        <el-icon class="fs-2x text-white" @click="handleRemove()">
          <Delete />
        </el-icon>
      </div>
    </div>
  </div>
  <div>
    <el-image-viewer
      v-if="dialogVisible"
      :url-list="[dialogImageUrl]"
      @close="dialogVisible = false"
    ></el-image-viewer>
  </div>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import type { UploadInstance, UploadProps, UploadRawFile } from "element-plus";
import TenantGlobalService from "../services/TenantGlobalService";
import { ElMessage } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Delete, Plus, ZoomIn } from "@element-plus/icons-vue";

const file = ref<UploadRawFile>();
const dialogImageUrl = ref("");
const dialogVisible = ref(false);
const gUids = ref<any>([]);
const uploading = ref(false);
const uploadRef = ref<UploadInstance>();
const imageUrl = ref("");

const uploadImage = async () => {
  uploading.value = true;
  const formData = new FormData();
  formData.append("file", file.value);
  try {
    const res = await TenantGlobalService.uploadImage("case", formData);
    gUids.value.push(res.guid);
  } catch (error) {
    MsgPrompt.error("Upload file failed, file too large or invalid format");
  }
  uploading.value = false;
};
const beforeAvatarUpload: UploadProps["beforeUpload"] = (rawFile) => {
  if (rawFile.type !== "image/jpeg") {
    ElMessage.error("Avatar picture must be JPG format!");
    return false;
  } else if (rawFile.size / 1024 / 1024 > 10) {
    ElMessage.error("Avatar picture size can not exceed 2MB!");
    return false;
  }
  file.value = rawFile;
  imageUrl.value = URL.createObjectURL(rawFile!);
  return true;
};

const handlePreview = () => {
  dialogImageUrl.value = imageUrl.value;
  dialogVisible.value = true;
};

const handleRemove = () => {
  file.value = undefined;
  imageUrl.value = "";
};

defineExpose({
  uploadImage,
});
</script>
<style scoped>
.avatar-uploader .el-upload {
  border: 1px dashed var(--el-border-color);
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: var(--el-transition-duration-fast);
}

.avatar-uploader .el-upload:hover {
  border-color: var(--el-color-primary);
}

.el-icon.avatar-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  width: 178px;
  height: 178px;
  text-align: center;
}
:deep .el-upload-list--picture-card .el-upload-list__item {
  justify-content: center;
}
:deep .el-image-viewer__img {
  max-width: 90% !important;
  max-height: 90% !important;
}
.image-container {
  width: 178px;
  height: 178px;
  position: relative;
  cursor: pointer;
}
.image-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5); /* Dark overlay */
  display: flex;
  justify-content: center;
  align-items: center;
  opacity: 0;
  transition: opacity 0.5s ease;
  gap: 20px; /* Space between icons */
}
.avatar-uploader:hover .image-overlay {
  opacity: 1;
}
</style>
