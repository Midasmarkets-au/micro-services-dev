<template>
  <SimpleForm
    ref="modelFormRef"
    :title="$t('action.uploadReceipt')"
    :is-loading="isLoading"
    :submit="submitUploadReceipt"
  >
    <div class="d-flex justify-content-center align-items-center">
      <FileUploader
        ref="fileUploaderRef"
        file-type="document"
        :title="''"
        @file-uploaded="onFileUploaded"
        hint=""
        side="client"
        disable-default-upload
      />
    </div>
  </SimpleForm>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import PaymentService from "../services/PaymentService";

import SimpleForm from "@/components/SimpleForm.vue";
import FileUploader from "@/components/FileUploader.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";

const emits = defineEmits(["fetchData"]);

const { t } = i18n.global;
const modelFormRef = ref<InstanceType<typeof SimpleForm>>(null);
const fileUploaderRef = ref<InstanceType<typeof FileUploader>>(null);
const isLoading = ref(false);

const fileGuid = ref<number>();
const fileForm = ref<FormData>();
const id = ref<number>(-1);

const onFileUploaded = (_guid: number, _fileForm: any) => {
  fileGuid.value = _guid;
  fileForm.value = _fileForm;
  // console.log("_guid", _guid);
  // console.log("_fileForm", _fileForm);
};

const show = async (_id: number) => {
  modelFormRef.value?.show();
  id.value = _id;
  fileUploaderRef.value?.clearPreview();
};

const submitUploadReceipt = async () => {
  isLoading.value = true;
  try {
    const res = await PaymentService.postDepositReceiptFile(
      id.value,
      fileForm.value
    );
    if (res) {
      MsgPrompt.success(t("tip.depositUploadSuccess")).then(() => {
        emits("fetchData");
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
