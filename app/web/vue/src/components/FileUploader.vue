<template>
  <div class="mb-6">
    <label class="col-lg-4 col-form-label fw-semobold fs-6">{{ title }}</label>

    <div class="col-lg-8">
      <div class="image-input image-input-outline" data-kt-image-input="true">
        <div
          v-if="!isLoading"
          class="image-input-wrapper overflow-hidden"
          :class="{
            ' w-350px h-250px': !isMobile,
            ' w-300px h-200px': isMobile,
          }"
          :style="
            previewUrl !== '' ? `background-image: url(${previewUrl})` : ''
          "
        >
          <VuePdfEmbed
            v-if="
              [FileFormatTypes.PDF].includes(contentType) && imageHasPreview
            "
            :source="previewUrl !== '' ? previewUrl : ''"
            class="w-100 h-100"
          />
          <div class="w-100 h-100 position-relative">
            <i
              v-if="!imageHasPreview"
              @click="handleFileUploadIconClick"
              class="fa-solid fa-folder-plus fa-2xl position-absolute cursor-pointer"
              style="
                color: #d7e1ee;
                font-size: 12rem;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
              "
            ></i>
          </div>
          <label
            :class="{
              'btn btn-icon btn-circle btn-active-color-primary w-25px h-25px bg-body shadow':
                imageHasPreview,
            }"
            data-kt-image-input-action="change"
            data-bs-toggle="tooltip"
            title="Upload File"
          >
            <i v-if="imageHasPreview" class="bi bi-pencil-fill fs-7"></i>

            <input
              ref="fileUploadInputRef"
              type="file"
              name="file"
              accept=".png, .jpg, .jpeg, .pdf, .heic"
              @change="onChangeFileHandler"
            />
          </label>
        </div>

        <span
          v-if="imageHasPreview"
          class="btn btn-icon btn-circle btn-active-color-primary w-25px h-25px bg-body shadow"
          data-kt-image-input-action="remove"
          data-bs-toggle="tooltip"
          @click="removeImage"
          title="Remove avatar"
        >
          <i class="bi bi-x fs-2"></i>
        </span>
      </div>
      <!--      <div>{{ imageHasPreview }}</div>-->
      <!--      <div>{{ previewUrl.substring(0, 80) }}</div>-->
      <!--      <div class="form-text">{{ props.disableDefaultUpload }}</div>-->
      <!--      <div class="form-text">{{ currentGuid }}</div>-->
      <!--      <div class="form-text">{{ disableRemoveImage }}</div>-->
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from "vue";
import blankImage2 from "@/assets/media/folder/blank-image.svg";
import clientSvc from "@/projects/client/services/ClientGlobalService";
import tenantSvc from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import JwtService from "@/core/services/JwtService";
import { useI18n } from "vue-i18n";
import { isMobile } from "@/core/config/WindowConfig";
import { FileFormatTypes } from "@/core/types/FileFormatTypes";
import VuePdfEmbed from "vue-pdf-embed";

const { t } = useI18n();

const props = withDefaults(
  defineProps<{
    side: string;
    title?: string;
    fileType: string;
    description?: string;
    hint?: string;
    disableDefaultUpload?: boolean;
    partyId?: number;
    handleFileUpload?: (file: any) => void;
  }>(),
  {
    disableDefaultUpload: false,
    description: "Files uploaded ",
    side: "client",
  }
);

const isLoading = ref<boolean>(false);
const blankImage = "";
const fileUploadInputRef = ref<any>();
const handleFileUploadIconClick = () => fileUploadInputRef.value.click();

const contentType = ref<FileFormatTypes>(FileFormatTypes.JPEG);
const previewUrl = ref<string>(blankImage); // used for preview image
let form = new FormData(); // the form of request body
const currentGuid = ref<string>(""); // the guid of current uploaded file
// emits to give back the guid of uploaded file for parent component
const emits = defineEmits<{
  (e: "fileUploaded", guid: string, fileForm?: any): void;
}>();

const onChangeFileHandler = async (e: Event) => {
  const target = e.target as HTMLInputElement;
  if (!target.files) return;
  const file = target.files[0];
  target.value = "";
  contentType.value =
    {
      ["application/pdf"]: FileFormatTypes.PDF,
      ["image/jpeg"]: FileFormatTypes.JPEG,
      ["image/jpg"]: FileFormatTypes.JPG,
      ["image/png"]: FileFormatTypes.PNG,
      ["text/plain"]: FileFormatTypes.TXT,
    }[file.type] ?? FileFormatTypes.JPEG;

  form.append("file", file);

  const reader = new FileReader();
  reader.onload = (e) => {
    console.log(e);
    previewUrl.value = (e.target as FileReader).result as string;
  };
  reader.readAsDataURL(file);

  const uploadFileFunc =
    props.handleFileUpload ??
    {
      client: clientSvc.uploadFile,
      tenant: tenantSvc.uploadFileToUser,
    }[props.side];

  if (props.disableDefaultUpload) {
    currentGuid.value = "-1";
    emits("fileUploaded", "-1", form);
    return;
  }

  // if uploadFileFunc is a promise
  // console.log("uploadFileFunc", typeof uploadFileFunc);
  if (typeof uploadFileFunc === "function") {
    try {
      const data = await uploadFileFunc(form, props.fileType, props.partyId);
      currentGuid.value = data.guid;

      if (props.partyId) {
        MsgPrompt.success(t("tip.submitSuccess")).then(() => {
          emits("fileUploaded", data.guid);
        });
      } else {
        emits("fileUploaded", data.guid);
      }
    } catch (err) {
      clearPreview();
    }
  } else {
    MsgPrompt.error("upload function not found");
  }
};

const removeImage = async () => {
  // console.log("remove image initiated", currentGuid.value);
  isLoading.value = true;
  previewUrl.value = blankImage;
  await nextTick();
  if (!currentGuid.value) return;
  form.delete("uploadedFile");

  if (!props.disableDefaultUpload) {
    const deleteFileFunc =
      {
        client: clientSvc.deleteMediaFileByGuid,
        tenant: tenantSvc.deleteMediaFileByGuid,
      }[props.side] ?? (() => -1);
    try {
      await deleteFileFunc(currentGuid.value);
    } catch (err) {
      MsgPrompt.error(err);
    }
  }
  currentGuid.value = "";
  isLoading.value = false;
};

// if default upload is disabled and there is an existing preview, disable delete func
// because the deleting func is always undefined
const disableRemoveImageFunc = computed(() => {
  return props.disableDefaultUpload && currentGuid.value === "-1";
});

const imageHasPreview = computed(() => previewUrl.value !== blankImage);

const clearPreview = () => {
  previewUrl.value = blankImage;
  form = new FormData();
};

const setPreviewByGuid = async (_guid: string) => {
  // console.log("setPreviewByGuid", props.guid);
  currentGuid.value = _guid;
  let img_src = "";
  img_src =
    window["api"] +
    "/api/api/v1/" +
    (props.side ? (props.side as string) : "tenant") +
    "/media/" +
    _guid +
    "?access_token=" +
    JwtService.getToken();
  previewUrl.value = img_src;
};

defineExpose({
  clearPreview,
  setPreviewByGuid,
});
</script>

<style scoped>
.image-input-wrapper {
  /* background: url(../assets/media/folder/blank-image.svg); */
  background-size: contain;
  background-position: center center;
  background-repeat: no-repeat;
}

/* url(../assets/media/folder/blank-image.svg)  */
</style>
