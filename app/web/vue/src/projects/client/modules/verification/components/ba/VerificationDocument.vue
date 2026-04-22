<template>
  <div class="w-100 card">
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.document") }}
      </div>
    </div>
    <div class="card-body">
      <!-- <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-md-3">
          {{ $t("title.document") }}
        </h2>
      </div>
      <hr /> -->

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

      <div class="mt-15 fs-5" style="color: #f7291a">
        {{ $t("tip.noDocumentAvailable") }}
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { reactive, inject } from "vue";
import FileUploader from "@/components/FileUploaderVerification.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { VerificationDocumentTypes } from "@/core/types/VerificationInfos";
import { isTradeBcr, sliceFile } from "@/core/helpers/fileUploadHelpers";
import { axiosInstance as axios } from "@/core/services/api.client";
import { useStore } from "@/store";
const { t } = useI18n();
const handleDocument = inject<any>("handleDocument");
const canMoveForward = inject<any>("canMoveForward");
const uploadedDocuments = reactive<any>({});
const store = useStore();
const user = store.state.AuthModule.user;

const processUploadFiles = (file: any, type: string) => {
  uploadedDocuments[type] = file;
};

const handleStepSubmit = async () => {
  const formData = new FormData();

  Object.keys(uploadedDocuments).forEach((key) => {
    const file = uploadedDocuments[key];
    if (!file) return;
    formData.append("files[]", file);
    formData.append("types[]", key);
  });

  try {
    await VerificationService.verificationSubmitWithFiles(formData);
    canMoveForward.value = true;
    handleDocument();
    MsgPrompt.success(t("tip.verificationFormSuccess"));
  } catch (e) {
    MsgPrompt.error(t("tip.fail"));
  }
};

const handleStepSubmitSlice = async () => {
  const media = Array<any>();

  for (const key of Object.keys(uploadedDocuments)) {
    const file = uploadedDocuments[key];
    if (!file) continue;

    const chunks = sliceFile(file);
    const contentType = file.type;

    const fieldId = `${user.uid}-${new Date()
      .toISOString()
      .replace(/[-:.]/g, "")}-${Math.floor(10000 + Math.random() * 90000)}`;

    let chunkFailed = false;
    const chunkUploadPromises = chunks.map((chunk, idx) => {
      {
        const chunk = chunks[idx];
        const formData = new FormData();

        formData.append(`FieldId`, fieldId);
        formData.append(`FileName`, file.name);
        formData.append(`ContentType`, contentType);
        formData.append(`Type`, key);
        formData.append(`ChunkIndex`, idx.toString());
        formData.append(`ChunkSize`, chunk.size.toString());
        formData.append(`TotalChunks`, chunks.length.toString());
        formData.append(`TotalSize`, file.size.toString());
        formData.append(`File`, chunk, file.name);

        return axios
          .post("/api/v2/client/media/upload/chunk", formData, {
            headers: { "Content-Type": undefined },
          })
          .then((response) => {
            console.log(`Chunk ${idx} uploaded`, response.data);
          })
          .catch((error) => {
            console.error(`Chunk ${idx} failed`, error);
            chunkFailed = true;
          });
      }
    });

    await Promise.all(chunkUploadPromises);

    if (chunkFailed) {
      MsgPrompt.error(t("tip.fail"));
      return;
    }

    const formData = new FormData();
    formData.append(`FieldId`, fieldId);
    formData.append(`FileName`, file.name);
    formData.append(`ContentType`, contentType);
    formData.append(`Type`, key);
    formData.append(`TotalChunks`, chunks.length.toString());

    try {
      const { data } = await axios.post(
        "/api/v2/client/media/upload/merge",
        formData,
        { headers: { "Content-Type": undefined } }
      );
      media.push(data);
    } catch (e) {
      MsgPrompt.error(t("tip.fail"));
      return;
    }
  }

  try {
    await axios.post("/api/v2/client/verification/document/media/submit", {
      media,
    });
    canMoveForward.value = true;
    handleDocument();
    MsgPrompt.success(t("tip.verificationFormSuccess"));
  } catch (e) {
    MsgPrompt.error(t("tip.fail"));
  }
};

const submit = async () => {
  console.log("istradebcr", isTradeBcr());
  if (isTradeBcr()) {
    await handleStepSubmitSlice();
  } else {
    await handleStepSubmit();
  }
};

defineExpose({
  submit,
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
