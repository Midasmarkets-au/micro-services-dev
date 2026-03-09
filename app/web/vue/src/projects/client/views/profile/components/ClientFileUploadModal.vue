<template>
  <SimpleForm
    ref="modelFormRef"
    :title="$t('title.fileUpload')"
    :is-loading="isLoading"
    :submit="submitUploadReceipt"
  >
    <div
      class="d-flex flex-column justify-content-center align-items-center w-full px-6"
    >
      <el-form-item :label="$t('fields.fileType')" class="w-full">
        <el-select v-model="fileType" class="w-full">
          <el-option
            :label="$t('title.idFront')"
            :value="VerificationDocumentTypes.IdFront"
          />
          <el-option
            :label="$t('title.idBack')"
            :value="VerificationDocumentTypes.IdBack"
          />
          <el-option
            :label="$t('title.proofOfAddress')"
            :value="VerificationDocumentTypes.Address"
          />
          <el-option
            :label="$t('title.additionalDocuments')"
            :value="VerificationDocumentTypes.Other"
          />
        </el-select>
      </el-form-item>
      <FileUploader
        ref="fileUploaderRef"
        :title="''"
        @file-uploaded="onFileUploaded"
        disable-default-upload
        disable-delete
      />
    </div>
  </SimpleForm>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import i18n from "@/core/plugins/i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import FileUploader from "@/components/FileUploader2.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import { VerificationDocumentTypes } from "@/core/types/VerificationInfos";

const { t } = i18n.global;
const isLoading = ref(false);
const modelFormRef = ref<InstanceType<typeof SimpleForm>>(null);
const fileUploaderRef = ref<InstanceType<typeof FileUploader>>(null);

const fileForm = ref<FormData>();
const fileType = ref(VerificationDocumentTypes.Other);

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const onFileUploaded = (_fileForm: any) => {
  fileForm.value = _fileForm;
  // console.log("_fileForm", _fileForm);
};

const show = async () => {
  modelFormRef.value?.show();
  fileUploaderRef.value?.clearPreview();
};

const submitUploadReceipt = async () => {
  isLoading.value = true;
  try {
    const res = await VerificationService.uploadDocumentsForVerification(
      fileType.value,
      fileForm.value
    );
    if (res) {
      MsgPrompt.success(t("tip.fileUploadSuccess")).then(() => {
        emits("refresh");
        modelFormRef.value?.hide();
      });
    }
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
  hide: () => modelFormRef.value?.hide(),
});
</script>
