<template>
  <div v-loading="uploading">
    <el-upload
      ref="uploadRef"
      class="avatar-uploader"
      :before-upload="beforeAvatarUpload"
      action="#"
      :auto-upload="false"
      :show-file-list="false"
      v-model:file-list="fileList"
      v-show="!imageUrl"
    >
      <div>
        <el-icon class="avatar-uploader-icon"><Plus /></el-icon>
      </div>
    </el-upload>

    <div
      v-if="imageUrl"
      class="avatar-uploader image-container d-flex align-items-center justify-content-center"
    >
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
import { ref, watch } from "vue";
import type {
  UploadInstance,
  UploadProps,
  UploadRawFile,
  UploadUserFile,
} from "element-plus";
import EventServices from "../../services/EventsServices";
import { ElMessage } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Delete, Plus, ZoomIn } from "@element-plus/icons-vue";
import { ElNotification } from "element-plus";
const file = ref<UploadRawFile>();
const fileList = ref<UploadUserFile[]>();
const dialogImageUrl = ref("");
const dialogVisible = ref(false);
const imageUrls = ref<any>([]);
const uploading = ref(false);
const uploadRef = ref<UploadInstance>();
const imageUrl = ref("");
const type = ref("cover");
const uploadImage = async () => {
  const formData = new FormData();
  if (file.value === undefined) {
    return;
  }
  formData.append("file", file.value);
  try {
    const res = await EventServices.uploadEventImage(type.value, formData);
    imageUrls.value = [res.guid];
  } catch (error) {
    MsgPrompt.error("Upload file failed, file too large or invalid format");
  }
};

const beforeAvatarUpload: UploadProps["beforeUpload"] = (rawFile) => {
  if (
    rawFile.type !== "image/jpeg" &&
    rawFile.type !== "image/jpg" &&
    rawFile.type !== "image/png"
  ) {
    fileList.value = [];
    ElNotification.error("Avatar picture must be JPG/JPEG/PNG format!");
    return false;
  } else if (rawFile.size / 1024 / 1024 > 10) {
    fileList.value = [];
    ElNotification.error("Avatar picture size can not exceed 10MB!");
    return false;
  }
  file.value = rawFile;
  if (rawFile) {
    imageUrl.value = URL.createObjectURL(rawFile);
  } else {
    // Handle the case when rawFile is null or undefined
    console.error("rawFile is null or undefined");
    fileList.value = [];
  }
  return true;
};

const handlePreview = () => {
  dialogImageUrl.value = imageUrl.value;
  dialogVisible.value = true;
};

const handleRemove = () => {
  fileList.value = [];
};

watch(fileList, (newVal) => {
  if (newVal) {
    if (newVal.length === 0) {
      imageUrl.value = "";
      return;
    }
    beforeAvatarUpload(newVal[0].raw as UploadRawFile);
  }
});

defineExpose({
  uploadImage,
  imageUrl,
  imageUrls,
  uploading,
  handleRemove,
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
.avatar-uploader .avatar {
  width: auto;
  height: auto;
  max-height: 178px;
  max-width: 178px;
  display: block;
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
