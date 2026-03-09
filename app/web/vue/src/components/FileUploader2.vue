<template>
  <div class="mb-6">
    <label class="col-lg-4 col-form-label fw-semobold fs-6">{{ title }}</label>

    <div class="col-lg-8">
      <div class="image-input image-input-outline" data-kt-image-input="true">
        <div
          v-if="!isLoading"
          class="image-input-wrapper overflow-hidden"
          :class="{
            'w-350px h-114px': !isMobile,
            'w-300px h-200px': isMobile,
          }"
          :style="
            previewUrl !== '' ? `background-image: url(${previewUrl})` : ''
          "
        >
          <VuePdfEmbed
            v-if="
              [FileFormatTypes.PDF].includes(contentType) && imageHasPreview
            "
            :source="previewUrl"
            class="w-100 h-100"
          />
          <div
            class="w-100 h-100 position-relative d-flex justify-content-center cursor-pointer"
            @click="handleFileUploadIconClick"
          >
            <i
              v-if="!imageHasPreview"
              class="defaut-file-icon position-absolute"
              >+</i
            >
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
          v-if="imageHasPreview && !props.disableDelete"
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
    <div v-if="showSizeError" class="col-lg-8">
      <div class="text-danger">{{ t("tip.fileSizeLimit") }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from "vue";
import clientSvc from "@/projects/client/services/ClientGlobalService";
import tenantSvc from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { isMobile } from "@/core/config/WindowConfig";
import { FileFormatTypes } from "@/core/types/FileFormatTypes";
import VuePdfEmbed from "vue-pdf-embed";
import store from "@/store";
import { getImageUrlWithToken } from "@/core/plugins/ProcessImageLink";

const { t } = useI18n();

const props = withDefaults(
  defineProps<{
    handleFileUpload?: (file: any) => Promise<any>;
    title?: string;
    disableDefaultUpload?: boolean;
    partyId?: number;
    previewMedia?: any;
    disableDelete?: boolean;
  }>(),
  {
    disableDefaultUpload: false,
    side: "client",
  }
);

const isLoading = ref<boolean>(false);

const fileUploadInputRef = ref<any>();
const handleFileUploadIconClick = () => fileUploadInputRef.value.click();
const showSizeError = ref<boolean>(false);
const contentType = ref<FileFormatTypes>(FileFormatTypes.JPEG);
const BlankImage = "";
const previewUrl = ref<string>(BlankImage); // used for preview image
let form = new FormData(); // the form of request body

const fileObj = ref<any>(null);

// emits to give back the guid of uploaded file for parent component

const emits = defineEmits<{
  (e: "fileUploaded", fileForm?: any): void;
}>();

const onChangeFileHandler = async (e: Event) => {
  const target = e.target as HTMLInputElement;
  if (!target.files) return;
  const file = target.files[0];
  const fileSize = file.size; // less than 20MB
  if (fileSize > 20000000) {
    clearPreview();
    showSizeError.value = true;
    return;
  }
  showSizeError.value = false;

  target.value = "";
  contentType.value =
    {
      ["application/pdf"]: FileFormatTypes.PDF,
      ["image/jpeg"]: FileFormatTypes.JPEG,
      ["image/jpg"]: FileFormatTypes.JPG,
      ["image/png"]: FileFormatTypes.PNG,
      ["text/plain"]: FileFormatTypes.TXT,
    }[file.type] ?? FileFormatTypes.JPEG;

  form.delete("file");
  form.append("file", file);
  const reader = new FileReader();
  reader.onload = (e) => {
    previewUrl.value = (e.target as FileReader).result as string;
  };
  reader.readAsDataURL(file);

  if (props.disableDefaultUpload || !props.handleFileUpload) {
    emits("fileUploaded", form);
    return;
  }

  // if uploadFileFunc is a promise
  // console.log("uploadFileFunc", typeof uploadFileFunc);
  try {
    fileObj.value = await props.handleFileUpload(form);

    MsgPrompt.success(t("tip.submitSuccess")).then(() =>
      emits("fileUploaded", fileObj)
    );
  } catch (err) {
    MsgPrompt.error(err);
  }
};

const removeImage = async () => {
  // console.log("remove image initiated", currentGuid.value);
  isLoading.value = true;
  previewUrl.value = BlankImage;
  await nextTick();
  if (!fileObj.value) return;
  form.delete("uploadedFile");

  const roles = store.state.AuthModule?.user?.roles;

  const deleteFileFunc =
    roles && roles.includes("TenantAdmin")
      ? tenantSvc.deleteMediaFileByGuid
      : clientSvc.deleteMediaFileByGuid;
  try {
    console.log(fileObj.value);
    if (fileObj.value.guid) await deleteFileFunc(fileObj.value.guid);
  } catch (err) {
    MsgPrompt.error(err);
  }
  fileObj.value = null;
  isLoading.value = false;
};

// if default upload is disabled and there is an existing preview, disable delete func
// because the deleting func is always undefined
const disableRemoveImageFunc = computed(() => {
  return props.disableDefaultUpload && fileObj.value === null;
});

const imageHasPreview = computed(() => previewUrl.value !== BlankImage);

const clearPreview = () => {
  previewUrl.value = BlankImage;
  form = new FormData();
};

watch(
  () => props.previewMedia,
  () => {
    // console.log("previewMedia", props.previewMedia);

    if (!props.previewMedia) return;
    if (props.previewMedia.guid === fileObj.value?.guid) return;

    previewUrl.value = getImageUrlWithToken(props.previewMedia.guid);
    fileObj.value = props.previewMedia;
  }
);

// onMounted(() => {
//   console.log("previewMedia", props.previewMedia);
//   if (props.previewMedia) {
//     previewUrl.value = getImageUrlWithToken(props.previewMedia.guid);
//     console.log(previewUrl.value);
//   }
// });

defineExpose({
  clearPreview,
});
</script>

<style scoped lang="scss">
.image-input-wrapper {
  /* background: url(../assets/media/folder/blank-image.svg); */
  background-size: contain;
  background-position: center center;
  background-repeat: no-repeat;
  box-shadow: none !important;
  border: 1px dashed #cde0ff !important;
}
.defaut-file-icon {
  background-image: url("/images/default-file.png");
  background-size: cover;
  background-repeat: no-repeat;
  background-position: 0px 10px;
  width: 141px;
  height: 115px;
  align-items: center;
  justify-content: center;
  padding-left: 27px;
  padding-top: 5px;
  color: #fff;
  font-size: 30px;
  display: flex;
  font-style: normal;
}
/* url(../assets/media/folder/blank-image.svg)  */
</style>
