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
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import FileUploader from "@/components/FileUploader.vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
import { ref } from "vue";

const fileUploaderRef = ref<InstanceType<typeof FileUploader>>();
const modelFormRef = ref<InstanceType<typeof SimpleForm>>();

const fileForm = ref<FormData>();
const fileGuid = ref<number>();
const hashId = ref<number>(-1);
const isLoading = ref(false);
const uid = ref<number>(0);
const { t } = i18n.global;

const onFileUploaded = (_guid: number, _fileForm: any) => {
  fileGuid.value = _guid;
  fileForm.value = _fileForm;
};

const show = async (_uid: number, _hashId: any) => {
  isLoading.value = true;

  fileUploaderRef.value?.clearPreview();
  modelFormRef.value?.show();

  hashId.value = _hashId;
  uid.value = _uid;

  try {
    const res = await WalletService.getDepositReceiptFileV2(_uid, _hashId);
    if (res && res.length > 0) {
      fileUploaderRef.value?.setPreviewByGuid(res[res.length - 1]);
    }
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

const submitUploadReceipt = async () => {
  isLoading.value = true;
  try {
    const res = await WalletService.postDepositReceiptFileV2(
      uid.value,
      hashId.value,
      fileForm.value
    );
    if (res) {
      MsgPrompt.success(t("tip.depositUploadSuccess")).then(() => {
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
