<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="card-body">
      <div class="d-flex justify-content-between">
        <div>
          <h2 class="fw-bold d-flex align-items-center text-dark ms-md-3">
            Chunk Upload Test
          </h2>

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
          </div>
          <div>
            <FileUploader
              :title="$t('title.idBack')"
              @file-uploaded="
                (file) =>
                  processUploadFiles(file, VerificationDocumentTypes.IdBack)
              "
              :previewMedia="uploadedDocuments.idBack"
              disable-delete
            />
          </div>
          <div>
            <el-button
              @click="submit"
              :loading="isLoading"
              :disabled="isLoading"
              >Submit by Domain</el-button
            >

            <el-button
              @click="submitSlice"
              :loading="isLoading"
              :disabled="isLoading"
              >Submit Slice</el-button
            >
          </div>
        </div>

        <div>
          <h2 class="fw-bold d-flex align-items-center text-dark ms-md-3">
            Regular Upload Test
          </h2>
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
          </div>
          <div>
            <FileUploader
              :title="$t('title.idBack')"
              @file-uploaded="
                (file) =>
                  processUploadFiles(file, VerificationDocumentTypes.IdBack)
              "
              :previewMedia="uploadedDocuments.idBack"
              disable-delete
            />
          </div>
          <div>
            <el-button
              @click="submitRegular"
              :loading="isLoading"
              :disabled="isLoading"
              >Submit</el-button
            >
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { reactive, ref } from "vue";
import FileUploader from "@/components/FileUploaderVerification.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { VerificationDocumentTypes } from "@/core/types/VerificationInfos";
import { isTradeBcr, sliceFile } from "@/core/helpers/fileUploadHelpers";
import { axiosInstance as axios } from "@/core/services/api.client";
import { useStore } from "@/store";
const { t } = useI18n();

const uploadedDocuments = reactive<any>({});
const isLoading = ref(false);
const store = useStore();
const user = store.state.AuthModule.user;

const processUploadFiles = (file: any, type: string) => {
  uploadedDocuments[type] = file;
};

const chunkSize = ref(1024 * 1024 * 1);

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
    MsgPrompt.success(t("tip.verificationFormSuccess"));
  } catch (e) {
    MsgPrompt.error(t("tip.fail")).then(() => location.reload());
  }
};

const handleStepSubmitSlice3 = async () => {
  const media = Array<any>();

  for (const key of Object.keys(uploadedDocuments)) {
    const file = uploadedDocuments[key];
    if (!file) continue;

    const chunks = sliceFile(file);
    const contentType = file.type;

    const fieldId = `${user.uid}-${new Date()
      .toISOString()
      .replace(/[-:.]/g, "")}-${Math.floor(10000 + Math.random() * 90000)}`;

    console.log(fieldId);
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
        formData.append(`File`, chunk);

        return axios
          .post("/api/v2/client/media/upload/chunk", formData)
          .then((response) => {
            console.log(`Chunk ${idx} uploaded`, response.data);
          })
          .catch((error) => {
            console.error(`Chunk ${idx} failed`, error);
            MsgPrompt.error(t("tip.fail")).then(() => location.reload());
          });
      }
    });

    try {
      await Promise.all(chunkUploadPromises);
    } catch (e) {
      MsgPrompt.error(t("tip.fail")).then(() => location.reload());
    }
    console.log("All chunks uploaded");

    const formData = new FormData();
    formData.append(`FieldId`, fieldId);
    formData.append(`FileName`, file.name);
    formData.append(`ContentType`, contentType);
    formData.append(`Type`, key);
    formData.append(`TotalChunks`, chunks.length.toString());

    try {
      const { data } = await axios.post(
        "/api/v2/client/media/upload/merge",
        formData
      );
      console.log(data);
      media.push(data);
    } catch (e) {
      MsgPrompt.error(t("tip.fail")).then(() => location.reload());
    }
  }

  try {
    await axios.post("/api/v2/client/verification/document/media/submit", {
      media,
    });
    // canMoveForward.value = true;
    // handleDocument();
    MsgPrompt.success(t("tip.verificationFormSuccess"));
  } catch (e) {
    MsgPrompt.error(t("tip.fail")).then(() => location.reload());
  }
};

const submit = async () => {
  isLoading.value = true;
  console.log("istradebcr", isTradeBcr());
  if (isTradeBcr()) {
    await handleStepSubmitSlice3();
  } else {
    await handleStepSubmit();
  }

  isLoading.value = false;
};

const submitSlice = async () => {
  isLoading.value = true;
  await handleStepSubmitSlice3();
  isLoading.value = false;
};

const submitRegular = async () => {
  isLoading.value = true;
  await handleStepSubmit();
  isLoading.value = false;
};

const handleStepSubmitSlice = async () => {
  const formData = new FormData();

  Object.keys(uploadedDocuments).forEach((key) => {
    const file = uploadedDocuments[key];
    const contentType = file.type;
    if (!file) return;

    const chunks = sliceFile(file);
    chunks.forEach((chunk, _) => {
      formData.append(`files[]`, chunk);
      formData.append(`types[]`, key);
      formData.append(`contentTypes[]`, contentType);
    });
  });

  try {
    await VerificationService.verificationSubmitWithSlicedFiles(formData);
    MsgPrompt.success(t("tip.verificationFormSuccess"));
  } catch (e) {
    MsgPrompt.error(t("tip.fail")).then(() => location.reload());
  }
};

const handleStepSubmitSlice2 = async () => {
  const media = Array<any>();

  for (const key of Object.keys(uploadedDocuments)) {
    const file = uploadedDocuments[key];
    if (!file) continue;

    const chunks = sliceFile(file);
    const contentType = file.type;

    let fieldId = "";
    console.log(fieldId);
    for (let idx = 0; idx < chunks.length; idx++) {
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
      formData.append(`File`, chunk);

      try {
        const { data } = await axios.post(
          "/api/v2/client/media/upload/chunk",
          formData
        );
        console.log(data);
        fieldId = data;
      } catch (e) {
        console.log(e);
      }
    }

    const formData = new FormData();
    formData.append(`FieldId`, fieldId);
    formData.append(`FileName`, file.name);
    formData.append(`ContentType`, contentType);
    formData.append(`Type`, key);
    formData.append(`TotalChunks`, chunks.length.toString());

    const { data } = await axios.post(
      "/api/v2/client/media/upload/merge",
      formData
    );
    console.log(data);
    media.push(data);
  }

  await axios.post("/api/v2/client/verification/document/media/submit", {
    media,
  });
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
