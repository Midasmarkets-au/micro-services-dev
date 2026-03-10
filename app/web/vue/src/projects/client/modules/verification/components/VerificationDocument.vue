<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-md-3">
          {{ $t("title.document") }}
        </h2>
      </div>
      <hr />

      <h3 class="mt-11 mb-5">{{ $t("title.photoID") }}</h3>
      <div class="fs-5 mb-5">
        <div class="d-flex">
          <div class="bullet"></div>
          <div>
            {{ $t("tip.verificationDocRule1") }}
          </div>
        </div>
        <div class="d-flex">
          <div class="bullet"></div>
          <div>
            {{ $t("tip.verificationDocRule2") }}
          </div>
        </div>
        <div class="d-flex">
          <div class="bullet"></div>
          <div>
            {{ $t("tip.verificationDocRule3") }}
          </div>
        </div>
        <div class="d-flex">
          <div class="bullet"></div>
          <div>
            {{ $t("tip.fileSizeLimit") }}
          </div>
        </div>
      </div>
      <div>
        <FileUploader
          :title="$t('title.idFront')"
          @file-uploaded="
            (file) =>
              processUploadFiles(file, VerificationDocumentTypes.IdFront)
          "
          :previewMedia="uploadedDocuments.idFront"
          disable-delete
        />
        <FileUploader
          :title="$t('title.idBack')"
          @file-uploaded="
            (file) => processUploadFiles(file, VerificationDocumentTypes.IdBack)
          "
          :previewMedia="uploadedDocuments.idBack"
          disable-delete
        />
        <hr class="mt-19 mb-15" />
        <h3 class="mt-11 mb-5">{{ $t("title.proofOfAddress") }}</h3>
        <div class="fs-5 mb-5">
          <div class="d-flex">
            <div class="bullet"></div>
            <div>
              {{ $t("tip.verificationDocRule5") }}
            </div>
          </div>
          <div class="d-flex">
            <div class="bullet"></div>
            <div>
              {{ $t("tip.verificationDocRule6") }}
            </div>
          </div>
        </div>
        <FileUploader
          file-type="document"
          :title="$t('title.addressDocument')"
          @file-uploaded="
            (file) =>
              processUploadFiles(file, VerificationDocumentTypes.Address)
          "
          :previewMedia="uploadedDocuments.addressDocument"
          disable-delete
        />
      </div>
      <!-- Upload File End -->

      <div class="mt-15 fs-5" style="color: #900000">
        {{ $t("tip.noDocumentAvailable") }}
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive } from "vue";
import FileUploader from "@/components/FileUploaderVerification.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { VerificationDocumentTypes } from "@/core/types/VerificationInfos";

const emits = defineEmits(["saved", "hasError", "resetIndicator"]);

const props = defineProps<{
  verificationDetails?: any;
}>();

const isSubmit = ref(false);
const { t } = useI18n();

const uploadedDocuments = reactive<any>({});

const processUploadFiles = (file: any, type: string) => {
  uploadedDocuments[type] = file;
  console.log("uploadedDocuments", uploadedDocuments);
};

const handleStepSubmit = async () => {
  isSubmit.value = true;
  const formData = new FormData();

  Object.keys(uploadedDocuments).forEach((key) => {
    const file = uploadedDocuments[key];
    if (!file) return;
    formData.append("files[]", file);
    formData.append("types[]", key);
  });

  try {
    await VerificationService.verificationSubmitWithFiles(formData);
    MsgPrompt.success(t("tip.verificationFormSuccess"));
    emits("saved");
  } catch (e) {
    MsgPrompt.error(t("tip.fail")).then(() => location.reload());
  }
};

defineExpose({
  handleStepSubmit,
});
</script>

<style type="scss" scoped>
.bullet {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: #000;
  margin-right: 10px;
  margin-top: 5px;
}
@media (max-width: 768px) {
  .verify-card {
    padding-left: 20px;
    padding-right: 20px;
  }
}
</style>
